using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Networking;
using System.Collections;
using System.Text.RegularExpressions;
using UnityEngine.SceneManagement;


public class LoginScript : MonoBehaviour
{
    public TMP_InputField emailField;
    public TMP_InputField passwordField;
    public TMP_Text emailErrorText;
    public Button loginButton;
    private bool passwordVisible = false;


    public void TogglePasswordVisibility()
    {
        Debug.Log("CLICK detected");
        passwordVisible = !passwordVisible;
        passwordField.contentType = passwordVisible ? TMP_InputField.ContentType.Standard : TMP_InputField.ContentType.Password;
        passwordField.ForceLabelUpdate(); 
    }

    private bool IsValidEmail(string email)
    {
        string pattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
        return Regex.IsMatch(email, pattern);
    }
    private void Start()
    {
        loginButton.onClick.AddListener(() => StartCoroutine(Login()));
    }

    IEnumerator Login()
    {
        loginButton.interactable = false;
        string baseUrl = ConfigLoader.Load().apiBaseUrl;
        string email = emailField.text;
        if (!IsValidEmail(email))
        {
            emailErrorText.text = "Adresse email invalide";
            yield break;
        }

        string password = passwordField.text;
        string url = baseUrl + "/api/login";

        string jsonBody = JsonUtility.ToJson(new LoginData(email, password));
       

        UnityWebRequest request = new UnityWebRequest(url, "POST");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonBody);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();
        if (request.result == UnityWebRequest.Result.Success)
        {
           if (request.responseCode == 200)
            {
                Debug.Log("Connexion réussie : " + request.downloadHandler.text);
                LoadingScreen.Load("MainPage", loadMenu: true, unloadMenu: false);
            }
            else
            {
                emailErrorText.text = "Erreur : " + request.responseCode;
            }
        }
        else if (request.responseCode == 401)
        {
            emailErrorText.text = "Email ou mot de passe incorrect.";
        }
        else
        {
            Debug.LogError("Erreur de connexion : " + request.error);
            emailErrorText.text = "Connexion avec le serveur impossible.";
        }
        loginButton.interactable = true;

    }

    [System.Serializable]
    private class LoginData
    {
        public string email;
        public string password;

        public LoginData(string email, string password)
        {
            this.email = email;
            this.password = password;
        }
    }

    public void OnEmailChanged(string input)
    {
        if (IsValidEmail(input))
        {
            emailErrorText.text = "";
        }
        else
        {
            emailErrorText.text = "Adresse email invalide.";
        }
    }

}
