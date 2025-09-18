from dotenv import load_dotenv
from io import BytesIO
from utils.helper import client
from utils.logger import logger
import os
import sys


load_dotenv()

OPENAI_STT_MODEL = os.getenv("OPENAI_STT_MODEL")
OPENAI_TTS_MODEL = os.getenv("OPENAI_TTS_MODEL")


def speech_to_text_from_binary(audio_data: bytes, filename: str = "audio.wav"):
    """
    Transcribes audio from binary data using OpenAI's Whisper model.

    Args:
        audio_data (bytes): Binary audio data
        filename (str): Filename for format detection (required by OpenAI API)

    Returns:
        dict: The transcribed text if successful, or error information if failed
    """
    try:
        # Create a file-like object from binary data
        audio_file = BytesIO(audio_data)
        audio_file.name = filename  # Required for OpenAI to detect format

        transcription = client.audio.transcriptions.create(
            model=OPENAI_STT_MODEL,
            language='en',
            file=audio_file,
            response_format="text"
        )
        logger.info(f"Transcript generated from binary data ({len(audio_data)} bytes): {transcription}")
        return {"success": True, "message": transcription}

    except Exception as e:
        _, _, exc_tb = sys.exc_info()
        fname = os.path.split(exc_tb.tb_frame.f_code.co_filename)[1]
        logger.error(f"Error in transcribing binary audio in {fname} at line {exc_tb.tb_lineno}: {str(e)}")
        return {"success": False, "message": "Error in transcribing audio"}


def text_to_speech(text):
    """
    Generates audio from text using OpenAI's text-to-speech API.
    
    Args:
        input_text (str): Text to be converted to speech
        interview_id (str): ID of the interview
        
    Returns:
        OpenAI response object if successful, or error dictionary if failed
    """
    try:
        logger.info(f"Generating audio...")
        with client.audio.speech.with_streaming_response.create(
            model=OPENAI_TTS_MODEL,
            voice="alloy",
            input=text,
            response_format="wav",
            instructions ="Speak in an Indian English accent, slightly formal and clear."
        ) as response:
            logger.info(f"Response: {response}")
            # Read binary audio data from streaming response
            audio_data = response.read()
            return {"success": True, "audio_data": audio_data, "message": "Audio generated successfully"}
    
    except Exception as e:
        _, _, exc_tb = sys.exc_info()
        fname = os.path.split(exc_tb.tb_frame.f_code.co_filename)[1]
        logger.error(f"Error in generating audio in {fname} at line {exc_tb.tb_lineno}: {str(e)}")
        return{"success": False, "message": "Error generating audio"}
