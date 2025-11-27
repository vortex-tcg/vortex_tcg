using UnityEngine;

public enum CardKind { Unit, Spell, Equipment }

[CreateAssetMenu(menuName = "Vortex/Cards/Card Definition")]
public class CardDefinition : ScriptableObject
{
    [Header("Identity")]
    public string id;
    public string displayName;
    [TextArea] public string description;
    public CardKind kind = CardKind.Unit;

    [Header("Gameplay")]
    public int cost = 0;
    public int baseAttack = 0; // utilisé si Unit
    public int baseHealth = 0; // utilisé si Unit
}
