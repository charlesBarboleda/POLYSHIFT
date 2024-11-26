using TMPro;
using UnityEngine;
using DG.Tweening;

public class GeneralCanvasUI : MonoBehaviour
{
    [SerializeField] TMP_Text countdownText;
    [SerializeField] TMP_Text gameLevelText;

    void Start()
    {
        GameManager.Instance.GameCountdown.OnValueChanged += UpdateCountdownText;
        GameManager.Instance.GameLevel.OnValueChanged += UpdateGameLevelText;
    }

    void UpdateCountdownText(float oldValue, float newValue)
    {
        countdownText.text = Mathf.Round(newValue).ToString("F0");
        if (newValue <= 0)
        {
            countdownText.DOFade(0, 0.5f).OnComplete(() => countdownText.gameObject.SetActive(false));
        }
        if (GameManager.Instance.CurrentGameState == GameState.OutLevel)
        {
            countdownText.gameObject.SetActive(true);
            countdownText.DOFade(1, 0.5f);
        }
    }

    void UpdateGameLevelText(int oldValue, int newValue)
    {
        gameLevelText.text = $"{newValue}";
    }
}
