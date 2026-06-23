using System.Collections;
using UnityEngine;

public class IntroStorySequence : MonoBehaviour
{
    [SerializeField] private bool playOnStart = true;
    [SerializeField] private float secondsPerLine = 4f;
    [SerializeField] private string[] storyLines =
    {
        "Sau khi Lang Lieu dang Banh Chung, Banh Giay len Vua Hung, hai mon banh tro thanh net dep khong the thieu trong ngay le.",
        "Hang nam, Vua Hung mo cuoc thi dang banh de dan lang cung tuong nho va giu gin truyen thong.",
        "Ban la mot nguoi dan trong lang. Hay thu thap nguyen lieu, che bien va nau du hai loai banh.",
        "Muc tieu: dang Banh Chung va Banh Giay len Vua Hung."
    };

    private IEnumerator Start()
    {
        if (playOnStart)
        {
            yield return PlayStory();
        }
    }

    public IEnumerator PlayStory()
    {
        for (int i = 0; i < storyLines.Length; i++)
        {
            NotificationUI.ShowMessage(storyLines[i], secondsPerLine);
            yield return new WaitForSeconds(secondsPerLine);
        }
    }
}
