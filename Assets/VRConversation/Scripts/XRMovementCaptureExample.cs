// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;
// using VRConversation.Utilities;

// namespace VRConversation.Examples
// {
//     /// <summary>
//     /// Example script demonstrating how to use XRMovementCaptureUtility for gesture recognition
//     /// This script shows integration with ML models and handling of gesture events
//     /// </summary>
//     public class XRMovementCaptureExample : MonoBehaviour
//     {
//         [Header("Movement Capture")]
//         [SerializeField] private XRMovementCaptureUtility movementCapture;
        
//         [Header("UI References")]
//         [SerializeField] private UnityEngine.UI.Text statusText;
//         [SerializeField] private UnityEngine.UI.Text gestureResultText;
//         [SerializeField] private UnityEngine.UI.Text movementStatsText;
//         [SerializeField] private UnityEngine.UI.Button startCaptureButton;
//         [SerializeField] private UnityEngine.UI.Button stopCaptureButton;
//         [SerializeField] private UnityEngine.UI.Button captureGestureButton;
//         [SerializeField] private UnityEngine.UI.Button clearHistoryButton;
        
//         [Header("Gesture Recognition Settings")]
//         [SerializeField] private string[] recognizedGestures = 
//         {
//             "wave_hello",
//             "thumbs_up", 
//             "point_forward",
//             "peace_sign",
//             "fist_bump",
//             "beckoning",
//             "stop_gesture",
//             "clap",
//             "salute",
//             "unknown"
//         };
        
//         [Header("Visual Feedback")]
//         [SerializeField] private GameObject gestureIndicator;
//         [SerializeField] private Material[] gestureMaterials;
//         [SerializeField] private AudioSource feedbackAudioSource;
//         [SerializeField] private AudioClip gestureDetectedSound;
//         [SerializeField] private AudioClip gestureCompletedSound;
        
//         [Header("ML Integration")]
//         [SerializeField] private bool useLocalMLModel = false;
//         [SerializeField] private string localModelPath = "Assets/MLModels/gesture_model.onnx";
//         [SerializeField] private bool enableGestureHistory = true;
//         [SerializeField] private int maxGestureHistory = 50;
        
//         // Private variables
//         private List<XRMovementCaptureUtility.GestureData> gestureHistory = new List<XRMovementCaptureUtility.GestureData>();
//         private Dictionary<string, int> gestureCount = new Dictionary<string, int>();
//         private float sessionStartTime;
//         private int totalGesturesDetected = 0;
//         private string currentGestureStatus = "Idle";
        
//         // Coroutines
//         private Coroutine statsUpdateCoroutine;
        
//         #region Unity Lifecycle
        
//         private void Start()
//         {
//             InitializeExample();
//             SetupMovementCapture();
//             StartStatsUpdate();
//         }
        
//         private void OnDestroy()
//         {
//             CleanupExample();
//         }
        
//         #endregion
        
//         #region Initialization
        
//         private void InitializeExample()
//         {
//             sessionStartTime = Time.time;
            
//             // Initialize gesture count dictionary
//             foreach (string gesture in recognizedGestures)
//             {
//                 gestureCount[gesture] = 0;
//             }
            
//             // Setup UI buttons
//             SetupUIButtons();
            
//             // Initialize visual feedback
//             if (gestureIndicator != null)
//             {
//                 gestureIndicator.SetActive(false);
//             }
            
//             UpdateStatusText("XR Movement Capture Example Initialized");
//             Debug.Log("[XRMovementExample] Example initialized successfully");
//         }
        
//         private void SetupUIButtons()
//         {
//             if (startCaptureButton != null)
//             {
//                 startCaptureButton.onClick.AddListener(() => StartMovementCapture());
//             }
            
//             if (stopCaptureButton != null)
//             {
//                 stopCaptureButton.onClick.AddListener(() => StopMovementCapture());
//             }
            
