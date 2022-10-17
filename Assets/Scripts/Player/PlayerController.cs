using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Cinemachine;
using UnityEngine.Animations.Rigging;
using TMPro;
using UnityEngine.Events;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    public enum Mode { NORMAL,CRAWL,COVER,HOSTAGE}

    [SerializeField] public Transform trfPickup;
    [SerializeField] private Transform trfFirepoint;
    [SerializeField] private GameObject bullet;
    [SerializeField] private Transform aimTarget;
    [SerializeField] private Rig aimRig;
    [SerializeField] public LayerMask interactableLayer;
    private Animator animator;
    [SerializeField] private WeaponController weaponController;
    [SerializeField] private SwitchVCam cameraController;
    [SerializeField] private BoxDamager punchBox;
    [SerializeField] private GameObject thermalCamera;
    [SerializeField] private GameObject soundMaker;
    [SerializeField] private LayerMask ignorePlayer;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private LabelManager labelManager;

    [SerializeField] private float normalSpeed = 10.0f;
    [SerializeField] private float aimingSpeed = 5.0f;
    [SerializeField] private float gravityValue = -20f;
    [SerializeField] private float rotationSpeed = 10;
    [SerializeField] private float rotationSpeedAiming = 200;
    [SerializeField] private float animationSmoothTime = 0.1f;
    [SerializeField] private float aimDistance = 1;

    private FootstepCamoController footstepController;
    private CharacterController controller;
    private GhostController ghostController;
    private Inventory playerInventory;
    private Vector3 playerVelocity;
    private PlayerInput playerInput;
    public bool isAiming = false;
    private float currentRotationSpeed;
    private Transform trfCameraMain;
    public bool isCrouching = false;
    private bool weaponMenuHidden = true;

    //Noisemaker
    private int priority = 1;
    private int range = 10;

    private float currentSpeed;
    private float speedMultiplier = 100;
    
    private InputAction moveAction;
    private InputAction lightAttackAction;
    private InputAction heavyAttackAction;
    private InputAction fireAction;
    private InputAction interactAction;
    private InputAction scrollAction;
    private InputAction inventoryAction;
    private InputAction reloadAction;
    private InputAction aimAction;
    private InputAction crouchAction;
    private InputAction rollAction;
    private InputAction toggleThermal;
    private InputAction makeSound;
    private InputAction summonAction;
    private InputAction priorityAction;
    private InputAction rangeAction;
    

    public Interactable interactable;
    public EnemyStateController hostageController;

    public bool grounded;
    public Mode mode = Mode.NORMAL;

    private void Awake()
    {
        Debug.Log("");
        currentSpeed = normalSpeed;
        Cursor.lockState = CursorLockMode.Locked;
        controller = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
        playerInput = GetComponent<PlayerInput>();
        ghostController = GetComponent<GhostController>();
        ghostController.playerController = this;
        footstepController = GetComponent<FootstepCamoController>();
        playerInventory = GetComponent<Inventory>();
        moveAction = playerInput.actions["Movement"];
        lightAttackAction = playerInput.actions["LightAttack"];
        heavyAttackAction = playerInput.actions["HeavyAttack"];
        fireAction = playerInput.actions["Fire"];
        interactAction = playerInput.actions["Interact"];
        scrollAction = playerInput.actions["Accelerate"];
        inventoryAction = playerInput.actions["Inventory"];
        reloadAction = playerInput.actions["Reload"];
        aimAction = playerInput.actions["Aim"];
        crouchAction = playerInput.actions["Crouch"];
        rollAction = playerInput.actions["Roll"];
        toggleThermal = playerInput.actions["ToggleThermal"];
        makeSound = playerInput.actions["MakeSound"];
        summonAction = playerInput.actions["Summon"];
        priorityAction = playerInput.actions["PriorityAxis"];
        rangeAction = playerInput.actions["RangeAxis"];
        trfCameraMain = Camera.main.transform;
        currentRotationSpeed = rotationSpeed;
        animator = GetComponent<Animator>();
    }

    private void OnEnable()
    {
        lightAttackAction.performed += LightAttack;
        heavyAttackAction.performed += HeavyAttack;

        priorityAction.performed += ChangePriority;
        rangeAction.performed += ChangeRange;
        aimAction.started += StartAiming;
        aimAction.canceled += StopAiming;
        interactAction.performed += _ => Interact();
        inventoryAction.performed += Inventory;
        reloadAction.performed += _ => Reload();
        crouchAction.performed += Crouch;
        toggleThermal.performed += _ => ToggleThermal();
        makeSound.performed += _ => MakeSound();
        
    }

    private void OnDisable()
    {
        lightAttackAction.performed -= LightAttack;
        heavyAttackAction.performed -= HeavyAttack;
        priorityAction.performed -= ChangePriority;
        rangeAction.performed -= ChangeRange;
        aimAction.started -= StartAiming;
        aimAction.canceled -= StopAiming;
        interactAction.performed -= _ => Interact();
        inventoryAction.performed -= Inventory;
        reloadAction.performed -= _ => Reload();
        
        crouchAction.performed -= Crouch;
        toggleThermal.performed -= _ => ToggleThermal();
        makeSound.performed -= _ => MakeSound();
        
    }

    void Update()
    {

        aimTarget.position = trfCameraMain.position + trfCameraMain.forward * aimDistance;

        Vector2 moveinput = moveAction.ReadValue<Vector2>();
        Vector3 move = new Vector3(moveinput.x, 0, moveinput.y);
        var scrollvalue = scrollAction.ReadValue<Vector2>();
        speedMultiplier += 4 * scrollvalue.y / 100;
        speedMultiplier = Mathf.Floor(Mathf.Clamp(speedMultiplier, 30, 100));
        animator.SetFloat("CROUCHSPEED",speedMultiplier / 100);
        var speedMultiplier2 = speedMultiplier / 100 * (isCrouching ? 0.7f : 1f);
        if (mode == Mode.HOSTAGE || isAiming) speedMultiplier2 = 0.45f;
        //Debug.Log(speedMultiplier2);
        var camForward = trfCameraMain.forward;
        camForward.y = 0;
        camForward.Normalize();

        move = camForward * move.z + trfCameraMain.right * move.x;
        move.y = 0;
        controller.Move(normalSpeed * Time.deltaTime * move * speedMultiplier2);

        if (controller.isGrounded && playerVelocity.y < 0)
        {
            playerVelocity.y = 0f;
        }

        playerVelocity.y += gravityValue * Time.deltaTime;
        controller.Move(playerVelocity * Time.deltaTime);

        if (moveinput != Vector2.zero & !isAiming)
        {
            animator.SetBool("IDLE", false);
            animator.SetFloat("MOVE", 1 * speedMultiplier2);
            var rotatedMoveInput = mode == Mode.HOSTAGE ? -moveinput : moveinput;
            float targetangle = Mathf.Atan2(rotatedMoveInput.x, rotatedMoveInput.y) * Mathf.Rad2Deg + trfCameraMain.eulerAngles.y;
            Quaternion rotation = Quaternion.Euler(0, targetangle, 0);
            transform.rotation = Quaternion.Lerp(transform.rotation, rotation, Time.deltaTime * currentRotationSpeed);
        }
        else
        {
            animator.SetBool("IDLE", true);
        }
        if (isAiming)
        {
            animator.SetBool("AIMING", true);
            animator.SetBool("IDLE", false);
            animator.SetFloat("MOVE", moveinput.y);
            animator.SetFloat("MOVE H", moveinput.x);
            var pos = new Vector3(aimTarget.position.x, transform.position.y, aimTarget.position.z);
            pos = (pos - transform.position).normalized;
            transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(pos, Vector3.up), Time.deltaTime * currentRotationSpeed);
        }
        else
        {
            animator.SetBool("AIMING", false);
        }
        //footsteps
        if (controller.isGrounded)
        {
            footstepController.Move(speedMultiplier2 * move.magnitude);
        }
        else
        {
            footstepController.Move(0);
        }
        if (interactable != null || (hostageController != null && mode != Mode.HOSTAGE))
        {
            labelManager.SetPrompt(UnityEngine.UIElements.DisplayStyle.Flex, "E");
        }
        else
        {
            labelManager.SetPrompt(UnityEngine.UIElements.DisplayStyle.None, "E");
        }
    }

    public void SetMode(Mode newMode)
    {
        switch (newMode)
        {
            case Mode.NORMAL:
                if (mode == Mode.HOSTAGE)
                {
                    crouchAction.performed -= Lethal;
                    crouchAction.performed += Crouch;
                    rollAction.performed -= NonLethal;
                }
                break;
            case Mode.CRAWL:
                
                break;
            case Mode.COVER:
                break;
            case Mode.HOSTAGE:
                if (mode == Mode.NORMAL)
                {
                    crouchAction.performed -= Crouch;
                    crouchAction.performed += Lethal;
                    rollAction.performed += NonLethal;
                    ghostController.preview = false;
                    EndCrouch();
                }
                break;
            default:
                break;
        }
        mode = newMode;
    }

    private void StartFiring(InputAction.CallbackContext context)
    {
        weaponController.Fire(true);
    }

    private void StopFiring(InputAction.CallbackContext context)
    {
        weaponController.Fire(false);
    }

    private void LightAttack(InputAction.CallbackContext context)
    {
        //Debug.Log("LIGHT");
        ghostController.ghost.SetActive(true);
        ghostController.Attack();
    }

    private void HeavyAttack(InputAction.CallbackContext context)
    {
        //Debug.Log("HEAVY");
        if (hostageController != null && mode == Mode.NORMAL)
        {
            ghostController.Takedown(hostageController);
            hostageController = null;
        }
        else
        {
            ghostController.ghost.SetActive(true);
            ghostController.Attack();
        }
    }


    private void Crouch(InputAction.CallbackContext context)
    {
        if (isCrouching)
        {
            EndCrouch();
        }
        else
        {
            StartCrouch();
        }
    }

    private void StartCrouch()
    {
        controller.height = 0.75f;
        isCrouching = true;
        animator.SetBool("CROUCHING", isCrouching);
    }

    private void EndCrouch()
    {
        controller.height = 2;
        isCrouching = false;
        animator.SetBool("CROUCHING", isCrouching);
    }

    private void MakeSound()
    {
        RaycastHit hit;
        Ray ray = new Ray(trfCameraMain.position, trfCameraMain.forward);
        if (Physics.Raycast(ray, out hit,Mathf.Infinity,ignorePlayer))
        {
            var sm = Instantiate(soundMaker,hit.point,Quaternion.identity).GetComponent<SoundMaker>();
            sm.range = range;
            sm.priority = priority;
        }
    }

    public void SpawnPunch()
    {
        punchBox.DamageBox();
    }

    public void StopPunch()
    {
        animator.SetBool("PUNCHING", false);
    }



    private void Reload()
    {
        if (weaponController.currentAmmo != weaponController.clipSize && weaponController.currentReserve != 0)
        {
            weaponController.Reload();
            animator.SetBool("RELOADING", true);
            animator.Play("Reloading", 2, animationSmoothTime);
        }
    }

    public void FinishReload()
    {
        weaponController.reloading = false;
        animator.SetBool("RELOADING", false);
    }

    private void Interact()
    {
        if (!isAiming)
        {
            if (ghostController.grabbing)
            {
                ghostController.Grab();

            }
            else if (mode == Mode.HOSTAGE)
            {
                hostageController.EndGrab();
                hostageController.GetComponent<EnemyBehaviour>().interrupt = new Interrupt(1, transform.position);
                SetMode(Mode.NORMAL);
            }
            else if (hostageController != null)
            {
                hostageController.SetGrab();
                SetMode(Mode.HOSTAGE);
            }
            else if (interactable != null)
            {
                if (interactable.TryGetComponent(out Grabbable grabbable))
                {
                    ghostController.Grab();

                }
                else
                {
                    interactable.Interact();
                }
            }
        }
        
       
    }

    private void Lethal(InputAction.CallbackContext context)
    {
        hostageController.EndGrab();
        hostageController.GetComponent<Health>().TakeDamage(200);
        Debug.Log("LETHAL");
        hostageController.GetComponent<Rigidbody>().AddForce((hostageController.transform.position - transform.position) * 2);
        SetMode(Mode.NORMAL);
    }

    private void NonLethal(InputAction.CallbackContext context)
    {
        hostageController.EndGrab();
        hostageController.TakeStaminaDamage(hostageController.stamina);
        SetMode(Mode.NORMAL);
    }

    public void Inventory(InputAction.CallbackContext context)
    {
        playerInventory.Menu();
        if (weaponMenuHidden)
        {
            lightAttackAction.Disable();
            heavyAttackAction.Disable();

            aimAction.started -= StartAiming;
            aimAction.canceled -= StopAiming;

            Cursor.lockState = CursorLockMode.Confined;
            Cursor.visible = true;
            cameraController.DisableInput();
            {//Stop Aiming
                isAiming = false;
                currentRotationSpeed = rotationSpeed;
                currentSpeed = normalSpeed;
                aimRig.weight = 0;
                fireAction.started -= StartFiring;
                cameraController.StopAim();
            }
        }
        else
        {
            lightAttackAction.Enable();
            heavyAttackAction.Enable();
            aimAction.started += StartAiming;
            aimAction.canceled += StopAiming;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            cameraController.EnableInput();
        }
        weaponMenuHidden = !weaponMenuHidden;
        
    }

    private void ToggleThermal()
    {
        thermalCamera.SetActive(!thermalCamera.activeInHierarchy);
    }

    public void StartAiming(InputAction.CallbackContext context)
    {
        isAiming = true;
        currentRotationSpeed = rotationSpeedAiming;
        currentSpeed = aimingSpeed;
        aimRig.weight = 100;
        lightAttackAction.performed -= LightAttack;
        heavyAttackAction.performed -= HeavyAttack;
        fireAction.started += StartFiring;
        fireAction.canceled += StopFiring;
        cameraController.StartAim();
    }

    public void StopAiming(InputAction.CallbackContext context)
    {
        isAiming = false;
        currentRotationSpeed = rotationSpeed;
        currentSpeed = normalSpeed;
        aimRig.weight = 0;
        lightAttackAction.performed += LightAttack;
        heavyAttackAction.performed += HeavyAttack;
        fireAction.started -= StartFiring;
        cameraController.StopAim();
    }

    public void ChangePriority(InputAction.CallbackContext context)
    {
        float input = priorityAction.ReadValue<float>();
        priority += (int)input;
        Debug.Log("Priority: " + priority);
    }

    public void ChangeRange(InputAction.CallbackContext context)
    {
        float input = rangeAction.ReadValue<float>();
        range += (int)input;
        Debug.Log("Range: " + range);
    }

    public void SetGrabTarget(Collider other)
    {
        if (mode == Mode.NORMAL)
        {
            var hc = other.GetComponent<EnemyStateController>();
            if (hc.state == EnemyStateController.EnemyState.Normal) 
            {
                hostageController = hc;
                ghostController.SetTarget(other.transform, hostageController.gAttach);
            } 
        }
    }

    public void RemoveGrabTarget(Collider other)
    {
        if (mode == Mode.NORMAL)
        {
            if (other.GetComponent<EnemyStateController>() == hostageController)
            {
                hostageController = null;
                ghostController.gAttach = ghostController.gAttachAttack;
                ghostController.preview = false;
            }
        }
    }
}
