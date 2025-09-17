// using UnityEngine;
// using VRConversation.Utilities;

// namespace VRConversation.Examples
// {
//     /// <summary>
//     /// Example script demonstrating how to use the VoiceCaptureUtility
//     /// Shows both Unity Events and direct callback integration
//     /// </summary>
//     public class VoiceCaptureExample : MonoBehaviour
//     {
//         [Header("Voice Capture Settings")]
//         [SerializeField] private VoiceCaptureUtility voiceCapture;
//         [SerializeField] private AudioSource playbackAudioSource;
//         [SerializeField] private bool enablePlayback = true;
        
//         [Header("UI Feedback")]
//         [SerializeField] private UnityEngine.UI.Text statusText;
//         [SerializeField] private UnityEngine.UI.Slider volumeSlider;
//         [SerializeField] private UnityEngine.UI.Button startButton;
//         [SerializeField] private UnityEngine.UI.Button stopButton;
        
//         [Header("Debug")]
//         [SerializeField] private bool enableDebugLogs = true;
        
//         private void Start()
//         {
//             InitializeVoiceCaptureExample();
//         }
        
//         private void InitializeVoiceCaptureExample()
//         {
//             // Find VoiceCaptureUtility if not assigned
//             if (voiceCapture == null)
//             {
//                 voiceCapture = FindObjectOfType<VoiceCaptureUtility>();
//                 if (voiceCapture == null)
//                 {
//                     // Create one if it doesn't exist
//                     GameObject vcGO = new GameObject("VoiceCaptureUtility");
//                     voiceCapture = vcGO.AddComponent<VoiceCaptureUtility>();
//                 }
//             }
            
//             // Set up Unity Events (if you prefer Unity Events over callbacks)
//             SetupUnityEvents();
            
//             // Set up direct callbacks (if you prefer code-based callbacks)
//             SetupDirectCallbacks();
            
//             // Set up UI
//             SetupUI();
            
//             LogDebug("Voice capture example initialized");
//         }
        
//         private void SetupUnityEvents()
//         {
//             if (voiceCapture == null) return;
            
//             // Subscribe to Unity Events
//             voiceCapture.OnVoiceClipReady.AddListener(OnVoiceClipReceived);
//             voiceCapture.OnVoiceActivityChanged.AddListener(OnVoiceActivityChanged);
//             voiceCapture.OnVoiceVolumeChanged.AddListener(OnVoiceVolumeChanged);
//             voiceCapture.OnRecordingStarted.AddListener(OnRecordingStarted);
//             voiceCapture.OnRecordingStopped.AddListener(OnRecordingStopped);
//             voiceCapture.OnSilenceDetected.AddListener(OnSilenceDetected);
            
//             LogDebug("Unity Events setup complete");
//         }
        
//         private void SetupDirectCallbacks()
//         {
//             if (voiceCapture == null) return;
            
//             // Set up direct callback delegates
//             voiceCapture.VoiceClipReadyCallback += OnVoiceClipReceivedCallback;
//             voiceCapture.VoiceActivityChangedCallback += OnVoiceActivityChangedCallback;
//             voiceCapture.VoiceVolumeChangedCallback += OnVoiceVolumeChangedCallback;
            
//             LogDebug("Direct callbacks setup complete");
//         }
        
//         private void SetupUI()
//         {
//             // Set up UI buttons
//             if (startButton != null)
//             {
//                 startButton.onClick.AddListener(StartVoiceCapture);
//             }
            
//             if (stopButton != null)
//             {
//                 stopButton.onClick.AddListener(StopVoiceCapture);
//             }
            
//             // Initialize UI state
//             UpdateStatusText("Ready to capture voice");
//             UpdateVolumeSlider(0f);
//         }
        
//         #region Unity Event Handlers
        
//         private void OnVoiceClipReceived(AudioClip audioClip, float duration)
//         {
//             LogDebug($"Voice clip received via Unity Event - Duration: {duration:F2}s, Samples: {audioClip.samples}");
            
//             // Play the captured audio if enabled
//             if (enablePlayback && playbackAudioSource != null)
//             {
//                 playbackAudioSource.clip = audioClip;
//                 playbackAudioSource.Play();
//                 LogDebug("Playing back captured audio");
//             }
            
//             UpdateStatusText($"Voice captured! Duration: {duration:F2}s");
            
