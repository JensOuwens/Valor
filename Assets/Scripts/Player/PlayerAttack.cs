using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAttack : MonoBehaviour
{
    [SerializeField] private Animator attackAnimator;
    [SerializeField] private InputActionReference attackInput;

    [Header("Animations")]
    [SerializeField] private string attack1Animation;
    [SerializeField] private string attack2Animation;
    [SerializeField] private string attack3Animation;
    [SerializeField] private string neutralAnimation;

    [Header("Combo")]
    [SerializeField, Range(0f, 1f)]
    private float comboWindow = 0.7f;

    [SerializeField]
    private float inputBufferTime = 0.25f;

    private int currentAttack = 0;

    private bool isAttacking = false;
    private bool attackBuffered = false;

    private float bufferTimer = 0f;

    private void Update()
    {
        HandleInput();
        UpdateAttack();
    }

    private void HandleInput()
    {
        if (!attackInput.action.WasPressedThisFrame())
            return;

        if (!isAttacking)
        {
            StartAttack(1);
            return;
        }

        attackBuffered = true;
        bufferTimer = inputBufferTime;
    }

    private void UpdateAttack()
    {
        if (!isAttacking)
            return;

        if (attackBuffered)
        {
            bufferTimer -= Time.deltaTime;

            if (bufferTimer <= 0f)
            {
                attackBuffered = false;
            }
        }

        AnimatorStateInfo state =
            attackAnimator.GetCurrentAnimatorStateInfo(0);

        if (attackAnimator.IsInTransition(0))
            return;

        if (state.normalizedTime >= comboWindow)
        {
            if (attackBuffered && currentAttack < 3)
            {
                StartAttack(currentAttack + 1);
                return;
            }
        }

        if (state.normalizedTime >= 1f)
        {
            EndCombo();
        }
    }

    private void StartAttack(int attackNumber)
    {
        currentAttack = attackNumber;

        isAttacking = true;
        attackBuffered = false;
        bufferTimer = 0f;

        string animationName = attackNumber switch
        {
            1 => attack1Animation,
            2 => attack2Animation,
            3 => attack3Animation,
            _ => neutralAnimation
        };

        attackAnimator.CrossFade(
            animationName,
            0.05f,
            0
        );
    }

    private void EndCombo()
    {
        currentAttack = 0;

        isAttacking = false;
        attackBuffered = false;
        bufferTimer = 0f;

        attackAnimator.CrossFade(
            neutralAnimation,
            0.1f,
            0
        );
    }

    private void OnEnable()
    {
        attackInput.action.Enable();
    }

    private void OnDisable()
    {
        attackInput.action.Disable();
    }
}