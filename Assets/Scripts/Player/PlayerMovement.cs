using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour, IDamagable
{
    [Header("Input")]
    [SerializeField] private InputActionReference moveAction;
    [SerializeField] private InputActionReference lookAction;
    [SerializeField] private InputActionReference jumpAction;
    [SerializeField] private InputActionReference gachaAction;
    [SerializeField] private InputActionReference inventoryAction;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float rotationSpeed = 15f;

    [Header("Jump / Gravity")]
    [SerializeField] private float jumpHeight = 1.5f;
    [SerializeField] private float gravity = -20f;

    [Header("Camera")]
    [SerializeField] private Transform cameraRig;
    [SerializeField] private Transform cameraYaw;
    [SerializeField] private Transform cameraPitch;
    [SerializeField] private float mouseSensitivity = 0.1f;
    [SerializeField] private float minPitch = -30f;
    [SerializeField] private float maxPitch = 70f;

    [Header("Health Regeneration")]
    [SerializeField] private float regenDelay = 3f;
    [SerializeField] private float regenRate = 5f;

    [Header("References")]
    [SerializeField] private GachaManager gachaManager;
    [SerializeField] private PlayerInventory inventory;
    [SerializeField] private Transform characterModelParent;
    [SerializeField] private HealthBarUI healthBar;

    private GameObject currentCharacterModel;

    private GachaItem currentCharachter;

    private CharacterController controller;

    private bool canMove = true;
    private Vector3 velocity;
    private float pitch;
    private float yaw;

    private float timeSinceLastDamage;

    public int health { get; set; }
    public int maxHealth { get; set; }

    private void Awake()
    {
        controller = GetComponent<CharacterController>();

        // Start the camera at the player's position.
        cameraRig.position = transform.position;

        // Store the camera's current world yaw.
        yaw = cameraRig.eulerAngles.y;

        maxHealth = 100;
        health = maxHealth;

        healthBar.SetMaxHealth(maxHealth);
        healthBar.SetHealth(health);
    }

    private void OnEnable()
    {
        moveAction.action.Enable();
        lookAction.action.Enable();
        jumpAction.action.Enable();
        gachaAction.action.Enable();
        inventoryAction.action.Enable();
    }

    private void OnDisable()
    {
        moveAction.action.Disable();
        lookAction.action.Disable();
        jumpAction.action.Disable();
        gachaAction.action.Disable();
        inventoryAction.action.Disable();
    }

    private void LateUpdate()
    {
        cameraRig.position = transform.position;
    }

    private void Update()
    {
        HandleHealthRegeneration();

        if (gachaAction.action.WasPressedThisFrame())
        {
            gachaManager.DisplayGacha();

            switch (canMove)
            {
                case true:
                    canMove = false;
                    break;

                case false:
                    canMove = true;
                    break;
            }
        }

        if (inventoryAction.action.WasPressedThisFrame())
        {
            inventory.DisplayInventory();

            switch (canMove)
            {
                case true:
                    canMove = false;
                    break;

                case false:
                    canMove = true;
                    break;
            }
        }

        if (!canMove) return;

        HandleCamera();
        HandleMovement();
    }

    private void HandleHealthRegeneration()
    {
        // Don't regenerate if already at full health.
        if (health >= maxHealth)
        {
            timeSinceLastDamage = 0f;
            return;
        }

        timeSinceLastDamage += Time.deltaTime;

        // Wait until the regeneration delay has passed.
        if (timeSinceLastDamage < regenDelay)
            return;

        // Regenerate health.
        float newHealth = health + regenRate * Time.deltaTime;

        health = Mathf.Min(
            Mathf.FloorToInt(newHealth),
            maxHealth
        );

        healthBar.SetHealth(health);
    }

    private void HandleCamera()
    {
        Vector2 lookInput = lookAction.action.ReadValue<Vector2>();

        // Store camera yaw independently from the player.
        yaw += lookInput.x * mouseSensitivity;

        // Apply horizontal rotation to the camera rig.
        cameraRig.rotation = Quaternion.Euler(
            0f,
            yaw,
            0f
        );

        // Vertical camera rotation.
        pitch -= lookInput.y * mouseSensitivity;

        pitch = Mathf.Clamp(
            pitch,
            minPitch,
            maxPitch
        );

        cameraPitch.localRotation = Quaternion.Euler(
            pitch,
            0f,
            0f
        );
    }

    private void HandleMovement()
    {
        Vector2 input = moveAction.action.ReadValue<Vector2>();

        // Get movement directions from the camera.
        Vector3 forward = cameraYaw.forward;
        Vector3 right = cameraYaw.right;

        // Ignore vertical rotation.
        forward.y = 0f;
        right.y = 0f;

        forward.Normalize();
        right.Normalize();

        // Camera-relative movement.
        Vector3 move =
            forward * input.y +
            right * input.x;

        move = Vector3.ClampMagnitude(
            move,
            1f
        );

        // Rotate player toward movement direction.
        if (move.sqrMagnitude > 0.001f)
        {
            Quaternion targetRotation =
                Quaternion.LookRotation(move);

            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                rotationSpeed * Time.deltaTime
            );
        }

        // Ground check.
        if (controller.isGrounded && velocity.y < 0f)
        {
            velocity.y = -2f;
        }

        // Jump.
        if (jumpAction.action.WasPressedThisFrame() &&
            controller.isGrounded)
        {
            velocity.y = Mathf.Sqrt(
                jumpHeight * -2f * gravity
            );
        }

        // Gravity.
        velocity.y += gravity * Time.deltaTime;

        // Final movement.
        Vector3 movement =
            move * moveSpeed +
            Vector3.up * velocity.y;

        controller.Move(
            movement * Time.deltaTime
        );
    }

    public void ReplaceModel(GachaItem item)
    {
        if (item == null)
        {
            Debug.LogError("ReplaceModel received a null GachaItem.");
            return;
        }

        if (item.Model == null)
        {
            Debug.LogError(
                "GachaItem '" + item.Name + "' has no Model assigned."
            );

            return;
        }

        // Remove the currently equipped model.
        if (currentCharacterModel != null)
        {
            Destroy(currentCharacterModel);
        }

        // Create the new model.
        currentCharacterModel = Instantiate(
            item.Model,
            characterModelParent
        );

        // Reset its position and rotation relative to the parent.
        currentCharacterModel.transform.localPosition = Vector3.zero;
        currentCharacterModel.transform.localRotation = Quaternion.identity;
        currentCharacterModel.transform.localScale = Vector3.one;

        currentCharachter = item;
    }

    public void Hurt(int damage)
    {
        health -= damage;

        // Reset regeneration timer whenever damage is taken.
        timeSinceLastDamage = 0f;

        if (health <= 0)
        {
            health = 0;
            Die();
        }

        healthBar.SetHealth(health);
    }

    private void Die()
    {
        Debug.Log("Death");
    }
}