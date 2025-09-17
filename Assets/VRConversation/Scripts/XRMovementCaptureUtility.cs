// using System;
// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;
// using UnityEngine.XR;
// using UnityEngine.XR.Interaction.Toolkit;
// using UnityEngine.Events;

// namespace VRConversation.Utilities
// {
//     /// <summary>
//     /// Utility class for capturing XR controller, head, and body movements for gesture recognition
//     /// Collects movement data and sends it to backend ML models for analysis
//     /// </summary>
//     public class XRMovementCaptureUtility : MonoBehaviour
//     {
//         [Header("XR References")]
//         [SerializeField] private XRRig xrRig;
//         [SerializeField] private Transform headTransform;
//         [SerializeField] private XRController leftController;
//         [SerializeField] private XRController rightController;
//         [SerializeField] private Transform leftHandTransform;
//         [SerializeField] private Transform rightHandTransform;
        
//         [Header("Capture Settings")]
//         [SerializeField] private float captureRate = 30f; // Hz
//         [SerializeField] private float gestureCaptureWindow = 3f; // seconds
//         [SerializeField] private bool enableContinuousCapture = true;
//         [SerializeField] private bool captureOnDemand = false;
        
//         [Header("Movement Filters")]
//         [SerializeField] private bool enablePositionSmoothing = true;
//         [SerializeField] private float smoothingFactor = 0.1f;
//         [SerializeField] private float minimumMovementThreshold = 0.001f;
//         [SerializeField] private bool normalizePositions = true;
        
//         [Header("Data Collection")]
//         [SerializeField] private int maxDataPoints = 300; // 10 seconds at 30fps
//         [SerializeField] private bool includeVelocityData = true;
//         [SerializeField] private bool includeAccelerationData = true;
//         [SerializeField] private bool includeAngularData = true;
        
//         [Header("Gesture Detection")]
//         [SerializeField] private float gestureStartThreshold = 0.1f;
//         [SerializeField] private float gestureEndThreshold = 0.05f;
//         [SerializeField] private float gestureTimeoutDuration = 2f;
        
//         [Header("ML Integration")]
//         [SerializeField] private string mlEndpointURL = "http://localhost:5000/predict";
//         [SerializeField] private bool enableRealtimeInference = false;
//         [SerializeField] private float inferenceInterval = 1f;
        
//         [Header("Debug Settings")]
//         [SerializeField] private bool enableDebugLogs = true;
//         [SerializeField] private bool enableVisualization = false;
//         [SerializeField] private LineRenderer debugLineRenderer;
        
//         // Events
//         [System.Serializable]
//         public class GestureEvent : UnityEvent<GestureData> { }
//         [System.Serializable]
//         public class MovementEvent : UnityEvent<MovementFrame> { }
//         [System.Serializable]
//         public class MLResultEvent : UnityEvent<MLGestureResult> { }
        
//         public GestureEvent OnGestureDetected;
//         public GestureEvent OnGestureCompleted;
//         public MovementEvent OnMovementCaptured;
//         public MLResultEvent OnMLResult;
//         public UnityEvent OnCaptureStarted;
//         public UnityEvent OnCaptureStopped;
        
//         // Callback delegates
//         public System.Action<GestureData> GestureDetectedCallback;
//         public System.Action<GestureData> GestureCompletedCallback;
//         public System.Action<MovementFrame> MovementCapturedCallback;
//         public System.Action<MLGestureResult> MLResultCallback;
        
//         // Private variables
//         private List<MovementFrame> movementHistory = new List<MovementFrame>();
//         private List<MovementFrame> currentGestureData = new List<MovementFrame>();
//         private Coroutine captureCoroutine;
//         private Coroutine inferenceCoroutine;
        
//         // State tracking
//         private bool isCapturing = false;
//         private bool isGestureActive = false;
//         private float lastGestureTime = 0f;
//         private float captureStartTime = 0f;
        
//         // Previous frame data for velocity/acceleration calculation
//         private MovementFrame previousFrame;
//         private Vector3 leftControllerPreviousVelocity = Vector3.zero;
//         private Vector3 rightControllerPreviousVelocity = Vector3.zero;
//         private Vector3 headPreviousVelocity = Vector3.zero;
        
