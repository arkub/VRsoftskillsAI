// using System;
// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;
// using UnityEngine.Events;
// // NOTE: Add Convai SDK using statements when SDK is imported
// // using Convai.Scripts.Runtime.Core;
// // using Convai.Scripts.Runtime.Features;

// namespace VRConversation
// {
//     /// <summary>
//     /// Controls NPC behavior, animation, and integration with Convai conversation system
//     /// Manages NPC states, responses, and visual feedback during conversations
//     /// </summary>
//     public class NPCController : MonoBehaviour
//     {
//         [Header("NPC Identity")]
//         [SerializeField] private string npcName = "Assistant";
//         [SerializeField] private string characterId = ""; // Convai character ID
//         [SerializeField] private string npcDescription = "A helpful AI assistant";
        
//         [Header("Animation System")]
//         [SerializeField] private Animator npcAnimator;
//         [SerializeField] private SkinnedMeshRenderer npcRenderer;
//         [SerializeField] private Transform lookAtTarget; // For eye tracking/head following
        
//         [Header("Audio Components")]
//         [SerializeField] private AudioSource voiceAudioSource;
//         [SerializeField] private AudioSource ambientAudioSource;
//         [SerializeField] private TTSManager ttsManager;
        
//         [Header("Convai Integration")]
//         [SerializeField] private bool useConvaiSDK = true;
//         [SerializeField] private string convaiAPIKey = "";
//         [SerializeField] private float responseTimeout = 10f;
        
//         [Header("Animation Parameters")]
//         [SerializeField] private string idleStateName = "Idle";
//         [SerializeField] private string listeningStateName = "Listening";
//         [SerializeField] private string speakingStateName = "Speaking";
//         [SerializeField] private string thinkingStateName = "Thinking";
        
//         [Header("Emotion System")]
//         [SerializeField] private bool enableEmotions = true;
//         [SerializeField] private List<EmotionMapping> emotionMappings = new List<EmotionMapping>();
//         [SerializeField] private float emotionTransitionSpeed = 2f;
        
//         [Header("Lip Sync")]
//         [SerializeField] private bool enableLipSync = true;
//         [SerializeField] private Transform[] lipSyncBones;
//         [SerializeField] private float lipSyncIntensity = 1f;
        
//         [Header("Gaze and Body Language")]
//         [SerializeField] private bool enableGazeTracking = true;
//         [SerializeField] private Transform headBone;
//         [SerializeField] private Transform[] eyeBones;
//         [SerializeField] private float gazeSpeed = 2f;
//         [SerializeField] private float gazeRandomness = 0.1f;
        
//         [Header("Gesture System")]
//         [SerializeField] private bool enableGestures = true;
//         [SerializeField] private List<GestureClip> gestureClips = new List<GestureClip>();
        
//         [Header("Debug Settings")]
//         [SerializeField] private bool enableDebugLogs = true;
//         [SerializeField] private bool showStateInUI = true;
        
//         // Events
//         [System.Serializable]
//         public class NPCStateChangeEvent : UnityEvent<NPCState.ConversationState> { }
//         [System.Serializable]
//         public class NPCResponseEvent : UnityEvent<string> { }
//         [System.Serializable]
//         public class NPCEmotionEvent : UnityEvent<string, float> { }
        
//         public NPCStateChangeEvent OnStateChanged;
//         public NPCResponseEvent OnNPCResponse;
//         public NPCEmotionEvent OnEmotionChanged;
        
//         // Private variables
//         private NPCState currentState = new NPCState();
//         private ConversationData currentConversation;
//         private Coroutine currentAnimationCoroutine;
//         private Coroutine gazeCoroutine;
//         private Vector3 originalHeadRotation;
//         private Vector3 currentGazeTarget;
        
//         // Convai SDK references (placeholders - replace with actual SDK types)
//         private object convaiConversationManager; // ConversationManager
//         private object convaiCharacter; // ConvaiCharacter
//         private object convaiAudioResponse; // ConvaiAudioResponse
        
