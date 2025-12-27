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

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;

            Debug.Log($"[OpponentBoardManager] Awake() on '{name}' activeInHierarchy={gameObject.activeInHierarchy}");

            Debug.Log($"[OpponentBoardManager] cardPrefab={(cardPrefab != null ? cardPrefab.name : "NULL")}");
            Debug.Log($"[OpponentBoardManager] enemySlots={(enemySlots == null ? "NULL" : enemySlots.Length.ToString())}");

            if (enemySlots != null)
            {
                if (enemySlots.Length != 5)
                    Debug.LogWarning($"[OpponentBoardManager] enemySlots.Length={enemySlots.Length} (attendu 5 ?) -> vérifie ton inspector.");

                for (int i = 0; i < enemySlots.Length; i++)
                {
                    CardSlot s = enemySlots[i];
                    if (s == null)
                    {
                        Debug.LogWarning($"[OpponentBoardManager] enemySlots[{i}] = NULL");
                        continue;
                    }

                    Debug.Log(
                        $"[OpponentBoardManager] enemySlots[{i}]='{s.name}' " +
                        $"activeInHierarchy={s.gameObject.activeInHierarchy} " +
                        $"slotIndex={s.slotIndex} " +
                        $"CurrentCard={(s.CurrentCard != null ? s.CurrentCard.cardId : "null")} " +
                        $"path='{GetPath(s.transform)}'"
                    );
                }
            }
        }

        public void PlaceOpponentCard(int location, GameCardDto playedCard)
        {
            Debug.Log($"[OpponentBoardManager] PlaceOpponentCard(location={location}, playedCard={(playedCard != null ? playedCard.GameCardId.ToString() : "NULL")})");
            if (enemySlots == null || enemySlots.Length == 0)
            {
                Debug.LogError("[OpponentBoardManager] enemySlots NULL/empty");
                return;
            }

            if (location < 0 || location >= enemySlots.Length)
            {
                Debug.LogError($"[OpponentBoardManager] location hors range: {location} (len={enemySlots.Length})");
                return;
            }

            CardSlot slot = enemySlots[location];
            if (slot == null)
            {
                Debug.LogError($"[OpponentBoardManager] slot NULL à enemySlots[{location}]");
                return;
            }

            Debug.Log(
                $"[OpponentBoardManager] TargetSlot='{slot.name}' " +
                $"activeInHierarchy={slot.gameObject.activeInHierarchy} " +
                $"slotIndex={slot.slotIndex} " +
                $"CurrentCard={(slot.CurrentCard != null ? slot.CurrentCard.cardId : "null")} " +
                $"parent='{(slot.transform.parent != null ? slot.transform.parent.name : "none")}' " +
                $"path='{GetPath(slot.transform)}' " +
                $"slotPosW={slot.transform.position} slotRotW={slot.transform.rotation.eulerAngles} " +
                $"slotScaleL={slot.transform.localScale} slotScaleW={slot.transform.lossyScale}"
            );

            if (!slot.gameObject.activeInHierarchy)
                Debug.LogWarning("[OpponentBoardManager] ⚠️ Slot pas activeInHierarchy (donc rien ne se verra).");

            if (slot.CurrentCard != null)
            {
                Debug.LogWarning("[OpponentBoardManager] Slot déjà occupé -> abort");
                return;
            }

            if (cardPrefab == null)
            {
                Debug.LogError("[OpponentBoardManager] cardPrefab non assigné");
                return;
            }

            // IMPORTANT: on instancie DIRECTEMENT sous le slot pour éviter un spawn 1 frame à l'origine
            Card c = Instantiate(cardPrefab, slot.transform, false);
            c.name = $"EnemyCard_{(playedCard != null ? playedCard.GameCardId.ToString() : "NULL")}";

            Debug.Log(
                $"[OpponentBoardManager] Instantiated '{c.name}' instanceId={c.GetInstanceID()} " +
                $"layer={c.gameObject.layer} activeInHierarchy={c.gameObject.activeInHierarchy} " +
                $"posW={c.transform.position} rotW={c.transform.rotation.eulerAngles} scaleL={c.transform.localScale}"
            );

            // Rendu / bounds debug
            Renderer[] rends = c.GetComponentsInChildren<Renderer>(true);
            Debug.Log($"[OpponentBoardManager] Renderers count={rends.Length}");
            if (rends.Length > 0)
            {
                Bounds b = rends[0].bounds;
                for (int i = 1; i < rends.Length; i++) b.Encapsulate(rends[i].bounds);
                Debug.Log($"[OpponentBoardManager] RendererBounds size={b.size} center={b.center}");
            }

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
                Debug.Log($"[OpponentBoardManager] ApplyDTO DONE -> cardId='{c.cardId}' name='{playedCard.Name}'");
            }
            else
            {
                Debug.LogWarning("[OpponentBoardManager] playedCard est NULL -> pas de ApplyDTO");
            }

            LogBoardState("BEFORE PlaceCard");
            Debug.Log($"[OpponentBoardManager] About to slot.PlaceCard(cardId='{c.cardId}', slot='{slot.name}')");

            slot.PlaceCard(c);

            Debug.Log(
                $"[OpponentBoardManager] PlaceCard DONE -> slot.CurrentCard={(slot.CurrentCard != null ? slot.CurrentCard.cardId : "null")} " +
                $"cardParent='{(c.transform.parent != null ? c.transform.parent.name : "none")}' " +
                $"cardPosL={c.transform.localPosition} cardRotL={c.transform.localRotation.eulerAngles} cardScaleL={c.transform.localScale}"
            );

            if (Camera.main != null)
            {
                Vector3 vp = Camera.main.WorldToViewportPoint(c.transform.position);
                Debug.Log($"[OpponentBoardManager] Camera viewportPoint={vp} (x/y in [0..1] visible, z>0 devant cam)");
            }
            else
            {
                Debug.LogWarning("[OpponentBoardManager] Camera.main NULL");
            }

            LogBoardState("AFTER PlaceCard");
        }

        public void ResetBoard()
        {
            Debug.Log("[OpponentBoardManager] ResetBoard()");
            LogBoardState("BEFORE RESET");

            if (enemySlots == null) return;

            foreach (CardSlot s in enemySlots)
            {
                if (s == null) continue;

                if (s.CurrentCard != null)
                {
                    Debug.Log($"[OpponentBoardManager] Destroy enemy card '{s.CurrentCard.name}' on slot '{s.name}'");
                    Destroy(s.CurrentCard.gameObject);
                }

                s.CurrentCard = null;
            }

            LogBoardState("AFTER RESET");
        }

        private void LogBoardState(string label)
        {
            if (enemySlots == null)
            {
                Debug.Log($"[OpponentBoardManager] BoardState {label}: enemySlots=NULL");
                return;
            }

            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"[OpponentBoardManager] BoardState {label}: enemySlots.Length={enemySlots.Length}");

            for (int i = 0; i < enemySlots.Length; i++)
            {
                CardSlot s = enemySlots[i];
                if (s == null)
                {
                    sb.AppendLine($"  [{i}] NULL");
                    continue;
                }

                string cc = (s.CurrentCard != null) ? $"{s.CurrentCard.cardId}('{s.CurrentCard.name}')" : "null";
                sb.AppendLine($"  [{i}] slot='{s.name}' active={s.gameObject.activeInHierarchy} slotIndex={s.slotIndex} CurrentCard={cc} path='{GetPath(s.transform)}'");
            }

            Debug.Log(sb.ToString());
        }

        private static string GetPath(Transform t)
        {
            if (t == null) return "null";
            StringBuilder sb = new StringBuilder(t.name);
            while (t.parent != null)
            {
                t = t.parent;
                sb.Insert(0, t.name + "/");
            }
            return sb.ToString();
        }
    }
}