//         // Reference positions for normalization
//         private Vector3 referencePosition = Vector3.zero;
//         private bool hasReferencePosition = false;
        
//         #region Data Structures
        
//         [System.Serializable]
//         public class MovementFrame
//         {
//             public float timestamp;
//             public Vector3 headPosition;
//             public Quaternion headRotation;
//             public Vector3 leftHandPosition;
//             public Quaternion leftHandRotation;
//             public Vector3 rightHandPosition;
//             public Quaternion rightHandRotation;
//             public Vector3 bodyPosition; // XR Rig position
//             public Quaternion bodyRotation; // XR Rig rotation
            
//             // Velocity data
//             public Vector3 headVelocity;
//             public Vector3 leftHandVelocity;
//             public Vector3 rightHandVelocity;
            
//             // Acceleration data
//             public Vector3 headAcceleration;
//             public Vector3 leftHandAcceleration;
//             public Vector3 rightHandAcceleration;
            
//             // Angular velocity data
//             public Vector3 headAngularVelocity;
//             public Vector3 leftHandAngularVelocity;
//             public Vector3 rightHandAngularVelocity;
            
//             // Input data
//             public bool leftTriggerPressed;
//             public bool rightTriggerPressed;
//             public bool leftGripPressed;
//             public bool rightGripPressed;
//             public Vector2 leftThumbstick;
//             public Vector2 rightThumbstick;
            
//             public MovementFrame()
//             {
//                 timestamp = Time.time;
//             }
//         }
        
//         [System.Serializable]
//         public class GestureData
//         {
//             public string gestureId;
//             public float startTime;
//             public float endTime;
//             public float duration;
//             public List<MovementFrame> frames = new List<MovementFrame>();
//             public Vector3 primaryHandStartPosition;
//             public Vector3 primaryHandEndPosition;
//             public float totalDistance;
//             public Vector3 averageVelocity;
//             public string recognizedGesture;
//             public float confidence;
            
//             public GestureData()
//             {
//                 gestureId = System.Guid.NewGuid().ToString();
//                 startTime = Time.time;
//             }
//         }
        
//         [System.Serializable]
//         public class MLGestureResult
//         {
//             public string predictedGesture;
//             public float confidence;
//             public Dictionary<string, float> allPredictions = new Dictionary<string, float>();
//             public float processingTime;
//             public bool isSuccess;
//             public string errorMessage;
            
//             public MLGestureResult()
//             {
//                 isSuccess = false;
//                 confidence = 0f;
//                 processingTime = 0f;
//             }
//         }
        
//         #endregion
        
//         #region Unity Lifecycle
        
//         private void Awake()
//         {
//             InitializeXRReferences();
//         }
        
//         private void Start()
//         {
//             InitializeMovementCapture();
            
//             if (enableContinuousCapture)
//             {
//                 StartCapture();
//             }
//         }
        
//         private void Update()
//         {
//             if (enableVisualization)
//             {
//                 UpdateVisualization();
//             }
//         }
        
//         private void OnDestroy()
//         {
//             StopCapture();
//             CleanupMovementCapture();
//         }
        
//         #endregion
        
//         #region Initialization
        
//         private void InitializeXRReferences()
//         {
//             try
//             {
//                 // Find XR Rig if not assigned
//                 if (xrRig == null)
//                 {
//                     xrRig = FindObjectOfType<XRRig>();
//                 }
                
//                 if (xrRig != null)
//                 {
//                     // Get head transform
//                     if (headTransform == null)
//                     {
//                         headTransform = xrRig.cameraGameObject?.transform;
//                     }
                    
//                     // Find controllers
//                     if (leftController == null || rightController == null)
//                     {
//                         XRController[] controllers = FindObjectsOfType<XRController>();
//                         foreach (var controller in controllers)
//                         {
//                             if (controller.controllerNode == XRNode.LeftHand)
//                             {
//                                 leftController = controller;
//                                 leftHandTransform = controller.transform;
//                             }
//                             else if (controller.controllerNode == XRNode.RightHand)
//                             {
//                                 rightController = controller;
//                                 rightHandTransform = controller.transform;
//                             }
//                         }
//                     }
//                 }
                
