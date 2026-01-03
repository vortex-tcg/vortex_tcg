using System;
using UnityEngine;

[CreateAssetMenu(fileName = "NetworkRef", menuName = "Vortex/Net/Network Ref")]
public class NetworkRef : ScriptableObject
{
    [SerializeField] private SignalRClient client;
    [NonSerialized] private int playerPosition; 
    [NonSerialized] private string roomKeyOrCode;

    public int PlayerPosition => playerPosition;
    public string RoomKeyOrCode => roomKeyOrCode;

    public void SetMatch(string keyOrCode, int pos)
    {
        roomKeyOrCode = keyOrCode;
        playerPosition = pos;
        Debug.Log($"[NetworkRef] SetMatch asset={name} key={keyOrCode} pos={pos}");

    }

    public void ResetMatch()
    {
        roomKeyOrCode = null;
        playerPosition = 0;
    }

    public SignalRClient Client
    {
        get
        {
            if (client != null)
            {
                client.BindNetworkRef(this); 
                return client;
            }

            client = SignalRClient.Instance
                     ?? UnityEngine.Object.FindAnyObjectByType<SignalRClient>(FindObjectsInactive.Include);

            if (client != null)
            {
                client.BindNetworkRef(this);  
                return client;
            }

            GameObject go = new GameObject("NetworkRoot");
            client = go.AddComponent<SignalRClient>();
            client.BindNetworkRef(this);       
            return client;
        }
    }


    public void Bind(SignalRClient c) => client = c;
    public bool IsReady => client != null && client.IsConnected;
}