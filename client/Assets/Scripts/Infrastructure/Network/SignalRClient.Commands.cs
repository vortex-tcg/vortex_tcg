using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using UnityEngine;

public partial class SignalRClient
{
    public async Task JoinQueue(Guid deckId)
    {
        RequireConnectedOrThrow();
        _mode = "queue";
        Debug.Log($"[SignalRClient] -> JoinQueue({deckId})");
        await SafeInvoke("JoinQueue", deckId);
    }

    public async Task LeaveQueue()
    {
        if (_conn == null) return;
        networkRef?.ResetMatch();
        await SafeSend("LeaveQueue");
        _currentKeyOrCode = null;
        OnLog?.Invoke("Quitte la file/room (matchmaking).");
        _startGameRequested = false;
    }

    public async Task CreateRoom(Guid deckId, string preferredCode = null)
    {
        RequireConnectedOrThrow();
        _mode = "code";
        await SafeInvoke("CreateRoom", deckId, string.IsNullOrWhiteSpace(preferredCode) ? null : preferredCode);
    }

    public async Task JoinRoom(Guid deckId, string code)
    {
        RequireConnectedOrThrow();
        _mode = "code";
        await SafeInvoke("JoinRoom", deckId, code?.Trim());
    }

    public async Task LeaveRoomByCode()
    {
        if (_conn == null) return;
        await SafeSend("LeaveRoomByCode");
        networkRef?.ResetMatch();
        _currentKeyOrCode = null;
        OnLog?.Invoke("Quitte la room (code).");
        _startGameRequested = false;
    }

    public async Task SendMessageToPeer(string text)
    {
        if (string.IsNullOrWhiteSpace(_currentKeyOrCode)) return;

        if (_mode == "code")
            await SafeSend("SendRoomMessageByCode", _currentKeyOrCode, text);
        else
            await SafeSend("SendRoomMessage", _currentKeyOrCode, text);

        OnLog?.Invoke($"Moi: {text}");
    }

    public async Task PlayCard(int cardId, int location)
    {
        RequireConnectedOrThrow();
        Debug.Log($"[SignalRClient] -> PlayCard(cardId={cardId}, location={location})");
        await SafeInvoke("PlayCard", cardId, location);
    }

    public async Task StartGame()
    {
        RequireConnectedOrThrow();
        await SafeInvoke("StartGame");
    }

    public async Task ChangePhase()
    {
        RequireConnectedOrThrow();
        await SafeInvoke("ChangePhase");
    }

    public async Task DrawCards(int playerPosition, int amount)
    {
        RequireConnectedOrThrow();
        Debug.Log($"[SignalRClient] -> Invoke DrawCards(pos={playerPosition}, amount={amount})");
        await SafeInvoke("DrawCards", playerPosition, amount);
    }

    public async Task HandleAttackPos(int cardId)
    {
        if (_conn == null) return;
        await _conn.InvokeAsync("HandleAttackPos", cardId);
    }

    public async Task HandleDefensePos(int cardId, int opponentCardId)
    {
        if (_conn == null) return;
        await _conn.InvokeAsync("HandleDefensePos", cardId, opponentCardId);
    }

    public bool IsConnected => _conn != null && _conn.State == HubConnectionState.Connected;
    public string CurrentKeyOrCode => _currentKeyOrCode;
    public string Mode => _mode;
}
