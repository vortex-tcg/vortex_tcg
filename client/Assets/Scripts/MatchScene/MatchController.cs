using System;
using UnityEngine;
using DrawDTOs;
using System.Collections.Generic;
using System.Threading.Tasks;      

public class MatchController : MonoBehaviour
{
    [SerializeField] private HandManager handManager;
    [SerializeField] private int initialHandSize = 5;
    [SerializeField] private NetworkRef networkRef;
    private SignalRClient client;
    public enum HandUpdateMode { Replace, Append }
    private bool _startStandbyBonusDone;

    private readonly Queue<HandUpdateMode> _pendingHandModes = new();

    private bool initialDrawRequested;

    private void OnEnable()
    {
        Debug.Log("[MatchController] OnEnable");

        client = SignalRClient.Instance;
        if (client == null)
        {
            Debug.LogError("[MatchController] SignalRClient.Instance NULL");
            return;
        }

        Debug.Log($"[MatchController] client.IsConnected={client.IsConnected} keyOrCode={client.CurrentKeyOrCode} mode={client.Mode}");

        client.OnCardsDrawn += HandleCardsDrawn;
        client.OnLog += HandleNetLog; 
        if (PhaseManager.Instance != null)
            PhaseManager.Instance.OnEnterStandBy += HandleEnterStandByDraw;

        RequestInitialHand();
    }

    private void OnDisable()
    {
        Debug.Log("[MatchController] OnDisable");
        if (client != null)
        {
            client.OnCardsDrawn -= HandleCardsDrawn;
            client.OnLog -= HandleNetLog;
        }
        if (PhaseManager.Instance != null)
            PhaseManager.Instance.OnEnterStandBy -= HandleEnterStandByDraw;

    }
    private async void HandleEnterStandByDraw()
    {
        try
        {
            await RequestDraw(1, HandUpdateMode.Append);
            Debug.Log("[MatchController] Enter StandBy -> drew 1 card");
        }
        catch (Exception ex)
        {
            Debug.LogError("[MatchController] StandBy draw failed: " + ex);
        }
    }

    private void HandleNetLog(string s)
    {
        Debug.Log("[SignalR] " + s);
    }

    private async Task RequestDraw(int amount, HandUpdateMode mode)
    {
        if (!client.IsConnected) return;

        int pos = networkRef.PlayerPosition;
        if (pos != 1 && pos != 2)
        {
            Debug.LogError($"[MatchController] PlayerPosition invalide: {pos}");
            return;
        }

        _pendingHandModes.Enqueue(mode);

        Debug.Log($"[MatchController] -> DrawCards(amount={amount}, mode={mode})");
        await networkRef.Client.DrawCards(pos, amount);
    }
    private async void RequestInitialHand()
    {
        if (initialDrawRequested) return;
        initialDrawRequested = true;

        try
        {
            await RequestDraw(initialHandSize, HandUpdateMode.Replace);
        }
        catch (Exception ex)
        {
            Debug.LogError("[MatchController] RequestInitialHand exception: " + ex);
            initialDrawRequested = false;
        }
    }


    private void HandleCardsDrawn(DrawResultForPlayerDTO result)
    {
        int count = result?.DrawnCards?.Count ?? -1;
        Debug.Log($"[MatchController] CardsDrawn received. count={count}");

        if (result?.DrawnCards == null || handManager == null) return;

        HandUpdateMode mode = _pendingHandModes.Count > 0
            ? _pendingHandModes.Dequeue()
            : HandUpdateMode.Append;

        if (mode == HandUpdateMode.Replace)
            handManager.SetHand(result.DrawnCards);
        else
            handManager.AddCards(result.DrawnCards);
        if (!_startStandbyBonusDone
            && mode == HandUpdateMode.Replace
            && PhaseManager.Instance != null
            && PhaseManager.Instance.CurrentPhase == GamePhase.StandBy)
        {
            _startStandbyBonusDone = true;
            _ = RequestDraw(1, HandUpdateMode.Append);
            Debug.Log("[MatchController] Start StandBy bonus draw +1");
        }
    }

}
