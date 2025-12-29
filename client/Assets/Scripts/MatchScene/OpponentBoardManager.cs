using System.Collections.Generic;
using System.Text;
using UnityEngine;
using VortexTCG.Scripts.DTOs;

namespace VortexTCG.Scripts.MatchScene
{
    public class OpponentBoardManager : MonoBehaviour
    {
        public static OpponentBoardManager Instance { get; private set; }

        [Header("Slots ennemis (P2 = ADVERSAIRE)")]
        [SerializeField] private CardSlot[] enemySlots;

        [Header("Prefab carte (affichage adversaire)")]
        [SerializeField] private Card cardPrefab;

        private readonly Dictionary<int, Card> opponentCardsById = new Dictionary<int, Card>();

        // On buffer le dernier état pour le rejouer quand une carte adverse arrive après coup
        private List<int> lastOpponentAttackIds;
        private DefenseDataResponseDto lastOpponentDefenseState;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;

            Debug.Log("[OpponentBoardManager] Awake()");
            Debug.Log("[OpponentBoardManager] object=" + name + " active=" + gameObject.activeInHierarchy);
            Debug.Log("[OpponentBoardManager] cardPrefab=" + (cardPrefab != null ? cardPrefab.name : "NULL"));
            Debug.Log("[OpponentBoardManager] enemySlots=" + (enemySlots == null ? "NULL" : enemySlots.Length.ToString()));
        }

        public void PlaceOpponentCard(int location, GameCardDto playedCard)
        {
            Debug.Log("[OpponentBoardManager] PlaceOpponentCard location=" + location +
                      " playedCard=" + (playedCard != null ? playedCard.GameCardId.ToString() : "NULL"));

            if (enemySlots == null || enemySlots.Length == 0)
            {
                Debug.LogError("[OpponentBoardManager] enemySlots NULL/empty");
                return;
            }

            if (location < 0 || location >= enemySlots.Length)
            {
                Debug.LogError("[OpponentBoardManager] location out of range: " + location + " len=" + enemySlots.Length);
                return;
            }

            CardSlot slot = enemySlots[location];
            if (slot == null)
            {
                Debug.LogError("[OpponentBoardManager] slot NULL at index " + location);
                return;
            }

            Debug.Log("[OpponentBoardManager] TargetSlot name=" + slot.name +
                      " active=" + slot.gameObject.activeInHierarchy +
                      " slotIndex=" + slot.slotIndex);

            if (slot.CurrentCard != null)
            {
                Debug.LogWarning("[OpponentBoardManager] Slot already occupied. CurrentCard=" + slot.CurrentCard.cardId);
                return;
            }

            if (cardPrefab == null)
            {
                Debug.LogError("[OpponentBoardManager] cardPrefab not assigned");
                return;
            }

            Card c = Instantiate(cardPrefab, slot.transform, false);
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
                opponentCardsById[id] = c;
                Debug.Log("[OpponentBoardManager] Registered opponent card id=" + id +
                          " dictCount=" + opponentCardsById.Count);
            }
            else
            {
                Debug.LogError("[OpponentBoardManager] cardId not int: '" + c.cardId + "'");
            }

            // Rejouer le dernier état connu (utile si l’état arrive avant que la carte soit posée)
            if (lastOpponentDefenseState != null)
                ApplyOpponentDefenseState(lastOpponentDefenseState);
            else if (lastOpponentAttackIds != null)
                ApplyOpponentAttackState(lastOpponentAttackIds);

