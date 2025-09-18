from datetime import datetime
from models import NPCResponse
from system_prompt import SYSTEM_PROMPT
from typing import Dict, List
from utils.helper import client
from utils.logger import logger
import json


# In-Memory Storage for conversation histories
message_histories: Dict[str, List[Dict]] = {}


# Conversation Management
def add_message(session_id: str, role: str, content: str):
    """
    Add a message to the session's conversation history.

    Args:
        session_id: The ID of the session to add the message to.
        role: The role of the message sender (e.g., "user", "assistant").
        content: The content of the message.

    Returns:
        None
    """
    logger.info(f"Adding message for session {session_id}, role: {role}")

    # Initialize new session with system prompt if it doesn't exist
    if session_id not in message_histories:
        logger.info(f"Creating new session {session_id} with system prompt")
        message_histories[session_id] = [{
            "role": "system",
            "content": SYSTEM_PROMPT,
            "timestamp": datetime.now().isoformat()
        }]

    # Append the new message to the session's history
    message_histories[session_id].append({
        "role": role,
        "content": content,
        "timestamp": datetime.now().isoformat()
    })

    logger.debug(f"Message added. Session {session_id} now has {len(message_histories[session_id])} messages")


def get_conversation_history(session_id: str, include_timestamps: bool = False):
    """
    Get the full conversation history for a given session.

    Args:
        session_id: The ID of the session to retrieve the history for.
        include_timestamps: Whether to include timestamps in the returned messages.

    Returns:
        A list of messages from the conversation history.
    """
    logger.debug(f"Retrieving conversation history for session {session_id}")

    # Get conversation history for the session, empty list if session doesn't exist
    history = message_histories.get(session_id, [])

    # Log warning if no history found for the session
    if not history:
        logger.warning(f"No conversation history found for session {session_id}")
    else:
        logger.debug(f"Found {len(history)} messages for session {session_id}")

    # Return full history with timestamps if requested
    if include_timestamps:
        return history

    # Return simplified history without timestamps for API calls
    return [{"role": m["role"], "content": m["content"]} for m in history]


def save_conversation_to_file(session_id: str, filename: str):
    """
    Save conversation history to a JSON file.

    Args:
        session_id: The ID of the session to save the history for.
        filename: The path to the file to save the history to.

    Returns:
        None
    """
    try:
        # Save conversation history to JSON file with proper formatting
        with open(filename, "w") as f:
            json.dump(message_histories.get(session_id, []), f, indent=4)
        logger.info(f"Conversation for session {session_id} saved to {filename}")

    except Exception as e:
        # Log any file I/O errors
        logger.error(f"Error saving conversation: {e}")


# Chat Agent
def chat_agent(
    session_id: str,
    user_message: str = None
):
    """
    Chat with the agent using message history.

    Args:
        session_id: The ID of the session to chat with.
        user_message: The message to send to the agent.

    Returns:
        A structured NPCResponse.
    """
    logger.info(f"Starting chat agent for session {session_id}")

    try:
        # Add user message to conversation history if provided
        if user_message:
            logger.info(f"Processing user message for session {session_id}: {user_message[:100]}...")
            add_message(session_id=session_id, role="user", content=user_message)

        # Get conversation history for OpenAI API call
        messages_to_send = get_conversation_history(session_id=session_id)
        logger.info(f"Sending {len(messages_to_send)} messages to OpenAI API")

        # Make API call to OpenAI with structured response format
        response = client.chat.completions.create(
            model="gpt-4o",  # Using GPT-4 for better quality responses
            messages=messages_to_send,
            response_format={
                "type": "json_schema",
                "json_schema": {
                    "name": "npc_response",
                    "schema": NPCResponse.model_json_schema()  # Enforce structured JSON response
                }
            }
        )

        # Extract and log the assistant's response
        logger.info(f"Received response from OpenAI API for session {session_id}")
        assistant_response = response.choices[0].message.content
        logger.debug(f"Assistant response: {assistant_response[:200]}...")

        # Parse the JSON response into NPCResponse model
        npc_output = NPCResponse.model_validate_json(assistant_response)
        logger.info(f"Successfully parsed NPC response for session {session_id}")

        # Save assistant's response to conversation history
        add_message(session_id=session_id, role="assistant", content=assistant_response)
        logger.info(f"Chat agent completed successfully for session {session_id}")

        # Save the conversation history to a file
        save_conversation_to_file(session_id=session_id, filename=f"conversation_{session_id}.json")

        return {"success": True, "npc_output": npc_output}

    except Exception as e:
        # Handle and log any errors that occur during the chat process
        logger.error(f"Error in chat agent for session {session_id}: {str(e)}")
        return {"success": False, "message": "Error in chat agent"}
