using UnityEngine;
using UnityEngine.UI;

public class CrosshairUI : MonoBehaviour
{
    // ─────────────────────────────────────────
    // SETTINGS
    // ─────────────────────────────────────────
    [Header("Crosshair Settings")]
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color aimColor    = Color.red;
    [SerializeField] private float normalSize  = 20f;
    [SerializeField] private float aimSize     = 12f;
    [SerializeField] private float scaleSpeed  = 8f;

    [Header("References")]
    [SerializeField] private BallShooter ballShooter;

    // ─────────────────────────────────────────
    // PRIVATE
    // ─────────────────────────────────────────
    private Image   _crosshairImage;
    private RectTransform _rectTransform;

    // ─────────────────────────────────────────
    private void Awake()
    {
        // Create crosshair dot automatically
        // No manual UI setup needed in Canvas
        GameObject crosshairObj = new GameObject("Crosshair");
        crosshairObj.transform.SetParent(transform, false);

        _rectTransform           = crosshairObj.AddComponent<RectTransform>();
        _rectTransform.sizeDelta = new Vector2(normalSize, normalSize);
        _rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        _rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        _rectTransform.anchoredPosition = Vector2.zero;

        // Simple dot crosshair using UI Image
        _crosshairImage        = crosshairObj.AddComponent<Image>();
        _crosshairImage.color  = normalColor;

        if (ballShooter == null)
            ballShooter = Object.FindFirstObjectByType<BallShooter>();
    }

    // ─────────────────────────────────────────
    private void Update()
    {
        if (ballShooter == null) return;

        bool isAiming = ballShooter.IsAiming;

        // Smoothly change crosshair size
        // Small + red = aiming, Large + white = normal
        float targetSize      = isAiming ? aimSize   : normalSize;
        _crosshairImage.color = isAiming ? aimColor  : normalColor;

        _rectTransform.sizeDelta = Vector2.Lerp(
            _rectTransform.sizeDelta,
            new Vector2(targetSize, targetSize),
            Time.deltaTime * scaleSpeed
        );
    }
}