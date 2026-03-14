using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Animations.Rigging;

public enum CharacterState
{
    Normal,
    Vehicle
}
public class AdvancedCharacterMovement : MonoBehaviour
{
    public CharacterState state;
    Vector3 PlayerMoveInput;
    Vector2 MouseMoveInput;
    Vector3 velocity;
    Animator animator;
    [SerializeField] private Rig aimLayer;
    private float VelocityX;
    private float VelocityZ;
    private float accleration = 3f;
    private float deccleration = 4f;
    private float Speed;
    public float SpeedMultiplier;
    private CharacterController Controller;
    private PlayerControls Controls;
    [SerializeField] private float ConstGravity = -9.81f;
    [SerializeField] private float jumpHeight = 1.5f;
    [SerializeField] private float jumpCooldown = 0.1f;
    bool running;
    bool crouching;
    bool jumping;
    bool jumpAnimationTriggered;
    bool hasGroundedParameter;
    bool hasVerticalVelocityParameter;
    [SerializeField] private float animationDampTime = 0.08f;
    float lastJumpTime = Mathf.NegativeInfinity;
    bool aiming;
    public bool Crouched;
    [HideInInspector]
    public bool IsWalking;
    public bool IsRunning;
    public bool IsCrouching;
    [SerializeField] private Camera cam;
    UIController controller;
    private ActiveWeapon weapon;
    public float turnSpeed = 15;
    private AmmoWidget widget;
    [SerializeField] private GameObject CameraLookAt;
    [SerializeField] private GameObject CameraLookAtOffset;
    private Vector3 CameraLookAtOffsetVector;
    private PlayerHealth playerHealth;
    private void Awake()
    {
        if (transform.parent != null)
        {
            widget = transform.parent.gameObject.GetComponentInChildren<AmmoWidget>();
        }

        state = CharacterState.Normal;
        weapon = GetComponent<ActiveWeapon>();
        animator = GetComponent<Animator>();
        controller = GetComponentInChildren<UIController>();
        Controls = InputManager.Actions;
        Controller = GetComponent<CharacterController>();

        CacheAnimatorParameters();

        playerHealth = GetComponent<PlayerHealth>();
        if (playerHealth == null)
        {
            playerHealth = gameObject.AddComponent<PlayerHealth>();
        }
    }

    private void OnEnable()
    {
        Controls.Enable();
        Controls.Keyboard.MovementKeyBinds.performed += OnMovementPerformed;
        Controls.Keyboard.MovementKeyBinds.canceled += OnMovementPerformed;
        Controls.Keyboard.Aim.started += OnAimStarted;
        Controls.Keyboard.Aim.canceled += OnAimCanceled;
        Controls.Keyboard.Sprint.performed += OnSprintPerformed;
        Controls.Keyboard.Sprint.canceled += OnSprintCanceled;
        Controls.Keyboard.Crouch.performed += OnCrouchPerformed;
        Controls.Keyboard.Jump.performed += OnJumpPerformed;
        Controls.Keyboard.Equip.performed += OnEquipPerformed;
        Controls.Car.ExitVehicle.performed += OnExitVehiclePerformed;
    }

    private void OnDisable()
    {
        if (Controls == null)
        {
            return;
        }

        Controls.Keyboard.MovementKeyBinds.performed -= OnMovementPerformed;
        Controls.Keyboard.MovementKeyBinds.canceled -= OnMovementPerformed;
        Controls.Keyboard.Aim.started -= OnAimStarted;
        Controls.Keyboard.Aim.canceled -= OnAimCanceled;
        Controls.Keyboard.Sprint.performed -= OnSprintPerformed;
        Controls.Keyboard.Sprint.canceled -= OnSprintCanceled;
        Controls.Keyboard.Crouch.performed -= OnCrouchPerformed;
        Controls.Keyboard.Jump.performed -= OnJumpPerformed;
        Controls.Keyboard.Equip.performed -= OnEquipPerformed;
        Controls.Car.ExitVehicle.performed -= OnExitVehiclePerformed;
    }

    private void OnMovementPerformed(InputAction.CallbackContext ctx)
    {
        Vector2 movement = ctx.ReadValue<Vector2>();
        PlayerMoveInput = new Vector3(movement.x, PlayerMoveInput.y, movement.y);
    }

