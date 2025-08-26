using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

public class UILoadingScene : MonoBehaviour
{

    public LoadingRequest menuParams;
    private UpMenuStatus status = UpMenuStatus.MAIN;
    public Button playButton;
    public Button profilButton;
    public Button collectionButton;
    public Button friendsButton;
    public Button cartButton;
    public Button optionButton;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        TMP_Text playText = playButton.GetComponentInChildren<TMP_Text>();
        TMP_Text profilText = profilButton.GetComponentInChildren<TMP_Text>();
        TMP_Text collectionText = collectionButton.GetComponentInChildren<TMP_Text>();
        TMP_Text friendsText = friendsButton.GetComponentInChildren<TMP_Text>();
        TMP_Text cartText = cartButton.GetComponentInChildren<TMP_Text>();
        TMP_Text optionText = optionButton.GetComponentInChildren<TMP_Text>();

        status = menuParams.status;
        switch(status) {
            case UpMenuStatus.MAIN:
                playText.fontStyle = FontStyles.Bold;
                profilText.fontStyle = FontStyles.Normal;
                collectionText.fontStyle = FontStyles.Normal;
                friendsText.fontStyle = FontStyles.Normal;
                cartText.fontStyle = FontStyles.Normal;
                optionText.fontStyle = FontStyles.Normal;
                break;
            case UpMenuStatus.PROFIL:
                playText.fontStyle = FontStyles.Normal;
                profilText.fontStyle = FontStyles.Bold;
                collectionText.fontStyle = FontStyles.Normal;
                friendsText.fontStyle = FontStyles.Normal;
                cartText.fontStyle = FontStyles.Normal;
                optionText.fontStyle = FontStyles.Normal;
                break;
            case UpMenuStatus.COLLECTION:
                playText.fontStyle = FontStyles.Normal;
                profilText.fontStyle = FontStyles.Normal;
                collectionText.fontStyle = FontStyles.Bold;
                friendsText.fontStyle = FontStyles.Normal;
                cartText.fontStyle = FontStyles.Normal;
                optionText.fontStyle = FontStyles.Normal;
                break;
            case UpMenuStatus.FRIENDS:
                playText.fontStyle = FontStyles.Normal;
                profilText.fontStyle = FontStyles.Normal;
                collectionText.fontStyle = FontStyles.Normal;
                friendsText.fontStyle = FontStyles.Bold;
                cartText.fontStyle = FontStyles.Normal;
                optionText.fontStyle = FontStyles.Normal;
                break;
            case UpMenuStatus.MARKET:
                playText.fontStyle = FontStyles.Normal;
                profilText.fontStyle = FontStyles.Normal;
                collectionText.fontStyle = FontStyles.Normal;
                friendsText.fontStyle = FontStyles.Normal;
                cartText.fontStyle = FontStyles.Underline;
                optionText.fontStyle = FontStyles.Normal;
                break;
            case UpMenuStatus.OPTIONS:
                playText.fontStyle = FontStyles.Normal;
                profilText.fontStyle = FontStyles.Normal;
                collectionText.fontStyle = FontStyles.Normal;
                friendsText.fontStyle = FontStyles.Normal;
                cartText.fontStyle = FontStyles.Normal;
                optionText.fontStyle = FontStyles.Underline;
                break;
        }
        playButton.onClick.AddListener(() => callLoadScene("Scene/MainPage", UpMenuStatus.MAIN));
        profilButton.onClick.AddListener(() => callLoadScene("Scene/ProfilScene", UpMenuStatus.PROFIL));
        collectionButton.onClick.AddListener(() => callLoadScene("Scene/CollectionScene", UpMenuStatus.COLLECTION));
        friendsButton.onClick.AddListener(() => callLoadScene("Scene/FriendsScene", UpMenuStatus.FRIENDS));
        cartButton.onClick.AddListener(() => callLoadScene("Scene/MarketScene", UpMenuStatus.MARKET));
        optionButton.onClick.AddListener(() => callLoadScene("Scene/OptionScene", UpMenuStatus.OPTIONS));
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void callLoadScene(string loadScene, UpMenuStatus newStatus) {
        Debug.Log("Holla");
        menuParams.status = newStatus;
        LoadingScreen.Load(loadScene, loadMenu: true, unloadMenu: false);
    }

    UpMenuStatus getStatus() {
        return status;
    }
}
