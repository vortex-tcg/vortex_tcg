using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Serialization;
using TMPro;
using UnityEngine.UI;
using VortexTCG.Scripts.DTOs;
using VortexTCG.Scripts.Features.Match.Events;
using VortexTCG.Scripts.Features.Match.Services;
using VortexTCG.Scripts.Features.Match.UI;

namespace VortexTCG.Scripts.MatchScene
{
    public class CardUI : MonoBehaviour
    {
        [Header("Data")] public string cardId;
        
        public string cardName;
        public int hp;
        public int attack;
        public int cost;
        public Transform VisualRoot;
        public Transform UIRoot;

        [TextArea(3, 6)] public string description;
        public string imageUrl;

        [SerializeField] private SpriteRenderer illustration;

        [Header("UI")] public TMP_Text nameText;
        public TMP_Text costText;
        public TMP_Text atkText;
        public TMP_Text hpText;
        public TMP_Text currentHpText;
        public TMP_Text descriptionText;
        [SerializeField] private SpriteRenderer costColor;
        [Header("Cost Colors")]
        [SerializeField] private Color costGreen = new Color(0.2f, 0.8f, 0.2f, 1f);
        [SerializeField] private Color costBlue = new Color(0.2f, 0.4f, 0.9f, 1f);
        [SerializeField] private Color costOrange = new Color(1f, 0.55f, 0.1f, 1f);
        [SerializeField] private Color costRed = new Color(0.9f, 0.15f, 0.15f, 1f);
        [SerializeField] private Color costViolet = new Color(0.6f, 0.2f, 0.8f, 1f);
        
        [SerializeField] private bool faceDown;
        public bool IsFaceDown => faceDown;

        private bool isSleepy;
        public bool IsSleepy => isSleepy;

        [Header("Attack Phase")] public TMP_Text attackOrderText;

        [Header("State")]
        [SerializeField] private GameObject AttackState;
        [SerializeField] private GameObject DefenseState;
        [SerializeField] private GameObject DefendingState;
        [SerializeField] private GameObject AttackOrder;
        [FormerlySerializedAs("sleepyEffect")]
        [SerializeField] private GameObject SleepyState;

        [Tooltip("Shown briefly when this card receives damage during end turn resolution")]
        [SerializeField] private GameObject DamageReceivedState;
        [Tooltip("Shown briefly before removing the card from board")]
        [SerializeField] private GameObject DeathState;

        [Header("Selection")]
        [SerializeField] private float selectedScaleMultiplier = 1.08f;
        private bool isSelected;
        private Vector3 selectionBaseScale;
        private bool hasAttackedThisPhase = false;
        private Sprite illustrationPlaceholder;
        private Coroutine illustrationLoadRoutine;
        private int illustrationLoadVersion;

        private static readonly Dictionary<string, Sprite> IllustrationCache = new Dictionary<string, Sprite>();

        void Awake()
        {
            selectionBaseScale = transform.localScale;

            if (illustration == null)
                TryResolveIllustration();

            if (illustration != null && illustrationPlaceholder == null)
                illustrationPlaceholder = illustration.sprite;

            if (currentHpText == null)
                TryResolveCurrentHpText();

            if (costColor == null)
                TryResolveCostColor();

            Collider col = GetComponent<Collider>();
            if (col == null)
            {
                BoxCollider bc = gameObject.AddComponent<BoxCollider>();
                bc.size = new Vector3(2f, 3f, 0.1f);
            }

            if (AttackOrder != null && attackOrderText == null)
            {
                attackOrderText = AttackOrder.GetComponentInChildren<TMP_Text>();
            }

            if (attackOrderText != null)
                attackOrderText.enabled = false;

            if (AttackOrder != null)
                AttackOrder.SetActive(false);

            if (DefendingState != null)
                DefendingState.SetActive(false);

            if (DamageReceivedState != null)
                DamageReceivedState.SetActive(false);

            if (DeathState != null)
                DeathState.SetActive(false);

            if (SleepyState != null)
                SleepyState.SetActive(false);

            UpdateCostColor();

            // if the sleep manager is currently active (first turn), start sleepy
            if (SleepManager.IsSleeping)
            {
                SetSleepy(true);
            }

            // currentHP display removed: ensure the currentHp UI is hidden at runtime
            UpdateCurrentHpDisplay();
            UpdateCurrentHpVisibility();
        }

        private void OnValidate()
        {
            if (costColor == null)
                TryResolveCostColor();

            if (currentHpText == null)
                TryResolveCurrentHpText();

            UpdateCostColor();
            UpdateCurrentHpDisplay();
            UpdateCurrentHpVisibility();
        }

