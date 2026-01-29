using UnityEngine;
using UnityEngine.UIElements;
using VortexTCG.Scripts.DTOs;

public class EndPhaseButtonHandler : MonoBehaviour
{
    private Button endPhaseButton;

    private void OnEnable()
    {
        UIDocument doc = GetComponent<UIDocument>();
        if (doc == null) return;

        VisualElement root = doc.rootVisualElement;
        endPhaseButton = root.Q<Button>("EndPhaseButton");

        if (endPhaseButton == null)
            Debug.LogWarning("Bouton EndPhaseButton introuvable dans le UXML !");
        else
            RefreshLabel();

        VortexTCG.Scripts.MatchScene.PhaseManager pm = VortexTCG.Scripts.MatchScene.PhaseManager.Instance;
        if (pm != null)
        {
            pm.OnEnterPlacement += RefreshLabel;
            pm.OnEnterAttack += RefreshLabel;
            pm.OnEnterDefense += RefreshLabel;
            pm.OnEnterEndTurn += RefreshLabel;
        }
    }

    private void OnDisable()
    {
        VortexTCG.Scripts.MatchScene.PhaseManager pm = VortexTCG.Scripts.MatchScene.PhaseManager.Instance;
        if (pm != null)
        {
            pm.OnEnterPlacement -= RefreshLabel;
            pm.OnEnterAttack -= RefreshLabel;
            pm.OnEnterDefense -= RefreshLabel;
            pm.OnEnterEndTurn -= RefreshLabel;
        }
    }

    private void RefreshLabel()
    {
        if (endPhaseButton == null) return;

        VortexTCG.Scripts.MatchScene.PhaseManager pm = VortexTCG.Scripts.MatchScene.PhaseManager.Instance;
        if (pm == null) return;

        endPhaseButton.text = (pm.CurrentPhase == GamePhase.END_TURN) ? "END TURN" : "END PHASE";
    }
}