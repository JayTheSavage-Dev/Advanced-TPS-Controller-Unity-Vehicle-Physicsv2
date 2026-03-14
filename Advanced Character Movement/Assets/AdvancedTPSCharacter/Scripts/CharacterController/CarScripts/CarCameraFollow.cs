using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CarCameraFollow : MonoBehaviour
{
    [SerializeField] private Vector3 offset;
    [SerializeField] private Transform target;
    [SerializeField] private float translateSpeed;
    private PlayerControls Controls;
    [SerializeField] private float rotationSpeed;

    private void Awake()
    {
        Controls = InputManager.Actions;
    }

    private void OnEnable()
    {
        Controls.Enable();
        Controls.Car.ToggleView.performed += OnToggleViewPerformed;
    }

    private void OnDisable()
    {
        if (Controls == null)
        {
            return;
        }

        Controls.Car.ToggleView.performed -= OnToggleViewPerformed;
    }

    private void OnToggleViewPerformed(InputAction.CallbackContext ctx)
    {
        if(transform.parent.gameObject.GetComponentInChildren<CarController>().carState == CarState.NotOccupied) { return; }
        ToggleView();
    }
    private void FixedUpdate()
    {
        HandleTranslation();
        HandleRotation();
    }

    private void HandleRotation()
    {
        var direction = target.position - transform.position;
        var rotation = Quaternion.LookRotation(direction, Vector3.up);
        transform.rotation = Quaternion.Lerp(transform.rotation, rotation, rotationSpeed * Time.deltaTime);
    }

    private void HandleTranslation()
    {
        var targetPosition = target.TransformPoint(offset);
        transform.position = Vector3.Lerp(transform.position, targetPosition, translateSpeed * Time.deltaTime);
    }
    private void ToggleView()
    {
        offset = new Vector3(offset.x, offset.y, -offset.z);
        var targetPosition = target.TransformPoint(offset);
        transform.position = Vector3.Lerp(transform.position, targetPosition, translateSpeed * Time.deltaTime);
    }
}
