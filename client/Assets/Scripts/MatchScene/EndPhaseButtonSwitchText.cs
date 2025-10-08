using UnityEngine;
using UnityEngine.UIElements;

public class EndPhaseButtonHandler : MonoBehaviour
{
    private Button endPhaseButton;
    private bool isPhase = true;

    void OnEnable()
    {
        // Récupère la racine du UI Document
        var root = GetComponent<UIDocument>().rootVisualElement;

        // Récupère le bouton par son nom défini dans le UXML
        endPhaseButton = root.Q<Button>("EndPhaseButton");

        if (endPhaseButton != null)
        {
            // Initialise le texte au démarrage
            endPhaseButton.text = "END PHASE";

            // Abonne-toi à l'événement de clic
            endPhaseButton.clicked += OnEndPhaseButtonClicked;
        }
        else
        {
            Debug.LogWarning("⚠️ Bouton 'EndPhaseButton' introuvable dans le UXML !");
        }
    }

    void OnDisable()
    {
        // Important : toujours se désabonner pour éviter les doublons
        if (endPhaseButton != null)
            endPhaseButton.clicked -= OnEndPhaseButtonClicked;
    }

    private void OnEndPhaseButtonClicked()
    {
        if (isPhase)
        {
            endPhaseButton.text = "END TURN";
        }
        else
        {
            endPhaseButton.text = "END PHASE";
        }

        isPhase = !isPhase; // Inverse l’état
    }
}
