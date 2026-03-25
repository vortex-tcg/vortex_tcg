using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Text;

public class AuthenticationService
{
    private TokenService tokenService = new TokenService();
    private LoginValidator validator = new LoginValidator();
    private EventBus eventBus = EventBus.Instance;

    public IEnumerator Login(string email, string password, string apiBaseUrl)
    {

        ValidateLoginInputEvent validateEvent = new ValidateLoginInputEvent(email, password);
        eventBus.Publish(validateEvent);

        if (!validateEvent.IsValid)
        {
            eventBus.Publish(new LoginFailedEvent(validateEvent.ValidationError));
            yield break;
        }

        string url = apiBaseUrl.TrimEnd('/') + "/login";
        using UnityWebRequest request = CreateLoginRequest(email, password, url);

        Debug.Log("[AuthenticationService] POST " + url);

        yield return request.SendWebRequest();
        yield return ProcessResponse(request, email);
    }

    private UnityWebRequest CreateLoginRequest(string email, string password, string url)
    {
        string jsonBody = JsonUtility.ToJson(new LoginData(email, password));

        UnityWebRequest request = new UnityWebRequest(url, "POST");
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonBody);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        return request;
    }

    private IEnumerator ProcessResponse(UnityWebRequest request, string email)
    {
        if (request.result == UnityWebRequest.Result.Success)
        {
            yield return ProcessSuccess(request.downloadHandler.text, email);
        }
        else
        {
            ProcessError(request);
        }
    }

    private IEnumerator ProcessSuccess(string response, string email)
    {
        Debug.Log("[AuthenticationService] Réponse: " + response);


        string token = tokenService.ExtractTokenFromResponse(response);

        if (string.IsNullOrEmpty(token))
        {
            token = tokenService.BuildMockJwt(email, 3600);
            Debug.LogWarning("[AuthenticationService] Utilisation d'un JWT mock (dev).");
        }

        bool success = tokenService.ValidateAndStoreToken(token);

        if (success)
        {
            Debug.Log("[AuthenticationService] Authentification réussie");
            eventBus.Publish(new LoginSuccessEvent(token));
        }
        else
        {
            eventBus.Publish(new LoginFailedEvent("Token invalide ou expiré"));
        }

        yield break;
    }

    private void ProcessError(UnityWebRequest request)
    {
        string errorMessage;
        int? errorCode = (int)request.responseCode;

        if (request.responseCode == 401)
        {
            errorMessage = "Email ou mot de passe incorrect.";
        }
        else
        {
            errorMessage = $"Erreur HTTP {request.responseCode} : {request.error}";
            Debug.LogError("[AuthenticationService] " + errorMessage);
        }

        eventBus.Publish(new LoginFailedEvent(errorMessage, errorCode));
    }
}
