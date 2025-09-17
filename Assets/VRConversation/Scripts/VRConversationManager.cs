// using System;
// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;
// using UnityEngine.Events;
// using UnityEngine.XR.Interaction.Toolkit;

// namespace VRConversation
// {
//     /// <summary>
//     /// Main conversation manager that orchestrates the entire VR conversation experience
//     /// Connects VoiceInputManager, NPCController, TTSManager, and ConversationReportUI
//     /// </summary>
//     public class VRConversationManager : MonoBehaviour
//     {
//         [Header("Core Components")]
//         [SerializeField] private VoiceInputManager voiceInputManager;
//         [SerializeField] private NPCController npcController;
//         [SerializeField] private TTSManager ttsManager;
//         [SerializeField] private ConversationReportUI reportUI;
        
//         [Header("Conversation Settings")]
//         [SerializeField] private string conversationTopic = "General Chat";
//         [SerializeField] private string userName = "User";
//         [SerializeField] private float maxConversationDuration = 300f; // 5 minutes
//         [SerializeField] private int maxDialogueTurns = 50;
//         [SerializeField] private bool autoStartConversation = false;
        
//         [Header("VR Interaction")]
//         [SerializeField] private XRRig xrRig;
//         [SerializeField] private Transform userHeadTransform;
//         [SerializeField] private XRController leftController;
//         [SerializeField] private XRController rightController;
        
//         [Header("Session Management")]
//         [SerializeField] private bool enableSessionRecording = true;
//         [SerializeField] private bool enableAnalytics = true;
//         [SerializeField] private float idleTimeout = 30f;
        
//         [Header("Debug Settings")]
//         [SerializeField] private bool enableDebugLogs = true;
//         [SerializeField] private bool showUIDebugInfo = false;
//         [SerializeField] private KeyCode debugToggleKey = KeyCode.F1;
        
//         // Events
//         [System.Serializable]
//         public class ConversationEvent : UnityEvent<ConversationData> { }
//         [System.Serializable]
//         public class ConversationStateEvent : UnityEvent<ConversationState> { }
        
//         public ConversationEvent OnConversationStarted;
//         public ConversationEvent OnConversationEnded;
//         public ConversationStateEvent OnStateChanged;
//         public UnityEvent OnUserSpeaking;
//         public UnityEvent OnNPCSpeaking;
        
//         // State management
//         public enum ConversationState
//         {
//             Idle,
//             Starting,
//             Active,
//             Paused,
//             Ending,
//             Completed
//         }
        
//         // Private variables
//         private ConversationData currentConversation;
//         private ConversationState currentState = ConversationState.Idle;
//         private Coroutine conversationCoroutine;
//         private Coroutine idleTimeoutCoroutine;
//         private float conversationStartTime;
//         private float lastInteractionTime;
//         private bool isInitialized = false;
        
//         // Voice recognition tracking
//         private VoiceRecognitionResult lastVoiceResult;
//         private bool isWaitingForUserInput = false;
//         private bool isWaitingForNPCResponse = false;
        
//         #region Unity Lifecycle
        
//         private void Awake()
//         {
//             InitializeComponents();
//         }
        
//         private void Start()
//         {
//             InitializeConversationManager();
            
//             if (autoStartConversation)
//             {
//                 StartCoroutine(DelayedAutoStart());
//             }
//         }
        
//         private void Update()
//         {
//             HandleDebugInput();
//             UpdateConversationState();
//         }
        
//         private void OnDestroy()
//         {
//             CleanupConversationManager();
//         }
        
//         #endregion
        
//         #region Initialization
        
//         private void InitializeComponents()
//         {
//             // Find components if not assigned
//             if (voiceInputManager == null)
//                 voiceInputManager = FindObjectOfType<VoiceInputManager>();
            
//             if (npcController == null)
//                 npcController = FindObjectOfType<NPCController>();
            
//             if (ttsManager == null)
//                 ttsManager = FindObjectOfType<TTSManager>();
            
//             if (reportUI == null)
//                 reportUI = FindObjectOfType<ConversationReportUI>();
            
