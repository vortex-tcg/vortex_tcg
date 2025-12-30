using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using VortexTCG.Scripts.DTOs;

namespace VortexTCG.Scripts.MatchScene
{
    public class DefenseManager : MonoBehaviour
    {
        public static DefenseManager Instance { get; private set; }

        [SerializeField] private List<CardSlot> P1BoardSlots = new List<CardSlot>();
        [SerializeField] private List<CardSlot> P2BoardSlots = new List<CardSlot>();

        private readonly Dictionary<int, Card> boardCardsById = new Dictionary<int, Card>();
        private readonly Dictionary<Card, Card> defenseAssignments = new Dictionary<Card, Card>();

        private Card currentDefender;

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            if (PhaseManager.Instance != null)
            {
                PhaseManager.Instance.OnEnterDefense += OnEnterDefense;
                PhaseManager.Instance.OnEnterStandBy += OnExitDefense;
                PhaseManager.Instance.OnEnterAttack += OnExitDefense;
            }

            RegisterExistingCardsFromSlots();
            if (SignalRClient.Instance != null)
                SignalRClient.Instance.OnDefenseEngage += ApplyDefenseStateFromServer;
        }

        private void OnDestroy()
        {
            if (PhaseManager.Instance != null)
            {
                PhaseManager.Instance.OnEnterDefense -= OnEnterDefense;
                PhaseManager.Instance.OnEnterStandBy -= OnExitDefense;
                PhaseManager.Instance.OnEnterAttack -= OnExitDefense;
            }

            if (SignalRClient.Instance != null)
                SignalRClient.Instance.OnDefenseEngage -= ApplyDefenseStateFromServer;
        }

        private void OnEnterDefense()
        {
            ClearAllDefense();
        }

        private void OnExitDefense()
        {
            ClearAllDefense();
        }

        private void RegisterExistingCardsFromSlots()
        {
            if (P1BoardSlots != null)
            {
                for (int i = 0; i < P1BoardSlots.Count; i++)
                {
                    CardSlot slot = P1BoardSlots[i];
                    if (slot == null) continue;
                    if (slot.CurrentCard == null) continue;
                    RegisterCard(slot.CurrentCard);
                }
            }

            if (P2BoardSlots != null)
            {
                for (int i = 0; i < P2BoardSlots.Count; i++)
                {
                    CardSlot slot = P2BoardSlots[i];
                    if (slot == null) continue;
                    if (slot.CurrentCard == null) continue;
                    RegisterCard(slot.CurrentCard);
                }
            }
        }

        public void RegisterCard(Card card)
        {
            if (card == null) return;

            if (!int.TryParse(card.cardId, out int id))
                return;

            boardCardsById[id] = card;
        }

        public bool IsP1BoardSlot(CardSlot slot)
        {
            return slot != null && P1BoardSlots != null && P1BoardSlots.Contains(slot);
        }

        public bool IsP2BoardSlot(CardSlot slot)
        {
            return slot != null && P2BoardSlots != null && P2BoardSlots.Contains(slot);
        }

        public bool IsCardOnP1Board(Card card)
        {
            if (card == null) return false;
            CardSlot slot = card.GetComponentInParent<CardSlot>();
            return IsP1BoardSlot(slot);
        }

        public bool IsCardOnP2Board(Card card)
        {
            if (card == null) return false;
            CardSlot slot = card.GetComponentInParent<CardSlot>();
            return IsP2BoardSlot(slot);
        }

        public void HandleCardClicked(Card card)
        {
            if (card == null) return;
            if (PhaseManager.Instance == null) return;
            if (PhaseManager.Instance.CurrentPhase != GamePhase.DEFENSE) return;

            CardSlot slot = card.GetComponentInParent<CardSlot>();
            if (slot == null) return;
            if (IsP1BoardSlot(slot))
            {
                SelectDefender(card);
                return;
            }

            if (IsP2BoardSlot(slot))
            {
                _ = TryAssignDefenseAndSend(card);
            }
        }

        private void SelectDefender(Card defender)
        {
            if (defender == null) return;
            if (currentDefender == defender) return;

            if (currentDefender != null)
                currentDefender.SetDefenseSelected(false);

            currentDefender = defender;
            currentDefender.SetDefenseSelected(true);
        }

        private async Task TryAssignDefenseAndSend(Card targetAttacker)
        {
            if (currentDefender == null) return;
            if (targetAttacker == null) return;
            if (!targetAttacker.IsAttackingOutlineActive()) return;

            if (!int.TryParse(currentDefender.cardId, out int defenderId))
                return;

            if (!int.TryParse(targetAttacker.cardId, out int attackerId))
                return;

            SignalRClient client = SignalRClient.Instance;
            if (client == null) return;

            if (defenseAssignments.ContainsKey(currentDefender))
                defenseAssignments.Remove(currentDefender);

            defenseAssignments[currentDefender] = targetAttacker;

            try
            {
                await client.HandleDefensePos(defenderId, attackerId);
            }
            catch (Exception)
            {
                if (defenseAssignments.ContainsKey(currentDefender) && defenseAssignments[currentDefender] == targetAttacker)
                    defenseAssignments.Remove(currentDefender);
            }
        }


        public void ApplyDefenseStateFromServer(DefenseDataResponseDto dto)
        {
            ClearAllDefense();

            if (dto == null) return;
            if (dto.DefenseCards == null) return;

            for (int i = 0; i < dto.DefenseCards.Count; i++)
            {
                DefenseCardDataDto pair = dto.DefenseCards[i];

                Card defenderCard = FindBoardCardById(pair.cardId);
                Card attackerCard = FindBoardCardById(pair.opponentCardId);

                if (defenderCard == null) continue;
                if (attackerCard == null) continue;

                defenderCard.SetDefenseSelected(true);
                defenseAssignments[defenderCard] = attackerCard;
            }

            currentDefender = null;
        }

        private Card FindBoardCardById(int id)
        {
            if (boardCardsById.TryGetValue(id, out Card found) && found != null)
                return found;
            if (P1BoardSlots != null)
            {
                for (int i = 0; i < P1BoardSlots.Count; i++)
                {
                    CardSlot slot = P1BoardSlots[i];
                    if (slot == null) continue;
                    if (slot.CurrentCard == null) continue;

                    if (int.TryParse(slot.CurrentCard.cardId, out int cid) && cid == id)
                    {
                        RegisterCard(slot.CurrentCard);
                        return slot.CurrentCard;
                    }
                }
            }

            if (P2BoardSlots != null)
            {
                for (int i = 0; i < P2BoardSlots.Count; i++)
                {
                    CardSlot slot = P2BoardSlots[i];
                    if (slot == null) continue;
                    if (slot.CurrentCard == null) continue;

                    if (int.TryParse(slot.CurrentCard.cardId, out int cid) && cid == id)
                    {
                        RegisterCard(slot.CurrentCard);
                        return slot.CurrentCard;
                    }
                }
            }

            return null;
        }

        public void ClearAllDefense()
        {
            if (currentDefender != null)
            {
                currentDefender.SetDefenseSelected(false);
                currentDefender = null;
            }

            foreach (KeyValuePair<Card, Card> kvp in defenseAssignments)
            {
                if (kvp.Key != null)
                    kvp.Key.SetDefenseSelected(false);
            }

            defenseAssignments.Clear();
        }
    }
}