//         // Animation parameter hashes for performance
//         private int stateParameterHash;
//         private int emotionParameterHash;
//         private int speakingParameterHash;
//         private int gestureParameterHash;
        
//         [System.Serializable]
//         public class EmotionMapping
//         {
//             public string emotionName;
//             public AnimationClip animationClip;
//             public string animatorParameter;
//             public Color emotionColor = Color.white;
//             public float intensity = 1f;
//         }
        
//         [System.Serializable]
//         public class GestureClip
//         {
//             public string gestureName;
//             public AnimationClip clip;
//             public string triggerKeyword;
//             public float probability = 0.3f;
//         }
        
//         #region Unity Lifecycle
        
//         private void Awake()
//         {
//             InitializeComponents();
//             InitializeAnimationParameters();
//         }
        
//         private void Start()
//         {
//             InitializeNPC();
//             InitializeConvaiIntegration();
            
//             if (enableGazeTracking)
//             {
//                 StartGazeTracking();
//             }
//         }
        
//         private void Update()
//         {
//             UpdateNPCBehavior();
//         }
        
//         private void OnDestroy()
//         {
//             CleanupNPC();
//         }
        
//         #endregion
        
//         #region Initialization
        
//         private void InitializeComponents()
//         {
//             // Initialize animator if not assigned
//             if (npcAnimator == null)
//             {
//                 npcAnimator = GetComponent<Animator>();
//                 if (npcAnimator == null)
//                 {
//                     npcAnimator = GetComponentInChildren<Animator>();
//                 }
//             }
            
//             // Initialize renderer if not assigned
//             if (npcRenderer == null)
//             {
//                 npcRenderer = GetComponentInChildren<SkinnedMeshRenderer>();
//             }
            
//             // Initialize audio sources
//             if (voiceAudioSource == null)
//             {
//                 voiceAudioSource = GetComponent<AudioSource>();
//                 if (voiceAudioSource == null)
//                 {
//                     voiceAudioSource = gameObject.AddComponent<AudioSource>();
//                 }
//             }
            
//             // Initialize TTS manager if not assigned
//             if (ttsManager == null)
//             {
//                 ttsManager = FindObjectOfType<TTSManager>();
//                 if (ttsManager == null)
//                 {
//                     LogDebug("TTSManager not found. Creating one...");
//                     GameObject ttsGO = new GameObject("TTSManager");
//                     ttsManager = ttsGO.AddComponent<TTSManager>();
//                 }
//             }
            
//             // Store original head rotation
//             if (headBone != null)
//             {
//                 originalHeadRotation = headBone.localEulerAngles;
//             }
//         }
        
//         private void InitializeAnimationParameters()
//         {
//             if (npcAnimator != null)
//             {
//                 // Cache animation parameter hashes for performance
//                 stateParameterHash = Animator.StringToHash("ConversationState");
//                 emotionParameterHash = Animator.StringToHash("Emotion");
//                 speakingParameterHash = Animator.StringToHash("IsSpeaking");
//                 gestureParameterHash = Animator.StringToHash("GestureTrigger");
//             }
//         }
        
//         private void InitializeNPC()
//         {
//             currentState.currentState = NPCState.ConversationState.Idle;
//             currentState.isActive = true;
            
//             SetNPCState(NPCState.ConversationState.Idle);
            
//             // Subscribe to TTS events
//             if (ttsManager != null)
//             {
//                 ttsManager.OnTTSStart.AddListener(OnTTSStarted);
//                 ttsManager.OnTTSComplete.AddListener(OnTTSCompleted);
//                 ttsManager.OnTTSError.AddListener(OnTTSError);
//             }
            
//             LogDebug($"NPC '{npcName}' initialized successfully");
//         }
        
//         private void InitializeConvaiIntegration()
//         {
//             if (!useConvaiSDK)
//             {
//                 LogDebug("Convai SDK integration disabled");
//                 return;
//             }
            
//             try
//             {
//                 // TODO: Initialize Convai SDK components
//                 // This is where you would set up the actual Convai integration
//                 /*
//                 convaiConversationManager = FindObjectOfType<ConversationManager>();
//                 if (convaiConversationManager == null)
//                 {
//                     GameObject convaiGO = new GameObject("ConvaiManager");
//                     convaiConversationManager = convaiGO.AddComponent<ConversationManager>();
//                 }
                