//                 // Initialize debug line renderer
//                 if (enableVisualization && debugLineRenderer == null)
//                 {
//                     GameObject lineGO = new GameObject("MovementVisualization");
//                     lineGO.transform.SetParent(transform);
//                     debugLineRenderer = lineGO.AddComponent<LineRenderer>();
//                     ConfigureLineRenderer();
//                 }
                
//                 LogDebug("XR references initialized successfully");
//             }
//             catch (Exception e)
//             {
//                 LogDebug($"Failed to initialize XR references: {e.Message}");
//             }
//         }
        
//         private void InitializeMovementCapture()
//         {
//             // Set reference position for normalization
//             if (normalizePositions && xrRig != null)
//             {
//                 referencePosition = xrRig.transform.position;
//                 hasReferencePosition = true;
//             }
            
//             LogDebug("Movement capture initialized");
//         }
        
//         private void ConfigureLineRenderer()
//         {
//             if (debugLineRenderer == null) return;
            
//             debugLineRenderer.material = new Material(Shader.Find("Sprites/Default"));
//             debugLineRenderer.color = Color.green;
//             debugLineRenderer.startWidth = 0.02f;
//             debugLineRenderer.endWidth = 0.02f;
//             debugLineRenderer.positionCount = 0;
//         }
        
//         #endregion
        
//         #region Capture Control
        
//         public void StartCapture()
//         {
//             if (isCapturing)
//             {
//                 LogDebug("Capture already running");
//                 return;
//             }
            
//             isCapturing = true;
//             captureStartTime = Time.time;
//             movementHistory.Clear();
            
//             captureCoroutine = StartCoroutine(CaptureMovementData());
            
//             if (enableRealtimeInference)
//             {
//                 inferenceCoroutine = StartCoroutine(RealtimeInferenceCoroutine());
//             }
            
//             OnCaptureStarted?.Invoke();
//             LogDebug("Movement capture started");
//         }
        
//         public void StopCapture()
//         {
//             if (!isCapturing) return;
            
//             isCapturing = false;
            
//             if (captureCoroutine != null)
//             {
//                 StopCoroutine(captureCoroutine);
//                 captureCoroutine = null;
//             }
            
//             if (inferenceCoroutine != null)
//             {
//                 StopCoroutine(inferenceCoroutine);
//                 inferenceCoroutine = null;
//             }
            
//             OnCaptureStopped?.Invoke();
//             LogDebug("Movement capture stopped");
//         }
        
//         public void CaptureGesture()
//         {
//             if (!captureOnDemand) return;
            
//             StartCoroutine(CaptureGestureCoroutine());
//         }
        
//         private IEnumerator CaptureMovementData()
//         {
//             float captureInterval = 1f / captureRate;
            
//             while (isCapturing)
//             {
//                 MovementFrame frame = CaptureCurrentFrame();
                
//                 if (frame != null)
//                 {
//                     // Add to history
//                     movementHistory.Add(frame);
                    
//                     // Maintain max history size
//                     if (movementHistory.Count > maxDataPoints)
//                     {
//                         movementHistory.RemoveAt(0);
//                     }
                    
//                     // Check for gesture detection
//                     CheckForGestureActivity(frame);
                    
//                     // Fire movement event
//                     OnMovementCaptured?.Invoke(frame);
//                     MovementCapturedCallback?.Invoke(frame);
                    
//                     previousFrame = frame;
//                 }
                
//                 yield return new WaitForSeconds(captureInterval);
//             }
//         }
        
//         private MovementFrame CaptureCurrentFrame()
//         {
//             if (headTransform == null || leftHandTransform == null || rightHandTransform == null)
//                 return null;
            
//             MovementFrame frame = new MovementFrame();
            
