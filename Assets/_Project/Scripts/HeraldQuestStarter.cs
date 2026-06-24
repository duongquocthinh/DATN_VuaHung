using System.Collections;
using UnityEngine;

public class HeraldQuestStarter : MonoBehaviour, IInteractable
{
    [SerializeField] private string npcName = "Nguoi truyen lenh";
    [SerializeField] private string orderMessage = "Vua Hung truyen lenh tim le vat y nghia nhat. Moi nguoi hay bat dau chuan bi!";
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

    [Header("Villager auto setup")]
    [SerializeField] private bool autoFindMeshyVillagers;
    [SerializeField] private string autoVillagerNameContains = "Meshy_AI_Village";
    [SerializeField] private float autoRoutineDistance = 3f;
    [SerializeField] private float autoRoutineSideOffset = 1.4f;

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
                yield return new WaitForSeconds(waitTime);
            }
        }
        else
        {
            ShowDialogue(orderMessage);
            yield return new WaitForSeconds(4f);
        }

        HideDialogue();

        StartVillagers();

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

    private void StartVillagers()
    {
        EnsureAutoVillagers();

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
                return "Dang hai la dong";
            case SimpleVillagerWorkMotion.WorkStyle.Cook:
                return "Dang chuan bi bep nau";
            case SimpleVillagerWorkMotion.WorkStyle.PoundRice:
                return "Dang gia gao nep";
            case SimpleVillagerWorkMotion.WorkStyle.SitByFire:
                return "Dang ngoi canh bep lua";
            default:
                return "Dang phu giup dan lang";
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
