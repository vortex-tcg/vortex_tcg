using UnityEngine;

[CreateAssetMenu(fileName = "NetworkRef", menuName = "Vortex/Net/Network Ref")]
public class NetworkRef : ScriptableObject
{
    [SerializeField] private SignalRClient client; 

    public SignalRClient Client
    {
        get
        {
            if (client != null) return client;

            client = SignalRClient.Instance ?? Object.FindObjectOfType<SignalRClient>(true);
            if (client != null) return client;

            var go = new GameObject("NetworkRoot");
            client = go.AddComponent<SignalRClient>();
            return client;
        }
    }

    public void Bind(SignalRClient c) => client = c;

    public bool IsReady => client != null && client.IsConnected;
}
