from fastapi import FastAPI, HTTPException, status, Request, Header
from fastapi.responses import Response
from fastapi.middleware.cors import CORSMiddleware
from utils.logger import logger
from utils.speech import text_to_speech, speech_to_text_from_binary
import os
import uvicorn
from typing import Optional
from models import TextRequest


app = FastAPI()


app.add_middleware(
    CORSMiddleware,
    allow_origins=["*"],
    allow_credentials=True,
    allow_methods=["*"],
    allow_headers=["*"],
)


@app.get("/")
async def root():
    return {"message": "Hello World"}


@app.post("/v1/speech/generate_audio", status_code=status.HTTP_200_OK, summary="Generate an audio file from a text.", description="Generate an audio file from a text.", tags=["Audio Generation"])
async def generate_audio(
    request: TextRequest,
    x_api_key: Optional[str] = Header(None, alias="x-api-key")
):
    """
    Generate an audio file from a text.
    Args:
        request: TextRequest containing the text to convert
        x_api_key: API key for authentication
    Returns:
        Binary WAV audio data
    """
    # Validate API key
    if x_api_key != os.getenv("API_KEY"):
        raise HTTPException(status_code=401, detail="Invalid API key")

    response = text_to_speech(text=request.text)
    if response["success"]:
        logger.info(f"Audio generated successfully!")
        return Response(
            content=response["audio_data"],
            media_type="audio/wav",
            headers={"Content-Disposition": "attachment; filename=response.wav"}
        )

    else:
        logger.error(f"Error in generating audio: {response['message']}")
        raise HTTPException(status_code=status.HTTP_400_BAD_REQUEST, detail=response["message"])


@app.post("/v1/speech/transcribe_audio", response_model=dict, status_code=status.HTTP_200_OK, summary="Stream audio transcription", description="Transcribe audio sent as binary data", tags=["Audio Transcription"])
async def stream_audio_transcription(
    request: Request,
    authorization: Optional[str] = Header(None),
    content_type: Optional[str] = Header(None)
):
    """
    Transcribe audio sent as binary data via --data-binary

    Args:
        request: Request object containing binary audio data
        authorization: Bearer token for authentication
        content_type: Content type of the audio file

    Returns:
        dict: Transcription result
    """
    try:
        # Validate authorization token
        token = authorization.replace("Bearer ", "") if authorization else ""
        if token != os.getenv("API_KEY"):
            raise HTTPException(status_code=401, detail="Invalid authorization token")

        # Read binary audio data from request body
        audio_data = await request.body()

        if not audio_data:
            raise HTTPException(status_code=400, detail="No audio data received")

        # Determine file extension from content type for format detection
        filename = "audio.wav"  # Default
        if content_type:
            if "mp3" in content_type:
                filename = "audio.mp3"
            elif "flac" in content_type:
                filename = "audio.flac"
            elif "m4a" in content_type:
                filename = "audio.m4a"
            elif "mp4" in content_type:
                filename = "audio.mp4"
            elif "ogg" in content_type:
                filename = "audio.ogg"
            elif "webm" in content_type:
                filename = "audio.webm"

        logger.info(f"Processing stream audio (size: {len(audio_data)} bytes, format: {filename})")

        # Transcribe the audio directly from binary data
        response = speech_to_text_from_binary(audio_data=audio_data, filename=filename)

        if response["success"]:
            logger.info("Stream audio transcribed successfully!")
            return {
                "success": True,
                "transcription": response["message"],
                "audio_size_bytes": len(audio_data),
            }
        
        else:
            logger.error(f"Error in transcribing stream audio: {response['message']}")
            return {
                "success": False,
                "message": response["message"],
            }

    except Exception as e:
        logger.error(f"Error in stream audio transcription: {str(e)}")
        raise HTTPException(status_code=500, detail=f"Internal server error: {str(e)}")


if __name__ == "__main__":
    uvicorn.run(app, host="0.0.0.0", port=8000)