//             if (captureGestureButton != null)
//             {
//                 captureGestureButton.onClick.AddListener(() => CaptureManualGesture());
//             }
            
//             if (clearHistoryButton != null)
//             {
//                 clearHistoryButton.onClick.AddListener(() => ClearGestureHistory());
//             }
//         }
        
//         private void SetupMovementCapture()
//         {
//             if (movementCapture == null)
//             {
//                 movementCapture = FindObjectOfType<XRMovementCaptureUtility>();
                
//                 if (movementCapture == null)
//                 {
//                     Debug.LogError("[XRMovementExample] XRMovementCaptureUtility not found in scene!");
//                     return;
//                 }
//             }
            
//             // Subscribe to events
//             movementCapture.OnGestureDetected.AddListener(OnGestureDetected);
//             movementCapture.OnGestureCompleted.AddListener(OnGestureCompleted);
//             movementCapture.OnMovementCaptured.AddListener(OnMovementCaptured);
//             movementCapture.OnMLResult.AddListener(OnMLResult);
//             movementCapture.OnCaptureStarted.AddListener(OnCaptureStarted);
//             movementCapture.OnCaptureStopped.AddListener(OnCaptureStopped);
            
//             // Configure ML endpoint
//             if (!useLocalMLModel)
//             {
//                 movementCapture.SetMLEndpoint("http://localhost:5000/api/predict_gesture");
//             }
            
//             // Set optimal capture settings for gesture recognition
//             movementCapture.SetCaptureRate(30f);
//             movementCapture.SetGestureThresholds(0.08f, 0.03f);
            
//             Debug.Log("[XRMovementExample] Movement capture configured");
//         }
        
//         #endregion
        
//         #region Movement Capture Control
        
//         public void StartMovementCapture()
//         {
//             if (movementCapture != null && !movementCapture.IsCapturing)
//             {
//                 movementCapture.StartCapture();
//                 currentGestureStatus = "Listening";
//                 UpdateStatusText("Movement capture started - Ready for gestures");
//             }
//         }
        
//         public void StopMovementCapture()
//         {
//             if (movementCapture != null && movementCapture.IsCapturing)
//             {
//                 movementCapture.StopCapture();
//                 currentGestureStatus = "Stopped";
//                 UpdateStatusText("Movement capture stopped");
//             }
//         }
        
//         public void CaptureManualGesture()
//         {
//             if (movementCapture != null)
//             {
//                 movementCapture.CaptureGesture();
//                 UpdateStatusText("Manual gesture capture initiated");
//             }
//         }
        
//         public void ClearGestureHistory()
//         {
//             gestureHistory.Clear();
//             gestureCount.Clear();
            
//             foreach (string gesture in recognizedGestures)
//             {
//                 gestureCount[gesture] = 0;
//             }
            
//             totalGesturesDetected = 0;
            
//             if (movementCapture != null)
//             {
//                 movementCapture.ClearHistory();
//             }
            
//             UpdateStatusText("Gesture history cleared");
//             UpdateGestureResultText("History cleared");
//         }
        
//         #endregion
        
//         #region Event Handlers
        
//         private void OnGestureDetected(XRMovementCaptureUtility.GestureData gestureData)
//         {
//             currentGestureStatus = "Recording";
//             UpdateStatusText($"Gesture detected - Recording... (ID: {gestureData.gestureId.Substring(0, 8)})");
            
//             // Visual feedback
//             ShowGestureIndicator(Color.yellow);
            
//             // Audio feedback
//             PlayFeedbackSound(gestureDetectedSound);
            
//             Debug.Log($"[XRMovementExample] Gesture detection started: {gestureData.gestureId}");
//         }
        
//         private void OnGestureCompleted(XRMovementCaptureUtility.GestureData gestureData)
//         {
//             currentGestureStatus = "Processing";
//             totalGesturesDetected++;
            
