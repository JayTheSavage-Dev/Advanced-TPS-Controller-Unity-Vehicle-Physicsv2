using UnityEngine;

public class AdvancedCameraController : MonoBehaviour
{
    [SerializeField] private bool LockCursor;
    private UIController uiController;

    private void Start()
    {
        ApplyCursorState(false);
    }

    private void LateUpdate()
    {
        if (uiController == null)
        {
            uiController = FindFirstObjectByType<UIController>();
        }

        bool unlockCursor = uiController != null && uiController.CancelAllMovement;
        ApplyCursorState(unlockCursor);
    }

    private static void ApplyCursorState(bool unlockCursor)
    {
        Cursor.visible = unlockCursor;
        Cursor.lockState = unlockCursor ? CursorLockMode.None : CursorLockMode.Locked;
    }
}
