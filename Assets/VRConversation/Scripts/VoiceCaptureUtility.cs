
// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;
// using UnityEngine.Events;

// namespace VRConversation.Utilities
// {
//     /// <summary>
//     /// Utility class for automatic voice capture with speech detection
//     /// Automatically starts recording when voice is detected and stops after silence period
//     /// Provides processed audio clips via callbacks
//     /// </summary>
//     public class VoiceCaptureUtility : MonoBehaviour
//     {
//         [Header("Voice Detection Settings")]
//         [SerializeField] private float voiceThreshold = 0.02f;
//         [SerializeField] private float silenceTimeBeforeStop = 2f;
//         [SerializeField] private float minimumRecordingTime = 0.5f;
//         [SerializeField] private float maximumRecordingTime = 30f;
        
//         [Header("Audio Configuration")]
//         [SerializeField] private int sampleRate = 44100;
//         [SerializeField] private int recordingBufferSize = 10; // seconds
//         [SerializeField] private bool enableNoiseReduction = true;
//         [SerializeField] private float noiseGateThreshold = 0.01f;
        
//         [Header("Voice Activity Detection")]
//         [SerializeField] private float vadSensitivity = 0.5f;
//         [SerializeField] private int vadWindowSize = 1024;
//         [SerializeField] private float vadUpdateRate = 20f; // Hz
        
//         [Header("Audio Processing")]
//         [SerializeField] private bool enableAutoGainControl = true;
//         [SerializeField] private bool enableEchoSuppression = false;
//         [SerializeField] private bool trimSilenceFromEnds = true;
        
//         [Header("Debug Settings")]
//         [SerializeField] private bool enableDebugLogs = true;
//         [SerializeField] private bool showVoiceActivityVisual = false;
//         [SerializeField] private AudioSource debugAudioSource;
        
//         // Events and Callbacks
//         [System.Serializable]
//         public class VoiceClipEvent : UnityEvent<AudioClip, float> { } // AudioClip, duration
//         [System.Serializable]
//         public class VoiceActivityEvent : UnityEvent<bool> { } // isActive
//         [System.Serializable]
//         public class VoiceVolumeEvent : UnityEvent<float> { } // volume level
        
//         public VoiceClipEvent OnVoiceClipReady;
//         public VoiceActivityEvent OnVoiceActivityChanged;
//         public VoiceVolumeEvent OnVoiceVolumeChanged;
//         public UnityEvent OnRecordingStarted;
//         public UnityEvent OnRecordingStopped;
//         public UnityEvent OnSilenceDetected;
        
//         // Callback delegates for code-based integration
//         public System.Action<AudioClip, float> VoiceClipReadyCallback;
//         public System.Action<bool> VoiceActivityChangedCallback;
//         public System.Action<float> VoiceVolumeChangedCallback;
        
//         // Private variables
//         private AudioClip recordingClip;
//         private string microphoneDevice;
//         private bool isRecording = false;
//         private bool isVoiceActive = false;
//         private float recordingStartTime;
//         private float lastVoiceTime;
//         private float currentVoiceLevel = 0f;
        
//         // Voice Activity Detection
//         private float[] audioBuffer;
//         private int bufferPosition = 0;
//         private float[] vadWindow;
//         private int vadWindowPosition = 0;
//         private Coroutine vadCoroutine;
        
//         // Audio processing
//         private List<float> processedAudioData = new List<float>();
//         private bool isProcessingAudio = false;
        
//         // State tracking
//         public enum CaptureState
//         {
//             Idle,
//             Listening,
//             Recording,
//             Processing,
//             Ready
//         }
        
//         private CaptureState currentState = CaptureState.Idle;
        
//         #region Unity Lifecycle
        
//         private void Awake()
//         {
//             InitializeVoiceCapture();
//         }
        
//         private void Start()
//         {
//             StartListening();
//         }
        
//         private void Update()
//         {
//             if (isRecording)
//             {
//                 UpdateVoiceDetection();
//             }
//         }
        
//         private void OnDestroy()
//         {
//             StopListening();
//             CleanupVoiceCapture();
//         }
        
//         #endregion
        
