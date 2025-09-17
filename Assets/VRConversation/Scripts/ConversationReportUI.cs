// using System;
// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;
// using UnityEngine.UI;
// using TMPro;
// using UnityEngine.Events;

// namespace VRConversation
// {
//     /// <summary>
//     /// Manages the conversation report UI in VR space
//     /// Displays conversation metrics, statistics, and session summary
//     /// </summary>
//     public class ConversationReportUI : MonoBehaviour
//     {
//         [Header("UI Canvas Settings")]
//         [SerializeField] private Canvas reportCanvas;
//         [SerializeField] private bool worldSpaceUI = true;
//         [SerializeField] private float canvasDistance = 2f;
//         [SerializeField] private Vector3 canvasOffset = Vector3.up;
        
//         [Header("Report Sections")]
//         [SerializeField] private GameObject reportPanel;
//         [SerializeField] private GameObject summarySection;
//         [SerializeField] private GameObject metricsSection;
//         [SerializeField] private GameObject dialogueSection;
//         [SerializeField] private GameObject emotionSection;
        
//         [Header("Summary UI Elements")]
//         [SerializeField] private TextMeshProUGUI sessionTitleText;
//         [SerializeField] private TextMeshProUGUI durationText;
//         [SerializeField] private TextMeshProUGUI participantsText;
//         [SerializeField] private TextMeshProUGUI topicText;
        
//         [Header("Metrics UI Elements")]
//         [SerializeField] private TextMeshProUGUI totalTurnsText;
//         [SerializeField] private TextMeshProUGUI userTurnsText;
//         [SerializeField] private TextMeshProUGUI npcTurnsText;
//         [SerializeField] private TextMeshProUGUI avgResponseTimeText;
//         [SerializeField] private TextMeshProUGUI recognitionAccuracyText;
        
//         [Header("Dialogue History")]
//         [SerializeField] private ScrollRect dialogueScrollView;
//         [SerializeField] private Transform dialogueContent;
//         [SerializeField] private GameObject dialogueTurnPrefab;
//         [SerializeField] private int maxDisplayedTurns = 20;
        
//         [Header("Emotion Visualization")]
//         [SerializeField] private Transform emotionGraphParent;
//         [SerializeField] private GameObject emotionBarPrefab;
//         [SerializeField] private Image dominantEmotionIcon;
//         [SerializeField] private TextMeshProUGUI dominantEmotionText;
        
//         [Header("Keywords Section")]
//         [SerializeField] private Transform keywordsParent;
//         [SerializeField] private GameObject keywordTagPrefab;
//         [SerializeField] private int maxKeywordsDisplayed = 10;
        
//         [Header("Interactive Elements")]
//         [SerializeField] private Button closeButton;
//         [SerializeField] private Button exportButton;
//         [SerializeField] private Button restartButton;
//         [SerializeField] private Button shareButton;
        
//         [Header("Visual Feedback")]
//         [SerializeField] private Animator uiAnimator;
//         [SerializeField] private CanvasGroup canvasGroup;
//         [SerializeField] private AudioSource uiAudioSource;
//         [SerializeField] private AudioClip showReportSound;
//         [SerializeField] private AudioClip buttonClickSound;
        
//         [Header("Color Scheme")]
//         [SerializeField] private Color userMessageColor = Color.cyan;
//         [SerializeField] private Color npcMessageColor = Color.green;
//         [SerializeField] private Color accentColor = Color.yellow;
//         [SerializeField] private Color backgroundColor = new Color(0.1f, 0.1f, 0.1f, 0.8f);
        
//         [Header("Debug Settings")]
//         [SerializeField] private bool enableDebugLogs = true;
//         [SerializeField] private bool autoShowOnSessionEnd = true;
        
//         // Events
//         [System.Serializable]
//         public class ReportActionEvent : UnityEvent<string> { }
        
//         public ReportActionEvent OnReportAction;
//         public UnityEvent OnReportClosed;
//         public UnityEvent OnReportExported;
        
