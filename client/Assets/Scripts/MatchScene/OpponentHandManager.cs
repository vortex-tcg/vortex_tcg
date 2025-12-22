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
        [SerializeField] private float cardSpacing = 1.2f;

        [Header("Face Down")]
        [Tooltip("Rotation appliquée aux cartes de l'adversaire (face down).")]
        [SerializeField] private Vector3 faceDownEuler = new Vector3(0f, 180f, 0f);

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

        public void ResetHand()
        {
            foreach (var c in handCards)
                if (c != null) Destroy(c.gameObject);

            handCards.Clear();
        }

        public void SetHandCount(int count)
        {
            ResetHand();
            AddFaceDownCards(count);
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
                // Important: parent + local transform propre
                Card card = Instantiate(cardPrefab, handRoot, false);

                PrepareFaceDownCard(card);
                handCards.Add(card);
            }

            LayoutHand();
        }

        // Si tu veux retirer quand l’adversaire joue une carte plus tard
        public void RemoveCards(int count)
        {
            for (int i = 0; i < count; i++)
            {
                if (handCards.Count == 0) break;

                int last = handCards.Count - 1;
                Card c = handCards[last];
                handCards.RemoveAt(last);
                if (c != null) Destroy(c.gameObject);
            }

            LayoutHand();
        }

        private void LayoutHand()
        {
            int n = handCards.Count;
            if (n == 0) return;

            // Centré: ex 5 cartes -> positions -2, -1, 0, 1, 2 * spacing
            float startX = -(n - 1) * cardSpacing * 0.5f;

            Quaternion faceDownRot = Quaternion.Euler(faceDownEuler);

            for (int i = 0; i < n; i++)
            {
                var c = handCards[i];
                if (c == null) continue;

                c.transform.localPosition = new Vector3(startX + i * cardSpacing, 0f, 0f);

                // ✅ FORCER la rotation ici (sinon ça “revient” ou ça ne s’applique pas)
                c.transform.localRotation = faceDownRot;

                c.transform.localScale = Vector3.one;
            }
        }

        private void PrepareFaceDownCard(Card card)
        {
            if (card == null) return;

            // Si ton Card a maintenant une logique "faceDown", appelle-la ici
            // Exemple si tu as ajouté un truc du genre:
            // card.SetFaceDown(true);

            // Sinon, on se contente de le rendre non cliquable et de “vider” l’UI (optionnel)
            Collider col = card.GetComponent<Collider>();
            if (col != null) col.enabled = false;

            // Optionnel: si tu as des textes TMP sur le front et que tu veux les cacher
            // card.nameText.gameObject.SetActive(false) etc.
        }
    }
}
