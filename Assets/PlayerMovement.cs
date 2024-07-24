using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    public Rigidbody2D rb;
    private bool isFacingRight = true;
    
    private float originalGravity;

    [Header("Movement")]
    public float normalMoveSpeed = 5f;
    private float moveSpeed;
    private float horizontalMovement;

    public float dashPower = 26f;
    private bool isDashing = false;
    public int maxDashes = 1;
    private int dashesRemaining = 1;
    private float dashTime = 0.5f;

    [Header("Jumping")]
    public float jumpPower = 5f;
    public int maxJumps = 2;
    private int jumpsRemaining;

    private bool isCharging = false;
    private float jumpCharge;
    private float chargeRate = 1f;
    private float maxJumpCharge = 2f;

    [Header("GroundCheck")]
    public Transform groundCheckPos;
    public Vector2 groundCheckSize = new Vector2(0.5f, 0.05f);
    public LayerMask groundLayer;
    private bool isGrounded;

    [Header("WallCheck")]
    public Transform wallCheckPos;
    public Vector2 wallCheckSize = new Vector2(0.5f, 0.05f);
    public LayerMask wallLayer;

    [Header("MoveCheck")]
    private bool isMoving;

    [Header("Gravity")]
    public float baseGravity = 2f;
    public float maxFallSpeed = 18f;
    public float fallSpeedMultiplier = 2f;

    [Header("WallMovement")]
    public float wallSlideSpeed = 2f;
    private bool isWallSliding;

    private bool isWallJumping;
    private float wallJumpDirection;
    private float wallJumpTime = 0.5f;
    private float wallJumpTimer;
    public Vector2 wallJumpPower = new Vector2(5f, 10f);

    [Header("Animation")]
    public Animator anim;
    private string[] triggers = new string[] {"Idle", "Moving", "Charging"};
    private string currentTrigger = "Idle";

    void Update()
    {
        GroundCheck();
        MovingCheck();

        ProcessGravity();
        ProcessWallSlide();
        ProcessWallJump();

        if(!isGrounded)
        {
            jumpCharge = 0f;
            isCharging = false;
        }

        if(isCharging && isGrounded)
        {
            if(!AnimationHasLooped())
            {
                if(moveSpeed != normalMoveSpeed / 4f)
                {
                    moveSpeed = normalMoveSpeed / 4f;
                }
                
                jumpCharge += Time.deltaTime * chargeRate;
            }
            else
            {
                anim.SetBool("FullCharge", true);
            }
        }
        else if(!isCharging)
        {
            if(moveSpeed != normalMoveSpeed)
            {
                moveSpeed = normalMoveSpeed;
            }
        }
        if(isCharging && isGrounded)
        {
            SwitchAnim("Charging");
        }
        else if(isMoving == true)
        {
            SwitchAnim("Moving");
        }
        else
        {
            SwitchAnim("Idle");
        }

        if(!isWallJumping || !isDashing)
        {
            rb.velocity = new Vector2(horizontalMovement * moveSpeed, rb.velocity.y);
            Flip();
        }
    }

    private void ProcessGravity()
    {
        // falling
        if(rb.velocity.y < 0)
        {
            rb.gravityScale = baseGravity * fallSpeedMultiplier;
            rb.velocity = new Vector2(rb.velocity.x, Mathf.Max(rb.velocity.y, -maxFallSpeed));
        }
        else
        {
            rb.gravityScale = baseGravity;
        }

        if(!isDashing)
        {
            originalGravity = rb.gravityScale;
        }
    }

    private void ProcessWallSlide()
    {
        if(!isGrounded && WallCheck() && horizontalMovement != 0)
        {
            isWallSliding = true;
            rb.velocity = new Vector2(rb.velocity.x, Mathf.Max(rb.velocity.y, -wallSlideSpeed));
        }
        else
        {
            isWallSliding = false;
        }
    }

    private void ProcessWallJump()
    {
        if(isWallSliding)
        {
            isWallJumping = false;
            wallJumpDirection = -transform.localScale.x;
            wallJumpTimer = wallJumpTime;

            CancelInvoke(nameof(CancelWallJump));
        }
        else if(wallJumpTimer > 0f)
        {
            wallJumpTimer -= Time.deltaTime;
        }
    }

    private void CancelWallJump()
    {
        isWallJumping = false;
    }

    public void Move(InputAction.CallbackContext context)
    {
        horizontalMovement = context.ReadValue<Vector2>().x;
    }

    public void Jump(InputAction.CallbackContext context)
    {
        double startTime = context.startTime;
        if(jumpsRemaining > 0)
        {
            // on jump down
            if(context.started)
            {
                // if is grounded
                if(isGrounded)
                {
                    jumpCharge = 0f;
                    isCharging = true;
                }
                else
                {
                    rb.velocity = new Vector2(rb.velocity.x, jumpPower);
                    jumpsRemaining--;
                }
            }
            // on jump up
            else if(context.canceled)
            {
                if(isGrounded && isCharging)
                {
                    rb.velocity = new Vector2(rb.velocity.x, jumpPower * (1 + jumpCharge));
                    jumpCharge = 0f;
                    jumpsRemaining--;
                }
                else if(!isGrounded)
                {
                    jumpsRemaining--;
                }
                isCharging = false;
                anim.SetBool("FullCharge", false);
            }
        }

        if(context.performed && wallJumpTimer > 0f)
        {
            isWallJumping = true;
            rb.velocity = new Vector2(wallJumpDirection * wallJumpPower.x, wallJumpPower.y);
            wallJumpTimer = 0;

            if(transform.localScale.x != wallJumpDirection)
            {
                isFacingRight = !isFacingRight;
                Vector3 ls = transform.localScale;
                ls.x *= -1f;
                transform.localScale = ls;
            }

            Invoke(nameof(CancelWallJump), wallJumpTime + 0.1f); // wall jump = 0.5s -- jump again = 0.6s
        }
    }

    public void Dash(InputAction.CallbackContext context)
    {
        if(dashesRemaining > 0)
        {
            if(context.started)
            {
                isDashing = true;
                Debug.Log(isDashing);
                originalGravity = rb.gravityScale;
                rb.gravityScale = 0f;
                Vector2 force = new Vector2(dashPower * 200, 0);
                if(!isFacingRight)
                {
                    force.x *= -1;
                }
                // why instant?
                rb.AddForce(force);
                rb.velocity = new Vector2(rb.velocity.x, 0f);

                dashesRemaining--;
                Invoke(nameof(CancelDash), dashTime + 0.1f);

            }
        }
    }

    private void CancelDash()
    {
        isDashing = false;
        Debug.Log(isDashing);
        rb.gravityScale = originalGravity;
        dashesRemaining += 1;
        Debug.Log($"added dash to: {dashesRemaining}");
    }

    private void GroundCheck()
    {
        if(Physics2D.OverlapBox(groundCheckPos.position, groundCheckSize, 0, groundLayer))
        {
            //Debug.Log("Refill jumps");
            jumpsRemaining = maxJumps;
            isGrounded = true;
        }
        else
        {
            isGrounded = false;
        }
    }

    private void MovingCheck()
    {
        if(horizontalMovement != 0)
        {
            isMoving = true;
        }
        else
        {
            isMoving = false;
        }
    }

    private bool WallCheck()
    {
        return Physics2D.OverlapBox(wallCheckPos.position, wallCheckSize, 0, wallLayer);
    }

    private void Flip()
    {
        if(isFacingRight && horizontalMovement < 0 || !isFacingRight && horizontalMovement > 0)
        {
            isFacingRight = !isFacingRight;
            Vector3 ls = transform.localScale;
            ls.x *= -1f;
            transform.localScale = ls;
        }
    }

    private void SwitchAnim(string state)
    {
        if(currentTrigger != state)
        {
            foreach(string trigger in triggers)
            {
                anim.ResetTrigger(trigger);
            }

            anim.SetTrigger(state);
            currentTrigger = state;
        }
    }

    private bool AnimationHasLooped()
    {
        return anim.GetCurrentAnimatorStateInfo(0).normalizedTime > 1 && !anim.IsInTransition(0);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.white;
        Gizmos.DrawWireCube(groundCheckPos.position, groundCheckSize);
        Gizmos.color = Color.blue;
        Gizmos.DrawWireCube(wallCheckPos.position, wallCheckSize);
    }
}
