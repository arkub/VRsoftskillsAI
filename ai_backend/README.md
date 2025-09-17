# AI Backend - Speech Processing API

A FastAPI-based backend service for speech-to-text and text-to-speech processing using OpenAI's APIs.

## Features

- **Speech-to-Text**: Transcribe audio files using OpenAI's Whisper model
- **Text-to-Speech**: Generate audio from text using OpenAI's TTS model
- Binary data processing (no file I/O required)
- Support for multiple audio formats

## Setup

1. Install dependencies:
```bash
uv sync
```

2. Set up environment variables in `.env`:
```
OPENAI_API_KEY=your_openai_api_key_here
API_KEY=your_api_key_for_authentication
OPENAI_STT_MODEL=whisper-1
OPENAI_TTS_MODEL=gpt-4o-mini-tts
```

3. Run the server:
```bash
uv run python main.py
```

The server will start on `http://0.0.0.0:8000`

## API Endpoints

### 1. Speech-to-Text (Audio Transcription)

**Endpoint:** `POST /v1/speech/transcribe_audio`

**Description:** Transcribe audio sent as binary data

**Headers:**
- `Authorization: Bearer YOUR_API_KEY`
- `Content-Type: audio/{format}` (e.g., `audio/wav`, `audio/mp3`, `audio/flac`)

**Example:**
```bash
curl -X POST "http://localhost:8000/v1/speech/transcribe_audio" \
  -H "Authorization: Bearer your-api-key-here" \
  -H "Content-Type: audio/wav" \
  --data-binary @recording.wav
```

**Supported Audio Formats:**
- WAV (`audio/wav`)
- MP3 (`audio/mp3`)
- FLAC (`audio/flac`)
- M4A (`audio/m4a`)
- MP4 (`audio/mp4`)
- OGG (`audio/ogg`)
- WebM (`audio/webm`)

**Response:**
```json
{
  "success": true,
  "transcription": "This is the transcribed text from your audio.",
  "audio_size_bytes": 1234567
}
```

### 2. Text-to-Speech (Audio Generation)

**Endpoint:** `POST /v1/speech/generate`

**Description:** Generate audio file from text

**Parameters:**
- `text` (string): The text to convert to speech

**Example:**
```bash
curl -X POST "http://localhost:8000/v1/speech/generate" \
  -H "Content-Type: application/json" \
  -d '{"text": "Hello, this is a test message"}'
```

**Response:**
```json
{
  "success": true,
  "message": "Audio generated successfully"
}
```

## Error Handling

All endpoints return structured error responses:

```json
{
  "success": false,
  "message": "Error description"
}
```

Common error codes:
- `400`: Bad request (invalid audio format, missing data, etc.)
- `401`: Unauthorized (invalid API key)
- `500`: Internal server error

## Audio Processing

- **Binary Processing**: Audio data is processed directly in memory without creating temporary files
- **Format Detection**: File format is determined from the Content-Type header
- **Language**: Currently configured for English transcription
- **Voice**: TTS uses "alloy" voice with Indian English accent

## Development

The API uses:
- **FastAPI** for the web framework
- **OpenAI Python SDK** for AI processing
- **Pydantic** for data validation
- **Custom logging** for debugging

## Notes

- Ensure your OpenAI API key has sufficient credits for speech processing
- Audio files are processed entirely in memory for better performance
- The API supports CORS for web application integration