//             // Add to history
//             if (enableGestureHistory)
//             {
//                 gestureHistory.Add(gestureData);
                
//                 // Maintain max history size
//                 if (gestureHistory.Count > maxGestureHistory)
//                 {
//                     gestureHistory.RemoveAt(0);
//                 }
//             }
            
//             // Update gesture count
//             string recognizedGesture = !string.IsNullOrEmpty(gestureData.recognizedGesture) 
//                 ? gestureData.recognizedGesture 
//                 : "unknown";
            
//             if (gestureCount.ContainsKey(recognizedGesture))
//             {
//                 gestureCount[recognizedGesture]++;
//             }
//             else
//             {
//                 gestureCount[recognizedGesture] = 1;
//             }
            
//             UpdateStatusText($"Gesture completed - Duration: {gestureData.duration:F2}s, Frames: {gestureData.frames.Count}");
            
//             // Visual feedback
//             ShowGestureIndicator(Color.green);
            
//             // Audio feedback
//             PlayFeedbackSound(gestureCompletedSound);
            
//             // Update result display
//             UpdateGestureResult(gestureData);
            
//             // Reset status after processing
//             StartCoroutine(ResetGestureStatus());
            
//             Debug.Log($"[XRMovementExample] Gesture completed: {gestureData.gestureId} - {recognizedGesture} ({gestureData.confidence:F2})");
//         }
        
//         private void OnMovementCaptured(XRMovementCaptureUtility.MovementFrame frame)
//         {
//             // Update real-time movement display
//             // This is called frequently, so keep processing minimal
//         }
        
//         private void OnMLResult(XRMovementCaptureUtility.MLGestureResult result)
//         {
//             if (result.isSuccess)
//             {
//                 UpdateStatusText($"ML Result: {result.predictedGesture} (confidence: {result.confidence:F2})");
                
//                 // Update gesture indicator color based on confidence
//                 Color indicatorColor = Color.Lerp(Color.red, Color.green, result.confidence);
//                 ShowGestureIndicator(indicatorColor);
                
//                 Debug.Log($"[XRMovementExample] ML Result - Gesture: {result.predictedGesture}, Confidence: {result.confidence:F2}, Processing Time: {result.processingTime:F3}s");
//             }
//             else
//             {
//                 UpdateStatusText($"ML Error: {result.errorMessage}");
//                 ShowGestureIndicator(Color.red);
                
//                 Debug.LogWarning($"[XRMovementExample] ML Error: {result.errorMessage}");
//             }
//         }
        
//         private void OnCaptureStarted()
//         {
//             currentGestureStatus = "Listening";
//             sessionStartTime = Time.time;
            
//             if (startCaptureButton != null) startCaptureButton.interactable = false;
//             if (stopCaptureButton != null) stopCaptureButton.interactable = true;
//             if (captureGestureButton != null) captureGestureButton.interactable = true;
            
//             Debug.Log("[XRMovementExample] Capture session started");
//         }
        
//         private void OnCaptureStopped()
//         {
//             currentGestureStatus = "Stopped";
            
//             if (startCaptureButton != null) startCaptureButton.interactable = true;
//             if (stopCaptureButton != null) stopCaptureButton.interactable = false;
//             if (captureGestureButton != null) captureGestureButton.interactable = false;
            
//             HideGestureIndicator();
            
//             Debug.Log("[XRMovementExample] Capture session stopped");
//         }
        
//         #endregion
        
//         #region Visual Feedback
        
//         private void ShowGestureIndicator(Color color)
//         {
//             if (gestureIndicator != null)
//             {
//                 gestureIndicator.SetActive(true);
                
//                 Renderer renderer = gestureIndicator.GetComponent<Renderer>();
//                 if (renderer != null)
//                 {
//                     renderer.material.color = color;
//                 }
                
//                 // Auto-hide after 2 seconds
//                 StartCoroutine(HideGestureIndicatorAfterDelay(2f));
//             }
//         }
        