//                 // Set up Convai character
//                 convaiCharacter = GetComponent<ConvaiCharacter>();
//                 if (convaiCharacter == null)
//                 {
//                     convaiCharacter = gameObject.AddComponent<ConvaiCharacter>();
//                 }
                
//                 // Configure Convai settings
//                 convaiCharacter.characterID = characterId;
//                 convaiCharacter.apiKey = convaiAPIKey;
                
//                 // Subscribe to Convai events
//                 convaiCharacter.OnCharacterResponse += OnConvaiResponse;
//                 convaiCharacter.OnCharacterEmotion += OnConvaiEmotion;
//                 */
                
//                 LogDebug("Convai SDK integration initialized (placeholder)");
//             }
//             catch (Exception e)
//             {
//                 LogDebug($"Failed to initialize Convai integration: {e.Message}");
//             }
//         }
        
//         #endregion
        
//         #region State Management
        
//         public void SetNPCState(NPCState.ConversationState newState)
//         {
//             if (currentState.currentState == newState) return;
            
//             NPCState.ConversationState previousState = currentState.currentState;
//             currentState.currentState = newState;
//             currentState.stateStartTime = Time.time;
            
//             // Update animator parameters
//             if (npcAnimator != null)
//             {
//                 npcAnimator.SetInteger(stateParameterHash, (int)newState);
//                 npcAnimator.SetBool(speakingParameterHash, newState == NPCState.ConversationState.Speaking);
//             }
            
//             // Trigger state-specific behaviors
//             switch (newState)
//             {
//                 case NPCState.ConversationState.Idle:
//                     OnEnterIdleState();
//                     break;
//                 case NPCState.ConversationState.Listening:
//                     OnEnterListeningState();
//                     break;
//                 case NPCState.ConversationState.Thinking:
//                     OnEnterThinkingState();
//                     break;
//                 case NPCState.ConversationState.Speaking:
//                     OnEnterSpeakingState();
//                     break;
//                 case NPCState.ConversationState.Emoting:
//                     OnEnterEmotingState();
//                     break;
//             }
            
//             OnStateChanged?.Invoke(newState);
//             LogDebug($"NPC state changed: {previousState} -> {newState}");
//         }
        
//         private void OnEnterIdleState()
//         {
//             // Play idle animation, neutral expression
//             PlayIdleAnimation();
//         }
        
//         private void OnEnterListeningState()
//         {
//             // Play listening animation, attentive expression
//             PlayListeningAnimation();
            
//             if (enableGazeTracking && lookAtTarget != null)
//             {
//                 LookAtTarget(lookAtTarget.position);
//             }
//         }
        
//         private void OnEnterThinkingState()
//         {
//             // Play thinking animation
//             PlayThinkingAnimation();
//         }
        
//         private void OnEnterSpeakingState()
//         {
//             // Play speaking animation
//             PlaySpeakingAnimation();
//         }
        
//         private void OnEnterEmotingState()
//         {
//             // Play emotion-specific animation
//             PlayEmotionAnimation(currentState.currentEmotion);
//         }
        
//         #endregion
        
//         #region Animation Control
        
//         private void PlayIdleAnimation()
//         {
//             if (npcAnimator != null)
//             {
//                 npcAnimator.Play(idleStateName);
//             }
//         }
        
//         private void PlayListeningAnimation()
//         {
//             if (npcAnimator != null)
//             {
//                 npcAnimator.Play(listeningStateName);
//             }
//         }
        
//         private void PlayThinkingAnimation()
//         {
//             if (npcAnimator != null)
//             {
//                 npcAnimator.Play(thinkingStateName);
//             }
//         }
        
//         private void PlaySpeakingAnimation()
//         {
//             if (npcAnimator != null)
//             {
//                 npcAnimator.Play(speakingStateName);
//             }
//         }
        
//         private void PlayEmotionAnimation(string emotion)
//         {
//             if (!enableEmotions || npcAnimator == null) return;
            
