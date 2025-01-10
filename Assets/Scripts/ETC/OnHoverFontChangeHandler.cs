using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;

public class HoverFontChange : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] TMP_FontAsset _normalFont;
    [SerializeField] TMP_FontAsset _hoverFont;

    private TextMeshProUGUI _textMeshPro;

    void Start()
    {
        // Automatically find the TextMeshPro component in children
        _textMeshPro = GetComponentInChildren<TextMeshProUGUI>();

        // Ensure the normal font is set initially
        if (_textMeshPro != null && _normalFont != null)
        {
            _textMeshPro.font = _normalFont;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (_textMeshPro != null && _hoverFont != null)
        {
            _textMeshPro.font = _hoverFont;
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (_textMeshPro != null && _normalFont != null)
        {
            _textMeshPro.font = _normalFont;
        }
    }
}