//         private void HideGestureIndicator()
//         {
//             if (gestureIndicator != null)
//             {
//                 gestureIndicator.SetActive(false);
//             }
//         }
        
//         private IEnumerator HideGestureIndicatorAfterDelay(float delay)
//         {
//             yield return new WaitForSeconds(delay);
//             HideGestureIndicator();
//         }
        
//         private void PlayFeedbackSound(AudioClip clip)
//         {
//             if (feedbackAudioSource != null && clip != null)
//             {
//                 feedbackAudioSource.clip = clip;
//                 feedbackAudioSource.Play();
//             }
//         }
        
//         #endregion
        
//         #region UI Updates
        
//         private void UpdateStatusText(string status)
//         {
//             if (statusText != null)
//             {
//                 statusText.text = $"Status: {status}";
//             }
//         }
        
//         private void UpdateGestureResult(XRMovementCaptureUtility.GestureData gestureData)
//         {
//             if (gestureResultText != null)
//             {
//                 string resultText = $"Last Gesture:\n";
//                 resultText += $"Type: {gestureData.recognizedGesture ?? "Processing..."}\n";
//                 resultText += $"Confidence: {gestureData.confidence:F2}\n";
//                 resultText += $"Duration: {gestureData.duration:F2}s\n";
//                 resultText += $"Frames: {gestureData.frames.Count}\n";
//                 resultText += $"Distance: {gestureData.totalDistance:F2}m\n";
//                 resultText += $"Avg Velocity: {gestureData.averageVelocity.magnitude:F2}m/s";
                
//                 gestureResultText.text = resultText;
//             }
//         }
        
//         private void UpdateGestureResultText(string text)
//         {
//             if (gestureResultText != null)
//             {
//                 gestureResultText.text = text;
//             }
//         }
        
//         private void StartStatsUpdate()
//         {
//             if (statsUpdateCoroutine != null)
//             {
//                 StopCoroutine(statsUpdateCoroutine);
//             }
            
//             statsUpdateCoroutine = StartCoroutine(UpdateStatsCoroutine());
//         }
        
//         private IEnumerator UpdateStatsCoroutine()
//         {
//             while (true)
//             {
//                 UpdateMovementStats();
//                 yield return new WaitForSeconds(1f); // Update every second
//             }
//         }
        
//         private void UpdateMovementStats()
//         {
//             if (movementStatsText == null) return;
            
//             string statsText = "Movement Statistics:\n";
//             statsText += $"Status: {currentGestureStatus}\n";
            
//             if (movementCapture != null)
//             {
//                 statsText += $"Capturing: {(movementCapture.IsCapturing ? "Yes" : "No")}\n";
//                 statsText += $"Active Gesture: {(movementCapture.IsGestureActive ? "Yes" : "No")}\n";
//                 statsText += $"History Count: {movementCapture.MovementHistoryCount}\n";
//                 statsText += $"Capture Time: {movementCapture.CurrentCaptureTime:F1}s\n";
//             }
            
//             statsText += $"Total Gestures: {totalGesturesDetected}\n";
//             statsText += $"Session Time: {(Time.time - sessionStartTime):F1}s\n";
            
//             // Add gesture count breakdown
//             statsText += "\nGesture Counts:\n";
//             foreach (var kvp in gestureCount)
//             {
//                 if (kvp.Value > 0)
//                 {
//                     statsText += $"{kvp.Key}: {kvp.Value}\n";
//                 }
//             }
            
//             movementStatsText.text = statsText;
//         }
        
//         #endregion
        
//         #region Utility Methods
        
//         private IEnumerator ResetGestureStatus()
//         {
//             yield return new WaitForSeconds(2f);
            
//             if (movementCapture != null && movementCapture.IsCapturing)
//             {
//                 currentGestureStatus = "Listening";
//                 UpdateStatusText("Ready for next gesture");
//             }
//         }
        
