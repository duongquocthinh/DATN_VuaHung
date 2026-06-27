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

        KeepPanelOnScreen();
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

    private void KeepPanelOnScreen()
    {
        RectTransform rectTransform = transform as RectTransform;
        if (rectTransform == null)
        {
            return;
        }

        bool anchoredToTop = Mathf.Approximately(rectTransform.anchorMin.y, 1f)
            && Mathf.Approximately(rectTransform.anchorMax.y, 1f);

        if (anchoredToTop && rectTransform.anchoredPosition.y > 0f)
        {
            Vector2 position = rectTransform.anchoredPosition;
            position.y = -Mathf.Abs(position.y);
            rectTransform.anchoredPosition = position;
        }
    }
}
