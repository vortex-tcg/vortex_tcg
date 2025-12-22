using UnityEngine;
using UnityEngine.UIElements;
using VortexTCG.Scripts.DTOs;

public class EndPhaseButtonHandler : MonoBehaviour
{
    private Button endPhaseButton;

    private void OnEnable()
    {
        var doc = GetComponent<UIDocument>();
        if (doc == null) return;

        VisualElement root = doc.rootVisualElement;
        endPhaseButton = root.Q<Button>("EndPhaseButton");

        if (endPhaseButton == null)
            Debug.LogWarning("Bouton EndPhaseButton introuvable dans le UXML !");
        else
            RefreshLabel();
        
        if (VortexTCG.Scripts.MatchScene.PhaseManager.Instance != null)
            VortexTCG.Scripts.MatchScene.PhaseManager.Instance.OnEnterPlacement += RefreshLabel;
        if (VortexTCG.Scripts.MatchScene.PhaseManager.Instance != null)
            VortexTCG.Scripts.MatchScene.PhaseManager.Instance.OnEnterAttack += RefreshLabel;
        if (VortexTCG.Scripts.MatchScene.PhaseManager.Instance != null)
            VortexTCG.Scripts.MatchScene.PhaseManager.Instance.OnEnterDefense += RefreshLabel;
        if (VortexTCG.Scripts.MatchScene.PhaseManager.Instance != null)
            VortexTCG.Scripts.MatchScene.PhaseManager.Instance.OnEnterEndTurn += RefreshLabel;
    }

    private void OnDisable()
    {
        var pm = VortexTCG.Scripts.MatchScene.PhaseManager.Instance;
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

        var pm = VortexTCG.Scripts.MatchScene.PhaseManager.Instance;
        if (pm == null) return;
        endPhaseButton.text = pm.CurrentPhase == GamePhase.END_TURN ? "END TURN" : "END PHASE";
    }
}
