// using System;
// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;
// using UnityEngine.Events;

// namespace VRConversation
// {
//     /// <summary>
//     /// Manages Text-to-Speech conversion and audio playback for NPC responses
//     /// Supports Oculus Voice SDK, Azure Cognitive Services, and other TTS engines
//     /// </summary>
//     public class TTSManager : MonoBehaviour
//     {
//         [Header("TTS Engine Settings")]
//         [SerializeField] private TTSEngine selectedEngine = TTSEngine.OculusVoice;
//         [SerializeField] private string voiceId = "default";
//         [SerializeField] private float speechRate = 1.0f;
//         [SerializeField] private float pitch = 1.0f;
//         [SerializeField] private float volume = 1.0f;
        
//         [Header("Audio Configuration")]
//         [SerializeField] private AudioSource audioSource;
//         [SerializeField] private bool use3DAudio = true;
//         [SerializeField] private float maxDistance = 10f;
//         [SerializeField] private AnimationCurve volumeCurve = AnimationCurve.Linear(0, 1, 1, 0);
        
//         [Header("Voice Profiles")]
//         [SerializeField] private List<VoiceProfile> voiceProfiles = new List<VoiceProfile>();
//         [SerializeField] private VoiceProfile currentVoiceProfile;
        
//         [Header("Performance Settings")]
//         [SerializeField] private bool enableAudioCaching = true;
//         [SerializeField] private int maxCacheSize = 50;
//         [SerializeField] private float audioQuality = 0.7f;
        
//         [Header("Debug Settings")]
//         [SerializeField] private bool enableDebugLogs = true;
//         [SerializeField] private bool showTTSTimings = false;
        
//         // Events
//         [System.Serializable]
//         public class TTSStartEvent : UnityEvent<string> { }
//         [System.Serializable]
//         public class TTSCompleteEvent : UnityEvent<TTSResult> { }
//         [System.Serializable]
//         public class TTSErrorEvent : UnityEvent<string> { }
        
//         public TTSStartEvent OnTTSStart;
//         public TTSCompleteEvent OnTTSComplete;
//         public TTSErrorEvent OnTTSError;
        
//         // Private variables
//         private Dictionary<string, AudioClip> audioCache = new Dictionary<string, AudioClip>();
//         private Queue<TTSRequest> ttsQueue = new Queue<TTSRequest>();
//         private bool isProcessing = false;
//         private Coroutine currentTTSCoroutine;
        
//         // TTS Engine references (placeholders for actual SDK integrations)
//         private object oculusVoiceService;
//         private object azureTTSService;
//         private object googleTTSService;
        
//         public enum TTSEngine
//         {
//             OculusVoice,
//             AzureCognitiveServices,
//             GoogleCloudTTS,
//             WindowsSAPITTS,
//             UnityBuiltIn
//         }
        
//         [System.Serializable]
//         public class VoiceProfile
//         {
//             public string profileName;
//             public string voiceId;
//             public TTSEngine engine;
//             public float speechRate;
//             public float pitch;
//             public string language;
//             public string accent;
            
//             public VoiceProfile()
//             {
//                 profileName = "Default";
//                 voiceId = "default";
//                 engine = TTSEngine.OculusVoice;
//                 speechRate = 1.0f;
//                 pitch = 1.0f;
//                 language = "en-US";
//                 accent = "neutral";
//             }
//         }
        
//         private class TTSRequest
//         {
//             public string text;
//             public VoiceProfile voiceProfile;
//             public Action<TTSResult> callback;
//             public float priority;
            
//             public TTSRequest(string inputText, VoiceProfile profile, Action<TTSResult> resultCallback, float requestPriority = 1f)
//             {
//                 text = inputText;
//                 voiceProfile = profile;
//                 callback = resultCallback;
//                 priority = requestPriority;
//             }
//         }
        
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
            
//             ConfigureAudioSource();
//             InitializeDefaultVoiceProfiles();
//         }
        
//         private void Start()
//         {
//             InitializeTTSEngines();
            
