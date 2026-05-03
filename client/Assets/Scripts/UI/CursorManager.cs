using UnityEngine;
using UnityEngine.UIElements;

public class CursorManager : MonoBehaviour
{
    [SerializeField] private UIDocument uiDocument;

    [SerializeField] private Texture2D normalCursor;
    [SerializeField] private Texture2D normalCursorClicked;
    [SerializeField] private Texture2D pointerCursor;
    [SerializeField] private Texture2D pointerCursorClicked;

    private bool isPointerHover;
    private bool isClicking;
    private Texture2D currentCursor;

    private void OnEnable()
    {
        VisualElement root = uiDocument.rootVisualElement;

        var interactives = root.Query<VisualElement>(className: "interactive").ToList();

        foreach (VisualElement element in interactives)
        {
            element.RegisterCallback<PointerEnterEvent>(_ =>
            {
                isPointerHover = true;
                RefreshCursor();
            });

            element.RegisterCallback<PointerLeaveEvent>(_ =>
            {
                isPointerHover = false;
                RefreshCursor();
            });

            element.RegisterCallback<PointerDownEvent>(_ =>
            {
                isClicking = true;
                RefreshCursor();
            });

            element.RegisterCallback<PointerUpEvent>(_ =>
            {
                isClicking = false;
                RefreshCursor();
            });
        }

        RefreshCursor();
    }

    private void Update()
    {
        bool newClickState = Input.GetMouseButton(0);

        if (newClickState != isClicking)
        {
            isClicking = newClickState;
            RefreshCursor();
        }
    }

    private void RefreshCursor()
    {
        Texture2D nextCursor;

        if (isPointerHover)
            nextCursor = isClicking ? pointerCursorClicked : pointerCursor;
        else
            nextCursor = isClicking ? normalCursorClicked : normalCursor;

        if (currentCursor == nextCursor)
            return;

        currentCursor = nextCursor;
        UnityEngine.Cursor.SetCursor(currentCursor, Vector2.zero, CursorMode.Auto);
    }
}