//             EmotionMapping mapping = emotionMappings.Find(em => em.emotionName.Equals(emotion, StringComparison.OrdinalIgnoreCase));
//             if (mapping != null)
//             {
//                 if (mapping.animationClip != null)
//                 {
//                     // Play emotion animation clip
//                     npcAnimator.Play(mapping.animationClip.name);
//                 }
                
//                 if (!string.IsNullOrEmpty(mapping.animatorParameter))
//                 {
//                     npcAnimator.SetFloat(mapping.animatorParameter, mapping.intensity);
//                 }
                
//                 // Update renderer color for emotion feedback
//                 if (npcRenderer != null && mapping.emotionColor != Color.white)
//                 {
//                     StartCoroutine(AnimateEmotionColor(mapping.emotionColor));
//                 }
//             }
//         }
        
//         private IEnumerator AnimateEmotionColor(Color targetColor)
//         {
//             if (npcRenderer == null) yield break;
            
//             Color originalColor = npcRenderer.material.color;
//             float elapsed = 0f;
//             float duration = 1f / emotionTransitionSpeed;
            
//             while (elapsed < duration)
//             {
//                 elapsed += Time.deltaTime;
//                 float t = elapsed / duration;
                
//                 Color currentColor = Color.Lerp(originalColor, targetColor, t);
//                 npcRenderer.material.color = currentColor;
                
//                 yield return null;
//             }
            
//             // Fade back to original color
//             elapsed = 0f;
//             while (elapsed < duration)
//             {
//                 elapsed += Time.deltaTime;
//                 float t = elapsed / duration;
                
//                 Color currentColor = Color.Lerp(targetColor, originalColor, t);
//                 npcRenderer.material.color = currentColor;
                
//                 yield return null;
//             }
            
//             npcRenderer.material.color = originalColor;
//         }
        
//         #endregion
        
//         #region Gesture System
        
//         public void TriggerGesture(string gestureName)
//         {
//             if (!enableGestures || npcAnimator == null) return;
            
//             GestureClip gesture = gestureClips.Find(g => g.gestureName.Equals(gestureName, StringComparison.OrdinalIgnoreCase));
//             if (gesture != null && gesture.clip != null)
//             {
//                 npcAnimator.SetTrigger(gestureParameterHash);
//                 npcAnimator.Play(gesture.clip.name);
                
//                 LogDebug($"Triggered gesture: {gestureName}");
//             }
//         }
        
//         public void TriggerRandomGesture(string text)
//         {
//             if (!enableGestures || gestureClips.Count == 0) return;
            
//             // Check for keyword-triggered gestures
//             foreach (var gesture in gestureClips)
//             {
//                 if (!string.IsNullOrEmpty(gesture.triggerKeyword) && 
//                     text.ToLower().Contains(gesture.triggerKeyword.ToLower()) &&
//                     UnityEngine.Random.value < gesture.probability)
//                 {
//                     TriggerGesture(gesture.gestureName);
//                     return;
//                 }
//             }
            
//             // Random gesture trigger
//             if (UnityEngine.Random.value < 0.2f) // 20% chance for random gesture
//             {
//                 int randomIndex = UnityEngine.Random.Range(0, gestureClips.Count);
//                 TriggerGesture(gestureClips[randomIndex].gestureName);
//             }
//         }
        
//         #endregion
        
//         #region Gaze and Head Tracking
        
//         private void StartGazeTracking()
//         {
//             if (gazeCoroutine != null)
//             {
//                 StopCoroutine(gazeCoroutine);
//             }
            
//             gazeCoroutine = StartCoroutine(GazeTrackingCoroutine());
//         }
        
//         private IEnumerator GazeTrackingCoroutine()
//         {
//             while (currentState.isActive)
//             {
//                 if (lookAtTarget != null && headBone != null)
//                 {
//                     Vector3 targetPosition = lookAtTarget.position;
                    
//                     // Add some randomness to make gaze more natural
//                     Vector3 randomOffset = UnityEngine.Random.insideUnitSphere * gazeRandomness;
//                     targetPosition += randomOffset;
                    