//             if (currentVoiceProfile == null && voiceProfiles.Count > 0)
//             {
//                 currentVoiceProfile = voiceProfiles[0];
//             }
//         }
        
//         private void Update()
//         {
//             ProcessTTSQueue();
//         }
        
//         private void OnDestroy()
//         {
//             CleanupTTSEngines();
//             ClearAudioCache();
//         }
        
//         #endregion
        
//         #region Initialization
        
//         private void ConfigureAudioSource()
//         {
//             audioSource.playOnAwake = false;
//             audioSource.loop = false;
//             audioSource.volume = volume;
            
//             if (use3DAudio)
//             {
//                 audioSource.spatialBlend = 1f; // 3D audio
//                 audioSource.rolloffMode = AudioRolloffMode.Custom;
//                 audioSource.maxDistance = maxDistance;
//                 audioSource.SetCustomCurve(AudioSourceCurveType.CustomRolloff, volumeCurve);
//             }
//             else
//             {
//                 audioSource.spatialBlend = 0f; // 2D audio
//             }
//         }
        
//         private void InitializeDefaultVoiceProfiles()
//         {
//             if (voiceProfiles.Count == 0)
//             {
//                 // Add default voice profiles
//                 voiceProfiles.Add(new VoiceProfile
//                 {
//                     profileName = "Male Adult",
//                     voiceId = "male_adult",
//                     engine = TTSEngine.OculusVoice,
//                     speechRate = 1.0f,
//                     pitch = 0.9f,
//                     language = "en-US"
//                 });
                
//                 voiceProfiles.Add(new VoiceProfile
//                 {
//                     profileName = "Female Adult",
//                     voiceId = "female_adult",
//                     engine = TTSEngine.OculusVoice,
//                     speechRate = 1.1f,
//                     pitch = 1.1f,
//                     language = "en-US"
//                 });
                
//                 voiceProfiles.Add(new VoiceProfile
//                 {
//                     profileName = "Robotic",
//                     voiceId = "robotic",
//                     engine = TTSEngine.UnityBuiltIn,
//                     speechRate = 0.9f,
//                     pitch = 0.8f,
//                     language = "en-US"
//                 });
//             }
//         }
        
//         private void InitializeTTSEngines()
//         {
//             try
//             {
//                 // Initialize selected TTS engines
//                 switch (selectedEngine)
//                 {
//                     case TTSEngine.OculusVoice:
//                         InitializeOculusVoiceTTS();
//                         break;
//                     case TTSEngine.AzureCognitiveServices:
//                         InitializeAzureTTS();
//                         break;
//                     case TTSEngine.GoogleCloudTTS:
//                         InitializeGoogleTTS();
//                         break;
//                     case TTSEngine.WindowsSAPITTS:
//                         InitializeWindowsTTS();
//                         break;
//                     case TTSEngine.UnityBuiltIn:
//                         InitializeUnityTTS();
//                         break;
//                 }
                
//                 LogDebug($"TTS Manager initialized with {selectedEngine} engine");
//             }
//             catch (Exception e)
//             {
//                 LogDebug($"Failed to initialize TTS engines: {e.Message}");
//             }
//         }
        
//         private void InitializeOculusVoiceTTS()
//         {
//             // TODO: Initialize Oculus Voice TTS
//             LogDebug("Oculus Voice TTS initialized (placeholder)");
//         }
        
//         private void InitializeAzureTTS()
//         {
//             // TODO: Initialize Azure Cognitive Services TTS
//             LogDebug("Azure TTS initialized (placeholder)");
//         }
        
//         private void InitializeGoogleTTS()
//         {
//             // TODO: Initialize Google Cloud TTS
//             LogDebug("Google TTS initialized (placeholder)");
//         }
        
//         private void InitializeWindowsTTS()
//         {
//             // TODO: Initialize Windows SAPI TTS
//             LogDebug("Windows TTS initialized (placeholder)");
//         }
        