            LogBoardState("AFTER PlaceOpponentCard");
        }

        public void ResetBoard()
        {
            Debug.Log("[OpponentBoardManager] ResetBoard()");

            if (enemySlots != null)
            {
                for (int i = 0; i < enemySlots.Length; i++)
                {
                    CardSlot s = enemySlots[i];
                    if (s == null) continue;

                    if (s.CurrentCard != null)
                    {
                        Debug.Log("[OpponentBoardManager] Destroy enemy card on slot=" + s.name +
                                  " cardId=" + s.CurrentCard.cardId);
                        Destroy(s.CurrentCard.gameObject);
                    }

                    s.CurrentCard = null;
                }
            }

            opponentCardsById.Clear();
            lastOpponentAttackIds = null;
            lastOpponentDefenseState = null;
        }

        // ✅ NOUVEAU : appelé quand TU reçois "HandleOpponentAttackEngage" (List<int>)
        public void ApplyOpponentAttackState(List<int> attackIds)
        {
            lastOpponentAttackIds = attackIds;
            lastOpponentDefenseState = null; // si on a un nouvel état d’attaque, on reset le defense buffer

            Debug.Log("[OpponentBoardManager] ApplyOpponentAttackState ids=" +
                      (attackIds == null ? "NULL" : string.Join(",", attackIds)));

            ClearOpponentAttackOutline();

            if (attackIds == null || attackIds.Count == 0) return;

            int found = 0;
            int missing = 0;

            for (int i = 0; i < attackIds.Count; i++)
            {
                int id = attackIds[i];

                if (opponentCardsById.TryGetValue(id, out Card card) && card != null)
                {
                    card.SetOpponentAttacking(true);
                    found++;
                    Debug.Log("[OpponentBoardManager] Attack OUTLINE ON for opponent card id=" + id + " name=" + card.name);
                }
                else
                {
                    missing++;
                    Debug.LogWarning("[OpponentBoardManager] Attack card id not found on opponent board: " + id);
                }
            }

            Debug.Log("[OpponentBoardManager] ApplyOpponentAttackState done found=" + found + " missing=" + missing);
        }

        // ✅ NOUVEAU : appelé quand TU reçois "HandleOpponentDefenseEngage" (DefenseDataResponseDto)
        // Ici on met l’outline sur les cartes ATTAQUANTES (côté adversaire) pendant ta phase DEFENSE
        public void ApplyOpponentDefenseState(DefenseDataResponseDto data)
        {
            lastOpponentDefenseState = data;

            Debug.Log("[OpponentBoardManager] ApplyOpponentDefenseState defenses=" +
                      (data?.DefenseCards == null ? "NULL" : data.DefenseCards.Count.ToString()));

            ClearOpponentAttackOutline();

            if (data == null || data.AttackCardsId == null || data.AttackCardsId.Count == 0)
                return;

            int found = 0;
            int missing = 0;

            for (int i = 0; i < data.AttackCardsId.Count; i++)
            {
                int id = data.AttackCardsId[i];

                if (opponentCardsById.TryGetValue(id, out Card card) && card != null)
                {
                    card.SetOpponentAttacking(true);
                    found++;
                    Debug.Log("[OpponentBoardManager] (Defense) Attack OUTLINE ON for opponent card id=" + id + " name=" + card.name);
                }
                else
                {
                    missing++;
                    Debug.LogWarning("[OpponentBoardManager] (Defense) attack card id not found on opponent board: " + id);
                }
            }

            Debug.Log("[OpponentBoardManager] ApplyOpponentDefenseState done found=" + found + " missing=" + missing);
        }

        public void ClearOpponentAttackOutline()
        {
            Debug.Log("[OpponentBoardManager] ClearOpponentAttackOutline()");

            if (enemySlots == null) return;

            for (int i = 0; i < enemySlots.Length; i++)
            {
                CardSlot s = enemySlots[i];
                if (s == null) continue;
                if (s.CurrentCard == null) continue;

                s.CurrentCard.SetOpponentAttacking(false);
            }
        }

        private void LogBoardState(string label)
        {
            if (enemySlots == null)
            {
                Debug.Log("[OpponentBoardManager] BoardState " + label + ": enemySlots=NULL");
                return;
            }

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("[OpponentBoardManager] BoardState " + label + ": enemySlots.Length=" + enemySlots.Length);
            sb.AppendLine("[OpponentBoardManager] opponentCardsById.Count=" + opponentCardsById.Count);

            for (int i = 0; i < enemySlots.Length; i++)
            {
                CardSlot s = enemySlots[i];
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