//         #region Initialization
        
//         private void InitializeVoiceCapture()
//         {
//             try
//             {
//                 // Get available microphone devices
//                 if (Microphone.devices.Length > 0)
//                 {
//                     microphoneDevice = Microphone.devices[0];
//                     LogDebug($"Using microphone device: {microphoneDevice}");
//                 }
//                 else
//                 {
//                     LogDebug("No microphone devices found!");
//                     return;
//                 }
                
//                 // Initialize audio buffers
//                 audioBuffer = new float[vadWindowSize];
//                 vadWindow = new float[vadWindowSize];
                
//                 // Initialize debug audio source
//                 if (debugAudioSource == null)
//                 {
//                     debugAudioSource = GetComponent<AudioSource>();
//                     if (debugAudioSource == null)
//                     {
//                         debugAudioSource = gameObject.AddComponent<AudioSource>();
//                     }
//                 }
                
//                 if (debugAudioSource != null)
//                 {
//                     debugAudioSource.playOnAwake = false;
//                     debugAudioSource.spatialBlend = 0f; // 2D audio
//                 }
                
//                 SetCaptureState(CaptureState.Idle);
//                 LogDebug("Voice capture utility initialized successfully");
//             }
//             catch (System.Exception e)
//             {
//                 LogDebug($"Failed to initialize voice capture: {e.Message}");
//             }
//         }
        
//         #endregion
        
//         #region Voice Capture Control
        
//         public void StartListening()
//         {
//             if (currentState != CaptureState.Idle && currentState != CaptureState.Ready)
//             {
//                 LogDebug("Already listening or processing");
//                 return;
//             }
            
//             try
//             {
//                 if (string.IsNullOrEmpty(microphoneDevice))
//                 {
//                     LogDebug("No microphone device available");
//                     return;
//                 }
                
//                 SetCaptureState(CaptureState.Listening);
//                 StartMicrophoneCapture();
                
//                 if (vadCoroutine != null)
//                 {
//                     StopCoroutine(vadCoroutine);
//                 }
//                 vadCoroutine = StartCoroutine(VoiceActivityDetectionCoroutine());
                
//                 LogDebug("Started listening for voice activity");
//             }
//             catch (Exception e)
//             {
//                 LogDebug($"Failed to start listening: {e.Message}");
//                 SetCaptureState(CaptureState.Idle);
//             }
//         }
        
//         public void StopListening()
//         {
//             if (currentState == CaptureState.Idle)
//                 return;
            
//             StopMicrophoneCapture();
            
//             if (vadCoroutine != null)
//             {
//                 StopCoroutine(vadCoroutine);
//                 vadCoroutine = null;
//             }
            
//             SetCaptureState(CaptureState.Idle);
//             LogDebug("Stopped listening");
//         }
        
//         private void StartMicrophoneCapture()
//         {
//             if (isRecording)
//                 return;
            
//             try
//             {
//                 // Start continuous microphone recording
//                 recordingClip = Microphone.Start(microphoneDevice, true, recordingBufferSize, sampleRate);
                
//                 if (recordingClip != null)
//                 {
//                     isRecording = true;
//                     LogDebug("Microphone capture started");
//                 }
//             }
//             catch (System.Exception e)
//             {
//                 LogDebug($"Failed to start microphone capture: {e.Message}");
//             }
//         }
        
//         private void StopMicrophoneCapture()
//         {
//             if (!isRecording)
//                 return;
            
//             try
//             {
//                 Microphone.End(microphoneDevice);
//                 isRecording = false;
                
//                 if (recordingClip != null)
//                 {
//                     DestroyImmediate(recordingClip);
//                     recordingClip = null;
//                 }
                
//                 LogDebug("Microphone capture stopped");
//             }
//             catch (Exception e)
//             {
//                 LogDebug($"Failed to stop microphone capture: {e.Message}");
//             }
//         }
        
//         #endregion
        
//         #region Voice Activity Detection
        
//         private IEnumerator VoiceActivityDetectionCoroutine()
//         {
//             float updateInterval = 1f / vadUpdateRate;
            