//         private void InitializeUnityTTS()
//         {
//             // Built-in Unity TTS (simple implementation)
//             LogDebug("Unity built-in TTS initialized");
//         }
        
//         #endregion
        
//         #region TTS Processing
        
//         public void ConvertTextToSpeech(string text, Action<TTSResult> callback = null)
//         {
//             ConvertTextToSpeech(text, currentVoiceProfile, callback);
//         }
        
//         public void ConvertTextToSpeech(string text, VoiceProfile voiceProfile, Action<TTSResult> callback = null)
//         {
//             if (string.IsNullOrEmpty(text))
//             {
//                 TTSResult errorResult = new TTSResult
//                 {
//                     isSuccess = false,
//                     errorMessage = "Text input is null or empty"
//                 };
//                 callback?.Invoke(errorResult);
//                 OnTTSError?.Invoke(errorResult.errorMessage);
//                 return;
//             }
            
//             // Check cache first
//             string cacheKey = GetCacheKey(text, voiceProfile);
//             if (enableAudioCaching && audioCache.ContainsKey(cacheKey))
//             {
//                 TTSResult cachedResult = new TTSResult
//                 {
//                     isSuccess = true,
//                     audioClip = audioCache[cacheKey],
//                     duration = audioCache[cacheKey].length
//                 };
                
//                 callback?.Invoke(cachedResult);
//                 OnTTSComplete?.Invoke(cachedResult);
//                 LogDebug($"TTS result served from cache: '{text.Substring(0, Math.Min(text.Length, 30))}...'");
//                 return;
//             }
            
//             // Add to queue for processing
//             TTSRequest request = new TTSRequest(text, voiceProfile, callback);
//             ttsQueue.Enqueue(request);
            
//             LogDebug($"TTS request queued: '{text.Substring(0, Math.Min(text.Length, 30))}...'");
//         }
        
//         private void ProcessTTSQueue()
//         {
//             if (isProcessing || ttsQueue.Count == 0) return;
            
//             TTSRequest request = ttsQueue.Dequeue();
//             currentTTSCoroutine = StartCoroutine(ProcessTTSRequest(request));
//         }
        
//         private IEnumerator ProcessTTSRequest(TTSRequest request)
//         {
//             isProcessing = true;
//             OnTTSStart?.Invoke(request.text);
            
//             float startTime = Time.time;
//             TTSResult result = new TTSResult();
            
//             try
//             {
//                 // Process TTS based on voice profile engine
//                 switch (request.voiceProfile.engine)
//                 {
//                     case TTSEngine.OculusVoice:
//                         yield return StartCoroutine(ProcessOculusVoiceTTS(request, result));
//                         break;
//                     case TTSEngine.AzureCognitiveServices:
//                         yield return StartCoroutine(ProcessAzureTTS(request, result));
//                         break;
//                     case TTSEngine.GoogleCloudTTS:
//                         yield return StartCoroutine(ProcessGoogleTTS(request, result));
//                         break;
//                     case TTSEngine.WindowsSAPITTS:
//                         yield return StartCoroutine(ProcessWindowsTTS(request, result));
//                         break;
//                     case TTSEngine.UnityBuiltIn:
//                         yield return StartCoroutine(ProcessUnityTTS(request, result));
//                         break;
//                 }
                
//                 // Cache successful results
//                 if (result.isSuccess && enableAudioCaching)
//                 {
//                     string cacheKey = GetCacheKey(request.text, request.voiceProfile);
//                     CacheAudioClip(cacheKey, result.audioClip);
//                 }
                
//                 float processingTime = Time.time - startTime;
//                 if (showTTSTimings)
//                 {
//                     LogDebug($"TTS processing completed in {processingTime:F2}s");
//                 }
//             }
//             catch (Exception e)
//             {
//                 result.isSuccess = false;
//                 result.errorMessage = e.Message;
//                 LogDebug($"TTS processing failed: {e.Message}");
//             }
            
//             // Invoke callbacks and events
//             request.callback?.Invoke(result);
            
