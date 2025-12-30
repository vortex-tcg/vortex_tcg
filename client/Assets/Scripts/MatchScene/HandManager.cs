using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using VortexTCG.Scripts.DTOs;

namespace VortexTCG.Scripts.MatchScene
{
    public class HandManager : MonoBehaviour
    {
        public static HandManager Instance { get; private set; }
        public Card CardPrefab => cardPrefab;
        public Transform HandRoot => handRoot;

        [Header("Hand Spawn")]
        [SerializeField] private Card cardPrefab;
        [SerializeField] private Transform handRoot;
        [SerializeField] private float cardSpacing = 1.2f;
        private const int MaxHandSize = 5;

        [HideInInspector] public Card SelectedCard;

        private readonly List<Card> handCards = new();
        private bool _playRequestInFlight;
        private Card _pendingCard;
        private CardSlot _pendingSlot;

        private CancellationTokenSource _pendingTimeoutCts;

        public bool HasPendingPlay => _playRequestInFlight;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        public void SetHand(List<DrawnCardDto> drawnCards)
        {
            ClearHand();
            AddCards(drawnCards);
        }

        public void AddCards(List<DrawnCardDto> drawnCards)
        {
            if (drawnCards == null || drawnCards.Count == 0) return;

            if (cardPrefab == null || handRoot == null)
            {
                Debug.LogError("[HandManager] cardPrefab ou handRoot non assigné.");
                return;
            }

            foreach (DrawnCardDto dto in drawnCards)
            {
                if (handCards.Count >= MaxHandSize) break;

                Card card = Instantiate(cardPrefab, handRoot);
                card.ApplyDTO(
                    dto.GameCardId.ToString(),
                    dto.Name,
                    dto.Hp,
                    dto.Attack,
                    dto.Cost,
                    dto.Description,
                    ""
                );

                EnsureCollider(card);
                handCards.Add(card);
            }

            LayoutHand();
        }

        public void SelectCard(Card card)
        {
            if (PhaseManager.Instance != null && PhaseManager.Instance.CurrentPhase == GamePhase.ATTACK)
                return;

            if (SelectedCard != null)
                SelectedCard.SetSelected(false);

            SelectedCard = card;

            if (SelectedCard != null)
                SelectedCard.SetSelected(true);
        }

        public void DeselectCurrentCard()
        {
            if (SelectedCard != null)
            {
                SelectedCard.SetSelected(false);
                SelectedCard = null;
            }
        }

        public async Task RequestPlaySelectedCard(CardSlot slot)
        {
            if (_playRequestInFlight)
            {
                Debug.Log("[HandManager] RequestPlaySelectedCard STOP: already in flight");
                return;
            }

            if (SelectedCard == null) return;
            if (slot == null) return;

            if (PhaseManager.Instance != null && PhaseManager.Instance.CurrentPhase != GamePhase.PLACEMENT)
                return;

            if (!slot.CanAccept(SelectedCard)) return;

            if (!int.TryParse(SelectedCard.cardId, out int gameCardId))
            {
                Debug.LogError("[HandManager] SelectedCard.cardId pas un int: " + SelectedCard.cardId);
                return;
            }

            SignalRClient client = SignalRClient.Instance;
            if (client == null || !client.IsConnected)
            {
                Debug.LogWarning("[HandManager] SignalRClient pas connecté.");
                return;
            }

            _playRequestInFlight = true;
            _pendingCard = SelectedCard;
            _pendingSlot = slot;

            Debug.Log($"[HandManager] RequestPlaySelectedCard -> PlayCard(gameCardId={gameCardId}, loc={slot.slotIndex})");
            StartPendingTimeout(2500);

            try
            {
                await client.PlayCard(gameCardId, slot.slotIndex);
            }
            catch (Exception ex)
            {
                Debug.LogError("[HandManager] PlayCard invoke failed: " + ex);
                CancelPendingPlay("Invoke exception");
            }
        }

        private void StartPendingTimeout(int ms)
        {
            try { _pendingTimeoutCts?.Cancel(); } catch { }

            _pendingTimeoutCts = new CancellationTokenSource();
            CancellationToken token = _pendingTimeoutCts.Token;

            _ = Task.Run(async () =>
            {
                try
                {
                    await Task.Delay(ms, token);
                    if (token.IsCancellationRequested) return;

                    if (_playRequestInFlight)
                    {
                        Debug.LogWarning("[HandManager] Pending play timeout -> unlock");
                        CancelPendingPlay("timeout");
                    }
                }
                catch
                {
                    /* ignore */
                }
            });
        }

        public void ConfirmPlayFromServer(int gameCardId, int location, bool canPlayed)
        {
            try { _pendingTimeoutCts?.Cancel(); } catch { }

            _playRequestInFlight = false;

            if (!canPlayed)
            {
                Debug.LogWarning("[HandManager] Serveur a refusé la pose (PlayCardResult canPlayed=false).");
                _pendingCard = null;
                _pendingSlot = null;
                return;
            }

            Card cardToPlace = null;

            if (_pendingCard != null && int.TryParse(_pendingCard.cardId, out int pendingId) && pendingId == gameCardId)
                cardToPlace = _pendingCard;
            else
                cardToPlace = FindCardInHand(gameCardId);

            if (cardToPlace == null)
            {
                Debug.LogWarning($"[HandManager] Carte {gameCardId} introuvable dans la main (déjà déplacée ?)");
                _pendingCard = null;
                _pendingSlot = null;
                return;
            }

            CardSlot slot = _pendingSlot;

            if (slot == null || slot.slotIndex != location)
            {
                Debug.LogWarning("[HandManager] Pending slot null ou location mismatch (prévois un mapping global si besoin).");
            }

            if (slot != null)
            {
                slot.PlaceCard(cardToPlace);
                handCards.Remove(cardToPlace);

                if (SelectedCard == cardToPlace) DeselectCurrentCard();
                LayoutHand();
            }

            _pendingCard = null;
            _pendingSlot = null;
        }

        public void CancelPendingPlay(string reason)
        {
            try { _pendingTimeoutCts?.Cancel(); } catch { }

            if (_playRequestInFlight)
                Debug.LogWarning("[HandManager] CancelPendingPlay -> " + reason);

            _playRequestInFlight = false;
            _pendingCard = null;
            _pendingSlot = null;
        }

        private Card FindCardInHand(int gameCardId)
        {
            foreach (Card c in handCards)
            {
                if (c == null) continue;
                if (int.TryParse(c.cardId, out int id) && id == gameCardId)
                    return c;
            }
            return null;
        }

        private void ClearHand()
        {
            DeselectCurrentCard();

            foreach (Card c in handCards)
                if (c != null) Destroy(c.gameObject);

            handCards.Clear();

            CancelPendingPlay("clear hand");
        }

        private void LayoutHand()
        {
            for (int i = 0; i < handCards.Count; i++)
            {
                Card c = handCards[i];
                if (c == null) continue;

                c.transform.localPosition = new Vector3(i * cardSpacing, 0f, 0f);
                c.transform.localRotation = Quaternion.identity;
            }
        }

        private static void EnsureCollider(Card card)
        {
            if (card == null) return;

            if (card.GetComponent<Collider>() == null)
            {
                BoxCollider bc = card.gameObject.AddComponent<BoxCollider>();
                bc.size = Vector3.one;
            }
        }
    }
}