//             while (isRecording)
//             {
//                 AnalyzeVoiceActivity();
//                 yield return new WaitForSeconds(updateInterval);
//             }
//         }
        
//         private void UpdateVoiceDetection()
//         {
//             if (recordingClip == null)
//                 return;
            
//             // Get current microphone position
//             int micPosition = Microphone.GetPosition(microphoneDevice);
//             if (micPosition <= 0)
//                 return;
            
//             // Calculate how many new samples we have
//             int samplesToRead = 0;
//             if (micPosition > bufferPosition)
//             {
//                 samplesToRead = micPosition - bufferPosition;
//             }
//             else if (micPosition < bufferPosition)
//             {
//                 // Wrapped around
//                 samplesToRead = (recordingClip.samples - bufferPosition) + micPosition;
//             }
            
//             if (samplesToRead > 0)
//             {
//                 // Read new audio data
//                 ReadNewAudioData(samplesToRead);
//                 bufferPosition = micPosition;
//             }
//         }
        
//         private void ReadNewAudioData(int samplesToRead)
//         {
//             // Limit samples to read to avoid buffer overflow
//             samplesToRead = Mathf.Min(samplesToRead, vadWindowSize);
            
//             float[] newSamples = new float[samplesToRead];
//             int startPosition = bufferPosition;
            
//             if (startPosition + samplesToRead <= recordingClip.samples)
//             {
//                 recordingClip.GetData(newSamples, startPosition);
//             }
//             else
//             {
//                 // Handle wrap-around
//                 int firstPart = recordingClip.samples - startPosition;
//                 int secondPart = samplesToRead - firstPart;
                
//                 float[] firstSamples = new float[firstPart];
//                 float[] secondSamples = new float[secondPart];
                
//                 recordingClip.GetData(firstSamples, startPosition);
//                 recordingClip.GetData(secondSamples, 0);
                
//                 // Combine the samples
//                 System.Array.Copy(firstSamples, 0, newSamples, 0, firstPart);
//                 System.Array.Copy(secondSamples, 0, newSamples, firstPart, secondPart);
//             }
            
//             // Add to VAD window
//             for (int i = 0; i < newSamples.Length; i++)
//             {
//                 vadWindow[vadWindowPosition] = newSamples[i];
//                 vadWindowPosition = (vadWindowPosition + 1) % vadWindowSize;
//             }
            
//             // If we're actively recording, add to processed audio data
//             if (currentState == CaptureState.Recording)
//             {
//                 processedAudioData.AddRange(newSamples);
//             }
//         }
        
//         private void AnalyzeVoiceActivity()
//         {
//             if (vadWindow == null || vadWindow.Length == 0)
//                 return;
            
//             // Calculate RMS (Root Mean Square) for volume level
//             float rms = CalculateRMS(vadWindow);
//             currentVoiceLevel = rms;
            
//             // Apply noise gate
//             if (rms < noiseGateThreshold)
//             {
//                 rms = 0f;
//             }
            
//             // Determine if voice is active
//             bool wasVoiceActive = isVoiceActive;
//             isVoiceActive = rms > voiceThreshold;
            
//             // Fire volume event
//             OnVoiceVolumeChanged?.Invoke(currentVoiceLevel);
//             VoiceVolumeChangedCallback?.Invoke(currentVoiceLevel);
            
//             // Handle voice activity state changes
//             if (isVoiceActive && !wasVoiceActive)
//             {
//                 OnVoiceActivityDetected();
//             }
//             else if (!isVoiceActive && wasVoiceActive)
//             {
//                 OnVoiceActivityStopped();
//             }
            
//             // Update last voice time
//             if (isVoiceActive)
//             {
//                 lastVoiceTime = Time.time;
//             }
            
//             // Check for silence timeout during recording
//             if (currentState == CaptureState.Recording)
//             {
//                 float silenceDuration = Time.time - lastVoiceTime;
//                 float recordingDuration = Time.time - recordingStartTime;
                
//                 if (silenceDuration > silenceTimeBeforeStop && recordingDuration > minimumRecordingTime)
//                 {
//                     StopRecordingAndProcess();
//                 }
//                 else if (recordingDuration > maximumRecordingTime)
//                 {
//                     StopRecordingAndProcess();
//                 }
//             }
//         }
        
