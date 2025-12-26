using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using VortexTCG.Scripts.DTOs;

namespace VortexTCG.Scripts.MatchScene
{
    public class MatchController : MonoBehaviour
    {
        [SerializeField] private HandManager handManager;
        [SerializeField] private GraveyardManager graveyardManager;
        [SerializeField] private NetworkRef networkRef;
        [SerializeField] private int initialHandSize = 5;

        private SignalRClient client;

        public enum HandUpdateMode { Replace, Append }
        private readonly Queue<HandUpdateMode> _pendingHandModes = new();

        private bool initialDrawRequested;
        private bool _startStandbyBonusDone;

        private void OnEnable()
        {
            client = SignalRClient.Instance;
            if (client == null)
            {
                Debug.LogError("[MatchController3D] SignalRClient.Instance NULL");
                return;
            }

            if (handManager == null) handManager = HandManager.Instance;
            if (graveyardManager == null) graveyardManager = GraveyardManager.Instance;

            client.OnCardsDrawn += HandleCardsDrawn;

            if (PhaseManager.Instance != null)
                PhaseManager.Instance.OnEnterStandBy += HandleEnterStandByDraw;

            RequestInitialHand();
        }

        private void OnDisable()
        {
            if (client != null) client.OnCardsDrawn -= HandleCardsDrawn;
            if (PhaseManager.Instance != null)
                PhaseManager.Instance.OnEnterStandBy -= HandleEnterStandByDraw;
        }

        private async void HandleEnterStandByDraw()
        {
            try
            {
                await RequestDraw(1, HandUpdateMode.Append);
            }
            catch (Exception ex)
            {
                Debug.LogError("[MatchController3D] StandBy draw failed: " + ex);
            }
        }

        private async Task RequestDraw(int amount, HandUpdateMode mode)
        {
            if (client == null || !client.IsConnected) return;
            if (networkRef == null || networkRef.Client == null) return;

            int pos = networkRef.PlayerPosition;
            if (pos != 1 && pos != 2)
            {
                Debug.LogError($"[MatchController3D] PlayerPosition invalide: {pos}");
                return;
            }

            _pendingHandModes.Enqueue(mode);
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
                Debug.LogError("[MatchController3D] RequestInitialHand exception: " + ex);
                initialDrawRequested = false;
            }
        }

        private void HandleCardsDrawn(DrawResultForPlayerDto result)
        {
            int handAdded = result?.DrawnCards?.Count ?? 0;
            int burned = result?.SentToGraveyard?.Count ?? 0;
            Debug.Log($"[MatchController3D] CardsDrawn received. hand+={handAdded} burned={burned}");

            if (handManager == null) return;

            HandUpdateMode mode = _pendingHandModes.Count > 0
                ? _pendingHandModes.Dequeue()
                : HandUpdateMode.Append;

            if (mode == HandUpdateMode.Replace)
                graveyardManager?.ResetGraveyard();
            if (burned > 0)
                graveyardManager?.AddCards(result.SentToGraveyard);
            if (result?.DrawnCards != null)
            {
                if (mode == HandUpdateMode.Replace) handManager.SetHand(result.DrawnCards);
                else handManager.AddCards(result.DrawnCards);
            }
    
            if (!_startStandbyBonusDone
                && mode == HandUpdateMode.Replace
                && PhaseManager.Instance != null
                && PhaseManager.Instance.CurrentPhase == GamePhase.StandBy)
            {
                _startStandbyBonusDone = true;
                _ = RequestDraw(1, HandUpdateMode.Append);
            }
        }
    }
}
