using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using VortexTCG.Scripts.Features.Match.Events;
using VortexTCG.Scripts.Features.Match.Services;
using VortexTCG.Scripts.Features.Match.UI;
using VortexTCG.Scripts.MatchScene;

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

        ResetLocalMatchState();

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

    private static void ResetLocalMatchState()
    {
        PhaseService.Instance?.ResetPhase();
        AttackUI.Instance?.ResetBoard();
        DefenseUI.Instance?.ClearAllDefense();
        OpponentBoardUI.Instance?.ResetBoard();
        OpponentUI.Instance?.ResetBoard();
        HandUI.Instance?.ClearHand();
        MatchEvents.ResetAll();
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