//         // Private variables
//         private ConversationData currentConversationData;
//         private Transform vrCamera;
//         private bool isReportVisible = false;
//         private Coroutine animationCoroutine;
//         private List<GameObject> instantiatedElements = new List<GameObject>();
        
//         // Emotion colors mapping
//         private Dictionary<string, Color> emotionColors = new Dictionary<string, Color>
//         {
//             {"happy", Color.yellow},
//             {"sad", Color.blue},
//             {"angry", Color.red},
//             {"excited", Color.magenta},
//             {"calm", Color.green},
//             {"confused", Color.cyan},
//             {"neutral", Color.gray},
//             {"surprised", Color.orange}
//         };
        
//         #region Unity Lifecycle
        
//         private void Awake()
//         {
//             InitializeUI();
//             FindVRCamera();
//         }
        
//         private void Start()
//         {
//             SetupUIInteractions();
//             HideReport();
//         }
        
//         private void Update()
//         {
//             if (isReportVisible && worldSpaceUI && vrCamera != null)
//             {
//                 UpdateCanvasPosition();
//             }
//         }
        
//         #endregion
        
//         #region Initialization
        
//         private void InitializeUI()
//         {
//             // Initialize canvas if not assigned
//             if (reportCanvas == null)
//             {
//                 reportCanvas = GetComponent<Canvas>();
//                 if (reportCanvas == null)
//                 {
//                     reportCanvas = gameObject.AddComponent<Canvas>();
//                     gameObject.AddComponent<CanvasScaler>();
//                     gameObject.AddComponent<GraphicRaycaster>();
//                 }
//             }
            
//             // Configure canvas for VR
//             if (worldSpaceUI)
//             {
//                 reportCanvas.renderMode = RenderMode.WorldSpace;
//                 reportCanvas.worldCamera = Camera.main;
//             }
//             else
//             {
//                 reportCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
//             }
            
//             // Initialize canvas group for fade effects
//             if (canvasGroup == null)
//             {
//                 canvasGroup = GetComponent<CanvasGroup>();
//                 if (canvasGroup == null)
//                 {
//                     canvasGroup = gameObject.AddComponent<CanvasGroup>();
//                 }
//             }
            
//             // Initialize audio source
//             if (uiAudioSource == null)
//             {
//                 uiAudioSource = GetComponent<AudioSource>();
//                 if (uiAudioSource == null)
//                 {
//                     uiAudioSource = gameObject.AddComponent<AudioSource>();
//                 }
//             }
            
//             uiAudioSource.playOnAwake = false;
//             uiAudioSource.spatialBlend = worldSpaceUI ? 1f : 0f;
//         }
        
//         private void FindVRCamera()
//         {
//             // Find VR camera/head transform
//             vrCamera = Camera.main?.transform;
            
//             if (vrCamera == null)
//             {
//                 // Try to find XR rig camera
//                 var xrRig = FindObjectOfType<UnityEngine.XR.Interaction.Toolkit.XRRig>();
//                 if (xrRig != null)
//                 {
//                     vrCamera = xrRig.cameraGameObject?.transform;
//                 }
//             }
            
//             if (vrCamera == null)
//             {
//                 LogDebug("VR Camera not found. UI positioning may not work correctly in VR.");
//             }
//         }
        
//         private void SetupUIInteractions()
//         {
//             // Setup button interactions
//             if (closeButton != null)
//             {
//                 closeButton.onClick.AddListener(() => {
//                     PlayButtonSound();
//                     HideReport();
//                 });
//             }
            
//             if (exportButton != null)
//             {
//                 exportButton.onClick.AddListener(() => {
//                     PlayButtonSound();
//                     ExportReport();
//                 });
//             }
            
//             if (restartButton != null)
//             {
//                 restartButton.onClick.AddListener(() => {
//                     PlayButtonSound();
//                     RestartConversation();
//                 });
//             }
            
