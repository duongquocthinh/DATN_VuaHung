using System.Collections;
using UnityEngine;

public class HeraldQuestStarter : MonoBehaviour, IInteractable
{
    [SerializeField] private string npcName = "Người truyền lệnh";
    [SerializeField] private string orderMessage = "Vua Hùng truyền lệnh tìm lễ vật ý nghĩa nhất. Mọi người hãy bắt đầu chuẩn bị!";
    [SerializeField] private VillageRoutineNPC[] villagers;

    [Header("Simple cutscene")]
    [SerializeField] private bool playOnStart;
    [SerializeField] private float startDelay = 1f;
    [SerializeField] private bool useCutscene;
    [SerializeField] private Transform readingPoint;
    [SerializeField] private Transform standBesideKingPoint;
    [SerializeField] private string[] cutsceneLines;
    [SerializeField] private float lineDuration = 3f;
    [SerializeField] private float moveSpeed = 1.8f;
    [SerializeField] private Animator animator;
    [SerializeField] private string readTriggerName = "Read";
    [SerializeField] private string walkingBoolName = "IsWalking";

    [Header("Dialogue UI")]
    [SerializeField] private bool showDialogueBox = true;
    [SerializeField] private string dialogueTitle = "Lac Hau";
    [SerializeField] private AudioSource voiceSource;
    [SerializeField] private AudioClip[] cutsceneVoiceClips;

    [Header("Village reaction camera")]
    [SerializeField] private Camera cutsceneCamera;
    [SerializeField] private Transform[] readingCameraPoints;
    [SerializeField] private Transform workStartCameraPoint;
    [SerializeField] private float cameraMoveDuration = 2.5f;
    [SerializeField] private float watchVillagersAfterOrder = 3f;

    [Header("Villager auto setup")]
    [SerializeField] private bool autoFindMeshyVillagers;
    [SerializeField] private string autoVillagerNameContains = "Meshy_AI_Village";
    [SerializeField] private float autoRoutineDistance = 3f;
    [SerializeField] private float autoRoutineSideOffset = 1.4f;
    [Header("Deadline")]
    [SerializeField] private OfferingDeadlineTimer offeringDeadlineTimer;
    [SerializeField] private bool startDeadlineAfterOrder = true;

    [Header("Royal offering area")]
    [SerializeField] private bool moveRoyalCharactersToOfferingAreaAfterOrder;
    [SerializeField] private Transform kingTransform;
    [SerializeField] private Transform kingOfferingPoint;
    [SerializeField] private Transform heraldOfferingPoint;
    [SerializeField] private Animator kingAnimator;
    [SerializeField] private string kingAfterMoveTriggerName = "";
    [SerializeField] private string heraldAfterMoveTriggerName = "";

    [Header("Player")]
    [SerializeField] private MonoBehaviour[] playerControlScripts;
    [SerializeField] private CharacterController playerController;
    [SerializeField] private GameObject playerObject;

    [Header("Position safety")]
    [SerializeField] private bool keepOriginalHeight = true;

    [Header("Rotation fix")]
    [SerializeField] private float modelForwardOffsetY = 0f;

    private bool started;
    private bool isRunningCutscene;
    private bool showingDialogue;
    private string currentDialogueLine;
    private GUIStyle dialogueBoxStyle;
    private GUIStyle dialogueTitleStyle;
    private GUIStyle dialogueTextStyle;
    private bool autoVillagersConfigured;
    private MonoBehaviour[] autoPlayerControlScripts;

    public bool IsRunningCutscene
    {
        get { return isRunningCutscene; }
    }

    private void Start()
    {
        if (playOnStart)
        {
            StartCoroutine(StartAfterDelay());
        }
    }

    public string GetInteractionText()
    {
        return npcName;
    }

    public void Interact()
    {
        if (started)
        {
            NotificationUI.ShowMessage(orderMessage, 4f);
            return;
        }

        started = true;
        if (useCutscene)
        {
            StartCoroutine(CutsceneRoutine());
            return;
        }

        NotificationUI.ShowMessage(orderMessage, 4f);
        StartVillagers();
        MoveRoyalCharactersToOfferingArea();
        StartDeadlineTimerIfNeeded();
    }

