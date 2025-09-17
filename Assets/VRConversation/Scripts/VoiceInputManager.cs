// using System;
// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;
// using UnityEngine.Events;
// using VRConversation.Utilities;

// namespace VRConversation
// {
//     /// <summary>
//     /// Manages voice input capture and speech-to-text conversion for VR conversation system
//     /// Supports Unity Microphone API and Oculus Voice SDK integration
//     /// Now integrates with VoiceCaptureUtility for automatic voice detection
//     /// </summary>
//     public class VoiceInputManager : MonoBehaviour
//     {
//         [Header("Voice Input Settings")]
//         [SerializeField] private bool useOculusVoice = true;
//         [SerializeField] private bool useContinuousListening = true;
//         [SerializeField] private bool useVoiceCaptureUtility = true;
//         [SerializeField] private VoiceCaptureUtility voiceCaptureUtility;
//         [SerializeField] private float silenceThreshold = 0.01f;
//         [SerializeField] private float maxRecordingTime = 10f;
//         [SerializeField] private float silenceTimeout = 2f;
        
//         [Header("Audio Configuration")]
//         [SerializeField] private int sampleRate = 44100;
//         [SerializeField] private int maxAudioLength = 300; // seconds
//         [SerializeField] private AudioSource audioSource;
        
//         [Header("Voice Recognition")]
//         [SerializeField] private List<string> keywordsList = new List<string>();
//         [SerializeField] private float confidenceThreshold = 0.7f;
        
//         [Header("Debug Settings")]
//         [SerializeField] private bool enableDebugLogs = true;
//         [SerializeField] private bool showVoiceVisualization = false;
        
//         // Events
//         [System.Serializable]
//         public class VoiceRecognitionEvent : UnityEvent<VoiceRecognitionResult> { }
//         [System.Serializable]
//         public class VoiceInputStartEvent : UnityEvent { }
//         [System.Serializable]
//         public class VoiceInputEndEvent : UnityEvent { }
        
//         public VoiceRecognitionEvent OnVoiceRecognized;
//         public VoiceInputStartEvent OnVoiceInputStart;
//         public VoiceInputEndEvent OnVoiceInputEnd;
        
//         // Private variables
//         private AudioClip microphoneClip;
//         private bool isRecording = false;
//         private bool isListening = false;
//         private float recordingStartTime;
//         private float lastSoundTime;
//         private string microphoneDevice;
        
//         // Voice recognition components (these would be actual SDK references)
//         private object oculusVoiceService; // Placeholder for Oculus Voice SDK
//         private object speechRecognizer; // Placeholder for Windows Speech Recognition
        
//         #region Unity Lifecycle
        
//         private void Awake()
//         {
//             // Initialize audio source if not assigned
//             if (audioSource == null)
//             {
//                 audioSource = GetComponent<AudioSource>();
//                 if (audioSource == null)
//                 {
//                     audioSource = gameObject.AddComponent<AudioSource>();
//                 }
//             }
            
//             // Configure audio source for voice input
//             audioSource.playOnAwake = false;
//             audioSource.spatialBlend = 0f; // 2D audio for voice input
//         }
        
//         private void Start()
//         {
//             InitializeVoiceInput();
            
//             if (useContinuousListening)
//             {
//                 StartListening();
//             }
//         }
        
//         private void Update()
//         {
//             if (isRecording)
//             {
//                 ProcessVoiceInput();
//             }
//         }
        
//         private void OnDestroy()
//         {
//             StopListening();
//             CleanupVoiceInput();
//         }
        
//         #endregion
        
//         #region Voice Input Initialization
        
//         private void InitializeVoiceInput()
//         {
//             try
//             {
//                 if (useVoiceCaptureUtility)
//                 {
//                     InitializeVoiceCaptureUtility();
//                 }
//                 else
//                 {
//                     InitializeLegacyVoiceInput();
//                 }
                
//                 LogDebug("Voice input manager initialized successfully");
//             }
//             catch (Exception e)
//             {
//                 LogDebug($"Failed to initialize voice input: {e.Message}");
//             }
//         }
        
//         private void InitializeVoiceCaptureUtility()
//         {
//             // Find or create VoiceCaptureUtility
//             if (voiceCaptureUtility == null)
//             {
//                 voiceCaptureUtility = GetComponent<VoiceCaptureUtility>();
//                 if (voiceCaptureUtility == null)
//                 {
//                     voiceCaptureUtility = FindObjectOfType<VoiceCaptureUtility>();
//                     if (voiceCaptureUtility == null)
//                     {
//                         GameObject utilityGO = new GameObject("VoiceCaptureUtility");
//                         utilityGO.transform.SetParent(transform);
//                         voiceCaptureUtility = utilityGO.AddComponent<VoiceCaptureUtility>();
//                     }
//                 }
//             }
            