//             if (xrRig == null)
//                 xrRig = FindObjectOfType<XRRig>();
            
//             // Find user head transform
//             if (userHeadTransform == null && xrRig != null)
//             {
//                 userHeadTransform = xrRig.cameraGameObject?.transform;
//             }
            
//             // Find controllers
//             if (leftController == null || rightController == null)
//             {
//                 var controllers = FindObjectsOfType<XRController>();
//                 foreach (var controller in controllers)
//                 {
//                     if (controller.controllerNode == UnityEngine.XR.XRNode.LeftHand)
//                         leftController = controller;
//                     else if (controller.controllerNode == UnityEngine.XR.XRNode.RightHand)
//                         rightController = controller;
//                 }
//             }
//         }
        
//         private void InitializeConversationManager()
//         {
//             try
//             {
//                 // Subscribe to component events
//                 SetupEventSubscriptions();
                
//                 // Configure NPC to look at user
//                 if (npcController != null && userHeadTransform != null)
//                 {
//                     npcController.SetLookAtTarget(userHeadTransform);
//                 }
                
//                 // Initialize conversation state
//                 SetConversationState(ConversationState.Idle);
                
//                 isInitialized = true;
//                 LogDebug("VR Conversation Manager initialized successfully");
//             }
//             catch (Exception e)
//             {
//                 LogDebug($"Failed to initialize conversation manager: {e.Message}");
//             }
//         }
        
//         private void SetupEventSubscriptions()
//         {
//             // Voice input events
//             if (voiceInputManager != null)
//             {
//                 voiceInputManager.OnVoiceRecognized.AddListener(OnVoiceRecognized);
//                 voiceInputManager.OnVoiceInputStart.AddListener(OnVoiceInputStarted);
//                 voiceInputManager.OnVoiceInputEnd.AddListener(OnVoiceInputEnded);
//             }
            
//             // NPC events
//             if (npcController != null)
//             {
//                 npcController.OnStateChanged.AddListener(OnNPCStateChanged);
//                 npcController.OnNPCResponse.AddListener(OnNPCResponseReceived);
//                 npcController.OnEmotionChanged.AddListener(OnNPCEmotionChanged);
//             }
            
//             // TTS events
//             if (ttsManager != null)
//             {
//                 ttsManager.OnTTSStart.AddListener(OnTTSStarted);
//                 ttsManager.OnTTSComplete.AddListener(OnTTSCompleted);
//                 ttsManager.OnTTSError.AddListener(OnTTSError);
//             }
            
//             // Report UI events
//             if (reportUI != null)
//             {
//                 reportUI.OnReportAction.AddListener(OnReportAction);
//                 reportUI.OnReportClosed.AddListener(OnReportClosed);
//             }
//         }
        
//         private IEnumerator DelayedAutoStart()
//         {
//             yield return new WaitForSeconds(2f); // Wait for everything to initialize
//             StartConversation();
//         }
        
//         #endregion
        
//         #region Conversation Control
        
//         public void StartConversation()
//         {
//             if (currentState != ConversationState.Idle)
//             {
//                 LogDebug("Cannot start conversation: already in progress");
//                 return;
//             }
            
//             LogDebug("Starting new conversation...");
            
//             // Create new conversation data
//             currentConversation = new ConversationData
//             {
//                 userName = userName,
//                 npcCharacterId = npcController?.name ?? "NPC",
//                 conversationTopic = conversationTopic,
//                 sessionStartTime = DateTime.Now
//             };
            
//             // Set initial state
//             SetConversationState(ConversationState.Starting);
            
//             // Start conversation coroutine
//             conversationCoroutine = StartCoroutine(ConversationFlow());
            
//             OnConversationStarted?.Invoke(currentConversation);
//         }
        
//         public void EndConversation()
//         {
//             if (currentState == ConversationState.Idle || currentState == ConversationState.Completed)
//             {
//                 LogDebug("No active conversation to end");
//                 return;
//             }
            
//             LogDebug("Ending conversation...");
//             SetConversationState(ConversationState.Ending);
//         }
        
