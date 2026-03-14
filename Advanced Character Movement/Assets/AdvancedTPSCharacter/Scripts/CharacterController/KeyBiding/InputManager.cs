using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using UnityEngine.UI;
using System;

public class InputManager : MonoBehaviour
{
    public static PlayerControls inputActions;
    private static bool bindingOverridesLoaded;
    public static event Action rebindComplete;
    public static event Action rebindCanceled;
    public static event Action<InputAction, int> rebindStarted;

    private void Awake()
    {
        EnsureInputActions();
    }

    public static PlayerControls Actions => EnsureInputActions();

    public static PlayerControls EnsureInputActions()
    {
        if (inputActions == null)
        {
            inputActions = new PlayerControls();
        }

        if (!bindingOverridesLoaded)
        {
            foreach (InputAction action in inputActions.asset)
            {
                ApplyBindingOverrides(action);
            }

            bindingOverridesLoaded = true;
        }

        return inputActions;
    }

    public static void StartRebind(string actionName, int bindingIndex, TMPro.TextMeshProUGUI statusText, bool excludeMouse)
    {
        InputAction action = EnsureInputActions().asset.FindAction(actionName);
        if (action == null || action.bindings.Count <= bindingIndex)
        {
            Debug.Log("Couldn't find action or binding");
            return;
        }

        if (action.bindings[bindingIndex].isComposite)
        {
            var firstPartIndex = bindingIndex + 1;
            if (firstPartIndex < action.bindings.Count && action.bindings[firstPartIndex].isPartOfComposite)
                DoRebind(action, firstPartIndex, statusText, true, excludeMouse);
        }
        else
            DoRebind(action, bindingIndex, statusText, false, excludeMouse);
    }

    private static void DoRebind(InputAction actionToRebind, int bindingIndex, TMPro.TextMeshProUGUI statusText, bool allCompositeParts, bool excludeMouse)
    {
        if (actionToRebind == null || bindingIndex < 0)
            return;

        statusText.text = $"Press a {actionToRebind.expectedControlType}";

        actionToRebind.Disable();

        var rebind = actionToRebind.PerformInteractiveRebinding(bindingIndex);

        rebind.OnComplete(operation =>
        {
            actionToRebind.Enable();
            operation.Dispose();

            if (allCompositeParts)
            {
                var nextBindingIndex = bindingIndex + 1;
                if (nextBindingIndex < actionToRebind.bindings.Count && actionToRebind.bindings[nextBindingIndex].isComposite)
                    DoRebind(actionToRebind, nextBindingIndex, statusText, allCompositeParts, excludeMouse);
            }
            SaveBindingOverride(actionToRebind);
            rebindComplete?.Invoke();
        });

        rebind.OnCancel(operation =>
        {
            actionToRebind.Enable();
            operation.Dispose();
            rebindCanceled?.Invoke();
        });
        rebind.WithCancelingThrough("<Keyboard>/escape");
        rebind.WithCancelingThrough("<DualShockGamepad>/start");
        rebindStarted?.Invoke(actionToRebind, bindingIndex);
        rebind.Start();
    }
    public static string GetBindingName(string actionName, int bindingIndex)
    {
        InputAction action = EnsureInputActions().asset.FindAction(actionName);
        return action.GetBindingDisplayString(bindingIndex);
    }

    private static void SaveBindingOverride(InputAction action)
    {
        for (int i = 0; i < action.bindings.Count; i++)
        {
            PlayerPrefs.SetString(action.actionMap + action.name + i, action.bindings[i].overridePath);
        }
    }
    public static void LoadBindingOverride(string actionName)
    {
        InputAction action = EnsureInputActions().asset.FindAction(actionName);
        if (action == null)
        {
            return;
        }

        ApplyBindingOverrides(action);
    }

    private static void ApplyBindingOverrides(InputAction action)
    {
        if (action == null)
        {
            return;
        }

        for(int i = 0; i < action.bindings.Count; i++)
        {
            if (!string.IsNullOrEmpty(PlayerPrefs.GetString(action.actionMap + action.name + i, action.bindings[i].overridePath)))
            {
                action.ApplyBindingOverride(i, PlayerPrefs.GetString(action.actionMap + action.name + i, action.bindings[i].overridePath));
            }
        }
    }
    public static void ResetBinding(string actionName, int bindingIndex)
    {
        InputAction action = EnsureInputActions().asset.FindAction(actionName);
        if(action == null || action.bindings.Count <= bindingIndex)
        {
            Debug.Log("Could not find action or binding");
            return;
        }
        if (action.bindings[bindingIndex].isComposite)
        {
            for(int i = bindingIndex; i < action.bindings.Count && action.bindings[i].isComposite; i++)
            {
                action.RemoveBindingOverride(i);
            }
        }
        else
        {
            action.RemoveBindingOverride(bindingIndex);
        }
    }
}
