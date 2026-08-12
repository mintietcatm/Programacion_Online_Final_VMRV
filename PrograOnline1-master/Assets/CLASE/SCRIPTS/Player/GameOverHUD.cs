using System;
using Fusion;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameOverHUD : MonoBehaviour
{
    private Health localHealth;
    private GameObject gameOverCanvas;
    private TMP_Text titleText;
    private TMP_Text resultText;
    private Button playAgainButton;

    private void Awake()
    {
        localHealth = GetComponent<Health>();
        CreateGameOverCanvas();
    }

    private void OnEnable()
    {
        Health.DeathAnnounced += ShowResult;
    }

    private void OnDisable()
    {
        Health.DeathAnnounced -= ShowResult;
    }

    private void ShowResult(int attackerId, int victimId)
    {
        if (localHealth == null || localHealth.Object == null)
            return;

        int localPlayerId = localHealth.Object.InputAuthority.PlayerId;

        if (localPlayerId != victimId && localPlayerId != attackerId)
            return;

        bool localPlayerDied = localPlayerId == victimId;
        gameOverCanvas.SetActive(true);

        if (localPlayerDied)
        {
            titleText.text = "HAS MUERTO";
            resultText.text = attackerId < 0
                ? "La tormenta te elimino"
                : $"Jugador {attackerId} te mato";
        }
        else
        {
            titleText.text = "GANASTE";
            resultText.text = $"Mataste al Jugador {victimId}";
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void CreateGameOverCanvas()
    {
        gameOverCanvas = new GameObject("Game Over Canvas");

        Canvas canvas = gameOverCanvas.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 200;

        CanvasScaler scaler = gameOverCanvas.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        gameOverCanvas.AddComponent<GraphicRaycaster>();

        GameObject panelObject = new GameObject("Game Over Panel");
        panelObject.transform.SetParent(gameOverCanvas.transform, false);

        RectTransform panelRect = panelObject.AddComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;

        Image panelImage = panelObject.AddComponent<Image>();
        panelImage.color = new Color(0f, 0f, 0f, 0.82f);

        titleText = CreateText(
            "Result Title",
            panelObject.transform,
            new Vector2(0.5f, 0.62f),
            new Vector2(900f, 120f),
            72f);

        resultText = CreateText(
            "Result Detail",
            panelObject.transform,
            new Vector2(0.5f, 0.49f),
            new Vector2(900f, 80f),
            40f);

        playAgainButton = CreateButton(panelObject.transform);
        playAgainButton.onClick.AddListener(ReturnToMenu);

        gameOverCanvas.SetActive(false);
    }

    private TMP_Text CreateText(
        string objectName,
        Transform parent,
        Vector2 anchor,
        Vector2 size,
        float fontSize)
    {
        GameObject textObject = new GameObject(objectName);
        textObject.transform.SetParent(parent, false);

        RectTransform rect = textObject.AddComponent<RectTransform>();
        rect.anchorMin = anchor;
        rect.anchorMax = anchor;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = size;

        TextMeshProUGUI text = textObject.AddComponent<TextMeshProUGUI>();
        text.fontSize = fontSize;
        text.color = Color.white;
        text.alignment = TextAlignmentOptions.Center;
        text.raycastTarget = false;
        return text;
    }

    private Button CreateButton(Transform parent)
    {
        GameObject buttonObject = new GameObject("Play Again Button");
        buttonObject.transform.SetParent(parent, false);

        RectTransform buttonRect = buttonObject.AddComponent<RectTransform>();
        buttonRect.anchorMin = new Vector2(0.5f, 0.34f);
        buttonRect.anchorMax = new Vector2(0.5f, 0.34f);
        buttonRect.pivot = new Vector2(0.5f, 0.5f);
        buttonRect.anchoredPosition = Vector2.zero;
        buttonRect.sizeDelta = new Vector2(360f, 90f);

        Image buttonImage = buttonObject.AddComponent<Image>();
        buttonImage.color = new Color(0.15f, 0.55f, 0.9f, 1f);

        Button button = buttonObject.AddComponent<Button>();
        button.targetGraphic = buttonImage;

        TMP_Text buttonText = CreateText(
            "Button Text",
            buttonObject.transform,
            new Vector2(0.5f, 0.5f),
            new Vector2(340f, 70f),
            34f);
        buttonText.text = "JUGAR DE NUEVO";

        return button;
    }

    private async void ReturnToMenu()
    {
        playAgainButton.interactable = false;

        NetworkRunner runner = localHealth != null ? localHealth.Runner : null;
        if (runner != null && runner.IsRunning)
            await runner.Shutdown();

        SceneManager.LoadScene(0, LoadSceneMode.Single);
    }

    private void OnDestroy()
    {
        if (gameOverCanvas != null)
            Destroy(gameOverCanvas);
    }
}