//             if (result.isSuccess)
//             {
//                 OnTTSComplete?.Invoke(result);
//             }
//             else
//             {
//                 OnTTSError?.Invoke(result.errorMessage);
//             }
            
//             isProcessing = false;
//         }
        
//         private IEnumerator ProcessOculusVoiceTTS(TTSRequest request, TTSResult result)
//         {
//             // TODO: Implement Oculus Voice TTS processing
//             yield return new WaitForSeconds(1f); // Simulate processing time
            
//             // Placeholder implementation
//             result.isSuccess = true;
//             result.audioClip = GeneratePlaceholderAudio(request.text, request.voiceProfile);
//             result.duration = result.audioClip != null ? result.audioClip.length : 0f;
//         }
        
//         private IEnumerator ProcessAzureTTS(TTSRequest request, TTSResult result)
//         {
//             // TODO: Implement Azure TTS processing
//             yield return new WaitForSeconds(0.8f);
            
//             result.isSuccess = true;
//             result.audioClip = GeneratePlaceholderAudio(request.text, request.voiceProfile);
//             result.duration = result.audioClip != null ? result.audioClip.length : 0f;
//         }
        
//         private IEnumerator ProcessGoogleTTS(TTSRequest request, TTSResult result)
//         {
//             // TODO: Implement Google Cloud TTS processing
//             yield return new WaitForSeconds(0.9f);
            
//             result.isSuccess = true;
//             result.audioClip = GeneratePlaceholderAudio(request.text, request.voiceProfile);
//             result.duration = result.audioClip != null ? result.audioClip.length : 0f;
//         }
        
//         private IEnumerator ProcessWindowsTTS(TTSRequest request, TTSResult result)
//         {
//             // TODO: Implement Windows SAPI TTS processing
//             yield return new WaitForSeconds(0.6f);
            
//             result.isSuccess = true;
//             result.audioClip = GeneratePlaceholderAudio(request.text, request.voiceProfile);
//             result.duration = result.audioClip != null ? result.audioClip.length : 0f;
//         }
        
//         private IEnumerator ProcessUnityTTS(TTSRequest request, TTSResult result)
//         {
//             // Simple Unity-based TTS (placeholder)
//             yield return new WaitForSeconds(0.3f);
            
//             result.isSuccess = true;
//             result.audioClip = GeneratePlaceholderAudio(request.text, request.voiceProfile);
//             result.duration = result.audioClip != null ? result.audioClip.length : 0f;
//         }
        
//         private AudioClip GeneratePlaceholderAudio(string text, VoiceProfile voiceProfile)
//         {
//             // Generate a simple placeholder audio clip
//             // In a real implementation, this would be replaced with actual TTS output
            
//             float duration = Mathf.Max(1f, text.Length * 0.05f); // Estimate duration based on text length
//             int sampleRate = 44100;
//             int samples = Mathf.RoundToInt(duration * sampleRate);
            
//             float[] audioData = new float[samples];
//             float frequency = voiceProfile.pitch * 440f; // Base frequency modified by pitch
            
//             for (int i = 0; i < samples; i++)
//             {
//                 float time = (float)i / sampleRate;
//                 audioData[i] = Mathf.Sin(frequency * 2 * Mathf.PI * time) * 0.1f; // Low volume sine wave
//             }
            
//             AudioClip clip = AudioClip.Create("GeneratedTTS", samples, 1, sampleRate, false);
//             clip.SetData(audioData, 0);
            
//             return clip;
//         }
        
//         #endregion
        
//         #region Audio Playback
        
//         public void PlayTTSAudio(AudioClip audioClip)
//         {
//             if (audioClip == null)
//             {
//                 LogDebug("Cannot play null audio clip");
//                 return;
//             }
            
//             audioSource.clip = audioClip;
//             audioSource.Play();
            
//             LogDebug($"Playing TTS audio (duration: {audioClip.length:F2}s)");
//         }
        
