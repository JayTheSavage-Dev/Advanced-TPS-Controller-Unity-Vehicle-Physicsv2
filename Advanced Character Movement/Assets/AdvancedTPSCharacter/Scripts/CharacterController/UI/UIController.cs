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
    public bool CancelAllMovement { get; set;}
    // Start is called before the first frame update
    void Start()
    {
        Controls = new PlayerControls();
        Controls.Enable();
        Controls.Keyboard.Escape.performed += ctx =>
        {
            Settings = !Settings;
        };
    }

    private void Update()
    {
        HandleOpenMenu();
    }
    private void HandleOpenMenu()
    {
        if (Settings)
        {
            if (RebindingUI != null)
            {
                RebindingUI.SetActive(true);
            }

            if (removecam != null)
            {
                removecam.SetActive(false);
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

            CancelAllMovement = false;
        }
    }
   
}
