using System.Collections;
using UnityEngine;
using TMPro;

public class QuestTurnInNPC : MonoBehaviour, IInteractable
{
    [SerializeField] private string npcName = "Dang banh";
    [SerializeField] private string requiredFirstItem = "Banh Chung";
    [SerializeField] private int requiredFirstAmount = 1;
    [SerializeField] private string requiredSecondItem = "Banh Giay";
    [SerializeField] private int requiredSecondAmount = 1;
    [SerializeField] private bool removeItemsOnComplete = true;

    [Header("Quest UI")]
    [SerializeField] private string questPanelTitle = "Dang banh";
    [SerializeField] private bool useLargeQuestPanel;
    [TextArea(2, 4)]
    [SerializeField] private string simpleTurnInPrompt = "Nhấn E để dâng bánh";
    [TextArea(2, 4)]
    [SerializeField] private string completedQuestPanelMessage;

    [Header("Offering Cutscene")]
    [SerializeField] private bool showOfferingCutscene = true;
    [SerializeField] private Camera offeringCamera;
    [SerializeField] private Transform ceremonyCameraPoint;
    [SerializeField] private Transform ceremonyLookAtPoint;
    [SerializeField] private Transform playerOfferingPoint;
    [SerializeField] private GameObject playerObject;
    [SerializeField] private MonoBehaviour[] playerControlScripts;
    [SerializeField] private CharacterController playerController;
    [SerializeField] private bool movePlayerToOfferingPoint = true;
    [SerializeField] private bool requireOfferAnimationToMovePlayer = true;
    [SerializeField] private bool hidePlayerRenderersDuringOffering = true;
    [SerializeField] private Animator offererAnimator;
    [SerializeField] private string offerTriggerName = "Offer";
    [SerializeField] private Transform offeringProp;
    [SerializeField] private Vector3 offeringStartLocalPosition = new Vector3(0f, -0.45f, 0.85f);
    [SerializeField] private Vector3 offeringEndLocalPosition = new Vector3(0f, -0.05f, 0.75f);
    [SerializeField] private Vector3 offeringLocalEuler = new Vector3(18f, 0f, 0f);
    [SerializeField] private float cameraMoveDuration = 1.6f;
    [SerializeField] private float ceremonyBeforeOfferDelay = 0.8f;
    [SerializeField] private float offeringMoveDuration = 1.6f;
    [SerializeField] private float offeringHoldDuration = 1.2f;

    [Header("Ending Story")]
    [SerializeField] private bool showEndingStory = true;
    [SerializeField] private float endingLineDuration = 4f;
    [SerializeField] private AudioSource endingVoiceSource;
    [SerializeField] private AudioClip endingVoiceClip;
    [SerializeField] private AudioClip[] endingVoiceClips;
    [TextArea(2, 5)]
    [SerializeField] private string[] endingStoryLines;
    [SerializeField] private bool closeInventoryUIWhenEndingStarts = true;
    [Header("End Game")]
    [SerializeField] private bool quitGameAfterEndingStory = true;
    [SerializeField] private float quitDelayAfterEndingStory = 2.0f;

    private bool questCompleted;
    private bool showingEndingStory;
    private string currentEndingText;
    private GUIStyle endingTextStyle;
    private Vector3 originalCameraPosition;
    private Quaternion originalCameraRotation;
    private bool hasOriginalCameraTransform;
    private Renderer[] hiddenPlayerRenderers;
    private bool[] hiddenPlayerRendererStates;

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
        SetPlayerControls(false);
        CloseInventoryUIIfNeeded();
        HidePlayerRenderersIfNeeded();
        yield return StartCoroutine(ShowOfferingRoutine());

        if (showEndingStory)
        {
            yield return StartCoroutine(ShowEndingStoryRoutine());
        }