//         public void StopTTSAudio()
//         {
//             if (audioSource.isPlaying)
//             {
//                 audioSource.Stop();
//                 LogDebug("Stopped TTS audio playback");
//             }
//         }
        
//         public bool IsPlaying => audioSource.isPlaying;
        
//         #endregion
        
//         #region Cache Management
        
//         private string GetCacheKey(string text, VoiceProfile voiceProfile)
//         {
//             return $"{text}_{voiceProfile.voiceId}_{voiceProfile.speechRate}_{voiceProfile.pitch}";
//         }
        
//         private void CacheAudioClip(string key, AudioClip audioClip)
//         {
//             if (audioCache.Count >= maxCacheSize)
//             {
//                 // Remove oldest cache entry
//                 var oldestKey = "";
//                 foreach (var cacheKey in audioCache.Keys)
//                 {
//                     oldestKey = cacheKey;
//                     break;
//                 }
                
//                 if (!string.IsNullOrEmpty(oldestKey))
//                 {
//                     if (audioCache[oldestKey] != null)
//                     {
//                         DestroyImmediate(audioCache[oldestKey]);
//                     }
//                     audioCache.Remove(oldestKey);
//                 }
//             }
            
//             audioCache[key] = audioClip;
//         }
        
//         public void ClearAudioCache()
//         {
//             foreach (var audioClip in audioCache.Values)
//             {
//                 if (audioClip != null)
//                 {
//                     DestroyImmediate(audioClip);
//                 }
//             }
//             audioCache.Clear();
//             LogDebug("Audio cache cleared");
//         }
        
//         #endregion
        
//         #region Utility Methods
        
//         private void CleanupTTSEngines()
//         {
//             if (currentTTSCoroutine != null)
//             {
//                 StopCoroutine(currentTTSCoroutine);
//             }
            
//             ttsQueue.Clear();
//         }
        
//         private void LogDebug(string message)
//         {
//             if (enableDebugLogs)
//             {
//                 Debug.Log($"[TTSManager] {message}");
//             }
//         }
        
//         #endregion
        
//         #region Public API
        
//         public void SetVoiceProfile(string profileName)
//         {
//             VoiceProfile profile = voiceProfiles.Find(p => p.profileName == profileName);
//             if (profile != null)
//             {
//                 currentVoiceProfile = profile;
//                 LogDebug($"Voice profile changed to: {profileName}");
//             }
//             else
//             {
//                 LogDebug($"Voice profile not found: {profileName}");
//             }
//         }
        
//         public void SetVoiceProfile(VoiceProfile profile)
//         {
//             if (profile != null)
//             {
//                 currentVoiceProfile = profile;
//                 LogDebug($"Voice profile changed to: {profile.profileName}");
//             }
//         }
        
//         public VoiceProfile GetCurrentVoiceProfile()
//         {
//             return currentVoiceProfile;
//         }
        
//         public List<string> GetAvailableVoiceProfiles()
//         {
//             List<string> profileNames = new List<string>();
//             foreach (var profile in voiceProfiles)
//             {
//                 profileNames.Add(profile.profileName);
//             }
//             return profileNames;
//         }
        
//         public void SetSpeechRate(float rate)
//         {
//             speechRate = Mathf.Clamp(rate, 0.1f, 3.0f);
//             if (currentVoiceProfile != null)
//             {
//                 currentVoiceProfile.speechRate = speechRate;
//             }
//         }
        
//         public void SetPitch(float pitchValue)
//         {
//             pitch = Mathf.Clamp(pitchValue, 0.1f, 2.0f);
//             if (currentVoiceProfile != null)
//             {
//                 currentVoiceProfile.pitch = pitch;
//             }
//         }
        
//         public void SetVolume(float volumeValue)
//         {
//             volume = Mathf.Clamp01(volumeValue);
//             audioSource.volume = volume;
//         }
        
//         public int GetQueueSize()
//         {
//             return ttsQueue.Count;
//         }
        
//         public int GetCacheSize()
//         {
//             return audioCache.Count;
//         }
        
//         #endregion
//     }
// }