//         private float CalculateRMS(float[] samples)
//         {
//             float sum = 0f;
//             for (int i = 0; i < samples.Length; i++)
//             {
//                 sum += samples[i] * samples[i];
//             }
//             return Mathf.Sqrt(sum / samples.Length);
//         }
        
//         #endregion
        
//         #region Recording Management
        
//         private void OnVoiceActivityDetected()
//         {
//             if (currentState == CaptureState.Listening)
//             {
//                 StartRecording();
//             }
            
//             OnVoiceActivityChanged?.Invoke(true);
//             VoiceActivityChangedCallback?.Invoke(true);
//         }
        
//         private void OnVoiceActivityStopped()
//         {
//             OnSilenceDetected?.Invoke();
//             OnVoiceActivityChanged?.Invoke(false);
//             VoiceActivityChangedCallback?.Invoke(false);
//         }
        
//         private void StartRecording()
//         {
//             if (currentState != CaptureState.Listening)
//                 return;
            
//             SetCaptureState(CaptureState.Recording);
//             recordingStartTime = Time.time;
//             lastVoiceTime = Time.time;
            
//             // Clear previous audio data
//             processedAudioData.Clear();
            
//             OnRecordingStarted?.Invoke();
//             LogDebug("Started recording voice");
//         }
        
//         private void StopRecordingAndProcess()
//         {
//             if (currentState != CaptureState.Recording)
//                 return;
            
//             SetCaptureState(CaptureState.Processing);
//             OnRecordingStopped?.Invoke();
            
//             float recordingDuration = Time.time - recordingStartTime;
//             LogDebug($"Stopped recording voice (duration: {recordingDuration:F2}s)");
            
//             // Process the recorded audio
//             StartCoroutine(ProcessRecordedAudio());
//         }
        
//         private IEnumerator ProcessRecordedAudio()
//         {
//             isProcessingAudio = true;
            
//             try
//             {
//                 // Create audio clip from processed data
//                 if (processedAudioData.Count > 0)
//                 {
//                     AudioClip processedClip = CreateAudioClipFromData(processedAudioData.ToArray());
                    
//                     if (processedClip != null)
//                     {
//                         // Apply audio processing
//                         AudioClip finalClip = yield return StartCoroutine(ApplyAudioProcessing(processedClip));
                        
//                         // Calculate actual duration
//                         float duration = finalClip.length;
                        
//                         // Fire callbacks and events
//                         OnVoiceClipReady?.Invoke(finalClip, duration);
//                         VoiceClipReadyCallback?.Invoke(finalClip, duration);
                        
//                         LogDebug($"Voice clip ready (duration: {duration:F2}s, samples: {finalClip.samples})");
                        
//                         // Play debug audio if enabled
//                         if (showVoiceActivityVisual && debugAudioSource != null)
//                         {
//                             debugAudioSource.clip = finalClip;
//                             debugAudioSource.Play();
//                         }
//                     }
//                     else
//                     {
//                         LogDebug("Failed to create audio clip from recorded data");
//                     }
//                 }
//                 else
//                 {
//                     LogDebug("No audio data recorded");
//                 }
//             }
//             catch (Exception e)
//             {
//                 LogDebug($"Error processing recorded audio: {e.Message}");
//             }
//             finally
//             {
//                 isProcessingAudio = false;
//                 SetCaptureState(CaptureState.Listening); // Return to listening state
//             }
//         }
        
//         private AudioClip CreateAudioClipFromData(float[] audioData)
//         {
//             if (audioData == null || audioData.Length == 0)
//                 return null;
            
//             try
//             {
//                 // Create audio clip
//                 AudioClip clip = AudioClip.Create("RecordedVoice", audioData.Length, 1, sampleRate, false);
//                 clip.SetData(audioData, 0);
//                 return clip;
//             }
//             catch (Exception e)
//             {
//                 LogDebug($"Failed to create audio clip: {e.Message}");
//                 return null;
//             }
//         }
        