//             // Capture positions and rotations
//             frame.headPosition = GetNormalizedPosition(headTransform.position);
//             frame.headRotation = headTransform.rotation;
//             frame.leftHandPosition = GetNormalizedPosition(leftHandTransform.position);
//             frame.leftHandRotation = leftHandTransform.rotation;
//             frame.rightHandPosition = GetNormalizedPosition(rightHandTransform.position);
//             frame.rightHandRotation = rightHandTransform.rotation;
            
//             if (xrRig != null)
//             {
//                 frame.bodyPosition = GetNormalizedPosition(xrRig.transform.position);
//                 frame.bodyRotation = xrRig.transform.rotation;
//             }
            
//             // Calculate velocities
//             if (previousFrame != null && includeVelocityData)
//             {
//                 float deltaTime = frame.timestamp - previousFrame.timestamp;
//                 if (deltaTime > 0)
//                 {
//                     frame.headVelocity = (frame.headPosition - previousFrame.headPosition) / deltaTime;
//                     frame.leftHandVelocity = (frame.leftHandPosition - previousFrame.leftHandPosition) / deltaTime;
//                     frame.rightHandVelocity = (frame.rightHandPosition - previousFrame.rightHandPosition) / deltaTime;
                    
//                     // Apply smoothing
//                     if (enablePositionSmoothing)
//                     {
//                         frame.headVelocity = Vector3.Lerp(headPreviousVelocity, frame.headVelocity, smoothingFactor);
//                         frame.leftHandVelocity = Vector3.Lerp(leftControllerPreviousVelocity, frame.leftHandVelocity, smoothingFactor);
//                         frame.rightHandVelocity = Vector3.Lerp(rightControllerPreviousVelocity, frame.rightHandVelocity, smoothingFactor);
//                     }
                    
//                     // Calculate accelerations
//                     if (includeAccelerationData)
//                     {
//                         frame.headAcceleration = (frame.headVelocity - headPreviousVelocity) / deltaTime;
//                         frame.leftHandAcceleration = (frame.leftHandVelocity - leftControllerPreviousVelocity) / deltaTime;
//                         frame.rightHandAcceleration = (frame.rightHandVelocity - rightControllerPreviousVelocity) / deltaTime;
//                     }
                    
//                     // Store previous velocities
//                     headPreviousVelocity = frame.headVelocity;
//                     leftControllerPreviousVelocity = frame.leftHandVelocity;
//                     rightControllerPreviousVelocity = frame.rightHandVelocity;
//                 }
//             }
            
//             // Calculate angular velocities
//             if (includeAngularData && previousFrame != null)
//             {
//                 float deltaTime = frame.timestamp - previousFrame.timestamp;
//                 if (deltaTime > 0)
//                 {
//                     frame.headAngularVelocity = CalculateAngularVelocity(previousFrame.headRotation, frame.headRotation, deltaTime);
//                     frame.leftHandAngularVelocity = CalculateAngularVelocity(previousFrame.leftHandRotation, frame.leftHandRotation, deltaTime);
//                     frame.rightHandAngularVelocity = CalculateAngularVelocity(previousFrame.rightHandRotation, frame.rightHandRotation, deltaTime);
//                 }
//             }
            
//             // Capture input data
//             CaptureInputData(frame);
            
//             return frame;
//         }
        
//         private Vector3 GetNormalizedPosition(Vector3 worldPosition)
//         {
//             if (!normalizePositions || !hasReferencePosition)
//                 return worldPosition;
            
//             return worldPosition - referencePosition;
//         }
        
//         private Vector3 CalculateAngularVelocity(Quaternion previousRotation, Quaternion currentRotation, float deltaTime)
//         {
//             Quaternion deltaRotation = currentRotation * Quaternion.Inverse(previousRotation);
//             deltaRotation.ToAngleAxis(out float angle, out Vector3 axis);
            
//             // Convert to degrees per second
//             angle *= Mathf.Deg2Rad;
//             return axis * (angle / deltaTime);
//         }
        
//         private void CaptureInputData(MovementFrame frame)
//         {
//             // Capture controller input states
//             if (leftController != null)
//             {
//                 leftController.inputDevice.TryGetFeatureValue(CommonUsages.triggerButton, out frame.leftTriggerPressed);
//                 leftController.inputDevice.TryGetFeatureValue(CommonUsages.gripButton, out frame.leftGripPressed);
//                 leftController.inputDevice.TryGetFeatureValue(CommonUsages.primary2DAxis, out frame.leftThumbstick);
//             }
            
