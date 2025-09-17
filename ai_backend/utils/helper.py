from dotenv import load_dotenv
from openai import OpenAI
import os
import secrets


load_dotenv()

client = OpenAI(api_key=os.getenv("OPENAI_API_KEY"))


def generate_secret_key(generate: bool = False):
    """
    Generate a secret key.
    """
    if generate:
        api_key = f"ag-{secrets.token_urlsafe(32)}"
        print(f"API Key: {api_key}")
        
        # Save the API key to the .env file
        with open(".env", "w") as f:
            f.write(f"API_KEY={api_key}")
    
    return os.getenv("OPENAI_API_KEY")
