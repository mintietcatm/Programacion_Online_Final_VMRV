using System;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HealthHUD : MonoBehaviour
{
    private GameObject hudCanvasObject;
    private TMP_Text healthText;
    private Handgun localWeapon;
    private readonly StringBuilder textBuilder = new StringBuilder();

    private void Start()
    {
        localWeapon = GetComponent<Handgun>();
        CreateHUD();
    }

    private void Update()
    {
        if (healthText == null)
            return;

        Health[] players = UnityEngine.Object.FindObjectsByType<Health>(
            FindObjectsInactive.Exclude,
            FindObjectsSortMode.None);

        Array.Sort(players, (first, second) =>
            first.Object.InputAuthority.PlayerId.CompareTo(second.Object.InputAuthority.PlayerId));

        textBuilder.Clear();

        foreach (Health playerHealth in players)
        {
            if (playerHealth.Object == null)
                continue;

            textBuilder.Append("Jugador ")
                       .Append(playerHealth.Object.InputAuthority.PlayerId)
                       .Append(": ")
                       .Append(playerHealth.ActualHealth)
                       .Append(" / ")
                       .Append(playerHealth.MaxHealth)
                       .AppendLine();
        }

        if (localWeapon != null)
        {
            textBuilder.AppendLine()
                       .Append("Arma: ")
                       .Append(localWeapon.GetWeaponName())
                       .AppendLine()
                       .Append("Balas: ")
                       .Append(localWeapon.CurrentAmmo)
                       .Append(" / ")
                       .Append(localWeapon.MagazineSize);
        }

        healthText.text = textBuilder.ToString();
    }

    private void CreateHUD()
    {
        hudCanvasObject = new GameObject("Health HUD Canvas");

        Canvas canvas = hudCanvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;

        CanvasScaler scaler = hudCanvasObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        GameObject backgroundObject = new GameObject("Health Background");
        backgroundObject.transform.SetParent(hudCanvasObject.transform, false);

        RectTransform backgroundRect = backgroundObject.AddComponent<RectTransform>();
        backgroundRect.anchorMin = new Vector2(0f, 1f);
        backgroundRect.anchorMax = new Vector2(0f, 1f);
        backgroundRect.pivot = new Vector2(0f, 1f);
        backgroundRect.anchoredPosition = new Vector2(24f, -24f);
        backgroundRect.sizeDelta = new Vector2(340f, 220f);

        Image background = backgroundObject.AddComponent<Image>();
        background.color = new Color(0f, 0f, 0f, 0.65f);

        GameObject textObject = new GameObject("Health Text");
        textObject.transform.SetParent(backgroundObject.transform, false);

        RectTransform textRect = textObject.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = new Vector2(16f, 10f);
        textRect.offsetMax = new Vector2(-16f, -10f);

        healthText = textObject.AddComponent<TextMeshProUGUI>();
        healthText.fontSize = 30f;
        healthText.color = Color.white;
        healthText.alignment = TextAlignmentOptions.TopLeft;
        healthText.raycastTarget = false;
        healthText.text = "Esperando jugadores...";

        GameObject crosshairObject = new GameObject("Crosshair");
        crosshairObject.transform.SetParent(hudCanvasObject.transform, false);

        RectTransform crosshairRect = crosshairObject.AddComponent<RectTransform>();
        crosshairRect.anchorMin = new Vector2(0.5f, 0.5f);
        crosshairRect.anchorMax = new Vector2(0.5f, 0.5f);
        crosshairRect.pivot = new Vector2(0.5f, 0.5f);
        crosshairRect.anchoredPosition = Vector2.zero;
        crosshairRect.sizeDelta = new Vector2(60f, 60f);

        TextMeshProUGUI crosshairText = crosshairObject.AddComponent<TextMeshProUGUI>();
        crosshairText.text = "+";
        crosshairText.fontSize = 38f;
        crosshairText.color = Color.white;
        crosshairText.alignment = TextAlignmentOptions.Center;
        crosshairText.raycastTarget = false;
    }

    private void OnDestroy()
    {
        if (hudCanvasObject != null)
            Destroy(hudCanvasObject);
    }
}