        if (!quitGameAfterEndingStory)
        {
            RestorePlayerRenderersIfNeeded();
            RestoreOfferingCamera();
            SetPlayerControls(true);
        }
    }

    private IEnumerator ShowOfferingRoutine()
    {
        if (!showOfferingCutscene)
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

        SaveOfferingCamera();
        bool canPlayOfferAnimation = CanPlayOfferAnimation();
        if (ShouldMovePlayerForOffering(canPlayOfferAnimation))
        {
            MovePlayerToOfferingPoint();
        }

        if (canPlayOfferAnimation)
        {
            PlayOfferAnimation();
        }

        yield return StartCoroutine(MoveOfferingCameraToCeremonyPoint());
        yield return new WaitForSeconds(Mathf.Max(0f, ceremonyBeforeOfferDelay));

        if (offeringProp == null)
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

    private void SaveOfferingCamera()
    {
        if (offeringCamera == null || hasOriginalCameraTransform)
        {
            return;
        }

        originalCameraPosition = offeringCamera.transform.position;
        originalCameraRotation = offeringCamera.transform.rotation;
        hasOriginalCameraTransform = true;
    }

    private void RestoreOfferingCamera()
    {
        if (offeringCamera == null || !hasOriginalCameraTransform)
        {
            return;
        }

        offeringCamera.transform.position = originalCameraPosition;
        offeringCamera.transform.rotation = originalCameraRotation;
    }

    private IEnumerator MoveOfferingCameraToCeremonyPoint()
    {
        if (offeringCamera == null || ceremonyCameraPoint == null)
        {
            yield break;
        }

        Transform cameraTransform = offeringCamera.transform;
        Vector3 startPosition = cameraTransform.position;
        Quaternion startRotation = cameraTransform.rotation;
        Quaternion targetRotation = GetCeremonyCameraRotation();
        float elapsed = 0f;
        float duration = Mathf.Max(0.05f, cameraMoveDuration);

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            cameraTransform.position = Vector3.Lerp(startPosition, ceremonyCameraPoint.position, t);
            cameraTransform.rotation = Quaternion.Slerp(startRotation, targetRotation, t);
            yield return null;
        }

        cameraTransform.position = ceremonyCameraPoint.position;
        cameraTransform.rotation = targetRotation;
    }

    private Quaternion GetCeremonyCameraRotation()
    {
        if (ceremonyLookAtPoint != null && ceremonyCameraPoint != null)
        {
            Vector3 direction = ceremonyLookAtPoint.position - ceremonyCameraPoint.position;
            if (direction.sqrMagnitude > 0.001f)
            {
                return Quaternion.LookRotation(direction.normalized, Vector3.up);
            }
        }

        return ceremonyCameraPoint != null ? ceremonyCameraPoint.rotation : offeringCamera.transform.rotation;
    }

    private void MovePlayerToOfferingPoint()
    {
        if (playerOfferingPoint == null)
        {
            return;
        }

        FindPlayerControlsIfNeeded();

        if (playerObject == null)
        {
            return;
        }

        bool controllerWasEnabled = playerController != null && playerController.enabled;
        if (playerController != null)
        {
            playerController.enabled = false;
        }

        playerObject.transform.position = playerOfferingPoint.position;
        playerObject.transform.rotation = playerOfferingPoint.rotation;

        if (playerController != null)
        {
            playerController.enabled = controllerWasEnabled;
        }
    }

    private void PlayOfferAnimation()
    {
        offererAnimator.SetTrigger(offerTriggerName);
    }

    private bool CanPlayOfferAnimation()
    {
        return offererAnimator != null && !string.IsNullOrWhiteSpace(offerTriggerName);
    }

    private bool ShouldMovePlayerForOffering(bool canPlayOfferAnimation)
    {
        if (!movePlayerToOfferingPoint)
        {
            return false;
        }

        return canPlayOfferAnimation || !requireOfferAnimationToMovePlayer;
    }

    private void SetPlayerControls(bool enabled)
    {
        FindPlayerControlsIfNeeded();

        if (playerControlScripts != null)
        {
            for (int i = 0; i < playerControlScripts.Length; i++)
            {
                if (playerControlScripts[i] != null)
                {
                    playerControlScripts[i].enabled = enabled;
                }
            }
        }

        if (playerController != null)
        {
            playerController.enabled = enabled;
        }

        Cursor.lockState = enabled ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !enabled;
    }

    private void FindPlayerControlsIfNeeded()
    {
        if (playerObject == null)
        {
            playerObject = GameObject.FindGameObjectWithTag("Player");
        }

        if (playerObject == null)
        {
            playerObject = GameObject.Find("Player");
        }

        if (playerObject == null)
        {
            return;
        }

        if (playerController == null)
        {
            playerController = playerObject.GetComponent<CharacterController>();
        }

        if (playerControlScripts != null && playerControlScripts.Length > 0)
        {
            return;
        }

        MonoBehaviour[] scripts = playerObject.GetComponents<MonoBehaviour>();
        var controls = new System.Collections.Generic.List<MonoBehaviour>();

        for (int i = 0; i < scripts.Length; i++)
        {
            if (scripts[i] == null)
            {
                continue;
            }

            string scriptName = scripts[i].GetType().Name;
            if (scriptName == "PlayerMovement" || scriptName == "MouseMovement" || scriptName == "PlayerAttack")
            {
                controls.Add(scripts[i]);
            }
        }

        playerControlScripts = controls.ToArray();
    }

    private void HidePlayerRenderersIfNeeded()
    {
        if (!hidePlayerRenderersDuringOffering)
        {
            return;
        }

        FindPlayerControlsIfNeeded();
        if (playerObject == null)
        {
            return;
        }

        hiddenPlayerRenderers = playerObject.GetComponentsInChildren<Renderer>(true);
        hiddenPlayerRendererStates = new bool[hiddenPlayerRenderers.Length];

        for (int i = 0; i < hiddenPlayerRenderers.Length; i++)
        {
            if (hiddenPlayerRenderers[i] == null)
            {
                continue;
            }

            hiddenPlayerRendererStates[i] = hiddenPlayerRenderers[i].enabled;
            hiddenPlayerRenderers[i].enabled = false;
        }
    }

    private void RestorePlayerRenderersIfNeeded()
    {
        if (hiddenPlayerRenderers == null || hiddenPlayerRendererStates == null)
        {
            return;
        }

        int count = Mathf.Min(hiddenPlayerRenderers.Length, hiddenPlayerRendererStates.Length);
        for (int i = 0; i < count; i++)
        {
            if (hiddenPlayerRenderers[i] != null)
            {
                hiddenPlayerRenderers[i].enabled = hiddenPlayerRendererStates[i];
            }
        }

        hiddenPlayerRenderers = null;
        hiddenPlayerRendererStates = null;
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
        EnsureEndingVoiceSource();

        float endingVoiceDuration = 0f;
        bool useLineVoiceClips = endingVoiceClips != null && endingVoiceClips.Length > 0;
        if (!useLineVoiceClips && endingVoiceSource != null && endingVoiceClip != null)
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
                AudioClip lineVoiceClip = GetEndingLineVoiceClip(i);
                if (endingVoiceSource != null && lineVoiceClip != null)
                {
                    endingVoiceSource.Stop();
                    endingVoiceSource.clip = lineVoiceClip;
                    endingVoiceSource.Play();
                    waitTime = Mathf.Max(waitTime, lineVoiceClip.length + 0.3f);
                }

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

    private AudioClip GetEndingLineVoiceClip(int index)
    {
        if (endingVoiceClips == null || index < 0 || index >= endingVoiceClips.Length)
        {
            return null;
        }

        return endingVoiceClips[index];
    }

    private void EnsureEndingVoiceSource()
    {
        if (endingVoiceSource != null)
        {
            return;
        }

        endingVoiceSource = GetComponent<AudioSource>();
        if (endingVoiceSource == null)
        {
            endingVoiceSource = gameObject.AddComponent<AudioSource>();
        }

        endingVoiceSource.playOnAwake = false;
        endingVoiceSource.loop = false;
    }

    private void CloseInventoryUIIfNeeded()
    {
        if (!closeInventoryUIWhenEndingStarts || InventorySystem.Instance == null)
        {
            return;
        }

        InventorySystem.Instance.isOpen = false;
        if (InventorySystem.Instance.inventoryScreenUI != null)
        {
            InventorySystem.Instance.inventoryScreenUI.SetActive(false);
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
