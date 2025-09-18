from pydantic import BaseModel


class TextRequest(BaseModel):
    text: str


# Note: TranscribeRequest is not used since we accept binary data directly
# in the request body for transcription endpoint