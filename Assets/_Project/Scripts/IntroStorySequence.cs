using System.Collections;
using UnityEngine;

public class IntroStorySequence : MonoBehaviour
{
    [SerializeField] private bool playOnStart = true;
    [SerializeField] private float secondsPerLine = 4f;
    [SerializeField] private string[] storyLines =
    {
        "Vua Hung muon truyen ngoi cho nguoi con tim duoc le vat y nghia nhat.",
        "Lac Hau truyen lenh cho cac hoang tu chuan bi san vat dang len Vua.",
        "Ban vao vai Lang Lieu. Hay thu thap nguyen lieu, che bien va nau du hai loai banh.",
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