    private void OnAimStarted(InputAction.CallbackContext ctx)
    {
        int index = weapon.activeWeaponIndex;
        GunController weaponUsing = weapon.GetWeapon(index);
        if (weaponUsing == null) { return; }
        aiming = true;
    }

    private void OnAimCanceled(InputAction.CallbackContext ctx)
    {
        int index = weapon.activeWeaponIndex;
        GunController weaponUsing = weapon.GetWeapon(index);
        if (weaponUsing == null) { return; }
        aiming = false;
    }

    private void OnSprintPerformed(InputAction.CallbackContext ctx)
    {
        if (aiming) { running = false; return; }
        running = true;
    }

    private void OnSprintCanceled(InputAction.CallbackContext ctx)
    {
        if (aiming) { running = false; return; }
        running = false;
    }

    private void OnCrouchPerformed(InputAction.CallbackContext ctx)
    {
        crouching = !crouching;
    }

    private bool IsMovementBlocked()
    {
        return state == CharacterState.Vehicle
            || (controller != null && controller.CancelAllMovement)
            || (weapon != null && weapon.CancelAllMovement);
    }

    private void OnJumpPerformed(InputAction.CallbackContext ctx)
    {
        if (!Controller.isGrounded) { return; }
        if (IsMovementBlocked()) { return; }
        if (Time.time < lastJumpTime + jumpCooldown) { return; }
        jumping = true;
    }

    private void OnEquipPerformed(InputAction.CallbackContext ctx)
    {
        if (state == CharacterState.Normal)
        {
            EnterOrEquipVehicle();
        }
    }

    private void OnExitVehiclePerformed(InputAction.CallbackContext ctx)
    {
        if (state == CharacterState.Vehicle)
        {
            ExitOrLeaveVehicle();
        }
    }

    private void CacheAnimatorParameters()
    {
        hasGroundedParameter = AnimatorHasParameter("Grounded", AnimatorControllerParameterType.Bool);
        hasVerticalVelocityParameter = AnimatorHasParameter("VerticalVelocity", AnimatorControllerParameterType.Float);
    }