//             // Here you could process the audio clip further:
//             // - Send to speech-to-text service
//             // - Save to file
//             // - Analyze for keywords
//             // - etc.
//         }
        
//         private void OnVoiceActivityChanged(bool isActive)
//         {
//             LogDebug($"Voice activity changed via Unity Event: {(isActive ? "ACTIVE" : "INACTIVE")}");
            
//             if (isActive)
//             {
//                 UpdateStatusText("Voice detected - Recording...");
//             }
//             else
//             {
//                 UpdateStatusText("Silence detected - Waiting...");
//             }
//         }
        
//         private void OnVoiceVolumeChanged(float volume)
//         {
//             UpdateVolumeSlider(volume);
//         }
        
//         private void OnRecordingStarted()
//         {
//             LogDebug("Recording started via Unity Event");
//             UpdateStatusText("Recording started");
//         }
        
//         private void OnRecordingStopped()
//         {
//             LogDebug("Recording stopped via Unity Event");
//             UpdateStatusText("Recording stopped - Processing...");
//         }
        
//         private void OnSilenceDetected()
//         {
//             LogDebug("Silence detected via Unity Event");
//         }
        
//         #endregion
        
//         #region Direct Callback Handlers
        
//         private void OnVoiceClipReceivedCallback(AudioClip audioClip, float duration)
//         {
//             LogDebug($"Voice clip received via Direct Callback - Duration: {duration:F2}s");
            
//             // This is called in addition to the Unity Event
//             // Use this if you prefer direct code callbacks over Unity Events
            
//             // Example: Automatically process the audio clip
//             ProcessAudioClip(audioClip, duration);
//         }
        
//         private void OnVoiceActivityChangedCallback(bool isActive)
//         {
//             LogDebug($"Voice activity callback: {(isActive ? "ACTIVE" : "INACTIVE")}");
            
//             // Handle voice activity changes directly in code
//             // This is useful for real-time voice activity indicators
//         }
        
//         private void OnVoiceVolumeChangedCallback(float volume)
//         {
//             // Real-time volume updates
//             // Could be used for voice visualization, threshold adjustments, etc.
//         }
        
//         #endregion
        
//         #region Audio Processing
        
//         private void ProcessAudioClip(AudioClip audioClip, float duration)
//         {
//             if (audioClip == null) return;
            
//             // Example processing - analyze the audio clip
//             float[] samples = new float[audioClip.samples * audioClip.channels];
//             audioClip.GetData(samples, 0);
            
//             // Calculate average volume
//             float totalVolume = 0f;
//             for (int i = 0; i < samples.Length; i++)
//             {
//                 totalVolume += Mathf.Abs(samples[i]);
//             }
//             float averageVolume = totalVolume / samples.Length;
            
//             LogDebug($"Audio analysis - Avg Volume: {averageVolume:F4}, Peak Count: {CountPeaks(samples)}");
            
//             // Here you could:
//             // 1. Send to speech-to-text API
//             // 2. Analyze for voice patterns
//             // 3. Extract features for machine learning
//             // 4. Save to persistent storage
//             // 5. Forward to conversation system
//         }
        
//         private int CountPeaks(float[] samples)
//         {
//             int peaks = 0;
//             float threshold = 0.1f;
            
//             for (int i = 1; i < samples.Length - 1; i++)
//             {
//                 if (Mathf.Abs(samples[i]) > threshold &&
//                     Mathf.Abs(samples[i]) > Mathf.Abs(samples[i - 1]) &&
//                     Mathf.Abs(samples[i]) > Mathf.Abs(samples[i + 1]))
//                 {
//                     peaks++;
//                 }
//             }
            
//             return peaks;
//         }
        
//         #endregion
        
//         #region UI Control Methods
        
//         public void StartVoiceCapture()
//         {
//             if (voiceCapture != null)
//             {
//                 voiceCapture.StartListening();
//                 LogDebug("Voice capture started via UI");
//                 UpdateStatusText("Listening for voice...");
//             }
//         }
        
//         public void StopVoiceCapture()
//         {
//             if (voiceCapture != null)
//             {
//                 voiceCapture.StopListening();
//                 LogDebug("Voice capture stopped via UI");
//                 UpdateStatusText("Voice capture stopped");
//             }
//         }
        
//         public void ForceStopRecording()
//         {
//             if (voiceCapture != null)
//             {
//                 voiceCapture.ForceStopRecording();
//                 LogDebug("Recording force stopped via UI");
//             }
//         }
        
