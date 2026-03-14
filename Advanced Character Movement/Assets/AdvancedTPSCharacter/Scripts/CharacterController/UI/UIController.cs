using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class UIController : MonoBehaviour
{
    private PlayerControls Controls;
    [SerializeField] public GameObject CarButton;
    [SerializeField] private GameObject RebindingUI;
    public AdvancedCharacterMovement Player;
    [SerializeField]
    private GameObject removecam;
    bool Settings;
    [SerializeField] private bool preferUIToolkit = true;
    public bool CancelAllMovement { get; set;}

    private void Awake()
    {
        Controls = InputManager.Actions;
    }

    private void OnEnable()
    {
        Controls.Enable();
        Controls.Keyboard.Escape.performed += OnEscapePerformed;
    }

    private void OnDisable()
    {
        if (Controls == null)
        {
            return;
        }

        Controls.Keyboard.Escape.performed -= OnEscapePerformed;
    }

    private void OnEscapePerformed(UnityEngine.InputSystem.InputAction.CallbackContext ctx)
    {
        Settings = !Settings;
    }

    private void Update()
    {
        HandleOpenMenu();
    }

    private bool UseUIToolkitOnly()
    {
        return preferUIToolkit && UIToolkitUIBridge.Instance != null;
    }

    private void HandleOpenMenu()
    {
        if (Settings)
        {
            if (RebindingUI != null)
            {
                RebindingUI.SetActive(!UseUIToolkitOnly());
            }

            if (removecam != null)
            {
                removecam.SetActive(false);
            }

            if (UIToolkitUIBridge.Instance != null)
            {
                UIToolkitUIBridge.Instance.SetSettingsVisible(true);
                UIToolkitUIBridge.Instance.SetStatusNotification("PAUSED", 1.2f);
            }

            CancelAllMovement = true;
        }
        else
        {
            if (RebindingUI != null)
            {
                RebindingUI.SetActive(false);
            }

            if (removecam != null)
            {
                removecam.SetActive(true);
            }

            if (UIToolkitUIBridge.Instance != null)
            {
                UIToolkitUIBridge.Instance.SetSettingsVisible(false);
                UIToolkitUIBridge.Instance.SetStatusNotification("RESUMED", 0.9f);
            }

            CancelAllMovement = false;
        }
    }
   
}