//             if (rightController != null)
//             {
//                 rightController.inputDevice.TryGetFeatureValue(CommonUsages.triggerButton, out frame.rightTriggerPressed);
//                 rightController.inputDevice.TryGetFeatureValue(CommonUsages.gripButton, out frame.rightGripPressed);
//                 rightController.inputDevice.TryGetFeatureValue(CommonUsages.primary2DAxis, out frame.rightThumbstick);
//             }
//         }
        
//         #endregion
        
//         #region Gesture Detection
        
//         private void CheckForGestureActivity(MovementFrame frame)
//         {
//             // Calculate movement intensity
//             float movementIntensity = CalculateMovementIntensity(frame);
            
//             // Check for gesture start
//             if (!isGestureActive && movementIntensity > gestureStartThreshold)
//             {
//                 StartGestureCapture(frame);
//             }
//             // Check for gesture end
//             else if (isGestureActive)
//             {
//                 if (movementIntensity < gestureEndThreshold)
//                 {
//                     float timeSinceLastSignificantMovement = Time.time - lastGestureTime;
//                     if (timeSinceLastSignificantMovement > gestureTimeoutDuration)
//                     {
//                         EndGestureCapture();
//                     }
//                 }
//                 else
//                 {
//                     lastGestureTime = Time.time;
//                     currentGestureData.Add(frame);
//                 }
//             }
//         }
        
//         private float CalculateMovementIntensity(MovementFrame frame)
//         {
//             if (previousFrame == null) return 0f;
            
//             float headMovement = Vector3.Distance(frame.headPosition, previousFrame.headPosition);
//             float leftHandMovement = Vector3.Distance(frame.leftHandPosition, previousFrame.leftHandPosition);
//             float rightHandMovement = Vector3.Distance(frame.rightHandPosition, previousFrame.rightHandPosition);
            
//             // Combine movements with weights (hands are more important for gestures)
//             float totalMovement = headMovement * 0.2f + leftHandMovement * 0.4f + rightHandMovement * 0.4f;
            
//             return totalMovement;
//         }
        
//         private void StartGestureCapture(MovementFrame frame)
//         {
//             isGestureActive = true;
//             lastGestureTime = Time.time;
//             currentGestureData.Clear();
//             currentGestureData.Add(frame);
            
//             GestureData gestureData = new GestureData();
//             gestureData.startTime = frame.timestamp;
//             gestureData.primaryHandStartPosition = frame.rightHandPosition; // Assume right hand is primary
            
//             OnGestureDetected?.Invoke(gestureData);
//             GestureDetectedCallback?.Invoke(gestureData);
            
//             LogDebug("Gesture detection started");
//         }
        
//         private void EndGestureCapture()
//         {
//             if (!isGestureActive) return;
            
//             isGestureActive = false;
            
//             // Create gesture data
//             GestureData gestureData = CreateGestureData();
            
//             // Send to ML model for recognition
//             if (!string.IsNullOrEmpty(mlEndpointURL))
//             {
//                 StartCoroutine(SendGestureToML(gestureData));
//             }
            
//             OnGestureCompleted?.Invoke(gestureData);
//             GestureCompletedCallback?.Invoke(gestureData);
            
//             LogDebug($"Gesture capture completed - Duration: {gestureData.duration:F2}s, Frames: {gestureData.frames.Count}");
//         }
        
//         private GestureData CreateGestureData()
//         {
//             GestureData gestureData = new GestureData();
//             gestureData.frames = new List<MovementFrame>(currentGestureData);
            
//             if (gestureData.frames.Count > 0)
//             {
//                 gestureData.startTime = gestureData.frames[0].timestamp;
//                 gestureData.endTime = gestureData.frames[gestureData.frames.Count - 1].timestamp;
//                 gestureData.duration = gestureData.endTime - gestureData.startTime;
                
