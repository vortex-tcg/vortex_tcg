using System.Collections.Generic;
using UnityEngine;
using VortexTCG.Scripts.DTOs;

namespace VortexTCG.Scripts.MatchScene
{
    public class HandManager : MonoBehaviour
    {
        public static HandManager Instance { get; private set; }

        [Header("Hand Spawn")]
        [SerializeField] private Card cardPrefab;
        [SerializeField] private Transform handRoot;
        [SerializeField] private float cardSpacing = 1.2f;

        [HideInInspector] public Card SelectedCard;

        private readonly List<Card> handCards = new();

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

            foreach (var dto in drawnCards)
            {
                Card card = Instantiate(cardPrefab, handRoot);
                
                string id = dto.GameCardId.ToString();
                string name = dto.Name;
                int hp = dto.Hp;
                int atk = dto.Attack;
                int cost = dto.Cost;
                string desc = dto.Description;
                string img = "";
                card.ApplyDTO(id, name, hp, atk, cost, desc, img);

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

		public void PlaceSelectedCardOnSlot(CardSlot slot)
		{
    		if (SelectedCard == null) return;

    		if (PhaseManager.Instance != null && PhaseManager.Instance.CurrentPhase != GamePhase.PLACEMENT)
       			return;

            if (!slot.CanAccept(SelectedCard)) return;

            slot.PlaceCard(SelectedCard);

            handCards.Remove(SelectedCard);
            SelectedCard = null;

            LayoutHand();
        }

        private void ClearHand()
        {
            DeselectCurrentCard();

            foreach (var c in handCards)
                if (c != null) Destroy(c.gameObject);

            handCards.Clear();
        }

        private void LayoutHand()
        {
            for (int i = 0; i < handCards.Count; i++)
            {
                var c = handCards[i];
                if (c == null) continue;

                c.transform.localPosition = new Vector3(i * cardSpacing, 0f, 0f);
                c.transform.localRotation = Quaternion.identity;
                c.transform.localScale = Vector3.one;
            }
        }

        private static void EnsureCollider(Card card)
        {
            if (card == null) return;
            if (card.GetComponent<Collider>() == null)
            {
                var bc = card.gameObject.AddComponent<BoxCollider>();
                bc.size = Vector3.one;
            }
        }
    }
}
