using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class MenuUI : MonoBehaviour
{
    [SerializeField] private UIDocument uiDocument;
    [SerializeField] private string quitButtonName = "QuitButton";
    [SerializeField] private bool loadHomeAfterSurrender = true;
    [SerializeField] private string homeSceneName = "HomeScene";

    private Button quitButton;

    private void OnEnable()
    {
        if (uiDocument == null)
            uiDocument = GetComponent<UIDocument>();

        if (uiDocument == null)
        {
            Debug.LogError("[MenuUI] UIDocument missing.");
            return;
        }

        VisualElement root = uiDocument.rootVisualElement;
        quitButton = root.Q<Button>(quitButtonName);

        if (quitButton == null)
        {
            Debug.LogWarning($"[MenuUI] Button '{quitButtonName}' not found in UXML.");
            return;
        }

        quitButton.clicked += OnQuitClicked;
    }

    private void OnDisable()
    {
        if (quitButton != null)
            quitButton.clicked -= OnQuitClicked;
    }

    private async void OnQuitClicked()
    {
        SignalRClient client = SignalRClient.Instance;

        if (client == null || !client.IsConnected)
        {
            Debug.LogWarning("[MenuUI] SignalRClient not connected, loading home scene directly.");
            LoadHomeIfConfigured();
            return;
        }

        try
        {
            await client.Surrender();
            Debug.Log("[MenuUI] Surrender sent successfully.");
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"[MenuUI] Surrender failed: {ex.Message}");
        }

        LoadHomeIfConfigured();
    }

    private void LoadHomeIfConfigured()
    {
        if (!loadHomeAfterSurrender)
            return;

        if (string.IsNullOrWhiteSpace(homeSceneName))
            return;

        SceneManager.LoadScene(homeSceneName);
    }
}
