using UnityEngine;
using UnityEngine.UIElements;
using System.Collections;

public class LoginScript : MonoBehaviour
{
    [Header("UI Toolkit")]
    [SerializeField] private UIDocument uiDocument;

    private TextField emailField;
    private TextField passwordField;
    private Label emailErrorLabel;
    private Button loginButton;
    private Button togglePasswordButton;

    private bool passwordVisible = false;
    private bool isSubmitting = false;

    private EventCallback<ChangeEvent<string>> emailChangedCb;
    private EventCallback<ChangeEvent<string>> passwordChangedCb;

    private AuthenticationService authService = new AuthenticationService();
    private LoginValidator validator = new LoginValidator();
    private EventBus eventBus = EventBus.Instance;

    private void OnEnable()
    {
        if (uiDocument == null)
            uiDocument = GetComponent<UIDocument>();

        if (uiDocument == null)
        {
            Debug.LogError("[LoginScript] UIDocument manquant.");
            return;
        }

        eventBus.Subscribe<LoginSuccessEvent>(OnLoginSuccess);
        eventBus.Subscribe<LoginFailedEvent>(OnLoginFailed);

        VisualElement root = uiDocument.rootVisualElement;

        emailField = root.Q<TextField>("UsernameField");
        passwordField = root.Q<TextField>("PasswordField");
        emailErrorLabel = root.Q<Label>("ErrorLabel");
        loginButton = root.Q<Button>("LoginButton");
        togglePasswordButton = root.Q<Button>("TogglePasswordButton");

        if (passwordField != null)
            passwordField.isPasswordField = true;

        emailChangedCb ??= _ => OnEmailValueChanged();
        passwordChangedCb ??= _ => UpdateLoginButtonState();

        if (emailField != null) emailField.RegisterValueChangedCallback(emailChangedCb);
        if (passwordField != null) passwordField.RegisterValueChangedCallback(passwordChangedCb);

        if (loginButton != null)
            loginButton.clicked += OnLoginClicked;

        if (togglePasswordButton != null)
            togglePasswordButton.clicked += TogglePasswordVisibility;

        HideError();
        UpdateLoginButtonState();
    }

    private void OnDisable()
    {

        eventBus.Unsubscribe<LoginSuccessEvent>(OnLoginSuccess);
        eventBus.Unsubscribe<LoginFailedEvent>(OnLoginFailed);

        if (emailField != null && emailChangedCb != null)
            emailField.UnregisterValueChangedCallback(emailChangedCb);

        if (passwordField != null && passwordChangedCb != null)
            passwordField.UnregisterValueChangedCallback(passwordChangedCb);

        if (loginButton != null)
            loginButton.clicked -= OnLoginClicked;

        if (togglePasswordButton != null)
            togglePasswordButton.clicked -= TogglePasswordVisibility;
    }

    private void OnLoginClicked()
    {
        if (isSubmitting) return;
        StartCoroutine(LoginCoroutine());
    }

    private void TogglePasswordVisibility()
    {
        passwordVisible = !passwordVisible;

        if (passwordField != null)
            passwordField.isPasswordField = !passwordVisible;

        if (togglePasswordButton != null)
            togglePasswordButton.text = passwordVisible ? "Hide" : "Show";
    }

    private void OnEmailValueChanged()
    {
        if (emailErrorLabel != null)
        {
            string email = emailField != null ? emailField.value : string.Empty;
            bool ok = validator.IsValidEmail(email);
            emailErrorLabel.text = ok ? string.Empty : "Adresse email invalide.";
            emailErrorLabel.style.display = ok ? DisplayStyle.None : DisplayStyle.Flex;
        }

        UpdateLoginButtonState();
    }

    private void UpdateLoginButtonState()
    {
        if (loginButton == null)
            return;

        if (isSubmitting)
        {
            loginButton.SetEnabled(false);
            return;
        }

        string email = emailField != null ? emailField.value : string.Empty;
        string password = passwordField != null ? passwordField.value : string.Empty;

        bool ready = validator.IsValidEmail(email) && !string.IsNullOrWhiteSpace(password);
        loginButton.SetEnabled(ready);
    }

    private void HideError()
    {
        if (emailErrorLabel == null) return;
        emailErrorLabel.text = string.Empty;
        emailErrorLabel.style.display = DisplayStyle.None;
    }

    private void ShowError(string message)
    {
        if (emailErrorLabel == null) return;
        emailErrorLabel.text = message ?? "Erreur inconnue.";
        emailErrorLabel.style.display = DisplayStyle.Flex;
    }

    private IEnumerator LoginCoroutine()
    {
        isSubmitting = true;
        UpdateLoginButtonState();
        HideError();

        AppConfig cfg = ConfigLoader.Load();
        if (cfg == null || string.IsNullOrEmpty(cfg.apiBaseUrl))
        {
            ShowError("Configuration API manquante.");
            isSubmitting = false;
            UpdateLoginButtonState();
            yield break;
        }

        string email = emailField != null ? emailField.value : string.Empty;
        string password = passwordField != null ? passwordField.value : string.Empty;

        eventBus.Publish(new LoginRequestedEvent(email, password));

        yield return authService.Login(email, password, cfg.apiBaseUrl);

        isSubmitting = false;
        UpdateLoginButtonState();
    }

    private void OnLoginSuccess(LoginSuccessEvent evt)
    {
        Debug.Log("Login réussi, navigation vers HomeScene");
        LoadingScreen.Load("HomeScene", loadMenu: true, unloadMenu: false);
    }

    private void OnLoginFailed(LoginFailedEvent evt)
    {
        Debug.LogError($"Login échoué: {evt.ErrorMessage}");
        ShowError(evt.ErrorMessage);
    }
}