    public void StartQuestCutscene()
    {
        if (!started)
        {
            Interact();
        }
    }

    private IEnumerator StartAfterDelay()
    {
        yield return new WaitForSeconds(Mathf.Max(0f, startDelay));

        if (!started)
        {
            Interact();
        }
    }

    private IEnumerator CutsceneRoutine()
    {
        if (isRunningCutscene)
        {
            yield break;
        }

        isRunningCutscene = true;
        SetPlayerControls(false);
        float fixedY = transform.position.y;

        if (animator != null)
        {
            animator.applyRootMotion = false;
        }

        if (readingPoint != null)
        {
            transform.position = GetSafePosition(readingPoint.position, fixedY);
            transform.rotation = ApplyModelOffset(readingPoint.rotation);
        }

        SetWalking(false);
        PlayTrigger(readTriggerName);

        if (cutsceneLines != null && cutsceneLines.Length > 0)
        {
            for (int i = 0; i < cutsceneLines.Length; i++)
            {
                if (!string.IsNullOrWhiteSpace(cutsceneLines[i]))
                {
                    ShowDialogue(cutsceneLines[i]);
                }

                float waitTime = PlayVoiceClip(i, lineDuration);
                yield return StartCoroutine(ShowReadingCameraPoint(i, waitTime));
            }
        }
        else
        {
            ShowDialogue(orderMessage);
            yield return new WaitForSeconds(4f);
        }

        HideDialogue();

        StartVillagers();

        if (workStartCameraPoint != null && watchVillagersAfterOrder > 0f)
        {
            yield return StartCoroutine(MoveCutsceneCameraTo(workStartCameraPoint, cameraMoveDuration));
            yield return new WaitForSeconds(watchVillagersAfterOrder);
            FinishVillagerMoves();
        }

        if (standBesideKingPoint != null)
        {
            SetWalking(true);

            Vector3 standPosition = GetSafePosition(standBesideKingPoint.position, fixedY);

            while (Vector3.Distance(transform.position, standPosition) > 0.05f)
            {
                transform.position = Vector3.MoveTowards(
                    transform.position,
                    standPosition,
                    moveSpeed * Time.deltaTime
                );

                Vector3 direction = standPosition - transform.position;
                direction.y = 0f;
                if (direction.sqrMagnitude > 0.001f)
                {
                    transform.rotation = Quaternion.Slerp(
                        transform.rotation,
                        ApplyModelOffset(Quaternion.LookRotation(direction)),
                        Time.deltaTime * 8f
                    );
                }

                yield return null;
            }

            transform.position = standPosition;
            transform.rotation = ApplyModelOffset(standBesideKingPoint.rotation);
            SetWalking(false);
        }

        MoveRoyalCharactersToOfferingArea();
        SetPlayerControls(true);
        StartDeadlineTimerIfNeeded();
        isRunningCutscene = false;
    }

    private Vector3 GetSafePosition(Vector3 targetPosition, float fixedY)
    {
        if (keepOriginalHeight)
        {
            targetPosition.y = fixedY;
        }

        return targetPosition;
    }

    private Quaternion ApplyModelOffset(Quaternion baseRotation)
    {
        return baseRotation * Quaternion.Euler(0f, modelForwardOffsetY, 0f);
    }

    private void ShowDialogue(string line)
    {
        if (!showDialogueBox)
        {
            NotificationUI.ShowMessage(line, lineDuration);
            return;
        }

        currentDialogueLine = line;
        showingDialogue = true;
    }

    private void HideDialogue()
    {
        currentDialogueLine = "";
        showingDialogue = false;
    }

    private float PlayVoiceClip(int index, float fallbackDuration)
    {
        if (voiceSource == null || cutsceneVoiceClips == null || index < 0 || index >= cutsceneVoiceClips.Length || cutsceneVoiceClips[index] == null)
        {
            return fallbackDuration;
        }

        voiceSource.Stop();
        voiceSource.clip = cutsceneVoiceClips[index];
        voiceSource.Play();
        return Mathf.Max(fallbackDuration, cutsceneVoiceClips[index].length + 0.3f);
    }