//         public void PauseConversation()
//         {
//             if (currentState == ConversationState.Active)
//             {
//                 SetConversationState(ConversationState.Paused);
//                 LogDebug("Conversation paused");
//             }
//         }
        
//         public void ResumeConversation()
//         {
//             if (currentState == ConversationState.Paused)
//             {
//                 SetConversationState(ConversationState.Active);
//                 LogDebug("Conversation resumed");
//             }
//         }
        
//         private IEnumerator ConversationFlow()
//         {
//             conversationStartTime = Time.time;
//             lastInteractionTime = Time.time;
            
//             // Initialize conversation
//             yield return StartCoroutine(InitializeConversation());
            
//             // Main conversation loop
//             SetConversationState(ConversationState.Active);
            
//             while (currentState == ConversationState.Active || currentState == ConversationState.Paused)
//             {
//                 // Check conversation limits
//                 if (ShouldEndConversation())
//                 {
//                     EndConversation();
//                     break;
//                 }
                
//                 // Handle paused state
//                 if (currentState == ConversationState.Paused)
//                 {
//                     yield return null;
//                     continue;
//                 }
                
//                 // Wait for user input or timeout
//                 yield return StartCoroutine(WaitForUserInteraction());
                
//                 yield return null;
//             }
            
//             // End conversation
//             yield return StartCoroutine(FinalizeConversation());
//         }
        
//         private IEnumerator InitializeConversation()
//         {
//             // NPC greeting
//             if (npcController != null)
//             {
//                 string greeting = $"Hello {userName}! I'm ready to chat about {conversationTopic}. How are you today?";
//                 npcController.ProcessNPCResponse(greeting, "friendly", 1f);
                
//                 // Wait for NPC to finish greeting
//                 yield return new WaitUntil(() => npcController.IsNPCAvailable());
//             }
            
//             // Start voice input
//             if (voiceInputManager != null)
//             {
//                 voiceInputManager.StartListening();
//             }
            
//             LogDebug("Conversation initialized");
//         }
        
//         private IEnumerator WaitForUserInteraction()
//         {
//             isWaitingForUserInput = true;
//             float waitStartTime = Time.time;
            
//             // Start idle timeout
//             if (idleTimeoutCoroutine != null)
//             {
//                 StopCoroutine(idleTimeoutCoroutine);
//             }
//             idleTimeoutCoroutine = StartCoroutine(IdleTimeoutCoroutine());
            
//             while (isWaitingForUserInput && currentState == ConversationState.Active)
//             {
//                 yield return null;
//             }
            
//             // Stop idle timeout
//             if (idleTimeoutCoroutine != null)
//             {
//                 StopCoroutine(idleTimeoutCoroutine);
//                 idleTimeoutCoroutine = null;
//             }
//         }
        
//         private IEnumerator IdleTimeoutCoroutine()
//         {
//             yield return new WaitForSeconds(idleTimeout);
            
//             if (isWaitingForUserInput && currentState == ConversationState.Active)
//             {
//                 // Prompt user
//                 if (npcController != null)
//                 {
//                     string[] prompts = {
//                         "Are you still there?",
//                         "Feel free to say something whenever you're ready.",
//                         "I'm here when you want to continue our conversation.",
//                         "Take your time, I'm listening."
//                     };
                    
//                     string prompt = prompts[UnityEngine.Random.Range(0, prompts.Length)];
//                     npcController.ProcessNPCResponse(prompt, "curious", 0.6f);
//                 }
                
//                 LogDebug("Idle timeout prompted user");
//             }
//         }
        
//         private IEnumerator FinalizeConversation()
//         {
//             // NPC farewell
//             if (npcController != null && npcController.IsNPCAvailable())
//             {
//                 string farewell = "Thank you for our conversation! It was really nice talking with you.";
//                 npcController.ProcessNPCResponse(farewell, "happy", 0.8f);
                
//                 yield return new WaitUntil(() => npcController.IsNPCAvailable());
//             }
            
