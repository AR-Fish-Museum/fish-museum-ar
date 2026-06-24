using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ARBackButtonController : MonoBehaviour
{
    [Header("Back Button")]
    [SerializeField] private string fallbackSceneName = SceneLoader.LoginScene;
    [SerializeField] private bool clearSelectedFishOnBack = true;
    [SerializeField] private string buttonText = "← Geri";

    [Header("Layout")]
    [SerializeField] private Vector2 buttonSize = new Vector2(180f, 64f);
    [SerializeField] private Vector2 topLeftMargin = new Vector2(32f, 32f);

    private void Start()
    {
        CreateBackButton();
    }

    private void CreateBackButton()
    {
        GameObject canvasGo = new GameObject("ARBackButtonCanvas");
        canvasGo.transform.SetParent(transform, false);

        Canvas canvas = canvasGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 9999;

        CanvasScaler scaler = canvasGo.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080f, 1920f);
        scaler.matchWidthOrHeight = 0.5f;

        canvasGo.AddComponent<GraphicRaycaster>();

        GameObject buttonGo = new GameObject("Btn_AR_Back");
        buttonGo.transform.SetParent(canvasGo.transform, false);

        RectTransform buttonRect = buttonGo.AddComponent<RectTransform>();
        buttonRect.anchorMin = new Vector2(0f, 1f);
        buttonRect.anchorMax = new Vector2(0f, 1f);
        buttonRect.pivot = new Vector2(0f, 1f);
        buttonRect.sizeDelta = buttonSize;
        buttonRect.anchoredPosition = new Vector2(topLeftMargin.x, -topLeftMargin.y);

        Image image = buttonGo.AddComponent<Image>();
        image.color = new Color(0.02f, 0.16f, 0.24f, 0.82f);

        Button button = buttonGo.AddComponent<Button>();
        button.onClick.AddListener(OnBackClicked);

        ColorBlock colors = button.colors;
        colors.normalColor = new Color(0.02f, 0.16f, 0.24f, 0.82f);
        colors.highlightedColor = new Color(0.04f, 0.28f, 0.38f, 0.9f);
        colors.pressedColor = new Color(0.02f, 0.12f, 0.18f, 1f);
        colors.selectedColor = colors.highlightedColor;
        colors.disabledColor = new Color(0.1f, 0.1f, 0.1f, 0.5f);
        button.colors = colors;

        GameObject textGo = new GameObject("Text");
        textGo.transform.SetParent(buttonGo.transform, false);

        RectTransform textRect = textGo.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        TextMeshProUGUI text = textGo.AddComponent<TextMeshProUGUI>();
        text.text = buttonText;
        text.fontSize = 30f;
        text.alignment = TextAlignmentOptions.Center;
        text.color = Color.white;
        text.raycastTarget = false;

        Debug.Log("[ARBackButtonController] AR geri butonu oluşturuldu.");
    }

    private void OnBackClicked()
    {
        Debug.Log("[ARBackButtonController] Geri butonuna basıldı.");

        if (clearSelectedFishOnBack)
        {
            SelectedFishSession.Clear();
            Debug.Log("[ARBackButtonController] SelectedFishSession temizlendi.");
        }

        SceneLoader.LoadPreviousSceneOrFallback(fallbackSceneName);
    }
}