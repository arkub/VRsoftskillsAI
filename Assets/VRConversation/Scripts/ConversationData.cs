using System;
using System.Collections.Generic;
using UnityEngine;

namespace VRConversation
{
    /// <summary>
    /// Data structure to store conversation session information
    /// </summary>
    [System.Serializable]
    public class ConversationData
    {
        [Header("Session Information")]
        public string sessionId;
        public DateTime sessionStartTime;
        public DateTime sessionEndTime;
        public float totalDuration;
        
        [Header("Participant Data")]
        public string userName;
        public string npcCharacterId;
        public string conversationTopic;
        
        [Header("Dialogue Metrics")]
        public List<DialogueTurn> dialogueTurns = new List<DialogueTurn>();
        public int totalUserTurns;
        public int totalNPCTurns;
        public float averageUserResponseTime;
        public float averageNPCResponseTime;
        
        [Header("Voice Recognition Metrics")]
        public int successfulRecognitions;
        public int failedRecognitions;
        public float recognitionAccuracy;
        public List<string> detectedKeywords = new List<string>();
        
        [Header("Emotional Analysis")]
        public List<EmotionData> emotionalFlow = new List<EmotionData>();
        public string dominantEmotion;
        
        public ConversationData()
        {
            sessionId = System.Guid.NewGuid().ToString();
            sessionStartTime = DateTime.Now;
        }
    }
    
    /// <summary>
    /// Individual dialogue turn data
    /// </summary>
    [System.Serializable]
    public class DialogueTurn
    {
        public enum SpeakerType { User, NPC }
        
        public SpeakerType speaker;
        public string text;
        public float timestamp;
        public float duration;
        public float responseTime;
        public string emotion;
        public float confidenceScore;
        public List<string> recognizedKeywords = new List<string>();
        
        public DialogueTurn(SpeakerType speakerType, string dialogueText, float time)
        {
            speaker = speakerType;
            text = dialogueText;
            timestamp = time;
            duration = 0f;
            responseTime = 0f;
            emotion = "neutral";
            confidenceScore = 1f;
        }
    }
    
    /// <summary>
    /// Emotion data for tracking emotional flow
    /// </summary>
    [System.Serializable]
    public class EmotionData
    {
        public string emotion;
        public float intensity;
        public float timestamp;
        
        public EmotionData(string emotionType, float emotionIntensity, float time)
        {
            emotion = emotionType;
            intensity = emotionIntensity;
            timestamp = time;
        }
    }
    
    /// <summary>
    /// Voice recognition result
    /// </summary>
    [System.Serializable]
    public class VoiceRecognitionResult
    {
        public bool isSuccess;
        public string recognizedText;
        public float confidence;
        public float duration;
        public List<string> keywords = new List<string>();
        public string errorMessage;
        
        public VoiceRecognitionResult()
        {
            isSuccess = false;
            recognizedText = "";
            confidence = 0f;
            duration = 0f;
            errorMessage = "";
        }
    }
    
    /// <summary>
    /// TTS generation result
    /// </summary>
    [System.Serializable]
    public class TTSResult
    {
        public bool isSuccess;
        public AudioClip audioClip;
        public float duration;
        public string errorMessage;
        
        public TTSResult()
        {
            isSuccess = false;
            audioClip = null;
            duration = 0f;
            errorMessage = "";
        }
    }
    
    /// <summary>
    /// NPC state information
    /// </summary>
    [System.Serializable]
    public class NPCState
    {
        public enum ConversationState
        {
            Idle,
            Listening,
            Thinking,
            Speaking,
            Emoting
        }
        
        public ConversationState currentState;
        public string currentEmotion;
        public float emotionIntensity;
        public bool isActive;
        public string lastResponse;
        public float stateStartTime;
        
        public NPCState()
        {
            currentState = ConversationState.Idle;
            currentEmotion = "neutral";
            emotionIntensity = 0.5f;
            isActive = false;
            lastResponse = "";
            stateStartTime = 0f;
        }
    }
}