    private bool AnimatorHasParameter(string parameterName, AnimatorControllerParameterType parameterType)
    {
        foreach (AnimatorControllerParameter parameter in animator.parameters)
        {
            if (parameter.name == parameterName && parameter.type == parameterType)
            {
                return true;
            }
        }

        return false;
    }
    private void Update()
    {
        Crouched = crouching;
        if (weapon != null && weapon.IsWeaponBusy())
        {
            running = false;
            IsRunning = false;
        }

        if (state == CharacterState.Vehicle) { IsWalking = false; IsCrouching = false; IsRunning = false; return; }
        HandleGravity();
        HandleMovement();
        HandleCharacterRotation();
        HandleAnimations();
    }
    //Move Character On Input
    private void HandleMovement()
    {
        //Establish Direction
        Vector3 MoveVec = transform.TransformDirection(PlayerMoveInput).normalized;
        //Establish Inputs
        bool forwardPressed = PlayerMoveInput.z > 0.5;
        bool backwardPressed = PlayerMoveInput.z < -0.5;
        bool leftPressed = PlayerMoveInput.x < -0.5;
        bool rightPressed = PlayerMoveInput.x > 0.5;
        if (controller.CancelAllMovement == true) return;
        if (weapon.CancelAllMovement == true) { return; }
        if (crouching)
        {
            Speed = 0.75f;
            Controller.Move(MoveVec * (Speed * SpeedMultiplier) * Time.deltaTime);
            if (forwardPressed || backwardPressed || leftPressed || rightPressed)
            {
                IsCrouching = true;
                IsRunning = false;
                IsWalking = false;
            }
        }
        else
        {
            if (forwardPressed && !running && !backwardPressed)
            {
                IsCrouching = false;
                IsWalking = true;
                IsRunning = false;
                Speed = 1.2f;
                Controller.Move(MoveVec * (Speed * SpeedMultiplier) * Time.deltaTime);
            }
            if (!forwardPressed && !running && (leftPressed || rightPressed || backwardPressed))
            {
                IsCrouching = false;
                IsWalking = true;
                IsRunning = false;
                Speed = 1f;
                Controller.Move(MoveVec * (Speed * SpeedMultiplier) * Time.deltaTime);
            }
            if (forwardPressed && running && !backwardPressed && !leftPressed && !rightPressed)
            {
                IsCrouching = false;
                IsWalking = false;
                IsRunning = true;
                Speed = 2f;
                Controller.Move(MoveVec * (Speed * SpeedMultiplier) * Time.deltaTime);
            }
            if (!forwardPressed && running && (backwardPressed))
            {
                IsCrouching = false;
                IsWalking = false;
                IsRunning = true;
                Speed = 1.4f;
                Controller.Move(MoveVec * (Speed * SpeedMultiplier) * Time.deltaTime);
            }
            if (!forwardPressed && running && (leftPressed || rightPressed) && !backwardPressed)
            {
                IsCrouching = false;
                IsWalking = false;
                IsRunning = true;
                Speed = 1f;
                Controller.Move(MoveVec * (Speed * SpeedMultiplier) * Time.deltaTime);
            }
            if (forwardPressed && running && !backwardPressed && (leftPressed || rightPressed))
            {
                IsCrouching = false;
                IsWalking = false;
                IsRunning = true;
                Speed = 1.8f;
                Controller.Move(MoveVec * (Speed * SpeedMultiplier) * Time.deltaTime);
            }
        }
    }
    private bool IsGrounded()
    {
        return Controller.isGrounded;
    }
    private void HandleGravity()
    {
        bool grounded = IsGrounded();
        bool jumpedThisFrame = false;

        if (jumping)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * ConstGravity);
            jumping = false;
            jumpAnimationTriggered = false;
            lastJumpTime = Time.time;
            jumpedThisFrame = true;
        }

        if (grounded && velocity.y < 0f && !jumpedThisFrame)
        {
            velocity.y = -2f;
        }

        velocity.y += ConstGravity * Time.deltaTime;
        Controller.Move(velocity * Time.deltaTime);
    }
    private void HandleAnimations()
    {
        bool grounded = IsGrounded();
        float verticalVelocity = velocity.y;

        if (!grounded && verticalVelocity > 0f && !jumpAnimationTriggered)
        {
            animator.SetTrigger("Jumping");
            jumpAnimationTriggered = true;
        }

        if (grounded && verticalVelocity <= 0f)
        {
            jumpAnimationTriggered = false;
            animator.ResetTrigger("Jumping");
        }

        if (hasGroundedParameter)
        {
            animator.SetBool("Grounded", grounded);
        }

        if (hasVerticalVelocityParameter)
        {
            animator.SetFloat("VerticalVelocity", verticalVelocity, animationDampTime, Time.deltaTime);
        }

        if (weapon.CancelAllMovement == true) { return; }
        if (controller.CancelAllMovement == true) return;
        bool forwardPressed = PlayerMoveInput.z > 0.5;
        bool backwardPressed = PlayerMoveInput.z < -0.5;
        bool leftPressed = PlayerMoveInput.x < -0.5;
        bool rightPressed = PlayerMoveInput.x > 0.5;

        if (crouching)
        {
            animator.SetBool("Crouching", true);
            if (forwardPressed && VelocityZ < 0.5f)
            {
                VelocityZ += Time.deltaTime * accleration;
            }
            if (leftPressed && VelocityX > -0.5f)
            {
                VelocityX -= Time.deltaTime * accleration;
            }
            if (rightPressed && VelocityX < 0.5f)
            {
                VelocityX += Time.deltaTime * accleration;
            }
            if (backwardPressed && VelocityZ > -0.5f)
            {
                VelocityZ -= Time.deltaTime * accleration;
            }
            //decrease velocity on axis
            if (!forwardPressed && VelocityZ > 0.0f)
            {
                VelocityZ -= Time.deltaTime * deccleration;
            }
            if (!backwardPressed && VelocityZ < 0.0f)
            {
                VelocityZ += Time.deltaTime * deccleration;
            }
            if (!leftPressed && VelocityX < 0.0f)
            {
                VelocityX += Time.deltaTime * deccleration;
            }
            if (!rightPressed && VelocityX > 0.0f)
            {
                VelocityX -= Time.deltaTime * deccleration;
            }
            // reset VelocityZ
            if (!forwardPressed && !backwardPressed && VelocityZ != 0.0f && (VelocityZ > -0.05f && VelocityZ < 0.05f))
            {
                IsCrouching = false;
                VelocityZ = 0.0f;
            }
            // reset VelocityX
            if (!leftPressed && !rightPressed && VelocityX != 0.0f && (VelocityX > -0.05f && VelocityX < 0.05f))
            {
                IsCrouching = false;
                VelocityX = 0.0f;
            }
            // set the parameters to our local variable values
            animator.SetFloat("CrouchingVelocityZ", VelocityZ, animationDampTime, Time.deltaTime);
            animator.SetFloat("VelocityX", VelocityX, animationDampTime, Time.deltaTime);
        }
        else
        {
            animator.SetBool("Crouching", false);
            //increase velocity on axis
            if (forwardPressed && VelocityZ < 0.5f && !running)
            {
                VelocityZ += Time.deltaTime * accleration;
            }
            if (leftPressed && VelocityX > -0.5f)
            {
                VelocityX -= Time.deltaTime * accleration;
            }
            if (rightPressed && VelocityX < 0.5f)
            {
                VelocityX += Time.deltaTime * accleration;
            }
            if (backwardPressed && VelocityZ > -0.5f && !running)
            {
                VelocityZ -= Time.deltaTime * accleration;
            }
            if (forwardPressed && VelocityZ < 1f && running)
            {
                VelocityZ += Time.deltaTime * accleration;
            }
            if (backwardPressed && VelocityZ > -1f && running)
            {
                VelocityZ -= Time.deltaTime * accleration;
            }
            //Make sure velocity isn't increasing/decreasing
            if (forwardPressed && leftPressed && !running && VelocityZ > 0.5)
            {
                VelocityZ -= Time.deltaTime * deccleration;
            }
            if (forwardPressed && rightPressed && !running && VelocityZ > 0.5)
            {
                VelocityZ -= Time.deltaTime * deccleration;
            }
            if (backwardPressed && leftPressed && !running && VelocityZ < -0.5)
            {
                VelocityZ += Time.deltaTime * deccleration;
            }
            if (backwardPressed && rightPressed && !running && VelocityZ < -0.5)
            {
                VelocityZ += Time.deltaTime * deccleration;
            }
            if (forwardPressed && !running && VelocityZ > 0.5)
            {
                VelocityZ -= Time.deltaTime * deccleration;
            }
            if (backwardPressed && !running && VelocityZ < -0.5)
            {
                VelocityZ += Time.deltaTime * deccleration;
            }
            //decrease velocity on axis
            if (!forwardPressed && VelocityZ > 0.0f)
            {
                VelocityZ -= Time.deltaTime * deccleration;
            }
            if (!backwardPressed && VelocityZ < 0.0f)
            {
                VelocityZ += Time.deltaTime * deccleration;
            }
            if (!leftPressed && VelocityX < 0.0f)
            {
                VelocityX += Time.deltaTime * deccleration;
            }
            if (!rightPressed && VelocityX > 0.0f)
            {
                VelocityX -= Time.deltaTime * deccleration;
            }
            // reset VelocityZ
            if (!forwardPressed && !backwardPressed && VelocityZ != 0.0f && (VelocityZ > -0.05f && VelocityZ < 0.05f))
            {
                IsCrouching = false;
                IsRunning = false;
                IsWalking = false;
                VelocityZ = 0.0f;
            }
            // reset VelocityX
            if (!leftPressed && !rightPressed && VelocityX != 0.0f && (VelocityX > -0.05f && VelocityX < 0.05f))
            {
                IsCrouching = false;
                IsWalking = false;
                IsRunning = false;
                VelocityX = 0.0f;
            }
            // set the parameters to our local variable values
            animator.SetFloat("StandingVelocityZ", VelocityZ, animationDampTime, Time.deltaTime);
            animator.SetFloat("VelocityX", VelocityX, animationDampTime, Time.deltaTime);
        }
    }
    private void HandleCharacterRotation()
    {
        if (controller.CancelAllMovement == true) return;
        float yawCamera = cam.transform.rotation.eulerAngles.y;
        transform.rotation = Quaternion.Euler(0, yawCamera, 0);
    }

    // Car Code Start
    void EnterOrEquipVehicle()
    {
        CarCheck carCheck = GetComponentInChildren<CarCheck>();
        if (carCheck == null) { return; }
        if (carCheck.CanEnterVehicle)
        {
            Debug.Log("We Can Enter Vehicle");
            carCheck.EnterVehicle();
        }
        else
        {
            return;
        }
    }
    public void EnterCar()
    {
        widget.gameObject.SetActive(false);
        SkinnedMeshRenderer[] meshRenders;
        meshRenders = GetComponentsInChildren<SkinnedMeshRenderer>();
        foreach (SkinnedMeshRenderer renderer in meshRenders)
        {
            renderer.enabled = false;
        }
        gameObject.GetComponent<CharacterController>().enabled = false;
        GetComponentInChildren<CarCheck>().gameObject.GetComponent<CapsuleCollider>().enabled = false;
        state = CharacterState.Vehicle;
        GunController[] gunControllers = GetComponentsInChildren<GunController>();
        foreach (GunController controller in gunControllers)
        {
            MeshRenderer[] renderers = controller.GetComponentsInChildren<MeshRenderer>();
            foreach (MeshRenderer renderer in renderers)
            {
                renderer.enabled = false;
            }
        }
    }
    public void EnterMotorcycle(GameObject CarEnter)
    {
        animator.Play("Driving");
        animator.SetBool("Driving", true);
        widget.gameObject.SetActive(false);
        gameObject.GetComponent<CharacterController>().enabled = false;
        GetComponentInChildren<CarCheck>().gameObject.GetComponent<CapsuleCollider>().enabled = false;
        state = CharacterState.Vehicle;
        GunController[] gunControllers = GetComponentsInChildren<GunController>();
        foreach (GunController controller in gunControllers)
        {
            MeshRenderer[] renderers = controller.GetComponentsInChildren<MeshRenderer>();
            foreach (MeshRenderer renderer in renderers)
            {
                renderer.enabled = false;
            }
        }
        GetComponent<AdvancedFootIKPlacement>().enabled = false;
        transform.localRotation = Quaternion.identity;
        transform.localPosition = Vector3.zero;
        transform.parent.transform.localPosition = new Vector3(CarEnter.transform.localPosition.x, CarEnter.transform.localPosition.y - 30f, CarEnter.transform.localPosition.z);
        transform.parent.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
        GetComponentInChildren<WeaponAnimationEvents>().gameObject.GetComponent<Animator>().enabled = false;
    }
    void ExitOrLeaveVehicle()
    {
        CarCheck carCheck = GetComponentInChildren<CarCheck>();
        if (carCheck == null) { return; }
        if (!carCheck.CanEnterVehicle)
        {
            carCheck.ExitVehicle();
        }
        else
        {
            return;
        }
    }
    public void ExitCar()
    {
        widget.gameObject.SetActive(true);
        SkinnedMeshRenderer[] meshRenders;
        meshRenders = GetComponentsInChildren<SkinnedMeshRenderer>();
        foreach (SkinnedMeshRenderer renderer in meshRenders)
        {
            renderer.enabled = true;
        }
        gameObject.GetComponent<CharacterController>().enabled = true;
        GetComponentInChildren<CarCheck>().gameObject.GetComponent<CapsuleCollider>().enabled = true;
        state = CharacterState.Normal;
        GunController[] gunControllers = GetComponentsInChildren<GunController>();
        foreach (GunController controller in gunControllers)
        {
            MeshRenderer[] renderers = controller.GetComponentsInChildren<MeshRenderer>();
            foreach (MeshRenderer renderer in renderers)
            {
                renderer.enabled = true;
            }
        }
    }
    public void ExitMotorcycle()
    {
        widget.gameObject.SetActive(true);
        gameObject.GetComponent<CharacterController>().enabled = true;
        GetComponentInChildren<CarCheck>().gameObject.GetComponent<CapsuleCollider>().enabled = true;
        state = CharacterState.Normal;
        GunController[] gunControllers = GetComponentsInChildren<GunController>();
        foreach (GunController controller in gunControllers)
        {
            MeshRenderer[] renderers = controller.GetComponentsInChildren<MeshRenderer>();
            foreach (MeshRenderer renderer in renderers)
            {
                renderer.enabled = true;
            }
        }
        animator.SetBool("Driving", false);
        GetComponent<AdvancedFootIKPlacement>().enabled = true;
    }
    // Car Code End

    public void ActivateRagdoll()
    {
        Debug.Log("Died");
    }

}
