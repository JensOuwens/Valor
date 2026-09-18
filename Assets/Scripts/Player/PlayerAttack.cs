using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// TODO: check for rank to decide what attack pattern. handle combo attacks for that class. return to neutral after not attacking for a duration
/// </summary>
public class PlayerAttack : MonoBehaviour
{
    [SerializeField] private Animator greatswordAnimator;

    [SerializeField] private InputActionReference attackInput;
    private bool IsAttacking;
    private int CurrentAttack = 0;

    private void Update()
    {
        if (attackInput.action.WasPressedThisFrame())
        {
            if (!IsAttacking)
            {
                switch (CurrentAttack)
                {
                    case 0:
                        Attack1();
                        break;
                    case 1:
                        CancelInvoke();
                        Attack2();
                        break;
                    case 2:
                        CancelInvoke();
                        Attack3();
                        break;
                    case 3:
                        Back();
                        break;
                }
            }
        }
    }
    
    private void Attack1()
    {
        CurrentAttack = 1;
        IsAttacking = true;
        greatswordAnimator.SetTrigger("Attack1");
        Invoke("Back", 0.50f);
    }
    
    private void Attack2()
    {
        CurrentAttack = 2;
        greatswordAnimator.SetTrigger("Attack2");
        Invoke("Back", 0.50f);
    }
    
    private void Attack3()
    {
        CurrentAttack = 3;
        greatswordAnimator.SetTrigger("Attack3");
        Invoke("Back", 0.45f);
    }

    private void Back()
    {
        CurrentAttack = 0;
        IsAttacking = false;
        greatswordAnimator.SetTrigger("Back");
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