//             if (shareButton != null)
//             {
//                 shareButton.onClick.AddListener(() => {
//                     PlayButtonSound();
//                     ShareReport();
//                 });
//             }
//         }
        
//         #endregion
        
//         #region Report Display
        
//         public void ShowReport(ConversationData conversationData)
//         {
//             if (conversationData == null)
//             {
//                 LogDebug("Cannot show report: conversation data is null");
//                 return;
//             }
            
//             currentConversationData = conversationData;
            
//             // Position canvas in VR space
//             if (worldSpaceUI && vrCamera != null)
//             {
//                 PositionCanvasInVR();
//             }
            
//             // Populate report sections
//             PopulateSummarySection();
//             PopulateMetricsSection();
//             PopulateDialogueSection();
//             PopulateEmotionSection();
//             PopulateKeywordsSection();
            
//             // Show the report with animation
//             ShowReportWithAnimation();
            
//             LogDebug("Conversation report displayed");
//         }
        
//         private void PositionCanvasInVR()
//         {
//             if (vrCamera == null) return;
            
//             Vector3 cameraPosition = vrCamera.position;
//             Vector3 cameraForward = vrCamera.forward;
            
//             // Position the canvas in front of the user
//             Vector3 targetPosition = cameraPosition + cameraForward * canvasDistance + canvasOffset;
//             transform.position = targetPosition;
            
//             // Make the canvas face the user
//             Vector3 lookDirection = (cameraPosition - transform.position).normalized;
//             transform.rotation = Quaternion.LookRotation(-lookDirection, Vector3.up);
            
//             // Scale the canvas appropriately for VR
//             transform.localScale = Vector3.one * 0.001f; // Scale down for world space
//         }
        
//         private void UpdateCanvasPosition()
//         {
//             // Optionally update canvas position to follow user (but not too aggressively)
//             if (Vector3.Distance(transform.position, vrCamera.position) > canvasDistance * 2f)
//             {
//                 PositionCanvasInVR();
//             }
//         }
        
//         #endregion
        
//         #region Report Population
        
//         private void PopulateSummarySection()
//         {
//             if (currentConversationData == null) return;
            
//             // Session title
//             if (sessionTitleText != null)
//             {
//                 sessionTitleText.text = $"Conversation Report - {currentConversationData.sessionId.Substring(0, 8)}";
//             }
            
//             // Duration
//             if (durationText != null)
//             {
//                 TimeSpan duration = TimeSpan.FromSeconds(currentConversationData.totalDuration);
//                 durationText.text = $"Duration: {duration.Minutes:D2}:{duration.Seconds:D2}";
//             }
            
//             // Participants
//             if (participantsText != null)
//             {
//                 participantsText.text = $"Participants: {currentConversationData.userName} & {currentConversationData.npcCharacterId}";
//             }
            
//             // Topic
//             if (topicText != null)
//             {
//                 topicText.text = $"Topic: {currentConversationData.conversationTopic}";
//             }
//         }
        
//         private void PopulateMetricsSection()
//         {
//             if (currentConversationData == null) return;
            
//             // Total turns
//             if (totalTurnsText != null)
//             {
//                 totalTurnsText.text = $"{currentConversationData.dialogueTurns.Count}";
//             }
            
//             // User turns
//             if (userTurnsText != null)
//             {
//                 userTurnsText.text = $"{currentConversationData.totalUserTurns}";
//             }
            
//             // NPC turns
//             if (npcTurnsText != null)
//             {
//                 npcTurnsText.text = $"{currentConversationData.totalNPCTurns}";
//             }
            
//             // Average response time
//             if (avgResponseTimeText != null)
//             {
//                 avgResponseTimeText.text = $"{currentConversationData.averageUserResponseTime:F1}s";
//             }
            
//             // Recognition accuracy
//             if (recognitionAccuracyText != null)
//             {
//                 recognitionAccuracyText.text = $"{currentConversationData.recognitionAccuracy * 100:F1}%";
//             }
//         }
        
//         private void PopulateDialogueSection()
//         {
//             if (currentConversationData == null || dialogueContent == null) return;
            
