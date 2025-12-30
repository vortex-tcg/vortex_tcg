using System.Collections.Generic;
using UnityEngine;
using VortexTCG.Scripts.DTOs;

namespace VortexTCG.Scripts.MatchScene
{
    public class OpponentHandManager : MonoBehaviour
    {
        public static OpponentHandManager Instance { get; private set; }

        [Header("Opponent Hand Spawn")]
        [SerializeField] private Card cardPrefab;
        [SerializeField] private GameObject cardFaceDownPrefab;
        [SerializeField] private Transform handRoot;
        [SerializeField] private float cardSpacing = 1.2f;

        private readonly List<GameObject> opponentHandCards = new List<GameObject>();
        private const int MaxHandSize = 5;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            if (handRoot == null) handRoot = transform;
            if (cardPrefab == null && HandManager.Instance != null)
                cardPrefab = HandManager.Instance.CardPrefab;
        }

        public void ResetHand()
        {
            foreach (GameObject obj in opponentHandCards)
            {
                if (obj != null) Destroy(obj);
            }
            opponentHandCards.Clear();
        }

        public void AddFaceDownCards(int count)
        {
            if (count < 0) return;

            if (cardFaceDownPrefab == null || handRoot == null)
            {
                Debug.LogError("[OpponentHandManager] cardFaceDownPrefab ou handRoot non assigné.");
                return;
            }

            int cardsToAdd = count;
            if (opponentHandCards.Count + cardsToAdd > MaxHandSize)
                cardsToAdd = MaxHandSize - opponentHandCards.Count;

            for (int i = 0; i < cardsToAdd; i++)
            {
                GameObject cardObj = Instantiate(cardFaceDownPrefab, handRoot);
                EnsureCollider(cardObj);
                opponentHandCards.Add(cardObj);
            }

            PadHandToMaxSize();
            LayoutHand();
        }

        private void PadHandToMaxSize()
        {
            while (opponentHandCards.Count < MaxHandSize)
            {
                GameObject cardObj = Instantiate(cardFaceDownPrefab, handRoot);
                EnsureCollider(cardObj);
                opponentHandCards.Add(cardObj);
            }
        }

        public void RemoveOneCardFromHand()
        {
            if (opponentHandCards.Count == 0) return;

            GameObject last = opponentHandCards[opponentHandCards.Count - 1];
            opponentHandCards.RemoveAt(opponentHandCards.Count - 1);

            if (last != null) Destroy(last);

            LayoutHand();
        }

        private void LayoutHand()
        {
            for (int i = 0; i < opponentHandCards.Count; i++)
            {
                GameObject obj = opponentHandCards[i];
                if (obj == null) continue;

                Transform t = obj.transform;
                t.localPosition = new Vector3(i * cardSpacing, 0f, 0f);
                t.localRotation = Quaternion.identity;
            }
        }

        private static void EnsureCollider(GameObject cardObj)
        {
            if (cardObj == null) return;
            if (cardObj.GetComponent<Collider>() == null)
            {
                BoxCollider bc = cardObj.AddComponent<BoxCollider>();
                bc.size = Vector3.one;
            }
        }
    }
}
