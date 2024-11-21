using TMPro;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

public class TitleScreenUIManager : MonoBehaviour
{
    [Header("Play Screen")]
    [SerializeField] Button backButton;
    [SerializeField] Button joinButton;
    [SerializeField] Button hostButton;

    [Header("Title Screen")]
    [SerializeField] Button playButton;
    [SerializeField] TMP_Text titleText;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

        AnimateTitleScreen();
    }

    void AnimateTitleScreen()
    {
        var titleCanvasGroup = titleText.GetComponent<CanvasGroup>();
        titleCanvasGroup.alpha = 0;
        titleCanvasGroup.DOFade(1, 3f).OnComplete(() =>
        {
            titleText.transform.DOMoveY(425f, 2f).SetEase(Ease.InOutSine).OnComplete(() =>
            {
                playButton.GetComponent<CanvasGroup>().DOFade(1, 1f);
            });
        });
    }

    public void PlayButton()
    {
        playButton.GetComponent<CanvasGroup>().DOFade(0, 0.5f).OnComplete(() =>
        {
            playButton.gameObject.SetActive(false);

            joinButton.gameObject.SetActive(true);
            joinButton.GetComponent<CanvasGroup>().DOFade(1, 0.5f);

            hostButton.gameObject.SetActive(true);
            hostButton.GetComponent<CanvasGroup>().DOFade(1, 0.5f);

            backButton.gameObject.SetActive(true);
            backButton.GetComponent<CanvasGroup>().DOFade(1, 0.5f);
        });
    }

    public void BackButtonPlayScreen()
    {
        backButton.GetComponent<CanvasGroup>().DOFade(0, 0.5f).OnComplete(() =>
        {
            backButton.gameObject.SetActive(false);

            playButton.gameObject.SetActive(true);
            playButton.GetComponent<CanvasGroup>().DOFade(1, 0.5f);
        });

        joinButton.GetComponent<CanvasGroup>().DOFade(0, 0.5f).OnComplete(() =>
            {
                joinButton.gameObject.SetActive(false);
            });

        hostButton.GetComponent<CanvasGroup>().DOFade(0, 0.5f).OnComplete(() =>
       {
           hostButton.gameObject.SetActive(false);
       });
    }
}
