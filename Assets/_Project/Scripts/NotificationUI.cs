using System.Collections;
using TMPro;
using UnityEngine;

public class NotificationUI : MonoBehaviour
{
    public static NotificationUI Instance { get; private set; }

    [SerializeField] private TextMeshProUGUI messageText;
    [SerializeField] private float defaultDuration = 2f;

    private CanvasGroup canvasGroup;
    private Coroutine hideCoroutine;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (messageText == null)
        {
            messageText = GetComponent<TextMeshProUGUI>();
        }

        canvasGroup = GetComponent<CanvasGroup>();

        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        HideImmediate();
    }

    public static void ShowMessage(string message, float duration = 0f)
    {
        if (Instance != null)
        {
            Instance.Show(message, duration);
        }
        else
        {
            Debug.Log(message);
        }
    }

    public void Show(string message, float duration = 0f)
    {
        if (messageText == null)
        {
            Debug.Log(message);
            return;
        }

        messageText.text = message;
        canvasGroup.alpha = 1f;

        if (hideCoroutine != null)
        {
            StopCoroutine(hideCoroutine);
        }

        hideCoroutine = StartCoroutine(HideAfter(duration > 0f ? duration : defaultDuration));
    }

    private IEnumerator HideAfter(float duration)
    {
        yield return new WaitForSeconds(duration);
        HideImmediate();
    }

    private void HideImmediate()
    {
        if (messageText != null)
        {
            messageText.text = "";
        }

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
        }
    }
}