//             // Subscribe to voice capture events
//             voiceCaptureUtility.OnVoiceClipReady.AddListener(OnVoiceClipReady);
//             voiceCaptureUtility.OnVoiceActivityChanged.AddListener(OnVoiceActivityChanged);
//             voiceCaptureUtility.OnRecordingStarted.AddListener(OnVoiceCaptureStarted);
//             voiceCaptureUtility.OnRecordingStopped.AddListener(OnVoiceCaptureStopped);
            
//             // Set up callback delegates for direct code access
//             voiceCaptureUtility.VoiceClipReadyCallback = OnVoiceClipReadyCallback;
//             voiceCaptureUtility.VoiceActivityChangedCallback = OnVoiceActivityChangedCallback;
            
//             LogDebug("VoiceCaptureUtility integration initialized");
//         }
        
//         private void InitializeLegacyVoiceInput()
//         {
//             // Get available microphone devices
//             if (Microphone.devices.Length > 0)
//             {
//                 microphoneDevice = Microphone.devices[0];
//                 LogDebug($"Using microphone device: {microphoneDevice}");
//             }
//             else
//             {
//                 LogDebug("No microphone devices found!");
//                 return;
//             }
            
//             // Initialize voice recognition services
//             if (useOculusVoice)
//             {
//                 InitializeOculusVoice();
//             }
//             else
//             {
//                 InitializeWindowsSpeechRecognition();
//             }
//         }
        
//         private void InitializeOculusVoice()
//         {
//             // TODO: Initialize Oculus Voice SDK
//             // This would involve setting up the Oculus Voice Service
//             // Example implementation:
//             /*
//             var voiceServiceRequest = new VoiceServiceRequest();
//             voiceServiceRequest.RequestEvents.OnPartialTranscription.AddListener(OnPartialTranscription);
//             voiceServiceRequest.RequestEvents.OnFullTranscription.AddListener(OnFullTranscription);
//             oculusVoiceService = voiceServiceRequest;
//             */
            
//             LogDebug("Oculus Voice SDK initialized (placeholder)");
//         }
        
//         private void InitializeWindowsSpeechRecognition()
//         {
//             // TODO: Initialize Windows Speech Recognition for editor testing
//             // This would involve setting up Windows Speech Platform SDK
//             LogDebug("Windows Speech Recognition initialized (placeholder)");
//         }
        
//         #endregion
        
//         #region Voice Input Control
        
//         public void StartListening()
//         {
//             if (isListening) return;
            
//             try
//             {
//                 if (useVoiceCaptureUtility && voiceCaptureUtility != null)
//                 {
//                     voiceCaptureUtility.StartListening();
//                     isListening = true;
//                     OnVoiceInputStart?.Invoke();
//                     LogDebug("Started listening with VoiceCaptureUtility");
//                 }
//                 else
//                 {
//                     StartLegacyListening();
//                 }
//             }
//             catch (Exception e)
//             {
//                 LogDebug($"Failed to start listening: {e.Message}");
//                 isListening = false;
//             }
//         }
        
//         private void StartLegacyListening()
//         {
//             if (string.IsNullOrEmpty(microphoneDevice))
//             {
//                 LogDebug("No microphone device available");
//                 return;
//             }
            
//             isListening = true;
//             StartRecording();
//             OnVoiceInputStart?.Invoke();
            
//             LogDebug("Started listening for voice input (legacy mode)");
//         }
        
//         public void StopListening()
//         {
//             if (!isListening) return;
            
//             if (useVoiceCaptureUtility && voiceCaptureUtility != null)
//             {
//                 voiceCaptureUtility.StopListening();
//                 LogDebug("Stopped listening with VoiceCaptureUtility");
//             }
//             else
//             {
//                 StopRecording();
//                 LogDebug("Stopped listening for voice input (legacy mode)");
//             }
            
//             isListening = false;
//             OnVoiceInputEnd?.Invoke();
//         }
        
//         private void StartRecording()
//         {
//             if (isRecording) return;
            
//             try
//             {
//                 // Start microphone recording
//                 microphoneClip = Microphone.Start(microphoneDevice, false, maxAudioLength, sampleRate);
                
//                 if (microphoneClip != null)
//                 {
//                     isRecording = true;
//                     recordingStartTime = Time.time;
//                     lastSoundTime = Time.time;
                    
//                     // Set audio source clip for monitoring
//                     audioSource.clip = microphoneClip;
                    