//             // Stop voice input
//             if (voiceInputManager != null)
//             {
//                 voiceInputManager.StopListening();
//             }
            
//             // Finalize conversation data
//             if (currentConversation != null)
//             {
//                 currentConversation.sessionEndTime = DateTime.Now;
//                 currentConversation.totalDuration = Time.time - conversationStartTime;
//                 CalculateConversationMetrics();
//             }
            
//             // Set final state
//             SetConversationState(ConversationState.Completed);
            
//             // Show report
//             if (reportUI != null && currentConversation != null)
//             {
//                 yield return new WaitForSeconds(1f); // Brief pause
//                 reportUI.ShowReport(currentConversation);
//             }
            
//             OnConversationEnded?.Invoke(currentConversation);
//             LogDebug("Conversation finalized");
//         }
        
//         #endregion
        
//         #region Event Handlers
        
//         private void OnVoiceRecognized(VoiceRecognitionResult result)
//         {
//             if (currentState != ConversationState.Active || !result.isSuccess)
//                 return;
            
//             lastVoiceResult = result;
//             lastInteractionTime = Time.time;
//             isWaitingForUserInput = false;
            
//             // Add to conversation data
//             if (currentConversation != null)
//             {
//                 DialogueTurn userTurn = new DialogueTurn(DialogueTurn.SpeakerType.User, result.recognizedText, Time.time);
//                 userTurn.confidenceScore = result.confidence;
//                 userTurn.duration = result.duration;
//                 userTurn.recognizedKeywords = result.keywords;
                
//                 currentConversation.dialogueTurns.Add(userTurn);
//                 currentConversation.totalUserTurns++;
                
//                 // Update detection metrics
//                 currentConversation.successfulRecognitions++;
//                 foreach (string keyword in result.keywords)
//                 {
//                     if (!currentConversation.detectedKeywords.Contains(keyword))
//                     {
//                         currentConversation.detectedKeywords.Add(keyword);
//                     }
//                 }
//             }
            
//             // Process user input with NPC
//             if (npcController != null)
//             {
//                 npcController.ProcessUserInput(result.recognizedText);
//             }
            
//             OnUserSpeaking?.Invoke();
//             LogDebug($"User input processed: '{result.recognizedText}'");
//         }
        
//         private void OnVoiceInputStarted()
//         {
//             // Visual feedback for voice input start
//             LogDebug("User started speaking");
//         }
        
//         private void OnVoiceInputEnded()
//         {
//             // Visual feedback for voice input end
//             LogDebug("User stopped speaking");
//         }
        
//         private void OnNPCStateChanged(NPCState.ConversationState npcState)
//         {
//             // Update conversation flow based on NPC state
//             switch (npcState)
//             {
//                 case NPCState.ConversationState.Speaking:
//                     isWaitingForNPCResponse = false;
//                     OnNPCSpeaking?.Invoke();
//                     break;
//                 case NPCState.ConversationState.Idle:
//                     if (currentState == ConversationState.Active)
//                     {
//                         isWaitingForUserInput = true;
//                     }
//                     break;
//             }
//         }
        
//         private void OnNPCResponseReceived(string response)
//         {
//             lastInteractionTime = Time.time;
//             LogDebug($"NPC response: '{response}'");
//         }
        
//         private void OnNPCEmotionChanged(string emotion, float intensity)
//         {
//             // Track emotional flow
//             if (currentConversation != null)
//             {
//                 EmotionData emotionData = new EmotionData(emotion, intensity, Time.time);
//                 currentConversation.emotionalFlow.Add(emotionData);
//             }
//         }
        
//         private void OnTTSStarted(string text)
//         {
//             LogDebug($"TTS started for: '{text}'");
//         }
        
//         private void OnTTSCompleted(TTSResult result)
//         {
//             if (result.isSuccess)
//             {
//                 LogDebug("TTS completed successfully");
//             }
//             else
//             {
//                 LogDebug($"TTS failed: {result.errorMessage}");
//             }
//         }
        
//         private void OnTTSError(string error)
//         {
//             LogDebug($"TTS error: {error}");
//         }
        
