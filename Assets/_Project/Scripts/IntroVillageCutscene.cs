using System.Collections;
using UnityEngine;

public class IntroVillageCutscene : MonoBehaviour
{
    [Header("Black Screen Intro")]
    [SerializeField] private bool useBlackScreenIntro = true;
    [TextArea(2, 5)]
    [SerializeField] private string[] blackScreenLines;
    [SerializeField] private float blackScreenLineDuration = 4f;
    [SerializeField] private AudioSource voiceSource;
    [SerializeField] private AudioClip[] blackScreenVoiceClips;

    [Header("Camera")]
    [SerializeField] private Camera cutsceneCamera;
    [SerializeField] private Transform[] cameraPoints;
    [SerializeField] private float moveTimePerPoint = 4f;
    [SerializeField] private bool showStoryDuringCameraMove;

    [Header("Herald")]
    [SerializeField] private HeraldQuestStarter heraldToStart;
    [SerializeField] private bool startHeraldAfterCameraMove = true;
    [SerializeField] private float waitAfterHeraldStarts = 12f;

    [Header("Player")]
    [SerializeField] private MonoBehaviour[] playerControlScripts;
    [SerializeField] private CharacterController playerController;
    [SerializeField] private GameObject playerObject;

    [Header("Story")]
    [TextArea(2, 4)]
    [SerializeField] private string[] storyLines;
    [SerializeField] private float lineDuration = 4f;
    [SerializeField] private AudioClip[] storyVoiceClips;
    [SerializeField] private bool playOnStart = true;

    private Transform cameraTransform;
    private Vector3 originalCameraLocalPosition;
    private Quaternion originalCameraLocalRotation;
    private MonoBehaviour[] autoPlayerControlScripts;
    private bool showingBlackScreen;
    private string blackScreenText;
    private GUIStyle blackScreenTextStyle;

    private void Start()
    {
        if (playOnStart)
        {
            StartCoroutine(PlayIntroRoutine());
        }
    }

    public void PlayIntro()
    {
        StartCoroutine(PlayIntroRoutine());
    }

    private IEnumerator PlayIntroRoutine()
    {
        if (cutsceneCamera == null)
        {
            cutsceneCamera = Camera.main;
        }

        if (cutsceneCamera == null)
        {
            yield break;
        }

        cameraTransform = cutsceneCamera.transform;
        originalCameraLocalPosition = cameraTransform.localPosition;
        originalCameraLocalRotation = cameraTransform.localRotation;

        SetPlayerControls(false);

        if (useBlackScreenIntro)
        {
            yield return StartCoroutine(ShowBlackScreenRoutine());
        }

        Coroutine textRoutine = null;
        if (showStoryDuringCameraMove)
        {
            textRoutine = StartCoroutine(ShowStoryLinesRoutine());
        }

        yield return StartCoroutine(MoveCameraRoutine());

        if (textRoutine != null)
        {
            StopCoroutine(textRoutine);
        }

        if (startHeraldAfterCameraMove && heraldToStart != null)
        {
            heraldToStart.StartQuestCutscene();
            yield return null;

            if (heraldToStart.IsRunningCutscene)
            {
                while (heraldToStart.IsRunningCutscene)
                {
                    yield return null;
                }
            }
            else
            {
                yield return new WaitForSeconds(Mathf.Max(0f, waitAfterHeraldStarts));
            }
        }

        NotificationUI.ShowMessage("Nhiem vu cua Lang Lieu: thu thap nguyen lieu, lam banh va dang len Vua Hung.", 5f);

        cameraTransform.localPosition = originalCameraLocalPosition;
        cameraTransform.localRotation = originalCameraLocalRotation;
        SetPlayerControls(true);
    }

    private IEnumerator ShowBlackScreenRoutine()
    {
        string[] linesToShow = blackScreenLines;
        if ((linesToShow == null || linesToShow.Length == 0) && storyLines != null && storyLines.Length > 0)
        {
            linesToShow = storyLines;
        }

        showingBlackScreen = true;

        if (linesToShow == null || linesToShow.Length == 0)
        {
            blackScreenText = "";
            yield return new WaitForSeconds(2f);
        }
        else
        {
            for (int i = 0; i < linesToShow.Length; i++)
            {
                blackScreenText = linesToShow[i];
                float waitTime = PlayVoiceClip(blackScreenVoiceClips, i, blackScreenLineDuration);
                yield return new WaitForSeconds(Mathf.Max(0.5f, waitTime));
            }
        }

        blackScreenText = "";
        showingBlackScreen = false;
    }

    private void OnGUI()
    {
        if (!showingBlackScreen)
        {
            return;
        }

        GUI.color = Color.black;
        GUI.DrawTexture(new Rect(0f, 0f, Screen.width, Screen.height), Texture2D.whiteTexture);
        GUI.color = Color.white;

        if (blackScreenTextStyle == null)
        {
            blackScreenTextStyle = new GUIStyle(GUI.skin.label);
            blackScreenTextStyle.alignment = TextAnchor.MiddleCenter;
            blackScreenTextStyle.wordWrap = true;
            blackScreenTextStyle.fontStyle = FontStyle.Bold;
        }

        blackScreenTextStyle.fontSize = Mathf.Clamp(Screen.height / 28, 22, 36);

        Rect textRect = new Rect(
            Screen.width * 0.15f,
            Screen.height * 0.25f,
            Screen.width * 0.7f,
            Screen.height * 0.5f
        );

        GUI.Label(textRect, blackScreenText, blackScreenTextStyle);
    }

    private IEnumerator ShowStoryLinesRoutine()
    {
        if (storyLines == null || storyLines.Length == 0)
        {
            yield break;
        }

        for (int i = 0; i < storyLines.Length; i++)
        {
            if (!string.IsNullOrWhiteSpace(storyLines[i]))
            {
                NotificationUI.ShowMessage(storyLines[i], lineDuration);
            }

            float waitTime = PlayVoiceClip(storyVoiceClips, i, lineDuration);
            yield return new WaitForSeconds(waitTime);
        }
    }

    private float PlayVoiceClip(AudioClip[] clips, int index, float fallbackDuration)
    {
        if (voiceSource == null || clips == null || index < 0 || index >= clips.Length || clips[index] == null)
        {
            return fallbackDuration;
        }

        voiceSource.Stop();
        voiceSource.clip = clips[index];
        voiceSource.Play();
        return Mathf.Max(fallbackDuration, clips[index].length + 0.3f);
    }

    private IEnumerator MoveCameraRoutine()
    {
        if (cameraPoints == null || cameraPoints.Length == 0)
        {
            yield return new WaitForSeconds(Mathf.Max(1f, lineDuration));
            yield break;
        }

        cameraTransform.position = cameraPoints[0].position;
        cameraTransform.rotation = cameraPoints[0].rotation;

        for (int i = 1; i < cameraPoints.Length; i++)
        {
            Transform targetPoint = cameraPoints[i];
            if (targetPoint == null)
            {
                continue;
            }

            Vector3 startPosition = cameraTransform.position;
            Quaternion startRotation = cameraTransform.rotation;
            float elapsed = 0f;
            float duration = Mathf.Max(0.1f, moveTimePerPoint);

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);

                cameraTransform.position = Vector3.Lerp(startPosition, targetPoint.position, t);
                cameraTransform.rotation = Quaternion.Slerp(startRotation, targetPoint.rotation, t);

                yield return null;
            }
        }
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
}
