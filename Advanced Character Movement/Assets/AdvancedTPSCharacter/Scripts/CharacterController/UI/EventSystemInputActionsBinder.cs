using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;

[AddComponentMenu("Advanced TPS/UI/EventSystem Input Actions Binder")]
[DisallowMultipleComponent]
[DefaultExecutionOrder(-1000)]
public sealed class EventSystemInputActionsBinder : MonoBehaviour
{
    [SerializeField] private InputActionAsset inputActionsAsset;
    [SerializeField] private string uiActionMapName = "UI";
    [SerializeField] private string moveActionName = "Navigate";
    [SerializeField] private string submitActionName = "Submit";
    [SerializeField] private string cancelActionName = "Cancel";
    [SerializeField] private string pointActionName = "Point";
    [SerializeField] private string leftClickActionName = "Click";
    [SerializeField] private string rightClickActionName = "RightClick";
    [SerializeField] private string middleClickActionName = "MiddleClick";
    [SerializeField] private string scrollWheelActionName = "ScrollWheel";
    [SerializeField] private string trackedDevicePositionActionName = "TrackedDevicePosition";
    [SerializeField] private string trackedDeviceOrientationActionName = "TrackedDeviceOrientation";

    private InputSystemUIInputModule inputModule;

    private void Awake()
    {
        Bind();
    }

    private void Reset()
    {
        inputModule = GetComponent<InputSystemUIInputModule>();
    }

    private void OnValidate()
    {
        if (inputModule == null)
        {
            inputModule = GetComponent<InputSystemUIInputModule>();
        }
    }

    private void Bind()
    {
        if (inputModule == null)
        {
            inputModule = GetComponent<InputSystemUIInputModule>();
        }

        if (inputModule == null)
        {
            return;
        }

        if (inputActionsAsset == null)
        {
            if (inputModule.actionsAsset == null)
            {
                inputModule.AssignDefaultActions();
            }

            return;
        }

        InputActionMap uiActionMap = inputActionsAsset.FindActionMap(uiActionMapName, false);
        if (uiActionMap == null)
        {
            Debug.LogWarning($"EventSystemInputActionsBinder could not find action map '{uiActionMapName}' on '{inputActionsAsset.name}'.");
            if (inputModule.actionsAsset == null)
            {
                inputModule.AssignDefaultActions();
            }

            return;
        }

        inputModule.actionsAsset = inputActionsAsset;
        inputModule.move = CreateReference(uiActionMap, moveActionName);
        inputModule.submit = CreateReference(uiActionMap, submitActionName);
        inputModule.cancel = CreateReference(uiActionMap, cancelActionName);
        inputModule.point = CreateReference(uiActionMap, pointActionName);
        inputModule.leftClick = CreateReference(uiActionMap, leftClickActionName);
        inputModule.rightClick = CreateReference(uiActionMap, rightClickActionName);
        inputModule.middleClick = CreateReference(uiActionMap, middleClickActionName);
        inputModule.scrollWheel = CreateReference(uiActionMap, scrollWheelActionName);
        inputModule.trackedDevicePosition = CreateReference(uiActionMap, trackedDevicePositionActionName);
        inputModule.trackedDeviceOrientation = CreateReference(uiActionMap, trackedDeviceOrientationActionName);
    }

    private static InputActionReference CreateReference(InputActionMap map, string actionName)
    {
        if (map == null || string.IsNullOrWhiteSpace(actionName))
        {
            return null;
        }

        InputAction action = map.FindAction(actionName, false);
        return action != null ? InputActionReference.Create(action) : null;
    }
}