//                     LookAtTarget(targetPosition);
//                 }
                
//                 yield return new WaitForSeconds(1f / gazeSpeed);
//             }
//         }
        
//         private void LookAtTarget(Vector3 targetPosition)
//         {
//             if (headBone == null) return;
            
//             Vector3 direction = (targetPosition - headBone.position).normalized;
//             Quaternion lookRotation = Quaternion.LookRotation(direction);
            
//             // Apply rotation with smoothing
//             headBone.rotation = Quaternion.Slerp(headBone.rotation, lookRotation, gazeSpeed * Time.deltaTime);
            
//             // Update eye bones if available
//             if (eyeBones != null && eyeBones.Length > 0)
//             {
//                 foreach (var eyeBone in eyeBones)
//                 {
//                     if (eyeBone != null)
//                     {
//                         eyeBone.rotation = Quaternion.Slerp(eyeBone.rotation, lookRotation, gazeSpeed * Time.deltaTime);
//                     }
//                 }
//             }
//         }
        
//         #endregion
        
//         #region Lip Sync
        
//         private void StartLipSync(AudioClip audioClip)
//         {
//             if (!enableLipSync || lipSyncBones == null || lipSyncBones.Length == 0)
//                 return;
            
//             if (currentAnimationCoroutine != null)
//             {
//                 StopCoroutine(currentAnimationCoroutine);
//             }
            
//             currentAnimationCoroutine = StartCoroutine(LipSyncCoroutine(audioClip));
//         }
        
//         private IEnumerator LipSyncCoroutine(AudioClip audioClip)
//         {
//             if (audioClip == null) yield break;
            
//             float clipLength = audioClip.length;
//             float elapsed = 0f;
            
//             while (elapsed < clipLength)
//             {
//                 // Simple lip sync based on audio amplitude
//                 // In a real implementation, you'd use phoneme detection
//                 float amplitude = GetAudioAmplitude() * lipSyncIntensity;
                
//                 foreach (var bone in lipSyncBones)
//                 {
//                     if (bone != null)
//                     {
//                         Vector3 scale = Vector3.one + Vector3.up * amplitude * 0.1f;
//                         bone.localScale = Vector3.Lerp(bone.localScale, scale, Time.deltaTime * 10f);
//                     }
//                 }
                
//                 elapsed += Time.deltaTime;
//                 yield return null;
//             }
            
//             // Reset lip sync bones
//             foreach (var bone in lipSyncBones)
//             {
//                 if (bone != null)
//                 {
//                     bone.localScale = Vector3.one;
//                 }
//             }
//         }
        
//         private float GetAudioAmplitude()
//         {
//             // Simple amplitude detection - in real implementation use proper audio analysis
//             return voiceAudioSource.isPlaying ? UnityEngine.Random.Range(0.1f, 0.8f) : 0f;
//         }
        
//         #endregion
        
//         #region Conversation Handling
        
//         public void StartConversation(ConversationData conversationData)
//         {
//             currentConversation = conversationData;
//             SetNPCState(NPCState.ConversationState.Idle);
            
//             LogDebug($"Started conversation with {npcName}");
//         }
        
//         public void ProcessUserInput(string userInput)
//         {
//             if (string.IsNullOrEmpty(userInput)) return;
            
//             SetNPCState(NPCState.ConversationState.Thinking);
            
//             // Add user input to conversation data
//             if (currentConversation != null)
//             {
//                 DialogueTurn userTurn = new DialogueTurn(DialogueTurn.SpeakerType.User, userInput, Time.time);
//                 currentConversation.dialogueTurns.Add(userTurn);
//                 currentConversation.totalUserTurns++;
//             }
            
//             // Process with Convai or generate placeholder response
//             if (useConvaiSDK && !string.IsNullOrEmpty(characterId))
//             {
//                 ProcessWithConvai(userInput);
//             }
//             else
//             {
//                 GeneratePlaceholderResponse(userInput);
//             }
//         }
        