//                 gestureData.primaryHandStartPosition = gestureData.frames[0].rightHandPosition;
//                 gestureData.primaryHandEndPosition = gestureData.frames[gestureData.frames.Count - 1].rightHandPosition;
//                 gestureData.totalDistance = CalculateTotalDistance(gestureData.frames);
//                 gestureData.averageVelocity = CalculateAverageVelocity(gestureData.frames);
//             }
            
//             return gestureData;
//         }
        
//         private float CalculateTotalDistance(List<MovementFrame> frames)
//         {
//             float totalDistance = 0f;
            
//             for (int i = 1; i < frames.Count; i++)
//             {
//                 totalDistance += Vector3.Distance(frames[i].rightHandPosition, frames[i - 1].rightHandPosition);
//             }
            
//             return totalDistance;
//         }
        
//         private Vector3 CalculateAverageVelocity(List<MovementFrame> frames)
//         {
//             if (frames.Count < 2) return Vector3.zero;
            
//             Vector3 totalVelocity = Vector3.zero;
//             int validFrames = 0;
            
//             foreach (var frame in frames)
//             {
//                 if (frame.rightHandVelocity != Vector3.zero)
//                 {
//                     totalVelocity += frame.rightHandVelocity;
//                     validFrames++;
//                 }
//             }
            
//             return validFrames > 0 ? totalVelocity / validFrames : Vector3.zero;
//         }
        
//         private IEnumerator CaptureGestureCoroutine()
//         {
//             LogDebug("Starting manual gesture capture");
            
//             List<MovementFrame> gestureFrames = new List<MovementFrame>();
//             float startTime = Time.time;
            
//             while (Time.time - startTime < gestureCaptureWindow)
//             {
//                 MovementFrame frame = CaptureCurrentFrame();
//                 if (frame != null)
//                 {
//                     gestureFrames.Add(frame);
//                 }
                
//                 yield return new WaitForSeconds(1f / captureRate);
//             }
            
//             // Create and process gesture data
//             GestureData gestureData = new GestureData();
//             gestureData.frames = gestureFrames;
//             gestureData.startTime = startTime;
//             gestureData.endTime = Time.time;
//             gestureData.duration = gestureData.endTime - gestureData.startTime;
            
//             if (gestureFrames.Count > 0)
//             {
//                 gestureData.primaryHandStartPosition = gestureFrames[0].rightHandPosition;
//                 gestureData.primaryHandEndPosition = gestureFrames[gestureFrames.Count - 1].rightHandPosition;
//                 gestureData.totalDistance = CalculateTotalDistance(gestureFrames);
//                 gestureData.averageVelocity = CalculateAverageVelocity(gestureFrames);
                
//                 // Send to ML model
//                 if (!string.IsNullOrEmpty(mlEndpointURL))
//                 {
//                     yield return StartCoroutine(SendGestureToML(gestureData));
//                 }
                
//                 OnGestureCompleted?.Invoke(gestureData);
//                 GestureCompletedCallback?.Invoke(gestureData);
//             }
            
//             LogDebug($"Manual gesture capture completed - {gestureFrames.Count} frames captured");
//         }
        
//         #endregion
        
//         #region ML Integration
        
//         private IEnumerator RealtimeInferenceCoroutine()
//         {
//             while (isCapturing)
//             {
//                 yield return new WaitForSeconds(inferenceInterval);
                
//                 if (movementHistory.Count >= captureRate) // At least 1 second of data
//                 {
//                     // Get recent frames for inference
//                     int framesToTake = Mathf.Min((int)captureRate, movementHistory.Count);
//                     List<MovementFrame> recentFrames = movementHistory.GetRange(movementHistory.Count - framesToTake, framesToTake);
                    
//                     GestureData realtimeData = new GestureData();
//                     realtimeData.frames = recentFrames;
//                     realtimeData.startTime = recentFrames[0].timestamp;
//                     realtimeData.endTime = recentFrames[recentFrames.Count - 1].timestamp;
//                     realtimeData.duration = realtimeData.endTime - realtimeData.startTime;
                    
//                     // Send to ML model without waiting
//                     StartCoroutine(SendGestureToML(realtimeData, true));
//                 }
//             }
//         }
        
