using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarCheck : MonoBehaviour
{
    public GameObject CarCheckUI;
    [SerializeField] private GameObject Crosshair;
    [HideInInspector] public bool CanEnterVehicle = false;
    GameObject CurrentVehicle;
    GameObject mainParent;

    private void Start()
    {
        CanEnterVehicle = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.gameObject.CompareTag("VehicleArea"))
        {
            return;
        }

        if (CarCheckUI != null)
        {
            CarCheckUI.SetActive(true);
        }

        if (Crosshair != null)
        {
            Crosshair.SetActive(false);
        }

        if (UIToolkitUIBridge.Instance != null)
        {
            UIToolkitUIBridge.Instance.SetVehiclePromptVisible(true);
            UIToolkitUIBridge.Instance.SetCrosshairVisible(false);
        }

        CurrentVehicle = other.gameObject;
        CanEnterVehicle = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.gameObject.CompareTag("VehicleArea"))
        {
            return;
        }

        if (CarCheckUI != null)
        {
            CarCheckUI.SetActive(false);
        }

        if (Crosshair != null)
        {
            Crosshair.SetActive(true);
        }

        if (UIToolkitUIBridge.Instance != null)
        {
            UIToolkitUIBridge.Instance.SetVehiclePromptVisible(false);
            UIToolkitUIBridge.Instance.SetCrosshairVisible(true);
        }

        CanEnterVehicle = false;
        CurrentVehicle = null;
    }

    public void EnterVehicle()
    {
        if (CurrentVehicle == null) { return; }
        CarController carController = CurrentVehicle.GetComponent<CarController>();
        carController.Speedometer.SetActive(true);
        CanEnterVehicle = false;

        if (CarCheckUI != null)
        {
            CarCheckUI.SetActive(false);
        }

        if (UIToolkitUIBridge.Instance != null)
        {
            UIToolkitUIBridge.Instance.SetVehiclePromptVisible(false);
        }

        if (carController == null) { return; }
        carController.carState = CarState.Occupied;
        carController.CarCamera.enabled = true;
        GetComponentInParent<AdvancedCharacterMovement>().EnterCar();
        GetComponentInParent<AdvancedCharacterMovement>().transform.parent.transform.parent = carController.CarExitTransform;
    }

    public void ExitVehicle()
    {
        if (CurrentVehicle == null) { return; }
        CarController carController = CurrentVehicle.GetComponent<CarController>();
        carController.Speedometer.SetActive(false);
        CanEnterVehicle = true;
        if (carController == null) { return; }
        carController.carState = CarState.NotOccupied;
        carController.CarCamera.enabled = false;
        GetComponentInParent<AdvancedCharacterMovement>().ExitCar();
        GetComponentInParent<AdvancedCharacterMovement>().transform.parent.transform.parent = null;
    }
}