//         private void OnReportAction(string action)
//         {
//             switch (action.ToLower())
//             {
//                 case "restart":
//                     RestartConversation();
//                     break;
//                 case "export":
//                     LogDebug("Report exported");
//                     break;
//                 case "share":
//                     LogDebug("Report shared");
//                     break;
//             }
//         }
        
//         private void OnReportClosed()
//         {
//             // Reset to idle state
//             SetConversationState(ConversationState.Idle);
//         }
        
//         #endregion
        
//         #region State Management
        
//         private void SetConversationState(ConversationState newState)
//         {
//             if (currentState == newState) return;
            
//             ConversationState previousState = currentState;
//             currentState = newState;
            
//             LogDebug($"Conversation state changed: {previousState} -> {newState}");
//             OnStateChanged?.Invoke(newState);
//         }
        
//         private bool ShouldEndConversation()
//         {
//             if (currentConversation == null) return true;
            
//             // Check time limit
//             float elapsed = Time.time - conversationStartTime;
//             if (elapsed > maxConversationDuration)
//             {
//                 LogDebug("Conversation ended: time limit reached");
//                 return true;
//             }
            
//             // Check turn limit
//             if (currentConversation.dialogueTurns.Count >= maxDialogueTurns)
//             {
//                 LogDebug("Conversation ended: turn limit reached");
//                 return true;
//             }
            
//             return false;
//         }
        
//         private void UpdateConversationState()
//         {
//             // Handle state-specific updates
//             switch (currentState)
//             {
//                 case ConversationState.Active:
//                     // Check for long idle periods
//                     if (Time.time - lastInteractionTime > idleTimeout * 3)
//                     {
//                         LogDebug("Ending conversation due to extended inactivity");
//                         EndConversation();
//                     }
//                     break;
//             }
//         }
        
//         #endregion
        
//         #region Metrics and Analytics
        
//         private void CalculateConversationMetrics()
//         {
//             if (currentConversation == null) return;
            
//             // Calculate response times
//             float totalUserResponseTime = 0f;
//             float totalNPCResponseTime = 0f;
//             int userResponseCount = 0;
//             int npcResponseCount = 0;
            
//             for (int i = 1; i < currentConversation.dialogueTurns.Count; i++)
//             {
//                 DialogueTurn currentTurn = currentConversation.dialogueTurns[i];
//                 DialogueTurn previousTurn = currentConversation.dialogueTurns[i - 1];
                
//                 float responseTime = currentTurn.timestamp - (previousTurn.timestamp + previousTurn.duration);
//                 currentTurn.responseTime = responseTime;
                
//                 if (currentTurn.speaker == DialogueTurn.SpeakerType.User)
//                 {
//                     totalUserResponseTime += responseTime;
//                     userResponseCount++;
//                 }
//                 else
//                 {
//                     totalNPCResponseTime += responseTime;
//                     npcResponseCount++;
//                 }
//             }
            
//             // Calculate averages
//             currentConversation.averageUserResponseTime = userResponseCount > 0 ? totalUserResponseTime / userResponseCount : 0f;
//             currentConversation.averageNPCResponseTime = npcResponseCount > 0 ? totalNPCResponseTime / npcResponseCount : 0f;
            
//             // Calculate recognition accuracy
//             float totalRecognitions = currentConversation.successfulRecognitions + currentConversation.failedRecognitions;
//             currentConversation.recognitionAccuracy = totalRecognitions > 0 ? currentConversation.successfulRecognitions / totalRecognitions : 1f;
            
//             // Determine dominant emotion
//             Dictionary<string, float> emotionTotals = new Dictionary<string, float>();
//             foreach (var emotionData in currentConversation.emotionalFlow)
//             {
//                 if (emotionTotals.ContainsKey(emotionData.emotion))
//                 {
//                     emotionTotals[emotionData.emotion] += emotionData.intensity;
//                 }
//                 else
//                 {
//                     emotionTotals[emotionData.emotion] = emotionData.intensity;
//                 }
//             }
            
