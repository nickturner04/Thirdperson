using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.DualShock;
using Cinemachine;
using UnityEngine.Animations.Rigging;
using TMPro;
using UnityEngine.Events;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    public enum Mode { NORMAL,CRAWL,COVER,HOSTAGE}
    public enum UpperBodyMode { NORMAL,AIM,SPECIAL}

    public Transform trfPickup;
    public Transform aimTarget;
    public Rig aimRig;
    public LayerMask interactableLayer;
    public Animator animator;
    private WeaponController weaponController;
    [SerializeField] private SwitchVCam cameraController;
    [SerializeField] private GameObject soundMaker;
    [SerializeField] private LayerMask ignorePlayer;
    [SerializeField] private LabelManager labelManager;
    [SerializeField] private GameObject ghostPrefab;

    [SerializeField] private float normalSpeed = 10.0f;
    [SerializeField] private float gravityValue = -20f;
    [SerializeField] private float rotationSpeed = 10;
    [SerializeField] private float rotationSpeedAiming = 200;
    [SerializeField] private float animationSmoothTime = 0.1f;
    [SerializeField] private float aimDistance = 1;

    private FootstepCamoController footstepController;
    private CharacterController controller;
    private GameObject ghost;
    private GhostController ghostController;
    public GhostAnimator ghostAnimator;
    private Inventory playerInventory;
    private Vector3 playerVelocity;
    private PlayerInput playerInput;
    public bool isAiming = false;
    private float currentRotationSpeed;
    private Transform trfCameraMain;
    public bool isCrouching = false;
    private int noClip = 1;
    private bool weaponMenuHidden = true;

    //Ghost Vars
    private bool attacking = false;
    public bool guard = false;
    private int lastWeapon = 0;
    //Noisemaker
    private int priority = 1;
    private int range = 10;
    private const int crouchMask = 1 << 7;
    private float preSpeedMultiplier = 100;
    private bool usingGamepad = false;
    
    private InputAction moveAction;
    private InputAction lightAttackAction;
    private InputAction heavyAttackAction;
    private InputAction fireAction;
    private InputAction chargeAbilityAction;
    private InputAction interactAction;
    private InputAction scrollAction;
    private InputAction inventoryAction;
    private InputAction reloadAction;
    private InputAction aimAction;
    private InputAction crouchAction;
    private InputAction rollAction;
    private InputAction makeSound;
    private InputAction blockAction;
    private InputAction summonAction;
    private InputAction quickSwapAction1;
    private InputAction quickSwapAction2;
    private InputAction quickSwapAction3;
    private InputAction priorityAction;
    private InputAction rangeAction;
    private InputAction anyController;
    private InputAction anyKey;
    

    public Interactable interactable;
    public EnemyStateController hostageController;
    public DroppedWeapon droppedWeapon;

    public Mode mode = Mode.NORMAL;
    public UpperBodyMode modeUpper = UpperBodyMode.NORMAL;

    private void Awake()
    {
        Debug.Log("");
        Cursor.lockState = CursorLockMode.Locked;
        controller = GetComponent<CharacterController>();
        weaponController = GetComponent<WeaponController>();
        playerInput = GetComponent<PlayerInput>();
        ghostController = GetComponent<GhostController>();
        ghostController.playerController = this;
        footstepController = GetComponent<FootstepCamoController>();
        playerInventory = GetComponent<Inventory>();
        moveAction = playerInput.actions["Movement"];
        lightAttackAction = playerInput.actions["LightAttack"];
        heavyAttackAction = playerInput.actions["HeavyAttack"];
        chargeAbilityAction = playerInput.actions["ChargeAbility"];
        fireAction = playerInput.actions["Fire"];
        interactAction = playerInput.actions["Interact"];
        scrollAction = playerInput.actions["Accelerate"];
        inventoryAction = playerInput.actions["Inventory"];
        reloadAction = playerInput.actions["Reload"];
        aimAction = playerInput.actions["Aim"];
        crouchAction = playerInput.actions["Crouch"];
        rollAction = playerInput.actions["Roll"];
        makeSound = playerInput.actions["MakeSound"];
        blockAction = playerInput.actions["Block"];
        summonAction = playerInput.actions["Summon"];
        priorityAction = playerInput.actions["PriorityAxis"];
        rangeAction = playerInput.actions["RangeAxis"];
        anyController = playerInput.actions["AnyController"];
        anyKey = playerInput.actions["AnyKey"];
        quickSwapAction1 = playerInput.actions["QuickSwap1"];
        quickSwapAction2 = playerInput.actions["QuickSwap2"];
        quickSwapAction3 = playerInput.actions["QuickSwap3"];
        trfCameraMain = Camera.main.transform;
        currentRotationSpeed = rotationSpeed;

        //Create Ghost
        //ghost = Instantiate(ghostPrefab, transform.parent);
    }

    //Called when GameObject is enabled, subscribes all the actions to events
    private void OnEnable()
    {
        lightAttackAction.performed += LightAttack;
        heavyAttackAction.performed += HeavyAttack;
        
        fireAction.started += StartFiring;
        fireAction.canceled += StopFiring;
        fireAction.Disable();
        priorityAction.performed += ChangePriority;
        rangeAction.performed += ChangeRange;
        aimAction.started += StartAiming;
        aimAction.canceled += StopAiming;
        interactAction.performed += Interact;
        inventoryAction.performed += Inventory;
        reloadAction.performed += Reload;
        crouchAction.performed += Crouch;
        makeSound.performed += MakeSound;
        anyController.started += LeftStickMove;
        blockAction.started += StartBlock;
        blockAction.canceled += EndBlock;
        summonAction.performed += Summon;
        quickSwapAction1.performed += QuickSwitch1;
        quickSwapAction2.performed += QuickSwitch2;
        quickSwapAction3.performed += QuickSwitch3;

    }

    //Called when GameObject is disabled, unsubscribes all actions from events
    private void OnDisable()
    {
        lightAttackAction.performed -= LightAttack;
        heavyAttackAction.performed -= HeavyAttack;
        chargeAbilityAction.started -= ghostAnimator.AbilityL1Start;
        chargeAbilityAction.canceled -= ghostAnimator.AbilityL1Stop;
        fireAction.started -= StartFiring;
        fireAction.canceled -= StopFiring;
        priorityAction.performed -= ChangePriority;
        rangeAction.performed -= ChangeRange;
        aimAction.started -= StartAiming;
        aimAction.canceled -= StopAiming;
        interactAction.performed -=  Interact;
        inventoryAction.performed -= Inventory;
        reloadAction.performed -= Reload;
        anyController.performed -= LeftStickMove;
        crouchAction.performed -= Crouch;
        makeSound.performed -= MakeSound;
        blockAction.started -= StartBlock;
        blockAction.canceled -= EndBlock;
        summonAction.performed -= Summon;
        quickSwapAction1.performed -= QuickSwitch1;
        quickSwapAction2.performed -= QuickSwitch2;
        quickSwapAction3.performed -= QuickSwitch3;
    }

    private void LeftStickMove(InputAction.CallbackContext context)
    {
        
        usingGamepad = true;
        
        anyController.performed -= LeftStickMove;
        anyKey.started += AnyKey;
    }

    private void AnyKey(InputAction.CallbackContext context)
    {
        
        usingGamepad = false;
        anyController.performed += LeftStickMove;
        anyKey.started -= AnyKey;

    }

    void Update()
    {//Update is called every frame by Unity
        aimTarget.position = trfCameraMain.position + trfCameraMain.forward * aimDistance;

        Vector2 moveinput = moveAction.ReadValue<Vector2>();
        Vector3 move = new Vector3(moveinput.x, 0, moveinput.y).normalized;
        //Get input from scroll wheel and use it to change speed multiplier
        var scrollvalue = scrollAction.ReadValue<Vector2>();
        preSpeedMultiplier += 4 * scrollvalue.y / 100;
        var speedMultiplier = usingGamepad ? (moveinput.magnitude > 1 ? 1 : moveinput.magnitude) * 100 : Mathf.Floor(Mathf.Clamp(preSpeedMultiplier, 30, 100));
        animator.SetFloat("CROUCHSPEED",speedMultiplier / 100);
        //Change Speed Multiplier Depending On If Crouching
        var speedMultiplier2 = speedMultiplier / 100 * (isCrouching ? 0.75f : 1f);
        if (mode == Mode.HOSTAGE || isAiming) speedMultiplier2 = 0.45f;
        //Debug.Log(speedMultiplier2);
        var camForward = trfCameraMain.forward;
        camForward.y = 0;
        camForward.Normalize();
        //Change movement direction based on where the camera is facing
        move = camForward * move.z + trfCameraMain.right * move.x;
        move.y = 0;
        //Move Player in x and z axis
        controller.Move(normalSpeed * speedMultiplier2 * Time.deltaTime * move);
        //Using Time.deltaTime prevents higher framerates from causing the player to move faster

        //Make player Effected By Gravity
        playerVelocity.y += gravityValue * Time.deltaTime;
        if (controller.isGrounded && playerVelocity.y < 0)
        {
            playerVelocity.y = 0f;
        }
        //move player in y axis
        controller.Move(noClip * Time.deltaTime * playerVelocity);

        if (moveinput != Vector2.zero & !isAiming)
        {
            //If moving and not aiming, rotate player in direction of movement
            animator.SetBool("IDLE", false);
            animator.SetFloat("MOVE", 1 * speedMultiplier2);
            var rotatedMoveInput = mode == Mode.HOSTAGE ? -moveinput : moveinput;
            float targetangle = Mathf.Atan2(rotatedMoveInput.x, rotatedMoveInput.y) * Mathf.Rad2Deg + trfCameraMain.eulerAngles.y;
            Quaternion rotation = Quaternion.Euler(0, targetangle, 0);
            transform.rotation = Quaternion.Lerp(transform.rotation, rotation, Time.deltaTime * currentRotationSpeed);
        }
        else
        {
            //Otherwise play idle animation
            animator.SetBool("IDLE", true);
        }
        if (isAiming)
        {//If aiming, rotate player in direction of camera at all times
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
        {//Play footsteps at a different volume depending on how fast the player is moving, but only if on the ground
            footstepController.Move(speedMultiplier2 * move.magnitude);
        }
        //If an interactable object is detected in range or an enemy can be grabbed, make the button prompt appear
        if (interactable != null )
        {
            labelManager.SetPrompt(UnityEngine.UIElements.DisplayStyle.Flex, "[E]: " + interactable.description);
        }
        else if (hostageController != null && mode != Mode.HOSTAGE)
        {
            labelManager.SetPrompt(UnityEngine.UIElements.DisplayStyle.Flex, "[E]: Grab");
        }
        else
        {
            labelManager.SetPrompt(UnityEngine.UIElements.DisplayStyle.None, "E");
        }
    }

    public void SetMode(Mode newMode)
    {//Change actions player can perform depending on their mode, crawl and cover are not implemented, crouching is not a separate mode from standing.
        switch (newMode)
        {
            case Mode.NORMAL:
                if (mode == Mode.HOSTAGE)
                {//Removes instant kill actions and allows player to crouch
                    crouchAction.performed -= Lethal;
                    crouchAction.performed += Crouch;
                    interactAction.performed -= NonLethal;
                    interactAction.performed += Interact;
                }
                break;
            case Mode.CRAWL:
                
                break;
            case Mode.COVER:
                break;
            case Mode.HOSTAGE:
                if (mode == Mode.NORMAL)
                {//Do not allow player to crouch in hostage mode, give options to instantly kill a captured enemy
                    crouchAction.performed -= Crouch;
                    crouchAction.performed += Lethal;
                    interactAction.performed += NonLethal;
                    interactAction.performed -= Interact;
                    ghostController.preview = false;
                    EndCrouch();
                }
                break;
            default:
                break;
        }
        mode = newMode;
    }

    public void SetUpperMode(UpperBodyMode newMode)
    {
        switch (modeUpper)
        {
            case UpperBodyMode.NORMAL:
                if (newMode == UpperBodyMode.AIM)
                {
                    isAiming = true;
                    currentRotationSpeed = rotationSpeedAiming;
                    aimRig.weight = 100;
                    lightAttackAction.Disable() ;
                    heavyAttackAction.Disable() ;
                    fireAction.Enable();
                    cameraController.StartAim();
                    summonAction.Disable();
                }
                else if (newMode == UpperBodyMode.SPECIAL)
                {
                    aimAction.started -= StartAiming;
                    aimAction.canceled -= StopAiming;
                    aimAction.started += ghostAnimator.AbilityL2;
                    chargeAbilityAction.started += ghostAnimator.AbilityL1Start;
                    chargeAbilityAction.canceled += ghostAnimator.AbilityL1Stop;
                }
                break;
            case UpperBodyMode.AIM:
                
                if (newMode == UpperBodyMode.NORMAL)
                {
                    isAiming = false;
                    currentRotationSpeed = rotationSpeed;
                    aimRig.weight = 0;
                    lightAttackAction.Enable();
                    heavyAttackAction.Disable();
                    fireAction.Disable();
                    cameraController.StopAim();
                    summonAction.Enable();
                }

                break;
            case UpperBodyMode.SPECIAL:
                if (newMode == UpperBodyMode.NORMAL)
                {
                    aimAction.Enable();
                    chargeAbilityAction.started -= ghostAnimator.AbilityL1Start;
                    chargeAbilityAction.canceled -= ghostAnimator.AbilityL1Stop;
                    aimAction.started += StartAiming;
                    aimAction.canceled += StopAiming;
                    aimAction.started -= ghostAnimator.AbilityL2;
                }
                
                break;
            default:
                break;
        }
        modeUpper = newMode;
    }

    private void Summon(InputAction.CallbackContext _)
    {
        if (modeUpper == UpperBodyMode.SPECIAL)
        {//Added check to prevent unsummoning when the ghost is occupied as doing this while charging an ability will cause it to get stuck
            //Removed check and changed dissapear to a virtual void which can be inherited to set charging to false when un summoned
            ghostAnimator.Disappear();
            SetUpperMode(UpperBodyMode.NORMAL);
            playerInventory.Equip(lastWeapon);
        }
        else if (modeUpper == UpperBodyMode.NORMAL)
        {
            ghostAnimator.Appear();
            SetUpperMode(UpperBodyMode.SPECIAL);
            lastWeapon = playerInventory.currentWeapon;
            playerInventory.Equip(-1);
        }
        
    }

    private void StartBlock(InputAction.CallbackContext context)
    {
        
        if (!ghostAnimator.occupied)
        {
            guard = true;
            
        }
        
    }

    public void EndBlock(InputAction.CallbackContext _)
    {
        guard = false;
    }

    private void NoClip(InputAction.CallbackContext _)
    {
        if (noClip == 1)
        {
            noClip = 0;
            gameObject.layer = 31;
        }
        else
        {
            noClip = 1;
            gameObject.layer = 3;
        }
    }

    private void StartFiring(InputAction.CallbackContext context)
    {//Begin Firing Weapon When Mouse1 is pressed
        weaponController.Fire(true);
    }

    private void StopFiring(InputAction.CallbackContext context)
    {//Stop Firing weapon When button released
        weaponController.Fire(false);
    }

    private void LightAttack(InputAction.CallbackContext context)
    {//Play a punching animation when Mouse1 is tapped
        //Debug.Log("LIGHT");
        ghostController.ghost.SetActive(true);
        ghostAnimator.Attack();
    }

    private void HeavyAttack(InputAction.CallbackContext context)
    {//Perform a stronger attack when Mouse1 is held down
        //Debug.Log("HEAVY");
        
        ghostController.ghost.SetActive(true);
        ghostController.Attack();
        
    }

    private void Roll(InputAction.CallbackContext _)
    {
        animator.SetTrigger("ROLL");
        animator.applyRootMotion = true;
    }

    public void EndRoll()
    {
        animator.applyRootMotion = false;
    }


    private void Crouch(InputAction.CallbackContext context)
    {
        if (isCrouching)
        {
            if (!Physics.Raycast(transform.position,Vector3.up,2,crouchMask))
            {
                EndCrouch();
            }
            
        }
        else
        {
            StartCrouch();
        }
    }

    public void StartCrouch()
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

    private void MakeSound(InputAction.CallbackContext context)
    {//Debug method for testing if enemies respond to noises, called when Q is pressed
        Ray ray = new(trfCameraMain.position, trfCameraMain.forward);
        if (Physics.Raycast(ray, out RaycastHit hit,Mathf.Infinity,ignorePlayer))
        {
            var sm = Instantiate(soundMaker,hit.point,Quaternion.identity).GetComponent<SoundMaker>();
            sm.range = range;
            sm.priority = priority;
        }
        GetComponent<PlayerHealth>().Die();
    }

    private void Reload(InputAction.CallbackContext context)
    {
        if (context.duration < 0.3)
        {
            if (weaponController.currentAmmo != weaponController.clipSize && weaponController.currentReserve != 0 && playerInventory.currentWeapon != -1)
            {
                weaponController.Reload();
                animator.SetBool("RELOADING", true);
                animator.Play("Reloading", 2, animationSmoothTime);
            }
        }
        else if (droppedWeapon != null)
        {
            playerInventory.ChangeWeapon(droppedWeapon.weaponData, droppedWeapon.ammo);
            Destroy(droppedWeapon.gameObject);
            RemoveDroppedWeapon();
        }
    }
    //This method is called by the reload animation when it finishes
    public void FinishReload()
    {
        weaponController.reloading = false;
        animator.SetBool("RELOADING", false);
    }

    private void Interact(InputAction.CallbackContext context)
    {
        Debug.Log("INTERACT");
        if (!isAiming)
        {
            if (mode == Mode.HOSTAGE)
            {
                hostageController.EndGrab();
                hostageController.GetComponent<EnemyBehaviour>().interrupt = new Interrupt(1, transform.position);
                hostageController = null;
                ghostController.gAttach = ghostController.gAttachAttack;
                ghostController.preview = false;
                SetMode(Mode.NORMAL);
            }
            else if (hostageController != null)
            {
                hostageController.SetGrab();
                SetMode(Mode.HOSTAGE);
            }
            else if (interactable != null)
            {
                interactable.Interact();
            }
        }
        
       
    }

    private void Lethal(InputAction.CallbackContext context)
    {//Kill Hostage Enemy
        hostageController.EndGrab();
        hostageController.GetComponent<Health>().TakeDamage(200);
        Debug.Log("LETHAL");
        hostageController.GetComponent<Rigidbody>().AddForce((hostageController.transform.position - transform.position) * 2);
        SetMode(Mode.NORMAL);
    }

    private void NonLethal(InputAction.CallbackContext context)
    {//Knock out Hostage Enemy
        hostageController.EndGrab();
        hostageController.TakeStaminaDamage(hostageController.stamina);
        SetMode(Mode.NORMAL);
    }

    public void Inventory(InputAction.CallbackContext context)
    {
        playerInventory.Menu();
        if (weaponMenuHidden)
        {
            //Prevent Player From Attacking Or Aiming When Inventory Is Open So That Clicking Does Not Attack
            lightAttackAction.Disable();
            heavyAttackAction.Disable();

            aimAction.started -= StartAiming;
            aimAction.canceled -= StopAiming;

            Cursor.lockState = CursorLockMode.Confined;
            Cursor.visible = true;
            //Prevent Mouse Movement From Moving Camera On Inventory
            cameraController.DisableInput();
            {//Stop Aiming
                isAiming = false;
                currentRotationSpeed = rotationSpeed;
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

    private void QuickSwitch1(InputAction.CallbackContext context)
    {
        QuickSwitch(0);
    }
    private void QuickSwitch2(InputAction.CallbackContext context)
    {
        QuickSwitch(1);
    }
    private void QuickSwitch3(InputAction.CallbackContext context)
    {
        QuickSwitch(2);
    }

    private void QuickSwitch(int i)
    {
        if (animator.GetBool("RELOADING")) return;
        if (isAiming)
        {
            StopAiming(new());
        }
        if (i == playerInventory.currentWeapon)
        {
            playerInventory.Equip(-1);
        }
        else
        {
            playerInventory.Equip(i);
        }
    }

    public void StartAiming(InputAction.CallbackContext _)
    {//Changes the turn speed so that the player instantly faces the camera, sets the aiming animation rig so that the player aims the gun.
        if (playerInventory.currentWeapon != -1)
        {
            SetUpperMode(UpperBodyMode.AIM);
        }
        
    }

    public void StopAiming(InputAction.CallbackContext _)
    {
        SetUpperMode(UpperBodyMode.NORMAL);
    }
    //Debug Method For Noisemaker
    public void ChangePriority(InputAction.CallbackContext context)
    {
        float input = priorityAction.ReadValue<float>();
        priority += (int)input;
        Debug.Log("Priority: " + priority);
    }
    //Debug Method For Noisemaker
    public void ChangeRange(InputAction.CallbackContext context)
    {
        float input = rangeAction.ReadValue<float>();
        range += (int)input;
        Debug.Log("Range: " + range);
    }
    //Called when an enemy enters the grab range trigger
    public void SetGrabTarget(Collider other)
    {
        if (mode == Mode.NORMAL && !attacking)
        {
            var hc = other.GetComponent<EnemyStateController>();
            if (hc.state == EnemyStateController.EnemyState.Normal) 
            {
                hostageController = hc;
                ghostController.SetTarget(other.transform, hostageController.gAttach);
            } 
        }
    }
    //Called when an enemy leaves the grab range trigger
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

    public void SetAttacking(bool attacking)
    {
        if (attacking)
        {
            if (mode != Mode.HOSTAGE)
            {
                hostageController = null;
                ghostController.gAttach = ghostController.gAttachAttack;
                ghostController.preview = false;
                this.attacking = true;
            }
        }
        else
        {
            this.attacking = false;
        }
    }

    public void SetDroppedWeapon(DroppedWeapon weapon)
    {
        droppedWeapon = weapon;
        labelManager.SetPickup(weapon);
    }
    public void RemoveDroppedWeapon()
    {
        droppedWeapon = null;
        labelManager.HidePickup();
    }

    public void Die()
    {
        trfCameraMain.GetComponent<AudioListener>().enabled = false;
        animator.SetTrigger("DEATH");
        EndCrouch();
        StopAiming(new InputAction.CallbackContext());
        moveAction.Disable();
        noClip = 0;
        gameObject.layer = 31;
        lightAttackAction.performed -= LightAttack;
        heavyAttackAction.performed -= HeavyAttack;
        priorityAction.performed -= ChangePriority;
        rangeAction.performed -= ChangeRange;
        aimAction.started -= StartAiming;
        aimAction.canceled -= StopAiming;
        interactAction.performed -= Interact;
        inventoryAction.performed -= Inventory;
        reloadAction.performed -= Reload;
        anyController.performed -= LeftStickMove;
        crouchAction.performed -= Crouch;
        makeSound.performed -= MakeSound;
        blockAction.started -= StartBlock;
        blockAction.canceled -= EndBlock;
    }
}
