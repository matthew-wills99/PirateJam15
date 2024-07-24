using System;
using UnityEngine;

public class JumpingState : BaseState<EPlayerState>
{
    private PlayerStateMachine _psm;

    private float jumpTime = 0.2f;  
    private float jumpTimeCounter;
    private float minJumpForce = 5f;
    private float maxJumpForce = 8f;
    private float jumpForce;
    private float jumpForceIncrement = 1f;
    private bool hasJumped = false;

    private float chargerDefault = 1f;
    private float charger;
    private float maxChargeTime = 1f;

    private float move;

    private Rigidbody2D rb;

    private bool hasCompletedChargeLoop;

    public JumpingState(PlayerStateMachine playerStateMachine) : base(EPlayerState.Jumping)
    {
        _psm = playerStateMachine;
    }

    public override void EnterState()
    {
        hasCompletedChargeLoop = false;

        _psm.ResetAnimation();

        Debug.Log("Entering Jumping State");
        charger = chargerDefault;

        jumpTimeCounter = jumpTime;
        jumpForce = minJumpForce;
        hasJumped = false;
        rb = _psm.GetComponent<Rigidbody2D>();
        
        _psm.SetBool(_psm.anim, "IsCharging", true);
        // playerChargeAnim
        // playerFullChargeAnim
    }

    public override void ExitState()
    {
        if(_psm.IsFalling())
        {
            _psm.SetBool(_psm.anim, "IsFalling", true);
        }
        Debug.Log("Exiting Jumping State");
    }

    public override void UpdateState()
    {


        move = Input.GetAxis("Horizontal");
        rb.velocity = new Vector2(move * _psm.moveSpeed, rb.velocity.y);
        //_psm.transform.Translate(Vector3.right * move * Time.deltaTime * _psm.moveSpeed);

        // small jump
        /*if(_psm.IsGrounded() && Input.GetButton("Jump"))
        {
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        }

        // hold jump
        if(Input.GetButton("Jump") && !hasJumped)
        {
            // time left to hold jump
            if(jumpTimeCounter > 0)
            {
                rb.velocity = Vector2.up * jumpForce;
                jumpTimeCounter -= Time.deltaTime;
            }
            // ran out of time
            else
            {
                hasJumped = true;
            }
        }*/

        // holding jump
        if(Input.GetButton("Jump") && !hasJumped)
        {
            if(!hasCompletedChargeLoop && _psm.AnimationHasLooped())
            {
                _psm.SetBool(_psm.anim, "IsFullCharge", true);
                hasCompletedChargeLoop = true;
            }

            if(charger < maxChargeTime + chargerDefault)
            {
                //Debug.Log($"Charger: {charger}");
                charger += Time.deltaTime;
                //jumpForce += jumpForceIncrement;
            }
            else
            {
                //Debug.Log("Max force achieved let go.");
            }
        }

        // release jump
        if(Input.GetButtonUp("Jump"))
        {
            Debug.Log("Released");
            hasCompletedChargeLoop = false;
            _psm.SetBool(_psm.anim, "IsCharging", false);
            _psm.SetBool(_psm.anim, "IsFullCharge", false);
            _psm.SetBool(_psm.anim, "HasJumped", true);
            _psm.SetBool(_psm.anim, "IsFlying", true);
            Vector2 pos = Vector2.up * jumpForce * charger;
            pos.x = move * jumpForce;
            rb.AddForce(pos, ForceMode2D.Impulse);
            //Debug.Log($"Jumped with force: {jumpForce * charger}");
            hasJumped = true;
        }

        // dont know how we got here
        if(_psm.IsGrounded() && !Input.GetButton("Jump") && !hasJumped)
        {
            Debug.Log("What");
            //rb.AddForce(Vector2.up * jumpHeight, ForceMode2D.Impulse);
            //hasJumped = true;
        }

        // on the ground and has jumped
        if (_psm.IsGrounded() && hasJumped)
        {
            _psm.TransitionToState(EPlayerState.Idle);
        }
    }

    public override void FixedUpdateState() {}

    public override EPlayerState GetNextState()
    {
        if (_psm.IsFalling())
        {
            return EPlayerState.Falling;
        }
        else if (_psm.IsMoving() && hasJumped)
        {
            return EPlayerState.Moving;
        }
        else if (_psm.IsGrounded() && hasJumped)
        {
            //Debug.Log("Entering Idle Here");
            return EPlayerState.Idle;
        }
        return EPlayerState.Jumping;
    }

    public override void OnTriggerEnter(Collider other) { }

    public override void OnTriggerStay(Collider other) { }

    public override void OnTriggerExit(Collider other) { }
}