//                     LogDebug("Started microphone recording");
//                 }
//             }
//             catch (Exception e)
//             {
//                 LogDebug($"Failed to start recording: {e.Message}");
//             }
//         }
        
//         private void StopRecording()
//         {
//             if (!isRecording) return;
            
//             try
//             {
//                 Microphone.End(microphoneDevice);
//                 isRecording = false;
                
//                 // Process the recorded audio
//                 if (microphoneClip != null)
//                 {
//                     ProcessRecordedAudio();
//                 }
                
//                 LogDebug("Stopped microphone recording");
//             }
//             catch (Exception e)
//             {
//                 LogDebug($"Failed to stop recording: {e.Message}");
//             }
//         }
        
//         #endregion
        
//         #region Voice Processing
        
//         private void ProcessVoiceInput()
//         {
//             if (microphoneClip == null) return;
            
//             // Check for audio activity
//             float audioLevel = GetAudioLevel();
            
//             if (audioLevel > silenceThreshold)
//             {
//                 lastSoundTime = Time.time;
//             }
            
//             // Check for silence timeout or max recording time
//             float recordingDuration = Time.time - recordingStartTime;
//             float silenceDuration = Time.time - lastSoundTime;
            
//             if (silenceDuration > silenceTimeout || recordingDuration > maxRecordingTime)
//             {
//                 StopRecording();
                
//                 if (useContinuousListening)
//                 {
//                     // Restart listening after a brief pause
//                     StartCoroutine(RestartListeningAfterDelay(0.5f));
//                 }
//             }
//         }
        
//         private float GetAudioLevel()
//         {
//             if (microphoneClip == null) return 0f;
            
//             int microphonePosition = Microphone.GetPosition(microphoneDevice);
//             if (microphonePosition <= 0) return 0f;
            
//             // Get recent audio samples
//             int sampleWindow = Mathf.Min(1024, microphonePosition);
//             float[] samples = new float[sampleWindow];
            
//             int startPosition = microphonePosition - sampleWindow;
//             if (startPosition < 0) startPosition = 0;
            
//             microphoneClip.GetData(samples, startPosition);
            
//             // Calculate RMS (Root Mean Square) for audio level
//             float sum = 0f;
//             for (int i = 0; i < samples.Length; i++)
//             {
//                 sum += samples[i] * samples[i];
//             }
            
//             return Mathf.Sqrt(sum / samples.Length);
//         }
        
//         private void ProcessRecordedAudio()
//         {
//             if (microphoneClip == null) return;
            
//             // Trim silence from the recording
//             AudioClip trimmedClip = TrimSilence(microphoneClip);
            
//             if (trimmedClip != null && trimmedClip.length > 0.1f) // Minimum audio length
//             {
//                 // Convert audio to text
//                 StartCoroutine(ConvertSpeechToText(trimmedClip));
//             }
//         }
        
//         private AudioClip TrimSilence(AudioClip clip)
//         {
//             // Simple silence trimming implementation
//             // In a real implementation, this would be more sophisticated
//             return clip;
//         }
        
//         private IEnumerator ConvertSpeechToText(AudioClip audioClip)
//         {
//             VoiceRecognitionResult result = new VoiceRecognitionResult();
            
//             try
//             {
//                 if (useOculusVoice)
//                 {
//                     // TODO: Use Oculus Voice SDK for speech recognition
//                     yield return StartCoroutine(ProcessWithOculusVoice(audioClip, result));
//                 }
//                 else
//                 {
//                     // TODO: Use alternative speech recognition
//                     yield return StartCoroutine(ProcessWithAlternativeSTT(audioClip, result));
//                 }
                
//                 // Post-process recognition result
//                 if (result.isSuccess)
//                 {
//                     result.keywords = ExtractKeywords(result.recognizedText);
//                     LogDebug($"Voice recognized: '{result.recognizedText}' (Confidence: {result.confidence:F2})");
//                 }
//                 else
//                 {
//                     LogDebug($"Voice recognition failed: {result.errorMessage}");
//                 }
                
//                 // Fire recognition event
//                 OnVoiceRecognized?.Invoke(result);
//             }
//             catch (Exception e)
//             {
//                 result.isSuccess = false;
//                 result.errorMessage = e.Message;
//                 LogDebug($"Speech-to-text conversion failed: {e.Message}");
//                 OnVoiceRecognized?.Invoke(result);
//             }
//         }
        
//         private IEnumerator ProcessWithOculusVoice(AudioClip audioClip, VoiceRecognitionResult result)
//         {
//             // TODO: Implement Oculus Voice SDK processing
//             // Placeholder implementation
//             yield return new WaitForSeconds(1f);
            
