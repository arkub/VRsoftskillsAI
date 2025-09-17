from dotenv import load_dotenv
from utils.helper import client
from utils.logger import logger
import os
import sys
import json
from datetime import datetime



load_dotenv()

# Global message history storage
message_histories = {}



def prepare_messages(messages: list, role: str):
    """
    Prepare the messages for the agent.
    """
    return [{"role": role, "content": message} for message in messages]


def initialize_conversation(session_id: str, system_prompt: str = None):
    """
    Initialize a new conversation with optional system prompt.

    Args:
        session_id: str - Unique identifier for the conversation
        system_prompt: str - Optional system prompt to set context
    """
    message_histories[session_id] = []
    if system_prompt:
        add_message(session_id, "system", system_prompt)
    logger.info(f"Initialized conversation for session: {session_id}")


def add_message(session_id: str, role: str, content: str, system_prompt: str = None):
    """
    Add a message to the conversation history.

    Args:
        session_id: str - Session identifier
        role: str - Message role (user, assistant, system)
        content: str - Message content
        system_prompt: str - System prompt to initialize conversation if new session
    """
    if session_id not in message_histories:
        default_system_prompt = system_prompt or "You are a helpful AI assistant conducting technical interviews."
        initialize_conversation(session_id, default_system_prompt)

    message = {
        "role": role,
        "content": content
    }
    message_histories[session_id].append(message)
    logger.info(f"Added {role} message to session {session_id}")


def get_conversation_history(session_id: str, include_timestamps: bool = False):
    """
    Get the conversation history for a session.

    Args:
        session_id: str - Session identifier
        include_timestamps: bool - Whether to include timestamps

    Returns:
        list: Conversation history
    """
    if session_id not in message_histories:
        return []

    if include_timestamps:
        return message_histories[session_id]
    else:
        return [{"role": msg["role"], "content": msg["content"]}
                for msg in message_histories[session_id]]


def clear_conversation(session_id: str):
    """
    Clear conversation history for a session.

    Args:
        session_id: str - Session identifier
    """
    if session_id in message_histories:
        del message_histories[session_id]
        logger.info(f"Cleared conversation history for session: {session_id}")


def save_conversation_to_file(session_id: str, file_path: str = None):
    """
    Save conversation history to a JSON file.

    Args:
        session_id: str - Session identifier
        file_path: str - Optional file path (defaults to logs/conversations/)
    """
    if session_id not in message_histories:
        logger.error(f"No conversation found for session: {session_id}")
        return False

    if not file_path:
        os.makedirs("logs/conversations", exist_ok=True)
        file_path = f"logs/conversations/{session_id}_{datetime.now().strftime('%Y%m%d_%H%M%S')}.json"

    try:
        with open(file_path, 'w') as f:
            json.dump(message_histories[session_id], f, indent=2)
        logger.info(f"Saved conversation to: {file_path}")
        return True
    except Exception as e:
        logger.error(f"Error saving conversation: {e}")
        return False


def chat_agent(messages: list = None, session_id: str = None, user_message: str = None, system_prompt: str = None):
    """
    Chat with the agent using message history.

    Args:
        messages: list - Direct messages (optional if using session_id)
        session_id: str - Session identifier for maintaining history
        user_message: str - New user message to add to history
        system_prompt: str - Custom system prompt for new sessions

    Returns:
        str: The response from the agent.
    """
    try:
        # If using session-based conversation
        if session_id:
            if user_message:
                add_message(session_id, "user", user_message, system_prompt)

            messages_to_send = get_conversation_history(session_id)
        else:
            # Use provided messages directly
            messages_to_send = messages or []

        response = client.chat.completions.create(
            model="gpt-4o-mini",
            messages=messages_to_send
        )

        assistant_response = response.choices[0].message.content

        # Add assistant response to history if using session
        if session_id:
            add_message(session_id, "assistant", assistant_response)

        return assistant_response

    except Exception as e:
        logger.error(f"Error in chat agent: {e}")
        return {"success": False, "message": str(e)}