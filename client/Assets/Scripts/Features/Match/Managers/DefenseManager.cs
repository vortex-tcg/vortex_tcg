using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using VortexTCG.Scripts.DTOs;
using VortexTCG.Scripts.Features.Match.Services;

namespace VortexTCG.Scripts.MatchScene
{
    public class DefenseManager : MonoBehaviour
    {
        public static DefenseManager Instance { get; private set; }

        [SerializeField] private List<CardSlotUI> P1BoardSlots = new List<CardSlotUI>();
        [SerializeField] private List<CardSlotUI> P2BoardSlots = new List<CardSlotUI>();

        private readonly Dictionary<int, CardUI> boardCardsById = new Dictionary<int, CardUI>();
        private readonly Dictionary<CardUI, CardUI> defenseAssignments = new Dictionary<CardUI, CardUI>();

        private CardUI currentDefender;

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            if (PhaseService.Instance != null)
            {
                PhaseService.Instance.OnEnterDefense += OnEnterDefense;
                PhaseService.Instance.OnEnterStandBy += OnExitDefense;
                PhaseService.Instance.OnEnterAttack += OnExitDefense;
            }

            RegisterExistingCardsFromSlots();
            if (SignalRClient.Instance != null)
                SignalRClient.Instance.OnDefenseEngage += ApplyDefenseStateFromServer;
        }

        private void OnDestroy()
        {
            if (PhaseService.Instance != null)
            {
                PhaseService.Instance.OnEnterDefense -= OnEnterDefense;
                PhaseService.Instance.OnEnterStandBy -= OnExitDefense;
                PhaseService.Instance.OnEnterAttack -= OnExitDefense;
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
                    CardSlotUI slot = P1BoardSlots[i];
                    if (slot == null) continue;
                    if (slot.CurrentCard == null) continue;
                    RegisterCard(slot.CurrentCard);
                }
            }

            if (P2BoardSlots != null)
            {
                for (int i = 0; i < P2BoardSlots.Count; i++)
                {
                    CardSlotUI slot = P2BoardSlots[i];
                    if (slot == null) continue;
                    if (slot.CurrentCard == null) continue;
                    RegisterCard(slot.CurrentCard);
                }
            }
        }

        public void RegisterCard(CardUI card)
        {
            if (card == null) return;

            if (!int.TryParse(card.cardId, out int id))
                return;

            boardCardsById[id] = card;
        }

        public bool IsP1BoardSlot(CardSlotUI slot)
        {
            return slot != null && P1BoardSlots != null && P1BoardSlots.Contains(slot);
        }

        public bool IsP2BoardSlot(CardSlotUI slot)
        {
            return slot != null && P2BoardSlots != null && P2BoardSlots.Contains(slot);
        }

        public bool IsCardOnP1Board(CardUI card)
        {
            if (card == null) return false;
            CardSlotUI slot = card.GetComponentInParent<CardSlotUI>();
            return IsP1BoardSlot(slot);
        }

        public bool IsCardOnP2Board(CardUI card)
        {
            if (card == null) return false;
            CardSlotUI slot = card.GetComponentInParent<CardSlotUI>();
            return IsP2BoardSlot(slot);
        }

        public void HandleCardClicked(CardUI card)
        {
            if (card == null) return;
            if (PhaseService.Instance == null) return;
            if (PhaseService.Instance.CurrentPhase != GamePhase.DEFENSE) return;

            CardSlotUI slot = card.GetComponentInParent<CardSlotUI>();
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

        private void SelectDefender(CardUI defender)
        {
            if (defender == null) return;
            if (currentDefender == defender) return;

            if (currentDefender != null)
                currentDefender.SetDefenseSelected(false);

            currentDefender = defender;
            currentDefender.SetDefenseSelected(true);
        }

        private async Task TryAssignDefenseAndSend(CardUI targetAttacker)
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

                CardUI defenderCard = FindBoardCardById(pair.cardId);
                CardUI attackerCard = FindBoardCardById(pair.opponentCardId);

                if (defenderCard == null) continue;
                if (attackerCard == null) continue;

                defenderCard.SetDefenseSelected(true);
                defenseAssignments[defenderCard] = attackerCard;
            }

            currentDefender = null;
        }

        private CardUI FindBoardCardById(int id)
        {
            if (boardCardsById.TryGetValue(id, out CardUI found) && found != null)
                return found;
            if (P1BoardSlots != null)
            {
                for (int i = 0; i < P1BoardSlots.Count; i++)
                {
                    CardSlotUI slot = P1BoardSlots[i];
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
                    CardSlotUI slot = P2BoardSlots[i];
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

            foreach (KeyValuePair<CardUI, CardUI> kvp in defenseAssignments)
            {
                if (kvp.Key != null)
                    kvp.Key.SetDefenseSelected(false);
            }

            defenseAssignments.Clear();
        }
    }
}
