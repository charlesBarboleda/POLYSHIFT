using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class HotbarSlot : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private Image icon;  // Reference to the slot icon
    private Transform originalParent;
    private Vector3 originalPosition;  // Store the initial position of the slot
    private RectTransform rectTransform; // For consistent UI positioning
    private CanvasGroup canvasGroup;
    private HotbarSlot swapTargetSlot;

    private void Awake()
    {
        Debug.Log("Awake called on " + gameObject.name); // Check if Awake is called
        canvasGroup = GetComponent<CanvasGroup>();
        rectTransform = GetComponent<RectTransform>(); // Get RectTransform for UI elements
        icon = GetComponentInChildren<Image>(); // Get the icon component
        originalParent = transform.parent;
        originalPosition = rectTransform.localPosition;  // Record the starting position
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        Debug.Log("Begin Drag on: " + gameObject.name); // Track which slot is being dragged

        canvasGroup.alpha = 0.6f;
        canvasGroup.blocksRaycasts = false;
        transform.SetParent(transform.root);  // Move to top-level Canvas for dragging
    }

    public void OnDrag(PointerEventData eventData)
    {
        Debug.Log("Dragging slot: " + gameObject.name); // Track during drag
        rectTransform.position = Input.mousePosition;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        Debug.Log("End Drag on: " + gameObject.name); // Track when dragging ends

        // Final check if `swapTargetSlot` is not null
        if (swapTargetSlot != null)
        {
            Debug.Log($"Attempting to swap {gameObject.name} with {swapTargetSlot.name}");
            SwapSkills(swapTargetSlot);
        }
        else
        {
            // If not over another slot, reset to original position
            Debug.Log("No swap target found, resetting position.");
            transform.SetParent(originalParent);
            rectTransform.localPosition = originalPosition;
        }

        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;
    }

    private void SwapSkills(HotbarSlot targetSlot)
    {
        Debug.Log($"Swapping icons between {gameObject.name} and {targetSlot.name}");

        // Swap skill icons between slots
        Sprite tempIcon = targetSlot.icon.sprite;
        targetSlot.icon.sprite = icon.sprite;
        icon.sprite = tempIcon;

        // Reset the parent and local position for both slots
        transform.SetParent(originalParent);
        rectTransform.localPosition = originalPosition;

        targetSlot.transform.SetParent(targetSlot.originalParent);
        targetSlot.rectTransform.localPosition = targetSlot.originalPosition;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.TryGetComponent(out HotbarSlot slot))
        {
            swapTargetSlot = slot;
            Debug.Log($"Swap target set to: {slot.name} by {gameObject.name}");
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.GetComponent<HotbarSlot>() == swapTargetSlot)
        {
            Debug.Log($"Swap target cleared for: {gameObject.name}");
            swapTargetSlot = null;
        }
    }
}