//         private IEnumerator ApplyAudioProcessing(AudioClip inputClip)
//         {
//             if (inputClip == null)
//             {
//                 yield return null;
//                 yield break;
//             }
            
//             AudioClip processedClip = inputClip;
            
//             try
//             {
//                 // Apply noise reduction
//                 if (enableNoiseReduction)
//                 {
//                     processedClip = ApplyNoiseReduction(processedClip);
//                     yield return null; // Allow frame to complete
//                 }
                
//                 // Apply auto gain control
//                 if (enableAutoGainControl)
//                 {
//                     processedClip = ApplyAutoGainControl(processedClip);
//                     yield return null;
//                 }
                
//                 // Trim silence from ends
//                 if (trimSilenceFromEnds)
//                 {
//                     processedClip = TrimSilence(processedClip);
//                     yield return null;
//                 }
                
//                 yield return processedClip;
//             }
//             catch (Exception e)
//             {
//                 LogDebug($"Error in audio processing: {e.Message}");
//                 yield return inputClip; // Return original clip if processing fails
//             }
//         }
        
//         private AudioClip ApplyNoiseReduction(AudioClip clip)
//         {
//             // Simple noise reduction implementation
//             // In a production environment, you'd use more sophisticated algorithms
            
//             float[] samples = new float[clip.samples];
//             clip.GetData(samples, 0);
            
//             // Apply simple noise gate
//             for (int i = 0; i < samples.Length; i++)
//             {
//                 if (Mathf.Abs(samples[i]) < noiseGateThreshold)
//                 {
//                     samples[i] = 0f;
//                 }
//             }
            
//             AudioClip processedClip = AudioClip.Create(clip.name + "_NoiseReduced", samples.Length, clip.channels, clip.frequency, false);
//             processedClip.SetData(samples, 0);
            
//             return processedClip;
//         }
        
//         private AudioClip ApplyAutoGainControl(AudioClip clip)
//         {
//             float[] samples = new float[clip.samples];
//             clip.GetData(samples, 0);
            
//             // Find peak amplitude
//             float peak = 0f;
//             for (int i = 0; i < samples.Length; i++)
//             {
//                 float abs = Mathf.Abs(samples[i]);
//                 if (abs > peak)
//                     peak = abs;
//             }
            
//             // Apply gain to normalize to target level (0.7)
//             if (peak > 0f)
//             {
//                 float gain = 0.7f / peak;
//                 gain = Mathf.Clamp(gain, 0.1f, 10f); // Limit gain range
                
//                 for (int i = 0; i < samples.Length; i++)
//                 {
//                     samples[i] *= gain;
//                     samples[i] = Mathf.Clamp(samples[i], -1f, 1f); // Prevent clipping
//                 }
//             }
            
//             AudioClip processedClip = AudioClip.Create(clip.name + "_AGC", samples.Length, clip.channels, clip.frequency, false);
//             processedClip.SetData(samples, 0);
            
//             return processedClip;
//         }
        
//         private AudioClip TrimSilence(AudioClip clip)
//         {
//             float[] samples = new float[clip.samples];
//             clip.GetData(samples, 0);
            
//             // Find start and end of actual audio content
//             int startIndex = 0;
//             int endIndex = samples.Length - 1;
            
//             // Find start
//             for (int i = 0; i < samples.Length; i++)
//             {
//                 if (Mathf.Abs(samples[i]) > voiceThreshold * 0.1f)
//                 {
//                     startIndex = Mathf.Max(0, i - (int)(sampleRate * 0.1f)); // Keep 100ms before voice
//                     break;
//                 }
//             }
            
//             // Find end
//             for (int i = samples.Length - 1; i >= 0; i--)
//             {
//                 if (Mathf.Abs(samples[i]) > voiceThreshold * 0.1f)
//                 {
//                     endIndex = Mathf.Min(samples.Length - 1, i + (int)(sampleRate * 0.1f)); // Keep 100ms after voice
//                     break;
//                 }
//             }
            
//             // Create trimmed clip
//             if (endIndex > startIndex)
//             {
//                 int trimmedLength = endIndex - startIndex + 1;
//                 float[] trimmedSamples = new float[trimmedLength];
//                 Array.Copy(samples, startIndex, trimmedSamples, 0, trimmedLength);
                
