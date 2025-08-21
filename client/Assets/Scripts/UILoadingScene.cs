using UnityEngine;

public class UILoadingScene : MonoBehaviour
{

    public UILoadingScript menuParams;
    private UpMenuStatus status = MAIN;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        status = menuParams.status;
        switch(status) {
            case UpMenuStatus.MAIN:
                break;
            case UpMenuStatus.PROFIL:
                break;
            case UpMenuStatus.COLLECTION:
                break;
            case UpMenuStatus.FRIENDS:
                break;
            case UpMenuStatus.MARKET:
                break;
            case UpMenuStatus.OPTIONS:
                break;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void SetBoldTitle() {

    }

    void setUnderlineLogo() {
        
    }

    UpMenuStatus getStatus() {
        return status;
    }
}
