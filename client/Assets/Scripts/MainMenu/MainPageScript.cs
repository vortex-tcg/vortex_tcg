    using System;
    using System.Threading.Tasks;
    using TMPro;
    using UnityEngine;
    using UnityEngine.SceneManagement;
    using UnityEngine.UI;

    public class MainPageMenu : MonoBehaviour
    {
        [Header("UI")]
        public Button searchOpponentButton;   
        public Button inviteFriendButton;     
        public TMP_Text statusText;          
        public GameObject searchingPanel;     

        [SerializeField] private NetworkRef networkRef;
    
        [Header("Options")]
        [Tooltip("Si aucune connexion n’existe, le menu la crée & s’identifie ici.")]
        public bool connectHereIfNeeded = true;
        public bool verboseLogs = true;

        [Header("Navigation")]
        [SerializeField] private string matchSceneName = "MatchScene";

        private SignalRClient client;

        private void OnEnable()
        {
            client = (networkRef != null && networkRef.Client != null)
                ? networkRef.Client
                : SignalRClient.Instance;

            if (searchOpponentButton != null)
                searchOpponentButton.onClick.AddListener(OnClickSearchOpponent);

            if (inviteFriendButton != null)
                inviteFriendButton.onClick.AddListener(OnClickInviteFriend);

            if (client != null)
            {
                Subscribe(client);
                if (client.IsConnected)
                {
                    SetStatus("Connecté, prêt.");
                }
                else
                {
                    SetStatus("Connexion en cours…");
                    if (connectHereIfNeeded)
                    {
                        Task<bool> _ = ConnectIfNeeded();
                    }
                }
            }
            else
            {
                SetStatus("Non connecté.");
                if (connectHereIfNeeded)
                {
                    Task<bool> _ = ConnectIfNeeded();
                }
            }
            if (searchingPanel != null)
                searchingPanel.SetActive(false);
        }

        private void OnDisable()
        {
            if (searchOpponentButton != null)
                searchOpponentButton.onClick.RemoveListener(OnClickSearchOpponent);

            if (inviteFriendButton != null)
                inviteFriendButton.onClick.RemoveListener(OnClickInviteFriend);

            if (client != null)
                Unsubscribe(client);
        }

        private void Subscribe(SignalRClient c)
        {
            c.OnStatus       += HandleStatus;
            c.OnMatched      += HandleMatched;
            c.OnOpponentLeft += HandleOpponentLeft;
            c.OnLog          += HandleLog;
        }

        private void Unsubscribe(SignalRClient c)
        {
            c.OnStatus       -= HandleStatus;
            c.OnMatched      -= HandleMatched;
            c.OnOpponentLeft -= HandleOpponentLeft;
            c.OnLog          -= HandleLog;
        }
        private async Task<bool> ConnectIfNeeded()
        {
            client = SignalRClient.Instance;
            if (client == null)
            {
                if (!connectHereIfNeeded)
                {
                    SetStatus("Réseau non prêt. Retour via Login.");
                    return false;
                }

                Log("[MainPage] Création d’un SignalRClient (menu).");
                GameObject go = new GameObject("NetworkRoot");
                client = go.AddComponent<SignalRClient>();
                Subscribe(client);
            }

            AppConfig cfg = ConfigLoader.Load();
            string hubUrl = ConfigLoader.BuildGameHubUrl(cfg);
            if (!string.IsNullOrWhiteSpace(hubUrl))
                client.hubUrl = hubUrl;

            if (Jwt.I != null && Jwt.I.IsJwtPresent())
                client.SetAuthToken(Jwt.I.Token);

            if (client.IsConnected)
                return true;

            string displayName = "UnityPlayer";
            string email;
            if (Jwt.I != null && Jwt.I.TryGetClaim("email", out email) && !string.IsNullOrEmpty(email))
                displayName = email.Split('@')[0];

            SetStatus("Connexion au serveur…");
            await client.ConnectAndIdentify(displayName);

            bool ok = await WaitUntilConnected(client, 6f);
            SetStatus(ok ? "Connecté, prêt." : "Pas connecté.");
            return ok;
        }

        private static async Task<bool> WaitUntilConnected(SignalRClient c, float timeoutSeconds)
        {
            float start = Time.realtimeSinceStartup;
            while (!c.IsConnected && (Time.realtimeSinceStartup - start) < timeoutSeconds)
                await Task.Yield();
            return c.IsConnected;
        } 
        private async void OnClickSearchOpponent()
        {
            if (searchOpponentButton != null)
                searchOpponentButton.interactable = false;
            if (inviteFriendButton != null)
                inviteFriendButton.interactable = false;

            try
            {
                bool connected = await ConnectIfNeeded();
                if (!connected)
                {
                    SetStatus("Connexion impossible.");
                    return;
                }

                SetStatus("Recherche d’un adversaire…");
                if (searchingPanel != null)
                    searchingPanel.SetActive(true);

                await client.JoinQueue();
                Log("[MainPage] JoinQueue envoyé.");
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                SetStatus("Erreur matchmaking.");
                if (searchingPanel != null)
                    searchingPanel.SetActive(false);
            }
            finally
            {
                if (searchOpponentButton != null)
                    searchOpponentButton.interactable = true;
                if (inviteFriendButton != null)
                    inviteFriendButton.interactable = true;
            }
        }

        private void OnClickInviteFriend()
        {
            Log("[MainPage] Inviter un ami (non implémenté pour l’instant).");
        }

        private void HandleStatus(string s)
        {
            SetStatus(s);
        }

        private void HandleLog(string s)
        {
            Log("[SignalR] " + s);
        }

        private void HandleMatched(string roomKey)
        {
            Log("[MAIN] HandleMatched reçu, salle: " + roomKey);
            if (searchingPanel != null)
                searchingPanel.SetActive(false);
            SceneManager.LoadScene(matchSceneName);
        }

        private void HandleOpponentLeft()
        {
            if (searchingPanel != null)
                searchingPanel.SetActive(false);

            SetStatus("L’adversaire a quitté.");
        }
        private void SetStatus(string msg)
        {
            if (statusText != null)
                statusText.text = msg;

            Log("[STATUS] " + msg);
        }

        private void Log(string m)
        {
            if (verboseLogs)
                Debug.Log(m);
        }
    }