//             result.isSuccess = true;
//             result.recognizedText = "Hello, this is a placeholder response.";
//             result.confidence = 0.85f;
//             result.duration = audioClip.length;
//         }
        
//         private IEnumerator ProcessWithAlternativeSTT(AudioClip audioClip, VoiceRecognitionResult result)
//         {
//             // TODO: Implement alternative STT processing (e.g., Azure, Google Cloud)
//             // Placeholder implementation
//             yield return new WaitForSeconds(1f);
            
//             result.isSuccess = true;
//             result.recognizedText = "Alternative STT placeholder response.";
//             result.confidence = 0.75f;
//             result.duration = audioClip.length;
//         }
        
//         private List<string> ExtractKeywords(string text)
//         {
//             List<string> foundKeywords = new List<string>();
            
//             if (string.IsNullOrEmpty(text)) return foundKeywords;
            
//             string lowerText = text.ToLower();
            
//             foreach (string keyword in keywordsList)
//             {
//                 if (lowerText.Contains(keyword.ToLower()))
//                 {
//                     foundKeywords.Add(keyword);
//                 }
//             }
            
//             return foundKeywords;
//         }
        
//         #endregion
        
//         #region VoiceCaptureUtility Integration
        
//         private void OnVoiceClipReady(AudioClip audioClip, float duration)
//         {
//             if (audioClip == null) return;
            
//             LogDebug($"Voice clip ready from utility (duration: {duration:F2}s)");
            
//             // Convert the audio clip to text
//             StartCoroutine(ConvertAudioClipToText(audioClip, duration));
//         }
        
//         private void OnVoiceClipReadyCallback(AudioClip audioClip, float duration)
//         {
//             // Direct callback version
//             OnVoiceClipReady(audioClip, duration);
//         }
        
//         private void OnVoiceActivityChanged(bool isActive)
//         {
//             LogDebug($"Voice activity changed: {(isActive ? "Active" : "Inactive")}");
            
//             // Update internal state tracking
//             if (isActive)
//             {
//                 lastSoundTime = Time.time;
//             }
//         }
        
//         private void OnVoiceActivityChangedCallback(bool isActive)
//         {
//             // Direct callback version
//             OnVoiceActivityChanged(isActive);
//         }
        
//         private void OnVoiceCaptureStarted()
//         {
//             LogDebug("Voice capture utility started recording");
//             recordingStartTime = Time.time;
//         }
        
//         private void OnVoiceCaptureStopped()
//         {
//             LogDebug("Voice capture utility stopped recording");
//         }
        
//         private IEnumerator ConvertAudioClipToText(AudioClip audioClip, float duration)
//         {
//             VoiceRecognitionResult result = new VoiceRecognitionResult();
            
//             try
//             {
//                 if (useOculusVoice)
//                 {
//                     yield return StartCoroutine(ProcessAudioWithOculusVoice(audioClip, result));
//                 }
//                 else
//                 {
//                     yield return StartCoroutine(ProcessAudioWithAlternativeSTT(audioClip, result));
//                 }
                
//                 // Set duration from the actual audio clip
//                 result.duration = duration;
                
//                 // Post-process recognition result
//                 if (result.isSuccess)
//                 {
//                     result.keywords = ExtractKeywords(result.recognizedText);
//                     LogDebug($"Voice recognized: '{result.recognizedText}' (Confidence: {result.confidence:F2})");
//                 }
//                 else
//                 {
//                     LogDebug($"Voice recognition failed: {result.errorMessage}");
//                 }
                
//                 // Fire recognition event
//                 OnVoiceRecognized?.Invoke(result);
//             }
//             catch (Exception e)
//             {
//                 result.isSuccess = false;
//                 result.errorMessage = e.Message;
//                 LogDebug($"Audio-to-text conversion failed: {e.Message}");
//                 OnVoiceRecognized?.Invoke(result);
//             }
//         }
        
//         private IEnumerator ProcessAudioWithOculusVoice(AudioClip audioClip, VoiceRecognitionResult result)
//         {
//             // TODO: Implement Oculus Voice SDK processing for AudioClip
//             // Placeholder implementation
//             yield return new WaitForSeconds(0.5f);
            
//             result.isSuccess = true;
//             result.recognizedText = "Voice recognized from utility capture.";
//             result.confidence = 0.85f;
//         }
        
//         private IEnumerator ProcessAudioWithAlternativeSTT(AudioClip audioClip, VoiceRecognitionResult result)
//         {
//             // TODO: Implement alternative STT processing for AudioClip
//             // Placeholder implementation
//             yield return new WaitForSeconds(0.3f);
            