//             // Clear existing dialogue items
//             ClearInstantiatedElements();
            
//             // Get recent dialogue turns
//             int startIndex = Mathf.Max(0, currentConversationData.dialogueTurns.Count - maxDisplayedTurns);
            
//             for (int i = startIndex; i < currentConversationData.dialogueTurns.Count; i++)
//             {
//                 DialogueTurn turn = currentConversationData.dialogueTurns[i];
//                 CreateDialogueTurnUI(turn);
//             }
            
//             // Scroll to bottom
//             if (dialogueScrollView != null)
//             {
//                 StartCoroutine(ScrollToBottom());
//             }
//         }
        
//         private void CreateDialogueTurnUI(DialogueTurn turn)
//         {
//             if (dialogueTurnPrefab == null || dialogueContent == null) return;
            
//             GameObject turnObject = Instantiate(dialogueTurnPrefab, dialogueContent);
//             instantiatedElements.Add(turnObject);
            
//             // Configure the dialogue turn UI
//             var turnUI = turnObject.GetComponent<DialogueTurnUI>();
//             if (turnUI != null)
//             {
//                 turnUI.SetupTurn(turn, turn.speaker == DialogueTurn.SpeakerType.User ? userMessageColor : npcMessageColor);
//             }
//             else
//             {
//                 // Fallback text setup
//                 var textComponent = turnObject.GetComponentInChildren<TextMeshProUGUI>();
//                 if (textComponent != null)
//                 {
//                     textComponent.text = $"{turn.speaker}: {turn.text}";
//                     textComponent.color = turn.speaker == DialogueTurn.SpeakerType.User ? userMessageColor : npcMessageColor;
//                 }
//             }
//         }
        
//         private void PopulateEmotionSection()
//         {
//             if (currentConversationData == null) return;
            
//             // Set dominant emotion
//             if (dominantEmotionText != null)
//             {
//                 dominantEmotionText.text = currentConversationData.dominantEmotion;
//             }
            
//             if (dominantEmotionIcon != null && emotionColors.ContainsKey(currentConversationData.dominantEmotion))
//             {
//                 dominantEmotionIcon.color = emotionColors[currentConversationData.dominantEmotion];
//             }
            
//             // Create emotion flow visualization
//             CreateEmotionGraph();
//         }
        
//         private void CreateEmotionGraph()
//         {
//             if (emotionGraphParent == null || emotionBarPrefab == null) return;
            
//             // Clear existing emotion bars
//             foreach (Transform child in emotionGraphParent)
//             {
//                 if (instantiatedElements.Contains(child.gameObject))
//                 {
//                     continue;
//                 }
//                 DestroyImmediate(child.gameObject);
//             }
            
//             // Count emotions
//             Dictionary<string, int> emotionCounts = new Dictionary<string, int>();
            
//             foreach (var emotionData in currentConversationData.emotionalFlow)
//             {
//                 if (emotionCounts.ContainsKey(emotionData.emotion))
//                 {
//                     emotionCounts[emotionData.emotion]++;
//                 }
//                 else
//                 {
//                     emotionCounts[emotionData.emotion] = 1;
//                 }
//             }
            
//             // Create bars for each emotion
//             int maxCount = 0;
//             foreach (var count in emotionCounts.Values)
//             {
//                 if (count > maxCount) maxCount = count;
//             }
            
//             foreach (var emotionPair in emotionCounts)
//             {
//                 GameObject barObject = Instantiate(emotionBarPrefab, emotionGraphParent);
//                 instantiatedElements.Add(barObject);
                
//                 // Configure emotion bar
//                 var barUI = barObject.GetComponent<EmotionBarUI>();
//                 if (barUI != null)
//                 {
//                     float normalizedHeight = (float)emotionPair.Value / maxCount;
//                     Color barColor = emotionColors.ContainsKey(emotionPair.Key) ? emotionColors[emotionPair.Key] : Color.gray;
//                     barUI.SetupBar(emotionPair.Key, normalizedHeight, barColor);
//                 }
//             }
//         }
        