    private IEnumerator ShowReadingCameraPoint(int lineIndex, float waitTime)
    {
        if (readingCameraPoints == null || readingCameraPoints.Length == 0)
        {
            yield return new WaitForSeconds(waitTime);
            yield break;
        }

        Transform cameraPoint = readingCameraPoints[Mathf.Min(lineIndex, readingCameraPoints.Length - 1)];
        if (cameraPoint == null)
        {
            yield return new WaitForSeconds(waitTime);
            yield break;
        }

        float moveDuration = Mathf.Min(Mathf.Max(0f, cameraMoveDuration), Mathf.Max(0f, waitTime));
        if (moveDuration > 0f)
        {
            yield return StartCoroutine(MoveCutsceneCameraTo(cameraPoint, moveDuration));
        }

        float remainingWait = waitTime - moveDuration;
        if (remainingWait > 0f)
        {
            yield return new WaitForSeconds(remainingWait);
        }
    }

    private IEnumerator MoveCutsceneCameraTo(Transform targetPoint, float duration)
    {
        if (targetPoint == null)
        {
            yield break;
        }

        if (cutsceneCamera == null)
        {
            cutsceneCamera = Camera.main;
        }

        if (cutsceneCamera == null)
        {
            yield break;
        }

        Transform cameraTransform = cutsceneCamera.transform;
        Vector3 startPosition = cameraTransform.position;
        Quaternion startRotation = cameraTransform.rotation;
        Quaternion targetRotation = GetCutsceneCameraRotation(targetPoint);
        float elapsed = 0f;
        float safeDuration = Mathf.Max(0.01f, duration);

        while (elapsed < safeDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / safeDuration);
            cameraTransform.position = Vector3.Lerp(startPosition, targetPoint.position, t);
            cameraTransform.rotation = Quaternion.Slerp(startRotation, targetRotation, t);
            yield return null;
        }

