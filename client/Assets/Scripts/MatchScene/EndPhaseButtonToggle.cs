using UnityEngine;
using UnityEngine.UIElements;

public class EndPhaseButtonHandler : MonoBehaviour
{
    private Button endPhaseButton;
    private bool isPhase = true;

    void OnEnable()
    {
        VisualElement root = GetComponent<UIDocument>().rootVisualElement;
        endPhaseButton = root.Q<Button>("EndPhaseButton");

        if (endPhaseButton != null)
        {
            endPhaseButton.text = "END PHASE";
            endPhaseButton.clicked += OnEndPhaseButtonClicked;
        }
        else
        {
            Debug.LogWarning("Bouton EndPhaseButton introuvable dans le UXML !");
        }
    }

    void OnDisable()
    {
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

        isPhase = !isPhase;
    }
}
