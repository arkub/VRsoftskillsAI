from fastapi import FastAPI, HTTPException, status, Request, Header
from fastapi.responses import Response
from fastapi.middleware.cors import CORSMiddleware
from utils.logger import logger
from utils.speech import text_to_speech, speech_to_text_from_binary
import os
import uvicorn
from typing import Optional
from models import TextRequest, AudioProcessRequest
from utils.agent import chat_agent


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


@app.post("/v1/generate_audio", status_code=status.HTTP_200_OK, summary="Generate an audio file from a text.", description="Generate an audio file from a text.", tags=["Audio Generation"])
async def generate_audio(
    request: TextRequest,
    api_key: Optional[str] = Header(None)
):
    """
    Generate an audio file from a text.
    Args:
        request: TextRequest containing the text to convert
        api_key: API key for authentication
    Returns:
        Binary WAV audio data
    """
    # Validate authorization token
    token = api_key.replace("Bearer ", "") if api_key else ""
    if token != os.getenv("API_KEY"):
        raise HTTPException(status_code=401, detail="Invalid authorization token")

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


@app.post("/v1/transcribe_audio", response_model=dict, status_code=status.HTTP_200_OK, summary="Stream audio transcription", description="Transcribe audio sent as binary data", tags=["Audio Transcription"])
async def stream_audio_transcription(
    request: Request,
    api_key: Optional[str] = Header(None),
    content_type: Optional[str] = Header(None)
):
    """
    Transcribe audio sent as binary data via --data-binary

    Args:
        request: Request object containing binary audio data
        api_key: Bearer token for authentication
        content_type: Content type of the audio file

    Returns:
        dict: Transcription result
    """
    try:
        # Validate authorization token
        token = api_key.replace("Bearer ", "") if api_key else ""
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


@app.post("/v1/chat", status_code=status.HTTP_200_OK, summary="Audio chat with AI agent", description="Transcribe audio, process through chat agent, and return speech response", tags=["Audio Chat"])
async def audio_chat(
    request: Request,
    api_key: Optional[str] = Header(None),
    content_type: Optional[str] = Header(None)
):
    """
    Process audio through the complete pipeline:
    1. Transcribe audio to text
    2. Process text through chat agent
    3. Convert response to speech
    4. Return audio binary

    Args:
        request: Request object containing binary audio data and session_id in query params
        api_key: API key for authentication
        content_type: Content type of the audio file

    Returns:
        Binary WAV audio data with the agent's response
    """
    try:
        logger.info(f"Received audio chat request with API key: {api_key}")
        # Validate authorization token
        token = api_key.replace("Bearer ", "") if api_key else ""
        logger.info(f"Received audio chat request with API key: {token}")
        if token != os.getenv("API_KEY"):
            raise HTTPException(status_code=401, detail="Invalid authorization token")

        # Get session_id from query parameters
        session_id = request.query_params.get("session_id")
        if not session_id:
            raise HTTPException(status_code=400, detail="session_id query parameter is required")
        
        logger.info(f"Received audio chat request with session_id: {session_id}")

        # Read binary audio data from request body
        audio_data = await request.body()
        if not audio_data:
            raise HTTPException(status_code=400, detail="No audio data received")
        # Determine file extension from content type
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

        logger.info(f"Starting audio chat pipeline for session {session_id} (size: {len(audio_data)} bytes, format: {filename})")

        # Step 1: Transcribe audio to text
        logger.info(f"Step 1: Transcribing audio for session {session_id}")
        transcription_response = speech_to_text_from_binary(audio_data=audio_data, filename=filename)
        if not transcription_response["success"]:
            logger.error(f"Transcription failed: {transcription_response['message']}")
            raise HTTPException(status_code=400, detail=f"Transcription failed: {transcription_response['message']}")

        transcribed_text = transcription_response["message"]
        logger.info(f"Step 1 completed: Audio transcribed successfully for session {session_id}: {transcribed_text[:100]}...")

        # Step 2: Process text through chat agent
        logger.info(f"Step 2: Processing text through chat agent for session {session_id}")
        agent_response = chat_agent(session_id=session_id, user_message=transcribed_text)
        if not agent_response["success"]:
            logger.error(f"Chat agent failed: {agent_response['message']}")
            raise HTTPException(status_code=500, detail=f"Chat agent processing failed: {agent_response['message']}")

        npc_output = agent_response["npc_output"]
        response_text = npc_output.npc_response
        logger.info(f"Step 2 completed: Chat agent processed successfully for session {session_id}")

        # Step 3: Convert response to speech
        logger.info(f"Step 3: Converting response to speech for session {session_id}")
        tts_response = text_to_speech(text=response_text)
        if not tts_response["success"]:
            logger.error(f"Text-to-speech failed: {tts_response['message']}")
            raise HTTPException(status_code=500, detail=f"Text-to-speech failed: {tts_response['message']}")

        logger.info(f"Step 3 completed: Text-to-speech conversion successful for session {session_id}")
        logger.info(f"Audio chat pipeline completed successfully for session {session_id}")

        # Step 4: Return audio binary
        return Response(
            content=tts_response["audio_data"],
            media_type="audio/wav",
            headers={
                "Content-Disposition": "attachment; filename=response.wav",
                "X-Session-ID": session_id,
                "X-Transcription": transcribed_text[:100] + "..." if len(transcribed_text) > 100 else transcribed_text,
                "X-Agent-Response": response_text[:100] + "..." if len(response_text) > 100 else response_text
            }
        )

    except HTTPException:
        raise
    except Exception as e:
        logger.error(f"Error in audio processing pipeline: {str(e)}")
        raise HTTPException(status_code=500, detail=f"Internal server error: {str(e)}")


if __name__ == "__main__":
    uvicorn.run("main:app", host="0.0.0.0", port=8000, reload=True)
