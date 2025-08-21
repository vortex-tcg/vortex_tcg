using UnityEngine;

[CreateAssetMenu(fileName = "UILoadingScript", menuName = "Scriptable Objects/UILoadingScript")]
public class UILoadingScript : ScriptableObject
{
    public enum UpMenuStatus {
        MAIN,
        PROFIL,
        COLLECTION,
        FRIENDS,
        MARKET,
        OPTIONS
    };

    public UpMenuStatus status;
}
