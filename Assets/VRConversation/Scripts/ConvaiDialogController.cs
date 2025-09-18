using System.Collections.Generic;
using UnityEngine;
using Convai.Scripts.Runtime.Core;

public class ConvaiDialogController : MonoBehaviour
{
    [Header("Dialog Settings")]
    [Tooltip("List of dialogs to be played in sequence")]
    public List<string> dialogList = new List<string>();
    
    [Header("Convai Character Reference")]
    [Tooltip("Reference to the Convai character that will speak the dialogs")]
    public ConvaiNPC convaiCharacter;
    
    [Header("Dialog Control")]
    [Tooltip("Current dialog index")]
    [SerializeField] private int currentDialogIndex = 0;
    
    [Tooltip("Auto-play next dialog after current one finishes")]
    public bool autoPlayNext = false;
    
    [Tooltip("Delay between dialogs when auto-playing (in seconds)")]
    public float delayBetweenDialogs = 1f;

    void Start()
    {
        // Find ConvaiNPC if not assigned
        if (convaiCharacter == null)
        {
            convaiCharacter = GetComponent<ConvaiNPC>();
            if (convaiCharacter == null)
            {
                // Try to find in parent objects
                convaiCharacter = GetComponentInParent<ConvaiNPC>();
                if (convaiCharacter == null)
                {
                    // Try to find in children
                    convaiCharacter = GetComponentInChildren<ConvaiNPC>();
                    if (convaiCharacter == null)
                    {
                        Debug.LogError("ConvaiNPC component not found on this GameObject, its parents, or children. Please assign it manually or add ConvaiNPC component.");
                    }
                    else
                    {
                        Debug.Log("ConvaiNPC found in children and assigned automatically.");
                    }
                }
                else
                {
                    Debug.Log("ConvaiNPC found in parent and assigned automatically.");
                }
            }
            else
            {
                Debug.Log("ConvaiNPC found on same GameObject and assigned automatically.");
            }
        }
        
        // Validate the ConvaiNPC setup
        if (convaiCharacter != null)
        {
            Debug.Log($"ConvaiDialogController initialized with character: {convaiCharacter.characterName}");
            // Check if the character has proper setup
            if (string.IsNullOrEmpty(convaiCharacter.characterID))
            {
                Debug.LogWarning("ConvaiNPC character ID is empty. Make sure to configure the character properly.");
            }
        }
        
        // Reset dialog index
        currentDialogIndex = 0;
    }

    /// <summary>
    /// Play the next dialog in the sequence
    /// </summary>
    [ContextMenu("Play Next Dialog")]
    public void PlayNextDialog()
    {
        if (dialogList == null || dialogList.Count == 0)
        {
            Debug.LogWarning("Dialog list is empty!");
            return;
        }

        if (convaiCharacter == null)
        {
            Debug.LogError("ConvaiNPC reference is missing!");
            return;
        }

        if (currentDialogIndex >= dialogList.Count)
        {
            Debug.Log("All dialogs have been played!");
            return;
        }

        // Get the current dialog text
        string currentDialog = dialogList[currentDialogIndex];
        
        if (string.IsNullOrEmpty(currentDialog))
        {
            Debug.LogWarning($"Dialog at index {currentDialogIndex} is empty, skipping...");
            currentDialogIndex++;
            PlayNextDialog(); // Recursively try next dialog
            return;
        }

        // Send the dialog to Convai character
        Debug.Log($"Playing dialog {currentDialogIndex + 1}/{dialogList.Count}: {currentDialog}");
        convaiCharacter.SendTextDataAsync(currentDialog);
        
        // Move to next dialog
        currentDialogIndex++;
        
        // Auto-play next dialog if enabled
        if (autoPlayNext && currentDialogIndex < dialogList.Count)
        {
            Invoke(nameof(PlayNextDialog), delayBetweenDialogs);
        }
    }

    /// <summary>
    /// Play a specific dialog by index
    /// </summary>
    /// <param name="index">Index of the dialog to play</param>
    public void PlayDialogAtIndex(int index)
    {
        if (index < 0 || index >= dialogList.Count)
        {
            Debug.LogError($"Dialog index {index} is out of range!");
            return;
        }
        
        currentDialogIndex = index;
        PlayNextDialog();
    }

    /// <summary>
    /// Reset dialog sequence to beginning
    /// </summary>
    public void ResetDialogSequence()
    {
        currentDialogIndex = 0;
        Debug.Log("Dialog sequence reset to beginning");
    }

    /// <summary>
    /// Get the current dialog index
    /// </summary>
    /// <returns>Current dialog index</returns>
    public int GetCurrentDialogIndex()
    {
        return currentDialogIndex;
    }

    /// <summary>
    /// Get total number of dialogs
    /// </summary>
    /// <returns>Total dialog count</returns>
    public int GetTotalDialogCount()
    {
        return dialogList?.Count ?? 0;
    }

    /// <summary>
    /// Check if there are more dialogs to play
    /// </summary>
    /// <returns>True if more dialogs are available</returns>
    public bool HasMoreDialogs()
    {
        return currentDialogIndex < dialogList.Count;
    }

    /// <summary>
    /// Debug method to check ConvaiNPC state and configuration
    /// </summary>
    [ContextMenu("Debug ConvaiNPC State")]
    public void DebugConvaiNPCState()
    {
        if (convaiCharacter == null)
        {
            Debug.LogError("ConvaiNPC reference is null!");
            return;
        }

        Debug.Log("=== ConvaiNPC Debug Info ===");
        Debug.Log($"Character Name: {convaiCharacter.characterName}");
        Debug.Log($"Character ID: {convaiCharacter.characterID}");
        Debug.Log($"Session ID: {convaiCharacter.sessionID}");
        Debug.Log($"Is Active: {convaiCharacter.gameObject.activeInHierarchy}");
        Debug.Log($"GameObject Name: {convaiCharacter.gameObject.name}");
        
        // Check if required components are present
        if (convaiCharacter.convaiLipSync == null)
            Debug.LogWarning("ConvaiLipSync component is missing!");
        
        if (convaiCharacter.actionsHandler == null)
            Debug.LogWarning("ActionsHandler component is missing!");
            
        Debug.Log("=== End Debug Info ===");
    }
}