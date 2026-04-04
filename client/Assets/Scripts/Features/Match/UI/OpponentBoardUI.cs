using System.Collections.Generic;
using System.Text;
using UnityEngine;
using VortexTCG.Scripts.DTOs;
using VortexTCG.Scripts.Features.Match.Services;
using VortexTCG.Scripts.MatchScene;

namespace VortexTCG.Scripts.MatchScene
{
    /// <summary>
    /// Gère l'interaction UI et l'instantiation des cartes du board adverse
    /// Wrapper MonoBehaviour autour d'OpponentBoardService
    /// À placer dans la scène Unity
    /// </summary>
    public class OpponentBoardUI : MonoBehaviour
    {
        public static OpponentBoardUI Instance { get; private set; }
        public OpponentBoardService OpponentBoardService { get; private set; }

        [Header("Slots ennemis (P2 = ADVERSAIRE)")]
        [SerializeField] private CardSlotUI[] enemySlots;

        [Header("Prefab carte (affichage adversaire)")]
        [SerializeField] private CardUI cardPrefab;

        private OpponentBoardService boardLogic;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            boardLogic = new OpponentBoardService();
            OpponentBoardService = boardLogic;

            Debug.Log("[OpponentBoardUI] Awake()");
            Debug.Log("[OpponentBoardUI] object=" + name + " active=" + gameObject.activeInHierarchy);
            Debug.Log("[OpponentBoardUI] cardPrefab=" + (cardPrefab != null ? cardPrefab.name : "NULL"));
            Debug.Log("[OpponentBoardUI] enemySlots=" + (enemySlots == null ? "NULL" : enemySlots.Length.ToString()));
        }

        public void PlaceOpponentCard(int location, GameCardDto playedCard)
        {
            Debug.Log("[OpponentBoardUI] PlaceOpponentCard location=" + location +
                      " playedCard=" + (playedCard != null ? playedCard.GameCardId.ToString() : "NULL"));

            if (enemySlots == null || enemySlots.Length == 0)
            {
                Debug.LogError("[OpponentBoardUI] enemySlots NULL/empty");
                return;
            }

            if (location < 0 || location >= enemySlots.Length)
            {
                Debug.LogError("[OpponentBoardUI] location out of range: " + location + " len=" + enemySlots.Length);
                return;
            }

            CardSlotUI slot = enemySlots[location];
            if (slot == null)
            {
                Debug.LogError("[OpponentBoardUI] slot NULL at index " + location);
                return;
            }

            Debug.Log("[OpponentBoardUI] TargetSlot name=" + slot.name +
                      " active=" + slot.gameObject.activeInHierarchy +
                      " slotIndex=" + slot.slotIndex);

            if (slot.CurrentCard != null)
            {
                Debug.LogWarning("[OpponentBoardUI] Slot already occupied. CurrentCard=" + slot.CurrentCard.cardId);
                return;
            }

            if (cardPrefab == null)
            {
                Debug.LogError("[OpponentBoardUI] cardPrefab not assigned");
                return;
            }

            CardUI c = Instantiate(cardPrefab, slot.transform, false);
            c.name = "EnemyCard_" + (playedCard != null ? playedCard.GameCardId.ToString() : "NULL");

            if (playedCard != null)
            {
                c.ApplyDTO(
                    playedCard.GameCardId.ToString(),
                    playedCard.Name,
                    playedCard.Hp,
                    playedCard.Attack,
                    playedCard.Cost,
                    playedCard.Description,
                    ""
                );
            }

            slot.PlaceCard(c);

            if (int.TryParse(c.cardId, out int id))
            {
                boardLogic.RegisterOpponentCard(id, c);
                Debug.Log($"[OpponentBoardUI] Registered opponent card: location={location}, GameCardId={id}, name={playedCard?.Name}");
            }
            else
            {
                Debug.LogError("[OpponentBoardUI] cardId not int: '" + c.cardId + "'");
            }

            LogBoardState("AFTER PlaceOpponentCard");
        }

        public void ResetBoard()
        {
            Debug.Log("[OpponentBoardUI] ResetBoard()");

            if (enemySlots != null)
            {
                for (int i = 0; i < enemySlots.Length; i++)
                {
                    CardSlotUI s = enemySlots[i];
                    if (s == null) continue;

                    if (s.CurrentCard != null)
                    {
                        Debug.Log("[OpponentBoardUI] Destroy enemy card on slot=" + s.name +
                                  " cardId=" + s.CurrentCard.cardId);
                        Destroy(s.CurrentCard.gameObject);
                    }

                    s.SetCurrentCard(null);
                }
            }

            boardLogic.ClearBoard();
        }

        public void ApplyOpponentAttackState(List<int> attackIds) => boardLogic.ApplyOpponentAttackState(attackIds);
        public void ApplyOpponentDefenseState(DefenseDataResponseDto data) => boardLogic.ApplyOpponentDefenseState(data);
        public void UpdateOpponentCardSnapshot(GameCardDto dto) => boardLogic.UpdateOpponentCardSnapshot(dto);
        public void RemoveOpponentCard(int gameCardId) => boardLogic.RemoveOpponentCard(gameCardId);

        public CardUI GetCardAtSlotIndex(int slotIndex)
        {
            if (enemySlots == null)
                return null;

            for (int i = 0; i < enemySlots.Length; i++)
            {
                CardSlotUI slot = enemySlots[i];
                if (slot != null && slot.slotIndex == slotIndex)
                    return slot.CurrentCard;
            }

            return null;
        }

        public CardUI FindOpponentCardByGameCardId(int gameCardId)
        {
            if (enemySlots == null)
                return null;

            for (int i = 0; i < enemySlots.Length; i++)
            {
                CardSlotUI slot = enemySlots[i];
                if (slot == null || slot.CurrentCard == null)
                    continue;

                if (int.TryParse(slot.CurrentCard.cardId, out int id) && id == gameCardId)
                    return slot.CurrentCard;
            }

            return null;
        }

        public bool IsCardOnOpponentBoard(CardUI card)
        {
            if (card == null || enemySlots == null)
                return false;

            CardSlotUI parentSlot = card.GetComponentInParent<CardSlotUI>();
            if (parentSlot == null)
                return false;

            for (int i = 0; i < enemySlots.Length; i++)
            {
                if (enemySlots[i] == parentSlot)
                    return true;
            }

            return false;
        }

        public void LogBoardStatePublic(string label) => boardLogic.LogBoardState(label);

        private void LogBoardState(string label)
        {
            if (enemySlots == null)
            {
                Debug.Log("[OpponentBoardUI] BoardState " + label + ": enemySlots=NULL");
                return;
            }

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("[OpponentBoardUI] BoardState " + label + ": enemySlots.Length=" + enemySlots.Length);

            for (int i = 0; i < enemySlots.Length; i++)
            {
                CardSlotUI s = enemySlots[i];
                if (s == null)
                {
                    sb.AppendLine("  [" + i + "] NULL");
                    continue;
                }

                string cc = (s.CurrentCard != null)
                    ? s.CurrentCard.cardId + "('" + s.CurrentCard.name + "')"
                    : "null";

                sb.AppendLine("  [" + i + "] slot='" + s.name + "' active=" + s.gameObject.activeInHierarchy +
                              " slotIndex=" + s.slotIndex + " CurrentCard=" + cc);
            }

            Debug.Log(sb.ToString());
        }
    }
}