//             result.isSuccess = true;
//             result.recognizedText = "Alternative STT from utility capture.";
//             result.confidence = 0.75f;
//         }
        
//         #endregion
        
//         #region Utility Methods
        
//         private IEnumerator RestartListeningAfterDelay(float delay)
//         {
//             yield return new WaitForSeconds(delay);
            
//             if (useContinuousListening && !isListening)
//             {
//                 StartListening();
//             }
//         }
        
//         private void CleanupVoiceInput()
//         {
//             if (isRecording)
//             {
//                 Microphone.End(microphoneDevice);
//             }
            
//             if (microphoneClip != null)
//             {
//                 DestroyImmediate(microphoneClip);
//                 microphoneClip = null;
//             }
//         }
        
//         private void LogDebug(string message)
//         {
//             if (enableDebugLogs)
//             {
//                 Debug.Log($"[VoiceInputManager] {message}");
//             }
//         }
        
//         #endregion
        
//         #region Public API
        
//         public bool IsListening => isListening;
//         public bool IsRecording => useVoiceCaptureUtility && voiceCaptureUtility != null ? voiceCaptureUtility.IsRecording : isRecording;
//         public float CurrentAudioLevel => useVoiceCaptureUtility && voiceCaptureUtility != null ? voiceCaptureUtility.CurrentVoiceLevel : GetAudioLevel();
        
//         public bool IsVoiceActive => useVoiceCaptureUtility && voiceCaptureUtility != null ? voiceCaptureUtility.IsVoiceActive : false;
//         public VoiceCaptureUtility.CaptureState CaptureState => useVoiceCaptureUtility && voiceCaptureUtility != null ? voiceCaptureUtility.CurrentState : VoiceCaptureUtility.CaptureState.Idle;
        
//         public void SetContinuousListening(bool enabled)
//         {
//             useContinuousListening = enabled;
            
//             if (!enabled && isListening)
//             {
//                 StopListening();
//             }
//             else if (enabled && !isListening)
//             {
//                 StartListening();
//             }
//         }
        
//         public void AddKeyword(string keyword)
//         {
//             if (!keywordsList.Contains(keyword))
//             {
//                 keywordsList.Add(keyword);
//             }
//         }
        
//         public void RemoveKeyword(string keyword)
//         {
//             keywordsList.Remove(keyword);
//         }
        
//         public void ClearKeywords()
//         {
//             keywordsList.Clear();
//         }
        
//         // VoiceCaptureUtility specific methods
//         public void SetVoiceThreshold(float threshold)
//         {
//             if (useVoiceCaptureUtility && voiceCaptureUtility != null)
//             {
//                 voiceCaptureUtility.SetVoiceThreshold(threshold);
//             }
//             else
//             {
//                 silenceThreshold = threshold;
//             }
//         }
        
//         public void SetSilenceTimeout(float timeout)
//         {
//             if (useVoiceCaptureUtility && voiceCaptureUtility != null)
//             {
//                 voiceCaptureUtility.SetSilenceTimeout(timeout);
//             }
//             else
//             {
//                 silenceTimeout = timeout;
//             }
//         }
        
//         public void ForceStopRecording()
//         {
//             if (useVoiceCaptureUtility && voiceCaptureUtility != null)
//             {
//                 voiceCaptureUtility.ForceStopRecording();
//             }
//             else if (isRecording)
//             {
//                 StopRecording();
//             }
//         }
        
//         public void ResetVoiceCapture()
//         {
//             if (useVoiceCaptureUtility && voiceCaptureUtility != null)
//             {
//                 voiceCaptureUtility.ResetCapture();
//             }
//             else
//             {
//                 StopListening();
//                 CleanupVoiceInput();
//             }
//         }
        
//         public string[] GetAvailableMicrophoneDevices()
//         {
//             if (useVoiceCaptureUtility && voiceCaptureUtility != null)
//             {
//                 return voiceCaptureUtility.GetAvailableMicrophoneDevices();
//             }
//             else
//             {
//                 return Microphone.devices;
//             }
//         }
        
//         public void SetMicrophoneDevice(string deviceName)
//         {
//             if (useVoiceCaptureUtility && voiceCaptureUtility != null)
//             {
//                 voiceCaptureUtility.SetMicrophoneDevice(deviceName);
//             }
//             else
//             {
//                 microphoneDevice = deviceName;
//             }
//         }
        
//         public VoiceCaptureUtility GetVoiceCaptureUtility()
//         {
//             return voiceCaptureUtility;
//         }
        
//         #endregion
//     }
// }