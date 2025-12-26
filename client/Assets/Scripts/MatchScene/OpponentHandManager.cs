using System.Collections.Generic;
using UnityEngine;

namespace VortexTCG.Scripts.MatchScene
{
    public class OpponentHandManager : MonoBehaviour
    {
        public static OpponentHandManager Instance { get; private set; }

        [Header("Opponent Hand Spawn")]
        [SerializeField] private Card cardPrefab;
        [SerializeField] private Transform handRoot;
        [SerializeField] private float cardSpacing = 1.0f;

        [Header("Face Down Rotation")]
        [SerializeField] private Vector3 faceDownEuler = new Vector3(0f, 180f, 0f);

        private readonly List<Card> opponentHandCards = new();

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            // Auto-bind pratique si tu ne peux pas sérialiser dans la scène
            if (handRoot == null) handRoot = transform;
            if (cardPrefab == null && HandManager.Instance != null)
                cardPrefab = HandManager.Instance.CardPrefab;
        }

        public void ResetHand()
        {
            foreach (var c in opponentHandCards)
                if (c != null) Destroy(c.gameObject);
            opponentHandCards.Clear();
        }

        public void AddFaceDownCards(int count)
        {
            if (count <= 0) return;

            if (cardPrefab == null || handRoot == null)
            {
                Debug.LogError("[OpponentHandManager] cardPrefab ou handRoot non assigné.");
                return;
            }

            for (int i = 0; i < count; i++)
            {
                Card card = Instantiate(cardPrefab, handRoot);
                card.transform.localRotation = Quaternion.Euler(faceDownEuler);

                opponentHandCards.Add(card);
            }

            LayoutHand();
        }

        public void RemoveOneCardFromHand()
        {
            if (opponentHandCards.Count == 0) return;

            var last = opponentHandCards[opponentHandCards.Count - 1];
            opponentHandCards.RemoveAt(opponentHandCards.Count - 1);

            if (last != null) Destroy(last.gameObject);

            LayoutHand();
        }

        private void LayoutHand()
        {
            for (int i = 0; i < opponentHandCards.Count; i++)
            {
                var c = opponentHandCards[i];
                if (c == null) continue;

                // spacing + rotation face down
                c.transform.localPosition = new Vector3(i * cardSpacing, 0f, 0f);
                c.transform.localRotation = Quaternion.Euler(faceDownEuler);
                c.transform.localScale = Vector3.one;
            }
        }
    }
}
