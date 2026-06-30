using UnityEngine;

public class OfferingDeadlineTimer : MonoBehaviour
{
    [SerializeField] private bool startOnAwake = false;
    [SerializeField] private float timeLimitSeconds = 900f;
    [SerializeField] private QuestTurnInNPC questTurnInNPC;
    [SerializeField] private string warningPrefix = "Thời gian còn lại";
    [SerializeField] private string failTitle = "Đã quá thời gian dâng bánh";
    [TextArea(2, 5)]
    [SerializeField] private string failMessage = "Lang Liêu không kịp dâng bánh lên Vua Hùng. Hãy thử lại và chuẩn bị lễ vật nhanh hơn.";
    [SerializeField] private bool showTimerOnGUI = true;
    [SerializeField] private bool stopPlayerOnFail = true;
    [SerializeField] private MonoBehaviour[] playerControlScripts;
    [SerializeField] private CharacterController playerController;
    [SerializeField] private float quitDelayAfterFail = 3f;
    [SerializeField] private bool quitGameAfterFail = false;

    private float remainingSeconds;
    private bool timerRunning;
    private bool failed;
    private float failAtTime;
    private GUIStyle timerStyle;
    private GUIStyle failTitleStyle;
    private GUIStyle failMessageStyle;

    private void Start()
    {
        remainingSeconds = Mathf.Max(1f, timeLimitSeconds);

        if (questTurnInNPC == null)
        {
            questTurnInNPC = FindObjectOfType<QuestTurnInNPC>();
        }

        if (startOnAwake)
        {
            StartTimer();
        }
    }

    private void Update()
    {
        if (!timerRunning || failed)
        {
            return;
        }

        if (IsQuestCompleted())
        {
            timerRunning = false;
            return;
        }

        remainingSeconds -= Time.deltaTime;
        if (remainingSeconds <= 0f)
        {
            FailDeadline();
        }
    }

    public void StartTimer()
    {
        if (failed || IsQuestCompleted())
        {
            return;
        }

        remainingSeconds = remainingSeconds > 0f ? remainingSeconds : Mathf.Max(1f, timeLimitSeconds);
        timerRunning = true;
    }

    public void StopTimer()
    {
        timerRunning = false;
    }

    private bool IsQuestCompleted()
    {
        return questTurnInNPC != null && questTurnInNPC.IsQuestCompleted;
    }

    private void FailDeadline()
    {
        remainingSeconds = 0f;
        timerRunning = false;
        failed = true;
        failAtTime = Time.time;

        if (stopPlayerOnFail)
        {
            SetPlayerControls(false);
        }

        NotificationUI.ShowMessage(failMessage, 5f);
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
        if (playerController != null && playerControlScripts != null && playerControlScripts.Length > 0)
        {
            return;
        }

        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
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

        if (playerControlScripts == null || playerControlScripts.Length == 0)
        {
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
    }

    private void OnGUI()
    {
        if (showTimerOnGUI && timerRunning && !failed)
        {
            CreateStylesIfNeeded();
            GUI.Label(new Rect(18f, 18f, 360f, 40f), warningPrefix + ": " + FormatTime(remainingSeconds), timerStyle);
        }

        if (!failed)
        {
            return;
        }

        CreateStylesIfNeeded();
        GUI.color = Color.black;
        GUI.DrawTexture(new Rect(0f, 0f, Screen.width, Screen.height), Texture2D.whiteTexture);
        GUI.color = Color.white;

        GUI.Label(new Rect(Screen.width * 0.15f, Screen.height * 0.28f, Screen.width * 0.7f, 60f), failTitle, failTitleStyle);
        GUI.Label(new Rect(Screen.width * 0.18f, Screen.height * 0.4f, Screen.width * 0.64f, 170f), failMessage, failMessageStyle);

        if (quitGameAfterFail && Time.time >= failAtTime + Mathf.Max(0f, quitDelayAfterFail))
        {
            EndGame();
        }
    }

    private string FormatTime(float seconds)
    {
        int totalSeconds = Mathf.CeilToInt(Mathf.Max(0f, seconds));
        int minutes = totalSeconds / 60;
        int secs = totalSeconds % 60;
        return minutes.ToString("00") + ":" + secs.ToString("00");
    }

    private void CreateStylesIfNeeded()
    {
        if (timerStyle != null)
        {
            return;
        }

        timerStyle = new GUIStyle(GUI.skin.label);
        timerStyle.fontStyle = FontStyle.Bold;
        timerStyle.fontSize = 24;
        timerStyle.normal.textColor = Color.white;

        failTitleStyle = new GUIStyle(GUI.skin.label);
        failTitleStyle.alignment = TextAnchor.MiddleCenter;
        failTitleStyle.fontStyle = FontStyle.Bold;
        failTitleStyle.fontSize = Mathf.Clamp(Screen.height / 24, 28, 42);
        failTitleStyle.normal.textColor = Color.white;

        failMessageStyle = new GUIStyle(GUI.skin.label);
        failMessageStyle.alignment = TextAnchor.UpperCenter;
        failMessageStyle.wordWrap = true;
        failMessageStyle.fontStyle = FontStyle.Bold;
        failMessageStyle.fontSize = Mathf.Clamp(Screen.height / 34, 20, 30);
        failMessageStyle.normal.textColor = Color.white;
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
