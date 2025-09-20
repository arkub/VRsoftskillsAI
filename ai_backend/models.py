from pydantic import BaseModel


class TextRequest(BaseModel):
    text: str


class NPCResponse(BaseModel):
    npc_response: str   # natural conversational reply
    reaction: str       # description of avatar's physical/voice reaction
    coach_tip: str      # short coaching feedback


class AudioProcessRequest(BaseModel):
    session_id: str