//         private void PopulateKeywordsSection()
//         {
//             if (currentConversationData == null || keywordsParent == null || keywordTagPrefab == null) return;
            
//             // Clear existing keyword tags
//             foreach (Transform child in keywordsParent)
//             {
//                 if (instantiatedElements.Contains(child.gameObject))
//                 {
//                     continue;
//                 }
//                 DestroyImmediate(child.gameObject);
//             }
            
//             // Create keyword tags
//             int keywordCount = 0;
//             foreach (string keyword in currentConversationData.detectedKeywords)
//             {
//                 if (keywordCount >= maxKeywordsDisplayed) break;
                
//                 GameObject tagObject = Instantiate(keywordTagPrefab, keywordsParent);
//                 instantiatedElements.Add(tagObject);
                
//                 var tagText = tagObject.GetComponentInChildren<TextMeshProUGUI>();
//                 if (tagText != null)
//                 {
//                     tagText.text = keyword;
//                 }
                
//                 keywordCount++;
//             }
//         }
        
//         #endregion
        
//         #region Animation and Effects
        
//         private void ShowReportWithAnimation()
//         {
//             if (animationCoroutine != null)
//             {
//                 StopCoroutine(animationCoroutine);
//             }
            
//             animationCoroutine = StartCoroutine(ShowReportCoroutine());
//         }
        
//         private IEnumerator ShowReportCoroutine()
//         {
//             isReportVisible = true;
//             reportPanel.SetActive(true);
            
//             // Play show sound
//             if (showReportSound != null && uiAudioSource != null)
//             {
//                 uiAudioSource.PlayOneShot(showReportSound);
//             }
            
//             // Fade in animation
//             if (canvasGroup != null)
//             {
//                 canvasGroup.alpha = 0f;
//                 canvasGroup.interactable = false;
                
//                 float elapsed = 0f;
//                 float duration = 0.5f;
                
//                 while (elapsed < duration)
//                 {
//                     elapsed += Time.deltaTime;
//                     canvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsed / duration);
//                     yield return null;
//                 }
                
//                 canvasGroup.alpha = 1f;
//                 canvasGroup.interactable = true;
//             }
            
//             // Scale in animation
//             if (uiAnimator != null)
//             {
//                 uiAnimator.SetTrigger("ShowReport");
//             }
//         }
        
//         public void HideReport()
//         {
//             if (animationCoroutine != null)
//             {
//                 StopCoroutine(animationCoroutine);
//             }
            
//             animationCoroutine = StartCoroutine(HideReportCoroutine());
//         }
        
//         private IEnumerator HideReportCoroutine()
//         {
//             // Fade out animation
//             if (canvasGroup != null)
//             {
//                 canvasGroup.interactable = false;
                
//                 float elapsed = 0f;
//                 float duration = 0.3f;
                
//                 while (elapsed < duration)
//                 {
//                     elapsed += Time.deltaTime;
//                     canvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsed / duration);
//                     yield return null;
//                 }
                
//                 canvasGroup.alpha = 0f;
//             }
            
//             reportPanel.SetActive(false);
//             isReportVisible = false;
            
//             OnReportClosed?.Invoke();
//         }
        
//         private IEnumerator ScrollToBottom()
//         {
//             yield return new WaitForEndOfFrame();
//             if (dialogueScrollView != null)
//             {
//                 dialogueScrollView.verticalNormalizedPosition = 0f;
//             }
//         }
        
//         #endregion
        
//         #region Button Actions
        
//         private void ExportReport()
//         {
//             if (currentConversationData == null) return;
            
//             try
//             {
//                 // TODO: Implement report export (JSON, CSV, etc.)
//                 string reportJson = JsonUtility.ToJson(currentConversationData, true);
//                 LogDebug($"Report exported: {reportJson.Length} characters");
                