//         private void CleanupExample()
//         {
//             if (statsUpdateCoroutine != null)
//             {
//                 StopCoroutine(statsUpdateCoroutine);
//             }
            
//             // Unsubscribe from events
//             if (movementCapture != null)
//             {
//                 movementCapture.OnGestureDetected.RemoveListener(OnGestureDetected);
//                 movementCapture.OnGestureCompleted.RemoveListener(OnGestureCompleted);
//                 movementCapture.OnMovementCaptured.RemoveListener(OnMovementCaptured);
//                 movementCapture.OnMLResult.RemoveListener(OnMLResult);
//                 movementCapture.OnCaptureStarted.RemoveListener(OnCaptureStarted);
//                 movementCapture.OnCaptureStopped.RemoveListener(OnCaptureStopped);
//             }
//         }
        
//         #endregion
        
//         #region Public API
        
//         public void ExportGestureData()
//         {
//             if (gestureHistory.Count == 0)
//             {
//                 UpdateStatusText("No gesture data to export");
//                 return;
//             }
            
//             try
//             {
//                 string fileName = $"gesture_data_{System.DateTime.Now:yyyyMMdd_HHmmss}.json";
//                 string filePath = System.IO.Path.Combine(Application.persistentDataPath, fileName);
                
//                 var exportData = new
//                 {
//                     session_info = new
//                     {
//                         start_time = sessionStartTime,
//                         end_time = Time.time,
//                         total_duration = Time.time - sessionStartTime,
//                         total_gestures = totalGesturesDetected
//                     },
//                     gesture_counts = gestureCount,
//                     gesture_history = gestureHistory
//                 };
                
//                 string jsonData = JsonUtility.ToJson(exportData, true);
//                 System.IO.File.WriteAllText(filePath, jsonData);
                
//                 UpdateStatusText($"Gesture data exported to: {fileName}");
//                 Debug.Log($"[XRMovementExample] Gesture data exported to: {filePath}");
//             }
//             catch (System.Exception e)
//             {
//                 UpdateStatusText($"Export failed: {e.Message}");
//                 Debug.LogError($"[XRMovementExample] Export failed: {e.Message}");
//             }
//         }
        
//         public void ImportGestureData(string filePath)
//         {
//             try
//             {
//                 if (System.IO.File.Exists(filePath))
//                 {
//                     string jsonData = System.IO.File.ReadAllText(filePath);
//                     // Parse and load gesture data
//                     // Implementation depends on your specific data format
                    
//                     UpdateStatusText("Gesture data imported successfully");
//                     Debug.Log($"[XRMovementExample] Gesture data imported from: {filePath}");
//                 }
//                 else
//                 {
//                     UpdateStatusText("Import file not found");
//                 }
//             }
//             catch (System.Exception e)
//             {
//                 UpdateStatusText($"Import failed: {e.Message}");
//                 Debug.LogError($"[XRMovementExample] Import failed: {e.Message}");
//             }
//         }
        
//         public List<XRMovementCaptureUtility.GestureData> GetGestureHistory()
//         {
//             return new List<XRMovementCaptureUtility.GestureData>(gestureHistory);
//         }
        
//         public Dictionary<string, int> GetGestureCount()
//         {
//             return new Dictionary<string, int>(gestureCount);
//         }
        
//         public void SetGestureRecognitionMode(bool useRealtime)
//         {
//             // Configure movement capture for different recognition modes
//             if (movementCapture != null)
//             {
//                 // Adjust settings based on mode
//                 if (useRealtime)
//                 {
//                     movementCapture.SetCaptureRate(60f);
//                     movementCapture.SetGestureThresholds(0.05f, 0.02f);
//                 }
//                 else
//                 {
//                     movementCapture.SetCaptureRate(30f);
//                     movementCapture.SetGestureThresholds(0.1f, 0.05f);
//                 }
//             }
//         }
        
//         #endregion
//     }
// }