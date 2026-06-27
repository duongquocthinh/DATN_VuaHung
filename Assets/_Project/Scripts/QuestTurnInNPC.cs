using System.Collections;
using UnityEngine;
using TMPro;

public class QuestTurnInNPC : MonoBehaviour, IInteractable
{
    [SerializeField] private string npcName = "Vua Hung";
    [SerializeField] private string requiredFirstItem = "Banh Chung";
    [SerializeField] private int requiredFirstAmount = 1;
    [SerializeField] private string requiredSecondItem = "Banh Giay";
    [SerializeField] private int requiredSecondAmount = 1;
    [SerializeField] private bool removeItemsOnComplete = true;

    [Header("Quest UI")]
    [SerializeField] private string questPanelTitle = "Vua Hung";
    [SerializeField] private bool useLargeQuestPanel;
    [TextArea(2, 4)]
    [SerializeField] private string simpleTurnInPrompt = "Nhấn E để dâng bánh";
    [TextArea(2, 4)]
    [SerializeField] private string completedQuestPanelMessage;

    [Header("Offering Cutscene")]
    [SerializeField] private bool showOfferingCutscene = true;
    [SerializeField] private Camera offeringCamera;
    [SerializeField] private Transform offeringProp;
    [SerializeField] private Vector3 offeringStartLocalPosition = new Vector3(0f, -0.45f, 0.85f);
    [SerializeField] private Vector3 offeringEndLocalPosition = new Vector3(0f, -0.05f, 0.75f);
    [SerializeField] private Vector3 offeringLocalEuler = new Vector3(18f, 0f, 0f);
    [SerializeField] private float offeringMoveDuration = 1.6f;
    [SerializeField] private float offeringHoldDuration = 1.2f;

    [Header("Ending Story")]
    [SerializeField] private bool showEndingStory = true;
    [SerializeField] private float endingLineDuration = 4f;
    [SerializeField] private AudioSource endingVoiceSource;
    [SerializeField] private AudioClip endingVoiceClip;
    [TextArea(2, 5)]
    [SerializeField] private string[] endingStoryLines;
    [Header("End Game")]
    [SerializeField] private bool quitGameAfterEndingStory = true;
    [SerializeField] private float quitDelayAfterEndingStory = 2.0f;

    private bool questCompleted;
    private bool showingEndingStory;
    private string currentEndingText;
    private GUIStyle endingTextStyle;

    public bool IsQuestCompleted { get { return questCompleted; } }

    public string QuestPanelTitle { get { return questPanelTitle; } }
    public bool UseLargeQuestPanel { get { return useLargeQuestPanel; } }
    public TextAlignmentOptions QuestPanelAlignment
    {
        get
        {
            return questCompleted
                ? TextAlignmentOptions.Center
                : TextAlignmentOptions.MidlineLeft;
        }
    }

    public string GetInteractionText()
    {
        return npcName;
    }

    public string GetQuestPanelText()
    {
        if (questCompleted)
        {
            if (!string.IsNullOrWhiteSpace(completedQuestPanelMessage))
            {
                return completedQuestPanelMessage;
            }

            return "Nhiệm vụ đã hoàn thành.\nCảm ơn bạn đã dâng bánh lên Vua Hùng.";
        }

        if (!string.IsNullOrWhiteSpace(simpleTurnInPrompt))
        {
            return simpleTurnInPrompt;
        }

        return "Nhấn E để dâng bánh";
    }

    public void Interact()
    {
        if (questCompleted)
        {
            NotificationUI.ShowMessage("Nhiệm vụ đã hoàn thành. Lang Liêu đã dâng bánh cho Vua Hùng.", 4f);
            return;
        }

        if (InventorySystem.Instance == null)
        {
            NotificationUI.ShowMessage("Không tìm thấy InventorySystem.");
            return;
        }

        bool hasFirstItem = InventorySystem.Instance.HasItem(requiredFirstItem, requiredFirstAmount);
        bool hasSecondItem = InventorySystem.Instance.HasItem(requiredSecondItem, requiredSecondAmount);

        if (!hasFirstItem || !hasSecondItem)
        {
            NotificationUI.ShowMessage(
                "Cần " + requiredFirstItem + " x" + requiredFirstAmount
                + " và " + requiredSecondItem + " x" + requiredSecondAmount
                + " để dâng lên Vua Hùng.",
                4f
            );
            return;
        }

        if (removeItemsOnComplete)
        {
            InventorySystem.Instance.RemoveItem(requiredFirstItem, requiredFirstAmount);
            InventorySystem.Instance.RemoveItem(requiredSecondItem, requiredSecondAmount);
        }

        questCompleted = true;
        NotificationUI.ShowMessage("Hoàn thành! Vua Hùng đã nhận lễ vật của Lang Liêu.", 5f);

        if (showOfferingCutscene || showEndingStory)
        {
            StartCoroutine(CompleteEndingRoutine());
        }
    }