//                 AudioClip trimmedClip = AudioClip.Create(clip.name + "_Trimmed", trimmedLength, clip.channels, clip.frequency, false);
//                 trimmedClip.SetData(trimmedSamples, 0);
                
//                 return trimmedClip;
//             }
            
//             return clip; // Return original if trimming failed
//         }
        
//         #endregion
        
//         #region State Management
        
//         private void SetCaptureState(CaptureState newState)
//         {
//             if (currentState == newState)
//                 return;
            
//             CaptureState previousState = currentState;
//             currentState = newState;
            
//             LogDebug($"Capture state changed: {previousState} -> {newState}");
//         }
        
//         #endregion
        
//         #region Utility Methods
        
//         private void CleanupVoiceCapture()
//         {
//             StopListening();
            
//             if (recordingClip != null)
//             {
//                 DestroyImmediate(recordingClip);
//                 recordingClip = null;
//             }
            
//             processedAudioData?.Clear();
//         }
        
//         private void LogDebug(string message)
//         {
//             if (enableDebugLogs)
//             {
//                 Debug.Log($"[VoiceCaptureUtility] {message}");
//             }
//         }
        
//         #endregion
        
//         #region Public API
        
//         public CaptureState CurrentState => currentState;
//         public bool IsListening => currentState == CaptureState.Listening || currentState == CaptureState.Recording;
//         public bool IsRecording => currentState == CaptureState.Recording;
//         public bool IsProcessing => currentState == CaptureState.Processing;
//         public float CurrentVoiceLevel => currentVoiceLevel;
//         public bool IsVoiceActive => isVoiceActive;
        
//         public void SetVoiceThreshold(float threshold)
//         {
//             voiceThreshold = Mathf.Clamp(threshold, 0.001f, 1f);
//             LogDebug($"Voice threshold set to: {voiceThreshold}");
//         }
        
//         public void SetSilenceTimeout(float timeout)
//         {
//             silenceTimeBeforeStop = Mathf.Clamp(timeout, 0.5f, 10f);
//             LogDebug($"Silence timeout set to: {silenceTimeBeforeStop}s");
//         }
        
//         public void SetMinimumRecordingTime(float minTime)
//         {
//             minimumRecordingTime = Mathf.Clamp(minTime, 0.1f, 5f);
//             LogDebug($"Minimum recording time set to: {minimumRecordingTime}s");
//         }
        
//         public void SetMaximumRecordingTime(float maxTime)
//         {
//             maximumRecordingTime = Mathf.Clamp(maxTime, 5f, 60f);
//             LogDebug($"Maximum recording time set to: {maximumRecordingTime}s");
//         }
        
//         public void ForceStopRecording()
//         {
//             if (currentState == CaptureState.Recording)
//             {
//                 StopRecordingAndProcess();
//                 LogDebug("Recording force stopped");
//             }
//         }
        
//         public void ResetCapture()
//         {
//             StopListening();
//             processedAudioData.Clear();
//             SetCaptureState(CaptureState.Idle);
//             LogDebug("Voice capture reset");
//         }
        
//         public string GetMicrophoneDevice()
//         {
//             return microphoneDevice;
//         }
        
//         public void SetMicrophoneDevice(string deviceName)
//         {
//             if (Microphone.devices.Length > 0)
//             {
//                 foreach (string device in Microphone.devices)
//                 {
//                     if (device.Equals(deviceName, StringComparison.OrdinalIgnoreCase))
//                     {
//                         bool wasListening = IsListening;
//                         StopListening();
                        
//                         microphoneDevice = device;
//                         LogDebug($"Microphone device changed to: {device}");
                        
//                         if (wasListening)
//                         {
//                             StartListening();
//                         }
//                         return;
//                     }
//                 }
//                 LogDebug($"Microphone device '{deviceName}' not found");
//             }
//         }
        
//         public string[] GetAvailableMicrophoneDevices()
//         {
//             return Microphone.devices;
//         }
        
//         #endregion
//     }
// }