        private void OnTransformParentChanged()
        {
            UpdateCurrentHpVisibility();
        }

        void OnMouseEnter()
        {
            if (faceDown) return;

            CardSlotUI slot = GetComponentInParent<CardSlotUI>();

            if (CardPreviewUI.Instance != null)
            {
                CardPreviewUI.Instance.ShowCardPreview(this);
            }
        }

        void OnMouseExit()
        {
            if (CardPreviewUI.Instance != null)
            {
                CardPreviewUI.Instance.HidePreview();
            }
        }

        void OnMouseDown()
        {
            if (string.IsNullOrEmpty(cardName) || string.IsNullOrEmpty(cardId))
            {
                return;
            }
            
            if (faceDown) return;

            if (isSleepy)
            {
                Debug.Log($"[CardUI] '{cardName}' is sleepy; click ignored");
                return;
            }

            Debug.Log($"[CardUI] OnMouseDown on '{cardName}' (ID: {cardId}) - Current Phase: {(PhaseService.Instance != null ? PhaseService.Instance.CurrentPhase.ToString() : "NULL")}");

            MatchEvents.FireCardClicked(this);
            

            if (PhaseService.Instance == null || PhaseService.Instance.CurrentPhase != GamePhase.DEFENSE)
            {
                MatchEvents.FireCardSelected(this);
            }
        }

        public void ApplyDTO(string id, string name, int hp, int attack, int cost, string desc, string imgUrl)
        {
            cardId = id;
            cardName = name;
            this.hp = hp;
            this.attack = attack;
            this.cost = cost;
            description = desc;
            SetIllustrationUrl(imgUrl);
            RefreshUI();
        }

        private void SetIllustrationUrl(string imgUrl)
        {
            imageUrl = imgUrl;

            illustrationLoadVersion++;

            if (illustrationLoadRoutine != null)
            {
                StopCoroutine(illustrationLoadRoutine);
                illustrationLoadRoutine = null;
            }

            if (illustration == null)
                TryResolveIllustration();

            if (illustration == null)
                return;

            if (string.IsNullOrWhiteSpace(imageUrl))
            {
                RestoreIllustrationPlaceholder();
                return;
            }

            if (IllustrationCache.TryGetValue(imageUrl, out Sprite cachedSprite) && cachedSprite != null)
            {
                illustration.sprite = cachedSprite;
                return;
            }

            RestoreIllustrationPlaceholder();
            illustrationLoadRoutine = StartCoroutine(LoadIllustration(imageUrl, illustrationLoadVersion));
        }

        public void RefreshUI()
        {

            if (nameText != null) nameText.text = cardName;
            if (costText != null) costText.text = cost.ToString();
            if (atkText != null) atkText.text = attack > 0 ? attack.ToString() : "-";
            if (hpText != null) hpText.text = Mathf.Max(0, hp).ToString();
            UpdateCurrentHpDisplay();
            if (descriptionText != null) descriptionText.text = description;
            UpdateCostColor();
            UpdateCurrentHpVisibility();
        }

        public void RefreshCurrentHpVisibility()
        {
            UpdateCurrentHpVisibility();
        }

        private void UpdateCurrentHpDisplay()
        {
            if (currentHpText == null)
                return;

            // We stopped showing current HP. Hide the TMP element if present.
            try
            {
                if (currentHpText.gameObject != null)
                    currentHpText.gameObject.SetActive(false);
            }
            catch (System.Exception)
            {
                // swallow any exception related to destroyed objects in edit mode/runtime
            }
        }

        private void UpdateCurrentHpVisibility()
        {
            if (currentHpText == null)
                return;

            // Always hide current HP display regardless of slot/board placement.
            try
            {
                if (currentHpText.gameObject != null)
                    currentHpText.gameObject.SetActive(false);
            }
            catch (System.Exception)
            {
                // ignore
            }
        }

        private void UpdateCostColor()
        {
            if (costColor == null)
                return;

            int clampedCost = Mathf.Clamp(cost, 0, 10);
            Color target = clampedCost switch
            {
                0 or 1 or 2 => costGreen,
                3 or 4 => costBlue,
                5 or 6 => costOrange,
                7 or 8 => costRed,
                _ => costViolet
            };

            Color current = costColor.color;
            costColor.color = new Color(target.r, target.g, target.b, current.a);
        }

        private void TryResolveCostColor()
        {
            SpriteRenderer[] renderers = GetComponentsInChildren<SpriteRenderer>(true);
            for (int i = 0; i < renderers.Length; i++)
            {
                SpriteRenderer sr = renderers[i];
                if (sr != null && (sr.name == "costColor" || sr.name == "CostColor"))
                {
                    costColor = sr;
                    return;
                }
            }
        }