//         private void ProcessWithConvai(string userInput)
//         {
//             try
//             {
//                 // TODO: Process with actual Convai SDK
//                 /*
//                 ConvaiRequest request = new ConvaiRequest
//                 {
//                     userText = userInput,
//                     characterId = characterId,
//                     sessionId = currentConversation?.sessionId
//                 };
                
//                 convaiConversationManager.SendRequest(request, OnConvaiResponse);
//                 */
                
//                 // Placeholder implementation
//                 StartCoroutine(SimulateConvaiResponse(userInput));
//             }
//             catch (Exception e)
//             {
//                 LogDebug($"Failed to process with Convai: {e.Message}");
//                 GeneratePlaceholderResponse(userInput);
//             }
//         }
        
//         private IEnumerator SimulateConvaiResponse(string userInput)
//         {
//             // Simulate Convai processing time
//             yield return new WaitForSeconds(UnityEngine.Random.Range(1f, 3f));
            
//             // Generate placeholder response
//             string[] responses = {
//                 "That's interesting! Tell me more about that.",
//                 "I understand what you're saying.",
//                 "How does that make you feel?",
//                 "That's a great point. What else would you like to discuss?",
//                 "I appreciate you sharing that with me."
//             };
            
//             string response = responses[UnityEngine.Random.Range(0, responses.Length)];
//             ProcessNPCResponse(response, "curious", 0.7f);
//         }
        
//         private void GeneratePlaceholderResponse(string userInput)
//         {
//             // Simple rule-based response generation
//             string response = "I hear what you're saying about " + userInput.Split(' ')[0] + ". That's very interesting.";
//             ProcessNPCResponse(response, "neutral", 0.8f);
//         }
        
//         public void ProcessNPCResponse(string responseText, string emotion = "neutral", float confidence = 1f)
//         {
//             if (string.IsNullOrEmpty(responseText)) return;
            
//             // Add NPC response to conversation data
//             if (currentConversation != null)
//             {
//                 DialogueTurn npcTurn = new DialogueTurn(DialogueTurn.SpeakerType.NPC, responseText, Time.time);
//                 npcTurn.emotion = emotion;
//                 npcTurn.confidenceScore = confidence;
//                 currentConversation.dialogueTurns.Add(npcTurn);
//                 currentConversation.totalNPCTurns++;
//             }
            
//             // Set emotion
//             SetEmotion(emotion, 0.8f);
            
//             // Generate TTS audio
//             if (ttsManager != null)
//             {
//                 ttsManager.ConvertTextToSpeech(responseText, OnTTSResponse);
//             }
            
//             // Trigger random gesture
//             TriggerRandomGesture(responseText);
            
//             OnNPCResponse?.Invoke(responseText);
//             currentState.lastResponse = responseText;
            
//             LogDebug($"NPC response: '{responseText}' (Emotion: {emotion})");
//         }
        
//         #endregion
        
//         #region Emotion System
        
//         public void SetEmotion(string emotion, float intensity = 1f)
//         {
//             if (!enableEmotions) return;
            
//             currentState.currentEmotion = emotion;
//             currentState.emotionIntensity = intensity;
            
//             // Update emotion in animator
//             if (npcAnimator != null)
//             {
//                 npcAnimator.SetFloat(emotionParameterHash, intensity);
//             }
            
//             // Play emotion animation
//             PlayEmotionAnimation(emotion);
            
//             OnEmotionChanged?.Invoke(emotion, intensity);
            
//             LogDebug($"NPC emotion set to: {emotion} (Intensity: {intensity:F2})");
//         }
        
//         #endregion
        
//         #region TTS Event Handlers
        
//         private void OnTTSStarted(string text)
//         {
//             SetNPCState(NPCState.ConversationState.Speaking);
//         }
        
//         private void OnTTSCompleted(TTSResult result)
//         {
//             if (result.isSuccess)
//             {
//                 PlayTTSAudio(result.audioClip);
//             }
//             else
//             {
//                 LogDebug($"TTS failed: {result.errorMessage}");
//                 SetNPCState(NPCState.ConversationState.Idle);
//             }
//         }
        
