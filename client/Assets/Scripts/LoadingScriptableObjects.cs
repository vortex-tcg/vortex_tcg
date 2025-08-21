using UnityEngine;

[CreateAssetMenu(fileName = "LoadingRequest", menuName = "Vortex/Loading Request")]
public class LoadingRequest : ScriptableObject
{
    [Header("Cible")]
    public string targetScene = "";

    [Header("Menu overlay (TODO: branche menu)")]
    public bool loadMenu = false;
    public bool unloadMenu = false;
    public string menuSceneName = "MenuOverlay"; 

    [Header("Visuel")]
    public float minShowTime = 0.5f;  
    public bool useFade = true;
    public float fadeDuration = 0.25f;
}