        private void TryResolveCurrentHpText()
        {
            TMP_Text[] texts = GetComponentsInChildren<TMP_Text>(true);
            for (int i = 0; i < texts.Length; i++)
            {
                TMP_Text txt = texts[i];
                if (txt != null && (txt.name == "CurrentHP" || txt.name == "currentHP" || txt.name == "CurrentHp"))
                {
                    currentHpText = txt;
                    return;
                }
            }
        }

        public void ShowAttackOrder(int order)
        {
            if (attackOrderText != null)
            {
                attackOrderText.text = order.ToString();
                attackOrderText.enabled = true;
            }

            if (AttackOrder != null)
                AttackOrder.SetActive(true);
        }

        public void ClearAttackOrder()
        {
            if (attackOrderText != null)
            {
                attackOrderText.text = "";
                attackOrderText.enabled = false;
            }

            if (AttackOrder != null)
                AttackOrder.SetActive(false);
        }


        public bool IsSelected => isSelected;
        public bool HasAttackedThisPhase => hasAttackedThisPhase;

        public void SetAttackedThisPhase(bool hasAttacked)
        {
            hasAttackedThisPhase = hasAttacked;
        }

        public void ResetAttackState()
        {
            hasAttackedThisPhase = false;
        }

        public void SetSelected(bool selected)
        {
            if (isSelected == selected) return;
            isSelected = selected;

            if (isSelected)
            {
                selectionBaseScale = transform.localScale;
                transform.localScale = selectionBaseScale * selectedScaleMultiplier;


                PhaseService phaseService = PhaseService.Instance;
                if (phaseService != null && phaseService.CurrentPhase == GamePhase.ATTACK)
                {
                    CardSlotUI slot = GetComponentInParent<CardSlotUI>();
                    if (slot != null && !slot.isOpponentSlot)
                    {
                        if (AttackOrder != null)
                            AttackOrder.SetActive(true);
                    }
                }
            }
            else
            {
                transform.localScale = selectionBaseScale;

                if (AttackOrder != null)
                    AttackOrder.SetActive(false);
            }

            AttackUI.Instance?.UpdateAttackStateForSelection(this, isSelected);
        }

        public void CardIsPlaced() {
            transform.localScale = Vector3.one;

            isSelected = false;

            if (AttackOrder != null)
                AttackOrder.SetActive(false);

            AttackUI.Instance?.UpdateAttackStateForSelection(this, false);
        }

        public void SetOpponentAttacking(bool active)
        {
            GameObject attackState = GetAttackState();
            if (attackState != null)
            {
                attackState.SetActive(active);
            }
        }

        public GameObject GetAttackState()
        {
            if (AttackState == null)
            {
                AttackState = FindOutlineByName("AttackState");
                Debug.Log($"[CardUI] GetAttackState for card '{cardName}': {(AttackState != null ? "FOUND" : "NOT FOUND")}");
            }

            return AttackState;
        }

        public void SetDefenseSelected(bool active)
        {
            DefenseUI.Instance?.SetDefenseState(this, active);
        }

        public GameObject GetDefenseState()
        {
            if (DefenseState == null)
                DefenseState = FindOutlineByName("DefenseState");

            return DefenseState;
        }

        public void SetDefendingState(bool active)
        {
            GameObject state = GetDefendingState();
            if (state != null)
                state.SetActive(active);
        }

        public GameObject GetDefendingState()
        {
            if (DefendingState == null)
                DefendingState = FindOutlineByName("DefendingState");

            return DefendingState;
        }

        public void SetDamageReceivedState(bool active)
        {
            GameObject state = GetDamageReceivedState();
            if (state != null)
                state.SetActive(active);
        }

        public GameObject GetDamageReceivedState()
        {
            if (DamageReceivedState == null)
                DamageReceivedState = FindOutlineByName("DamageReceivedState");

            return DamageReceivedState;
        }

        public void SetDeathState(bool active)
        {
            GameObject state = GetDeathState();
            if (state != null)
                state.SetActive(active);
        }

        public GameObject GetDeathState()
        {
            if (DeathState == null)
                DeathState = FindOutlineByName("DeathState");

            return DeathState;
        }

        public void SetSleepyState(bool active)
        {
            GameObject state = GetSleepyState();
            if (state != null)
                state.SetActive(active);
        }

        public GameObject GetSleepyState()
        {
            if (SleepyState == null)
                SleepyState = FindOutlineByName("SleepyState");

            return SleepyState;
        }