//                 OnReportExported?.Invoke();
//                 OnReportAction?.Invoke("export");
//             }
//             catch (Exception e)
//             {
//                 LogDebug($"Failed to export report: {e.Message}");
//             }
//         }
        
//         private void RestartConversation()
//         {
//             OnReportAction?.Invoke("restart");
//             HideReport();
//         }
        
//         private void ShareReport()
//         {
//             // TODO: Implement sharing functionality
//             OnReportAction?.Invoke("share");
//             LogDebug("Share functionality not implemented yet");
//         }
        
//         private void PlayButtonSound()
//         {
//             if (buttonClickSound != null && uiAudioSource != null)
//             {
//                 uiAudioSource.PlayOneShot(buttonClickSound);
//             }
//         }
        
//         #endregion
        
//         #region Utility Methods
        
//         private void ClearInstantiatedElements()
//         {
//             foreach (GameObject element in instantiatedElements)
//             {
//                 if (element != null)
//                 {
//                     DestroyImmediate(element);
//                 }
//             }
//             instantiatedElements.Clear();
//         }
        
//         private void LogDebug(string message)
//         {
//             if (enableDebugLogs)
//             {
//                 Debug.Log($"[ConversationReportUI] {message}");
//             }
//         }
        
//         #endregion
        
//         #region Public API
        
//         public bool IsReportVisible => isReportVisible;
        
//         public void SetAutoShow(bool autoShow)
//         {
//             autoShowOnSessionEnd = autoShow;
//         }
        
//         public void SetWorldSpaceMode(bool worldSpace)
//         {
//             worldSpaceUI = worldSpace;
            
//             if (reportCanvas != null)
//             {
//                 reportCanvas.renderMode = worldSpace ? RenderMode.WorldSpace : RenderMode.ScreenSpaceOverlay;
//             }
//         }
        
//         public void UpdateReport(ConversationData conversationData)
//         {
//             if (isReportVisible)
//             {
//                 ShowReport(conversationData);
//             }
//         }
        
//         #endregion
//     }
    
//     /// <summary>
//     /// Helper component for individual dialogue turn UI elements
//     /// </summary>
//     public class DialogueTurnUI : MonoBehaviour
//     {
//         [SerializeField] private TextMeshProUGUI speakerText;
//         [SerializeField] private TextMeshProUGUI messageText;
//         [SerializeField] private TextMeshProUGUI timestampText;
//         [SerializeField] private Image backgroundImage;
        
//         public void SetupTurn(DialogueTurn turn, Color backgroundColor)
//         {
//             if (speakerText != null)
//             {
//                 speakerText.text = turn.speaker.ToString();
//             }
            
//             if (messageText != null)
//             {
//                 messageText.text = turn.text;
//             }
            
//             if (timestampText != null)
//             {
//                 TimeSpan time = TimeSpan.FromSeconds(turn.timestamp);
//                 timestampText.text = time.ToString(@"mm\:ss");
//             }
            
//             if (backgroundImage != null)
//             {
//                 backgroundImage.color = backgroundColor;
//             }
//         }
//     }
    
//     /// <summary>
//     /// Helper component for emotion bar visualization
//     /// </summary>
//     public class EmotionBarUI : MonoBehaviour
//     {
//         [SerializeField] private Image barFill;
//         [SerializeField] private TextMeshProUGUI emotionLabel;
//         [SerializeField] private RectTransform barRect;
        
//         public void SetupBar(string emotion, float normalizedHeight, Color barColor)
//         {
//             if (emotionLabel != null)
//             {
//                 emotionLabel.text = emotion;
//             }
            
//             if (barFill != null)
//             {
//                 barFill.color = barColor;
//                 barFill.fillAmount = normalizedHeight;
//             }
            
//             if (barRect != null)
//             {
//                 Vector2 sizeDelta = barRect.sizeDelta;
//                 sizeDelta.y = normalizedHeight * 100f; // Adjust scale as needed
//                 barRect.sizeDelta = sizeDelta;
//             }
//         }
//     }
// }