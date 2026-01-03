using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System;

public class GameUIController : MonoBehaviour
{
    [Header("Refs")]
    public SignalRClient client;
    private static readonly Guid DeckGuid =
        Guid.Parse("d3b07384-d9a1-4d3b-92d8-4f5c6e7a8b9c");

    [Header("UI Inputs")]
    public TMP_InputField playerNameInput;
    public TMP_InputField roomCodeInput;
    public TMP_InputField messageInput;

    [Header("UI Buttons")]
    public Button connectBtn;
    public Button matchmakingBtn;
    public Button createRoomBtn;
    public Button joinRoomBtn;
    public Button leaveQueueBtn;
    public Button leaveRoomBtn;
    public Button sendBtn;

    [Header("UI Texts")]
    public TMP_Text statusText;
    public TMP_Text logText;

    private void OnEnable()
    {
        client.OnStatus += HandleStatus;
        client.OnLog += HandleLog;
        client.OnMatched += HandleMatched;
        client.OnOpponentLeft += HandleOpponentLeft;
        connectBtn.onClick.AddListener(OnClickConnect);
        matchmakingBtn.onClick.AddListener(OnClickMatchmaking);
        createRoomBtn.onClick.AddListener(OnClickCreateRoom);
        joinRoomBtn.onClick.AddListener(OnClickJoinRoom);
        leaveQueueBtn.onClick.AddListener(OnClickLeaveQueue);
        leaveRoomBtn.onClick.AddListener(OnClickLeaveRoom);
        sendBtn.onClick.AddListener(OnClickSend);
    }

    private void OnDisable()
    {
        client.OnStatus -= HandleStatus;
        client.OnLog -= HandleLog;
        client.OnMatched -= HandleMatched;
        client.OnOpponentLeft -= HandleOpponentLeft;

        connectBtn.onClick.RemoveListener(OnClickConnect);
        matchmakingBtn.onClick.RemoveListener(OnClickMatchmaking);
        createRoomBtn.onClick.RemoveListener(OnClickCreateRoom);
        joinRoomBtn.onClick.RemoveListener(OnClickJoinRoom);
        leaveQueueBtn.onClick.RemoveListener(OnClickLeaveQueue);
        leaveRoomBtn.onClick.RemoveListener(OnClickLeaveRoom);
        sendBtn.onClick.RemoveListener(OnClickSend);
    }

    // ----- Events du client -----
    private void HandleStatus(string s) => statusText.text = s;
    private void HandleLog(string s) => AppendLog(s);
    private void HandleMatched(string key) => AppendLog($"[MATCHED] Salle: {key}");
    private void HandleOpponentLeft() => AppendLog("L'adversaire a quitté.");

    private void AppendLog(string s)
    {
        if (logText == null) return;
        if (logText.text.Length > 4000) logText.text = "";
        logText.text += (logText.text.Length > 0 ? "\n" : "") + s;
    }

    // ----- Boutons -----
    private async void OnClickConnect()
    {
        await client.EnsureConnected(playerNameInput?.text);
    }

    private async void OnClickMatchmaking()
    {
        await client.JoinQueue(DeckGuid);
    }

    private async void OnClickCreateRoom()
    {
        string preferred = roomCodeInput?.text;
        await client.CreateRoom(DeckGuid, preferred);
    }


    private async void OnClickJoinRoom()
    {
        string code = roomCodeInput?.text;
        if (string.IsNullOrWhiteSpace(code)) { AppendLog("Entre un code."); return; }

        await client.JoinRoom(DeckGuid, code);
    }


    private async void OnClickLeaveQueue()
    {
        await client.LeaveQueue();
    }

    private async void OnClickLeaveRoom()
    {
        await client.LeaveRoomByCode();
    }

    private async void OnClickSend()
    {
        string txt = messageInput?.text;
        if (string.IsNullOrWhiteSpace(txt)) return;
        await client.SendMessageToPeer(txt);
        if (messageInput) messageInput.text = "";
    }
}
