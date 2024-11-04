using UnityEngine;
using DG.Tweening;
using UnityEngine.Rendering.Universal;  // Import DOTween namespace

public class DecalGrowAndFade : MonoBehaviour
{
    [Tooltip("Time it takes to reach full size.")]
    public float growthDuration = 1f;
    [Tooltip("Time it takes to fade out after reaching full size.")]
    public float fadeDuration = 5.0f;
    [Tooltip("The final size of the decal when it stops growing.")]
    public Vector3 targetScale = new Vector3(1, 1, 1);

    private Material decalMaterial;
    private float initialOpacity;

    void OnEnable()
    {
        decalMaterial = GetComponent<DecalProjector>().material;

        // Store the initial opacity value from the material
        if (decalMaterial.HasProperty("_BaseColor"))
        {
            initialOpacity = decalMaterial.color.a;
        }

        // Start the scaling and fading sequence
        AnimateDecal();
    }

    void AnimateDecal()
    {
        // Step 1: Scale up the decal using DOTween
        transform.DOScale(targetScale, growthDuration).SetEase(Ease.OutBack);

        // Step 2: Fade out the decal after growth using DOTween
        decalMaterial.DOFade(0, fadeDuration)
            .SetEase(Ease.OutQuad)
            .SetDelay(growthDuration)  // Delay until growth completes
            .OnComplete(() => ObjectPooler.Instance.Despawn("GroundCrackDecal", gameObject));  // Destroy after fade
    }
}