//         private IEnumerator SendGestureToML(GestureData gestureData, bool isRealtime = false)
//         {
//             float startTime = Time.time;
//             MLGestureResult result = new MLGestureResult();
            
//             try
//             {
//                 // Prepare data for ML model
//                 string jsonData = PrepareMLData(gestureData);
                
//                 // Create web request
//                 using (UnityEngine.Networking.UnityWebRequest request = UnityEngine.Networking.UnityWebRequest.Post(mlEndpointURL, jsonData))
//                 {
//                     request.SetRequestHeader("Content-Type", "application/json");
//                     request.timeout = 10; // 10 second timeout
                    
//                     yield return request.SendWebRequest();
                    
//                     result.processingTime = Time.time - startTime;
                    
//                     if (request.result == UnityEngine.Networking.UnityWebRequest.Result.Success)
//                     {
//                         result = ParseMLResponse(request.downloadHandler.text);
//                         result.processingTime = Time.time - startTime;
                        
//                         if (!isRealtime)
//                         {
//                             gestureData.recognizedGesture = result.predictedGesture;
//                             gestureData.confidence = result.confidence;
//                         }
                        
//                         LogDebug($"ML prediction: {result.predictedGesture} (confidence: {result.confidence:F2})");
//                     }
//                     else
//                     {
//                         result.isSuccess = false;
//                         result.errorMessage = request.error;
//                         LogDebug($"ML request failed: {request.error}");
//                     }
//                 }
//             }
//             catch (Exception e)
//             {
//                 result.isSuccess = false;
//                 result.errorMessage = e.Message;
//                 LogDebug($"ML processing error: {e.Message}");
//             }
            
//             // Fire ML result events
//             OnMLResult?.Invoke(result);
//             MLResultCallback?.Invoke(result);
//         }
        
//         private string PrepareMLData(GestureData gestureData)
//         {
//             // Create a simplified data structure for ML model
//             var mlData = new
//             {
//                 gesture_id = gestureData.gestureId,
//                 duration = gestureData.duration,
//                 frame_count = gestureData.frames.Count,
//                 frames = gestureData.frames.ConvertAll(frame => new
//                 {
//                     timestamp = frame.timestamp,
//                     head_pos = new float[] { frame.headPosition.x, frame.headPosition.y, frame.headPosition.z },
//                     head_rot = new float[] { frame.headRotation.x, frame.headRotation.y, frame.headRotation.z, frame.headRotation.w },
//                     left_hand_pos = new float[] { frame.leftHandPosition.x, frame.leftHandPosition.y, frame.leftHandPosition.z },
//                     left_hand_rot = new float[] { frame.leftHandRotation.x, frame.leftHandRotation.y, frame.leftHandRotation.z, frame.leftHandRotation.w },
//                     right_hand_pos = new float[] { frame.rightHandPosition.x, frame.rightHandPosition.y, frame.rightHandPosition.z },
//                     right_hand_rot = new float[] { frame.rightHandRotation.x, frame.rightHandRotation.y, frame.rightHandRotation.z, frame.rightHandRotation.w },
//                     left_hand_vel = includeVelocityData ? new float[] { frame.leftHandVelocity.x, frame.leftHandVelocity.y, frame.leftHandVelocity.z } : null,
//                     right_hand_vel = includeVelocityData ? new float[] { frame.rightHandVelocity.x, frame.rightHandVelocity.y, frame.rightHandVelocity.z } : null,
//                     left_trigger = frame.leftTriggerPressed,
//                     right_trigger = frame.rightTriggerPressed,
//                     left_grip = frame.leftGripPressed,
//                     right_grip = frame.rightGripPressed
//                 })
//             };
            
//             return JsonUtility.ToJson(mlData);
//         }
        
//         private MLGestureResult ParseMLResponse(string jsonResponse)
//         {
//             MLGestureResult result = new MLGestureResult();
            
//             try
//             {
//                 // Parse JSON response
//                 var response = JsonUtility.FromJson<MLResponse>(jsonResponse);
                