        cameraTransform.position = targetPoint.position;
        cameraTransform.rotation = targetRotation;
    }
    private Quaternion GetCutsceneCameraRotation(Transform targetPoint)
    {
        Vector3 eulerAngles = targetPoint.rotation.eulerAngles;
        eulerAngles.z = 0f;
        return Quaternion.Euler(eulerAngles);
    }

    private void OnGUI()
    {
        if (!showingDialogue || string.IsNullOrWhiteSpace(currentDialogueLine))
        {
            return;
        }

        CreateDialogueStylesIfNeeded();

        float width = Mathf.Min(860f, Screen.width * 0.78f);
        float height = Mathf.Min(235f, Screen.height * 0.31f);
        float x = (Screen.width - width) * 0.5f;
        float y = Screen.height - height - 55f;

        Rect boxRect = new Rect(x, y, width, height);
        GUI.Box(boxRect, GUIContent.none, dialogueBoxStyle);

        Rect titleRect = new Rect(x + 24f, y + 16f, width - 48f, 34f);
        GUI.Label(titleRect, dialogueTitle, dialogueTitleStyle);

        Rect textRect = new Rect(x + 28f, y + 62f, width - 56f, height - 82f);
        GUI.Label(textRect, currentDialogueLine, dialogueTextStyle);
    }

    private void CreateDialogueStylesIfNeeded()
    {
        if (dialogueBoxStyle != null)
        {
            return;
        }

        Texture2D boxTexture = new Texture2D(1, 1);
        boxTexture.SetPixel(0, 0, new Color(0.42f, 0.27f, 0.12f, 0.92f));
        boxTexture.Apply();

        dialogueBoxStyle = new GUIStyle(GUI.skin.box);
        dialogueBoxStyle.normal.background = boxTexture;

        dialogueTitleStyle = new GUIStyle(GUI.skin.label);
        dialogueTitleStyle.alignment = TextAnchor.MiddleCenter;
        dialogueTitleStyle.fontStyle = FontStyle.Bold;
        dialogueTitleStyle.fontSize = Mathf.Clamp(Screen.height / 32, 22, 32);
        dialogueTitleStyle.normal.textColor = Color.white;

        dialogueTextStyle = new GUIStyle(GUI.skin.label);
        dialogueTextStyle.alignment = TextAnchor.UpperLeft;
        dialogueTextStyle.wordWrap = true;
        dialogueTextStyle.fontStyle = FontStyle.Bold;
        dialogueTextStyle.fontSize = Mathf.Clamp(Screen.height / 38, 18, 25);
        dialogueTextStyle.normal.textColor = Color.white;
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

        if (autoPlayerControlScripts != null)
        {
            for (int i = 0; i < autoPlayerControlScripts.Length; i++)
            {
                if (autoPlayerControlScripts[i] != null)
                {
                    autoPlayerControlScripts[i].enabled = enabled;
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
        if (playerController != null && autoPlayerControlScripts != null)
        {
            return;
        }

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

        autoPlayerControlScripts = controls.ToArray();
    }

    private void StartVillagers()
    {
        EnsureAutoVillagers();
        EnsureSceneVillagers();

        if (villagers == null)
        {
            return;
        }

        for (int i = 0; i < villagers.Length; i++)
        {
            if (villagers[i] != null)
            {
                villagers[i].BeginRoutine();
            }
        }
    }

    private void StartDeadlineTimerIfNeeded()
    {
        if (!startDeadlineAfterOrder)
        {
            return;
        }

        if (offeringDeadlineTimer == null)
        {
            offeringDeadlineTimer = FindObjectOfType<OfferingDeadlineTimer>();
        }

        if (offeringDeadlineTimer == null)
        {
            offeringDeadlineTimer = gameObject.AddComponent<OfferingDeadlineTimer>();
        }

        if (offeringDeadlineTimer != null)
        {
            offeringDeadlineTimer.StartTimer();
        }
    }

    private void MoveRoyalCharactersToOfferingArea()
    {
        if (!moveRoyalCharactersToOfferingAreaAfterOrder)
        {
            return;
        }

        MoveCharacterToPoint(kingTransform, kingOfferingPoint, false);
        PlayAnimatorTrigger(kingAnimator, kingAfterMoveTriggerName);

        if (heraldOfferingPoint != null)
        {
            MoveCharacterToPoint(transform, heraldOfferingPoint, true);
            SetWalking(false);
            PlayTrigger(heraldAfterMoveTriggerName);
        }
    }

    private void MoveCharacterToPoint(Transform character, Transform point, bool applyHeraldModelOffset)
    {
        if (character == null || point == null)
        {
            return;
        }

        character.position = GetSafePosition(point.position, character.position.y);
        character.rotation = applyHeraldModelOffset ? ApplyModelOffset(point.rotation) : point.rotation;
    }

    private void PlayAnimatorTrigger(Animator targetAnimator, string triggerName)
    {
        if (targetAnimator != null && !string.IsNullOrWhiteSpace(triggerName))
        {
            targetAnimator.SetTrigger(triggerName);
        }
    }

    private void FinishVillagerMoves()
    {
        EnsureSceneVillagers();

        if (villagers == null)
        {
            return;
        }

        for (int i = 0; i < villagers.Length; i++)
        {
            if (villagers[i] != null)
            {
                villagers[i].FinishCurrentMove();
            }
        }
    }

    private void EnsureSceneVillagers()
    {
        bool hasMissingVillager = villagers == null || villagers.Length == 0;
        if (!hasMissingVillager)
        {
            for (int i = 0; i < villagers.Length; i++)
            {
                if (villagers[i] == null)
                {
                    hasMissingVillager = true;
                    break;
                }
            }
        }

        if (!hasMissingVillager)
        {
            return;
        }

        VillageRoutineNPC[] foundVillagers = FindObjectsOfType<VillageRoutineNPC>(true);
        System.Collections.Generic.List<VillageRoutineNPC> activeVillagers = new System.Collections.Generic.List<VillageRoutineNPC>();

        for (int i = 0; i < foundVillagers.Length; i++)
        {
            if (foundVillagers[i] != null && foundVillagers[i].gameObject.activeInHierarchy)
            {
                activeVillagers.Add(foundVillagers[i]);
            }
        }

        if (activeVillagers.Count > 0)
        {
            villagers = activeVillagers.ToArray();
        }
    }
    private void EnsureAutoVillagers()
    {
        if (autoVillagersConfigured || !autoFindMeshyVillagers || (villagers != null && villagers.Length > 0))
        {
            return;
        }

        autoVillagersConfigured = true;

        Transform[] allTransforms = FindObjectsOfType<Transform>(true);
        System.Collections.Generic.List<VillageRoutineNPC> foundVillagers = new System.Collections.Generic.List<VillageRoutineNPC>();

        for (int i = 0; i < allTransforms.Length; i++)
        {
            Transform candidate = allTransforms[i];
            if (candidate == null || !candidate.gameObject.activeInHierarchy)
            {
                continue;
            }

            if (string.IsNullOrWhiteSpace(candidate.name) || !candidate.name.Contains(autoVillagerNameContains))
            {
                continue;
            }

            VillageRoutineNPC routineNPC = candidate.GetComponent<VillageRoutineNPC>();
            if (routineNPC == null)
            {
                routineNPC = candidate.gameObject.AddComponent<VillageRoutineNPC>();
            }

            SimpleVillagerWorkMotion motion = candidate.GetComponentInChildren<SimpleVillagerWorkMotion>();
            if (motion == null)
            {
                motion = candidate.gameObject.AddComponent<SimpleVillagerWorkMotion>();
            }

            SimpleVillagerWorkMotion.WorkStyle workStyle = GetAutoWorkStyle(foundVillagers.Count);
            string activity = GetAutoActivityName(workStyle);
            Transform[] points = CreateAutoRoutinePoints(candidate, foundVillagers.Count);

            routineNPC.ConfigureSimpleRoutine("Dan lang", activity, points, workStyle, motion);
            foundVillagers.Add(routineNPC);
        }

        villagers = foundVillagers.ToArray();
    }

    private Transform[] CreateAutoRoutinePoints(Transform villager, int index)
    {
        Vector3 startPosition = villager.position;
        Vector3 forward = transform.forward;
        Vector3 right = transform.right;

        if (forward.sqrMagnitude < 0.001f)
        {
            forward = Vector3.forward;
        }

        if (right.sqrMagnitude < 0.001f)
        {
            right = Vector3.right;
        }

        float sideSign = index % 2 == 0 ? 1f : -1f;
        Vector3 workPosition = startPosition
            + forward.normalized * autoRoutineDistance
            + right.normalized * autoRoutineSideOffset * sideSign;

        GameObject startPoint = new GameObject(villager.name + "_ListenPoint");
        startPoint.transform.position = startPosition;
        startPoint.transform.rotation = villager.rotation;

        GameObject workPoint = new GameObject(villager.name + "_WorkPoint");
        workPoint.transform.position = workPosition;
        workPoint.transform.rotation = villager.rotation;

        return new Transform[] { workPoint.transform, startPoint.transform };
    }

    private SimpleVillagerWorkMotion.WorkStyle GetAutoWorkStyle(int index)
    {
        switch (index % 4)
        {
            case 0:
                return SimpleVillagerWorkMotion.WorkStyle.GatherLeaves;
            case 1:
                return SimpleVillagerWorkMotion.WorkStyle.Cook;
            case 2:
                return SimpleVillagerWorkMotion.WorkStyle.PoundRice;
            default:
                return SimpleVillagerWorkMotion.WorkStyle.SitByFire;
        }
    }

    private string GetAutoActivityName(SimpleVillagerWorkMotion.WorkStyle workStyle)
    {
        switch (workStyle)
        {
            case SimpleVillagerWorkMotion.WorkStyle.GatherLeaves:
                return "Đang hái lá dong";
            case SimpleVillagerWorkMotion.WorkStyle.Cook:
                return "Đang chuẩn bị bếp nấu";
            case SimpleVillagerWorkMotion.WorkStyle.PoundRice:
                return "Đang giã gạo nếp";
            case SimpleVillagerWorkMotion.WorkStyle.SitByFire:
                return "Đang ngồi cạnh bếp lửa";
            default:
                return "Đang phụ giúp dân làng";
        }
    }

    private void PlayTrigger(string triggerName)
    {
        if (animator != null && !string.IsNullOrWhiteSpace(triggerName))
        {
            animator.SetTrigger(triggerName);
        }
    }

    private void SetWalking(bool isWalking)
    {
        if (animator != null && !string.IsNullOrWhiteSpace(walkingBoolName))
        {
            animator.SetBool(walkingBoolName, isWalking);
        }
    }
}