//         public void ResetCapture()
//         {
//             if (voiceCapture != null)
//             {
//                 voiceCapture.ResetCapture();
//                 LogDebug("Voice capture reset via UI");
//                 UpdateStatusText("Voice capture reset");
//             }
//         }
        
//         private void UpdateStatusText(string status)
//         {
//             if (statusText != null)
//             {
//                 statusText.text = $"Status: {status}";
//             }
//         }
        
//         private void UpdateVolumeSlider(float volume)
//         {
//             if (volumeSlider != null)
//             {
//                 volumeSlider.value = volume;
//             }
//         }
        
//         #endregion
        
//         #region Voice Capture Configuration
        
//         public void SetVoiceThreshold(float threshold)
//         {
//             if (voiceCapture != null)
//             {
//                 voiceCapture.SetVoiceThreshold(threshold);
//                 LogDebug($"Voice threshold set to: {threshold}");
//             }
//         }
        
//         public void SetSilenceTimeout(float timeout)
//         {
//             if (voiceCapture != null)
//             {
//                 voiceCapture.SetSilenceTimeout(timeout);
//                 LogDebug($"Silence timeout set to: {timeout}s");
//             }
//         }
        
//         public void SetMinimumRecordingTime(float minTime)
//         {
//             if (voiceCapture != null)
//             {
//                 voiceCapture.SetMinimumRecordingTime(minTime);
//                 LogDebug($"Minimum recording time set to: {minTime}s");
//             }
//         }
        
//         #endregion
        
//         #region Debug and Info
        
//         private void Update()
//         {
//             // Update UI with current state information
//             if (voiceCapture != null)
//             {
//                 // Update UI based on current state
//                 string stateInfo = $"State: {voiceCapture.CurrentState}";
//                 if (voiceCapture.IsListening)
//                 {
//                     stateInfo += $" | Volume: {voiceCapture.CurrentVoiceLevel:F3}";
//                     stateInfo += $" | Active: {(voiceCapture.IsVoiceActive ? "YES" : "NO")}";
//                 }
                
//                 // You could update a debug text UI element here
//             }
//         }
        
//         public void PrintVoiceCaptureInfo()
//         {
//             if (voiceCapture == null) return;
            
//             LogDebug("=== Voice Capture Info ===");
//             LogDebug($"Current State: {voiceCapture.CurrentState}");
//             LogDebug($"Is Listening: {voiceCapture.IsListening}");
//             LogDebug($"Is Recording: {voiceCapture.IsRecording}");
//             LogDebug($"Is Processing: {voiceCapture.IsProcessing}");
//             LogDebug($"Current Voice Level: {voiceCapture.CurrentVoiceLevel:F4}");
//             LogDebug($"Is Voice Active: {voiceCapture.IsVoiceActive}");
//             LogDebug($"Microphone Device: {voiceCapture.GetMicrophoneDevice()}");
//             LogDebug("========================");
//         }
        
//         private void LogDebug(string message)
//         {
//             if (enableDebugLogs)
//             {
//                 Debug.Log($"[VoiceCaptureExample] {message}");
//             }
//         }
        
//         #endregion
        
//         #region Cleanup
        
//         private void OnDestroy()
//         {
//             // Clean up event subscriptions
//             if (voiceCapture != null)
//             {
//                 voiceCapture.OnVoiceClipReady.RemoveListener(OnVoiceClipReceived);
//                 voiceCapture.OnVoiceActivityChanged.RemoveListener(OnVoiceActivityChanged);
//                 voiceCapture.OnVoiceVolumeChanged.RemoveListener(OnVoiceVolumeChanged);
//                 voiceCapture.OnRecordingStarted.RemoveListener(OnRecordingStarted);
//                 voiceCapture.OnRecordingStopped.RemoveListener(OnRecordingStopped);
//                 voiceCapture.OnSilenceDetected.RemoveListener(OnSilenceDetected);
                
//                 // Clean up direct callbacks
//                 voiceCapture.VoiceClipReadyCallback -= OnVoiceClipReceivedCallback;
//                 voiceCapture.VoiceActivityChangedCallback -= OnVoiceActivityChangedCallback;
//                 voiceCapture.VoiceVolumeChangedCallback -= OnVoiceVolumeChangedCallback;
//             }
//         }
        
//         #endregion
//     }
// }