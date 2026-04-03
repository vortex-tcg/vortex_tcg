using UnityEngine;

public class EscapeToggleObject : MonoBehaviour
{
    [SerializeField] private GameObject target;
    [SerializeField] private bool startHidden = true;

    private void Start()
    {
        if (target == null)
        {
            Debug.LogWarning("[EscapeToggleObject] Target is not assigned.");
            return;
        }

        target.SetActive(!startHidden);
    }

    private void Update()
    {
        if (!Input.GetKeyDown(KeyCode.Escape))
            return;

        if (target == null)
        {
            Debug.LogWarning("[EscapeToggleObject] Target is not assigned.");
            return;
        }

        if (target == gameObject)
        {
            Debug.LogWarning("[EscapeToggleObject] Target is the same GameObject as this component. Use another GameObject.");
            return;
        }

        target.SetActive(!target.activeSelf);
    }
}