//             string dominantEmotion = "neutral";
//             float maxIntensity = 0f;
//             foreach (var emotionPair in emotionTotals)
//             {
//                 if (emotionPair.Value > maxIntensity)
//                 {
//                     maxIntensity = emotionPair.Value;
//                     dominantEmotion = emotionPair.Key;
//                 }
//             }
//             currentConversation.dominantEmotion = dominantEmotion;
            
//             LogDebug("Conversation metrics calculated");
//         }
        
//         #endregion
        
//         #region Debug and Utility
        
//         private void HandleDebugInput()
//         {
//             if (Input.GetKeyDown(debugToggleKey))
//             {
//                 enableDebugLogs = !enableDebugLogs;
//                 LogDebug($"Debug logging {(enableDebugLogs ? "enabled" : "disabled")}");
//             }
//         }
        
//         private void LogDebug(string message)
//         {
//             if (enableDebugLogs)
//             {
//                 Debug.Log($"[VRConversationManager] {message}");
//             }
//         }
        
//         private void CleanupConversationManager()
//         {
//             // Stop coroutines
//             if (conversationCoroutine != null)
//             {
//                 StopCoroutine(conversationCoroutine);
//             }
            
//             if (idleTimeoutCoroutine != null)
//             {
//                 StopCoroutine(idleTimeoutCoroutine);
//             }
            
//             // Unsubscribe from events
//             if (voiceInputManager != null)
//             {
//                 voiceInputManager.OnVoiceRecognized.RemoveListener(OnVoiceRecognized);
//                 voiceInputManager.OnVoiceInputStart.RemoveListener(OnVoiceInputStarted);
//                 voiceInputManager.OnVoiceInputEnd.RemoveListener(OnVoiceInputEnded);
//             }
            
//             if (npcController != null)
//             {
//                 npcController.OnStateChanged.RemoveListener(OnNPCStateChanged);
//                 npcController.OnNPCResponse.RemoveListener(OnNPCResponseReceived);
//                 npcController.OnEmotionChanged.RemoveListener(OnNPCEmotionChanged);
//             }
            
//             if (ttsManager != null)
//             {
//                 ttsManager.OnTTSStart.RemoveListener(OnTTSStarted);
//                 ttsManager.OnTTSComplete.RemoveListener(OnTTSCompleted);
//                 ttsManager.OnTTSError.RemoveListener(OnTTSError);
//             }
//         }
        
//         public void RestartConversation()
//         {
//             // End current conversation
//             if (currentState != ConversationState.Idle && currentState != ConversationState.Completed)
//             {
//                 EndConversation();
//             }
            
//             // Wait a moment, then start new conversation
//             StartCoroutine(RestartConversationCoroutine());
//         }
        
//         private IEnumerator RestartConversationCoroutine()
//         {
//             yield return new WaitUntil(() => currentState == ConversationState.Completed || currentState == ConversationState.Idle);
//             yield return new WaitForSeconds(1f);
//             StartConversation();
//         }
        
//         #endregion
        
//         #region Public API
        
//         public ConversationState CurrentState => currentState;
//         public ConversationData CurrentConversation => currentConversation;
//         public bool IsInitialized => isInitialized;
        
//         public void SetConversationTopic(string topic)
//         {
//             conversationTopic = topic;
//             LogDebug($"Conversation topic set to: {topic}");
//         }
        
//         public void SetUserName(string name)
//         {
//             userName = name;
//             LogDebug($"User name set to: {name}");
//         }
        
//         public void SetMaxDuration(float duration)
//         {
//             maxConversationDuration = duration;
//             LogDebug($"Max conversation duration set to: {duration}s");
//         }
        
//         public float GetConversationProgress()
//         {
//             if (currentState != ConversationState.Active || currentConversation == null)
//                 return 0f;
            
//             float elapsed = Time.time - conversationStartTime;
//             return Mathf.Clamp01(elapsed / maxConversationDuration);
//         }
        
//         public int GetTurnCount()
//         {
//             return currentConversation?.dialogueTurns.Count ?? 0;
//         }
        
//         #endregion
//     }
// }