using UnityEngine;
using TMPro;

public class Card3D : MonoBehaviour
{
    [Header("Bindings (UI on FaceCanvas)")]
    public TMP_Text titleText;
    public TMP_Text costText;
    public TMP_Text descText;
    public GameObject statsRow;   // Activer/désactiver selon kind
    public TMP_Text atkText;
    public TMP_Text hpText;

    [Header("Runtime State (read-only)")]
    [SerializeField] private CardDefinition definition;
    [SerializeField] private int currentAttack;
    [SerializeField] private int currentHealth;

    public void Setup(CardDefinition def)
    {
        definition = def;
        currentAttack = def.kind == CardKind.Unit ? def.baseAttack : 0;
        currentHealth = def.kind == CardKind.Unit ? def.baseHealth : 0;
        RefreshView();
    }

    public void Setup(CardPayload p)
    {
        definition = null;
        if (titleText) titleText.text = p.displayName;
        if (costText)  costText.text  = p.cost.ToString();
        if (descText)  descText.text  = p.description;

        bool isUnit = p.kind == CardKind.Unit;
        if (statsRow) statsRow.SetActive(isUnit);
        currentAttack = isUnit ? p.baseAttack : 0;
        currentHealth = isUnit ? p.baseHealth : 0;
        if (isUnit)
        {
            if (atkText) atkText.text = currentAttack.ToString();
            if (hpText)  hpText.text  = currentHealth.ToString();
        }
    }

    public void ResetRuntimeStats()
    {
        if (definition == null) return;
        currentAttack = definition.kind == CardKind.Unit ? definition.baseAttack : 0;
        currentHealth = definition.kind == CardKind.Unit ? definition.baseHealth : 0;
        UpdateStatsTexts();
    }

    public void SetHealth(int hp)
    {
        currentHealth = Mathf.Max(0, hp);
        UpdateStatsTexts();
    }

    public void SetAttack(int atk)
    {
        currentAttack = Mathf.Max(0, atk);
        UpdateStatsTexts();
    }

    public int ApplyDamage(int amount)
    {
        SetHealth(currentHealth - Mathf.Max(0, amount));
        return currentHealth;
    }


    private void RefreshView()
    {
        if (definition == null) return;

        if (titleText) titleText.text = definition.displayName;
        if (costText)  costText.text  = definition.cost.ToString();
        if (descText)  descText.text  = definition.description;

        bool isUnit = definition.kind == CardKind.Unit;
        if (statsRow) statsRow.SetActive(isUnit);

        UpdateStatsTexts();
    }

    private void UpdateStatsTexts()
    {
        bool showStats = statsRow ? statsRow.activeSelf : true;
        if (!showStats) return;

        if (atkText) atkText.text = currentAttack.ToString();
        if (hpText)  hpText.text  = currentHealth.ToString();
    }
}

// fallback si le ScriptableObject est vide
[System.Serializable]
public struct CardPayload
{
    public string id;
    public string displayName;
    public string description;
    public CardKind kind;
    public int cost;
    public int baseAttack;
    public int baseHealth;
}