//         private void OnTTSError(string error)
//         {
//             LogDebug($"TTS error: {error}");
//             SetNPCState(NPCState.ConversationState.Idle);
//         }
        
//         private void OnTTSResponse(TTSResult result)
//         {
//             OnTTSCompleted(result);
//         }
        
//         private void PlayTTSAudio(AudioClip audioClip)
//         {
//             if (audioClip == null || voiceAudioSource == null) return;
            
//             voiceAudioSource.clip = audioClip;
//             voiceAudioSource.Play();
            
//             // Start lip sync
//             StartLipSync(audioClip);
            
//             // Wait for audio to finish, then return to idle
//             StartCoroutine(WaitForAudioComplete(audioClip.length));
//         }
        
//         private IEnumerator WaitForAudioComplete(float duration)
//         {
//             yield return new WaitForSeconds(duration);
//             SetNPCState(NPCState.ConversationState.Idle);
//         }
        
//         #endregion
        
//         #region Update Loop
        
//         private void UpdateNPCBehavior()
//         {
//             // Update state-specific behaviors
//             switch (currentState.currentState)
//             {
//                 case NPCState.ConversationState.Listening:
//                     UpdateListeningBehavior();
//                     break;
//                 case NPCState.ConversationState.Speaking:
//                     UpdateSpeakingBehavior();
//                     break;
//             }
//         }
        
//         private void UpdateListeningBehavior()
//         {
//             // Subtle head movements, eye tracking
//             if (enableGazeTracking && lookAtTarget != null)
//             {
//                 // Small random head movements to appear more alive
//                 Vector3 randomMovement = UnityEngine.Random.insideUnitSphere * 0.01f;
//                 currentGazeTarget = Vector3.Lerp(currentGazeTarget, lookAtTarget.position + randomMovement, Time.deltaTime);
//             }
//         }
        
//         private void UpdateSpeakingBehavior()
//         {
//             // Update lip sync and gestures while speaking
//             if (!voiceAudioSource.isPlaying && currentState.currentState == NPCState.ConversationState.Speaking)
//             {
//                 SetNPCState(NPCState.ConversationState.Idle);
//             }
//         }
        
//         #endregion
        
//         #region Cleanup
        
//         private void CleanupNPC()
//         {
//             if (gazeCoroutine != null)
//             {
//                 StopCoroutine(gazeCoroutine);
//             }
            
//             if (currentAnimationCoroutine != null)
//             {
//                 StopCoroutine(currentAnimationCoroutine);
//             }
            
//             // Unsubscribe from TTS events
//             if (ttsManager != null)
//             {
//                 ttsManager.OnTTSStart.RemoveListener(OnTTSStarted);
//                 ttsManager.OnTTSComplete.RemoveListener(OnTTSCompleted);
//                 ttsManager.OnTTSError.RemoveListener(OnTTSError);
//             }
//         }
        
//         #endregion
        
//         #region Utility Methods
        
//         private void LogDebug(string message)
//         {
//             if (enableDebugLogs)
//             {
//                 Debug.Log($"[NPCController] {message}");
//             }
//         }
        
//         #endregion
        
//         #region Public API
        
//         public NPCState GetCurrentState()
//         {
//             return currentState;
//         }
        
//         public void SetLookAtTarget(Transform target)
//         {
//             lookAtTarget = target;
//         }
        
//         public void SetCharacterId(string id)
//         {
//             characterId = id;
//             LogDebug($"Character ID set to: {id}");
//         }
        
//         public void SetAPIKey(string apiKey)
//         {
//             convaiAPIKey = apiKey;
//             LogDebug("Convai API key updated");
//         }
        
//         public bool IsNPCAvailable()
//         {
//             return currentState.isActive && 
//                    (currentState.currentState == NPCState.ConversationState.Idle || 
//                     currentState.currentState == NPCState.ConversationState.Listening);
//         }
        
//         public void StopCurrentAction()
//         {
//             if (voiceAudioSource.isPlaying)
//             {
//                 voiceAudioSource.Stop();
//             }
            
//             SetNPCState(NPCState.ConversationState.Idle);
//         }
        
//         #endregion
//     }
// }