//                 result.isSuccess = true;
//                 result.predictedGesture = response.predicted_gesture;
//                 result.confidence = response.confidence;
                
//                 // Parse all predictions if available
//                 if (response.all_predictions != null)
//                 {
//                     foreach (var prediction in response.all_predictions)
//                     {
//                         result.allPredictions[prediction.gesture] = prediction.confidence;
//                     }
//                 }
//             }
//             catch (Exception e)
//             {
//                 result.isSuccess = false;
//                 result.errorMessage = $"Failed to parse ML response: {e.Message}";
//             }
            
//             return result;
//         }
        
//         [System.Serializable]
//         private class MLResponse
//         {
//             public string predicted_gesture;
//             public float confidence;
//             public MLPrediction[] all_predictions;
//         }
        
//         [System.Serializable]
//         private class MLPrediction
//         {
//             public string gesture;
//             public float confidence;
//         }
        
//         #endregion
        
//         #region Visualization
        
//         private void UpdateVisualization()
//         {
//             if (debugLineRenderer == null || !isCapturing) return;
            
//             // Visualize recent hand movement
//             if (movementHistory.Count > 1)
//             {
//                 int pointsToShow = Mathf.Min(50, movementHistory.Count);
//                 debugLineRenderer.positionCount = pointsToShow;
                
//                 for (int i = 0; i < pointsToShow; i++)
//                 {
//                     int index = movementHistory.Count - pointsToShow + i;
//                     debugLineRenderer.SetPosition(i, movementHistory[index].rightHandPosition);
//                 }
//             }
//         }
        
//         #endregion
        
//         #region Utility Methods
        
//         private void CleanupMovementCapture()
//         {
//             movementHistory.Clear();
//             currentGestureData.Clear();
//         }
        
//         private void LogDebug(string message)
//         {
//             if (enableDebugLogs)
//             {
//                 Debug.Log($"[XRMovementCapture] {message}");
//             }
//         }
        
//         #endregion
        
//         #region Public API
        
//         public bool IsCapturing => isCapturing;
//         public bool IsGestureActive => isGestureActive;
//         public int MovementHistoryCount => movementHistory.Count;
//         public float CurrentCaptureTime => isCapturing ? Time.time - captureStartTime : 0f;
        
//         public void SetCaptureRate(float rate)
//         {
//             captureRate = Mathf.Clamp(rate, 1f, 120f);
//             LogDebug($"Capture rate set to: {captureRate} Hz");
//         }
        
//         public void SetGestureThresholds(float startThreshold, float endThreshold)
//         {
//             gestureStartThreshold = startThreshold;
//             gestureEndThreshold = endThreshold;
//             LogDebug($"Gesture thresholds set - Start: {startThreshold}, End: {endThreshold}");
//         }
        
//         public void SetMLEndpoint(string url)
//         {
//             mlEndpointURL = url;
//             LogDebug($"ML endpoint set to: {url}");
//         }
        
//         public void ForceEndGesture()
//         {
//             if (isGestureActive)
//             {
//                 EndGestureCapture();
//                 LogDebug("Gesture capture force ended");
//             }
//         }
        
//         public void ClearHistory()
//         {
//             movementHistory.Clear();
//             LogDebug("Movement history cleared");
//         }
        
//         public List<MovementFrame> GetRecentFrames(int count)
//         {
//             if (movementHistory.Count == 0) return new List<MovementFrame>();
            
//             count = Mathf.Min(count, movementHistory.Count);
//             return movementHistory.GetRange(movementHistory.Count - count, count);
//         }
        
//         public GestureData GetLastGesture()
//         {
//             if (currentGestureData.Count == 0) return null;
            
//             return CreateGestureData();
//         }
        
//         public void ExportMovementData(string filePath)
//         {
//             try
//             {
//                 string jsonData = JsonUtility.ToJson(new { movements = movementHistory }, true);
//                 System.IO.File.WriteAllText(filePath, jsonData);
//                 LogDebug($"Movement data exported to: {filePath}");
//             }
//             catch (Exception e)
//             {
//                 LogDebug($"Failed to export movement data: {e.Message}");
//             }
//         }
        
//         #endregion
//     }
// }