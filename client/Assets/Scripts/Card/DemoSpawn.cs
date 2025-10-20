using UnityEngine;   
public class DemoSpawn : MonoBehaviour
{
    public GameObject card3dPrefab;
    public Transform spawnParent;

    void Start()
    {
        // Spawner une unité (avec ATK/PV)
        var c1 = CardFactory.SpawnFromValues(
            card3dPrefab,
            spawnParent,
            new Vector3(0, 0, 0),
            Quaternion.identity,
            id: "unit_goblin",
            displayName: "Gobelin",
            description: "Petit mais teigneux.",
            kind: CardKind.Unit,
            cost: 2,
            baseAttack: 1,
            baseHealth: 3
        );

        // Spawner un sort (pas d'ATK/PV)
        var c2 = CardFactory.SpawnFromValues(
            card3dPrefab,
            spawnParent,
            new Vector3(2, 0, 0),
            Quaternion.identity,
            id: "spell_fireball",
            displayName: "Boule de feu",
            description: "Inflige 3 dégâts.",
            kind: CardKind.Spell,
            cost: 3
        );

        // Modifier ensuite l’état runtime si besoin :
        c1.ApplyDamage(1);     // PV-- (UI mise à jour)
        c1.SetAttack(2);       // ATK = 2
        c1.ResetRuntimeStats();// retour aux valeurs de base
    }
}