    private IEnumerator CompleteEndingRoutine()
    {
        yield return StartCoroutine(ShowOfferingRoutine());

        if (showEndingStory)
        {
            yield return StartCoroutine(ShowEndingStoryRoutine());
        }
    }

    private IEnumerator ShowOfferingRoutine()
    {
        if (!showOfferingCutscene || offeringProp == null)
        {
            yield break;
        }

        if (offeringCamera == null)
        {
            offeringCamera = Camera.main;
        }

        if (offeringCamera == null)
        {
            yield break;
        }

        Transform originalParent = offeringProp.parent;
        Vector3 originalLocalPosition = offeringProp.localPosition;
        Quaternion originalLocalRotation = offeringProp.localRotation;
        bool wasActive = offeringProp.gameObject.activeSelf;

        offeringProp.SetParent(offeringCamera.transform);
        offeringProp.gameObject.SetActive(true);
        offeringProp.localPosition = offeringStartLocalPosition;
        offeringProp.localRotation = Quaternion.Euler(offeringLocalEuler);

        float elapsed = 0f;
        float duration = Mathf.Max(0.1f, offeringMoveDuration);

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            offeringProp.localPosition = Vector3.Lerp(offeringStartLocalPosition, offeringEndLocalPosition, t);
            yield return null;
        }

        offeringProp.localPosition = offeringEndLocalPosition;
        yield return new WaitForSeconds(Mathf.Max(0f, offeringHoldDuration));

        offeringProp.gameObject.SetActive(wasActive);
        offeringProp.SetParent(originalParent);
        offeringProp.localPosition = originalLocalPosition;
        offeringProp.localRotation = originalLocalRotation;
    }

    private IEnumerator ShowEndingStoryRoutine()
    {
        string[] linesToShow = endingStoryLines;
        if (linesToShow == null || linesToShow.Length == 0)
        {
            linesToShow = new[]
            {
                "Vua Hùng rất hài lòng với lễ vật của Lang Liêu.",
                "Bánh Chưng tượng trưng cho đất, nơi gói trọn sản vật của đồng ruộng.",
                "Bánh Giầy tượng trưng cho trời, thể hiện lòng biết ơn tổ tiên và trời đất.",
                "Từ đó, Bánh Chưng và Bánh Giầy trở thành biểu tượng tốt đẹp của dân tộc."
            };
        }

        showingEndingStory = true;

        float endingVoiceDuration = 0f;
        if (endingVoiceSource != null && endingVoiceClip != null)
        {
            endingVoiceSource.Stop();
            endingVoiceSource.clip = endingVoiceClip;
            endingVoiceSource.Play();
            endingVoiceDuration = endingVoiceClip.length + 0.3f;
        }

        for (int i = 0; i < linesToShow.Length; i++)
        {
            if (!string.IsNullOrWhiteSpace(linesToShow[i]))
            {
                currentEndingText = linesToShow[i];
                float waitTime = endingLineDuration;
                if (linesToShow.Length == 1 && endingVoiceDuration > 0f)
                {
                    waitTime = Mathf.Max(waitTime, endingVoiceDuration);
                }

                yield return new WaitForSeconds(Mathf.Max(1f, waitTime));
            }
        }

        currentEndingText = "";
        showingEndingStory = false;

        if (quitGameAfterEndingStory)
        {
            yield return new WaitForSeconds(Mathf.Max(0f, quitDelayAfterEndingStory));
            EndGame();
        }
    }

    private void OnGUI()
    {
        if (!showingEndingStory)
        {
            return;
        }

        GUI.color = Color.black;
        GUI.DrawTexture(new Rect(0f, 0f, Screen.width, Screen.height), Texture2D.whiteTexture);
        GUI.color = Color.white;

        if (endingTextStyle == null)
        {
            endingTextStyle = new GUIStyle(GUI.skin.label);
            endingTextStyle.alignment = TextAnchor.MiddleCenter;
            endingTextStyle.wordWrap = true;
            endingTextStyle.fontStyle = FontStyle.Bold;
        }

        endingTextStyle.fontSize = Mathf.Clamp(Screen.height / 30, 22, 34);

        Rect textRect = new Rect(
            Screen.width * 0.14f,
            Screen.height * 0.24f,
            Screen.width * 0.72f,
            Screen.height * 0.52f
        );

        GUI.Label(textRect, currentEndingText, endingTextStyle);
    }

    private void EndGame()
    {
    #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
    #else
    Application.Quit();
    #endif
    }
}