        private GameObject FindOutlineByName(string name)
        {
            Debug.Log($"[CardUI] FindOutlineByName searching for '{name}' in card '{cardName}' (ID: {cardId})");
            
            Transform[] children = GetComponentsInChildren<Transform>(true);
            Debug.Log($"[CardUI] Found {children.Length} children (including self and inactive)");
            
            foreach (Transform t in children)
            {
                if (t != null)
                {
                    Debug.Log($"[CardUI]   - Child: '{t.name}' (active: {t.gameObject.activeSelf})");
                    if (t.name == name)
                    {
                        Debug.Log($"[CardUI] ✓ MATCH FOUND: '{name}' on card '{cardName}'");
                        return t.gameObject;
                    }
                }
            }

            Debug.LogWarning($"[CardUI] ✗ '{name}' NOT FOUND on card '{cardName}' (ID: {cardId})");
            return null;
        }

        public bool IsAttackingOutlineActive()
        {
            return AttackState != null && AttackState.activeSelf;
        }

        public bool IsDefenseSelected()
        {
            return DefenseState != null && DefenseState.activeSelf;
        }

        public bool IsDefendingStateActive()
        {
            return DefendingState != null && DefendingState.activeSelf;
        }

        public bool IsDamageReceivedStateActive()
        {
            return DamageReceivedState != null && DamageReceivedState.activeSelf;
        }

        public bool IsDeathStateActive()
        {
            return DeathState != null && DeathState.activeSelf;
        }

        private IEnumerator LoadIllustration(string url, int version)
        {
            string resolvedUrl = ResolveIllustrationUrl(url);
            if (string.IsNullOrWhiteSpace(resolvedUrl))
                yield break;

            using (UnityWebRequest request = UnityWebRequestTexture.GetTexture(resolvedUrl))
            {
                yield return request.SendWebRequest();

                if (version != illustrationLoadVersion)
                    yield break;

                if (request.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogWarning($"[CardUI] Illustration load failed for '{cardName}' ({resolvedUrl}): {request.error}");
                    RestoreIllustrationPlaceholder();
                    yield break;
                }

                Texture2D texture = DownloadHandlerTexture.GetContent(request);
                if (texture == null)
                {
                    RestoreIllustrationPlaceholder();
                    yield break;
                }

                Sprite sprite = Sprite.Create(
                    texture,
                    new Rect(0f, 0f, texture.width, texture.height),
                    new Vector2(0.5f, 0.5f),
                    100f
                );

                IllustrationCache[url] = sprite;

                if (version == illustrationLoadVersion && illustration != null)
                    illustration.sprite = sprite;
            }
        }

        private void RestoreIllustrationPlaceholder()
        {
            if (illustration == null)
                return;

            if (illustrationPlaceholder != null)
                illustration.sprite = illustrationPlaceholder;
        }

        private void TryResolveIllustration()
        {
            SpriteRenderer[] renderers = GetComponentsInChildren<SpriteRenderer>(true);
            for (int i = 0; i < renderers.Length; i++)
            {
                SpriteRenderer sr = renderers[i];
                if (sr != null && (sr.name == "Illustration" || sr.name == "illustration"))
                {
                    illustration = sr;
                    if (illustrationPlaceholder == null)
                        illustrationPlaceholder = illustration.sprite;
                    return;
                }
            }
        }

        private static string ResolveIllustrationUrl(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
                return string.Empty;

            if (Uri.TryCreate(url, UriKind.Absolute, out Uri absoluteUri))
                return absoluteUri.ToString();

            AppConfig cfg = ConfigLoader.Load();
            if (cfg == null || string.IsNullOrWhiteSpace(cfg.baseUrl))
                return $"images/{url.TrimStart('/')}";

            string baseUrl = cfg.baseUrl.TrimEnd('/');
            string normalizedPath = url.TrimStart('/');
            return $"{baseUrl}/images/{normalizedPath}";
        }

        public bool IsSleepyStateActive()
        {
            return SleepyState != null && SleepyState.activeSelf;
        }
   

        public void SetFaceDown(bool value)
        {
            faceDown = value;
            transform.localRotation = value ? Quaternion.Euler(0f, 0f, 180f) : Quaternion.identity;
            Collider col = GetComponent<Collider>();
            if (col != null) col.enabled = !value;
        }

        /// <summary>
        /// Marks the card as sleepy (cannot be used for attack/defense) and toggles the associated effect.
        /// </summary>
        public void SetSleepy(bool sleepy)
        {
            if (isSleepy == sleepy) return;
            isSleepy = sleepy;

            SetSleepyState(sleepy);

            // optional: disable collider to prevent any interaction
            // Collider col = GetComponent<Collider>();
            // if (col != null) col.enabled = !sleepy;

            Debug.Log($"[CardUI] SetSleepy('{cardName}' ID:{cardId}) -> {sleepy}");
        }

    }
}
