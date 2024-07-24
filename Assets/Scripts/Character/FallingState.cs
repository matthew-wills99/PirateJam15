using System;
using UnityEngine;

public class FallingState : BaseState<EPlayerState>
{
    private PlayerStateMachine _psm;
    public float move;
    public float moveSpeed = 3f;

    public FallingState(PlayerStateMachine playerStateMachine) : base(EPlayerState.Falling)
    {
        _psm = playerStateMachine;
    }

    public override void EnterState()
    {
        _psm.ResetAnimation();
        _psm.SetBool(_psm.anim, "IsFalling", true);
        Debug.Log("Entering Falling State");
    }

    public override void ExitState()
    {
        //Debug.Log("Exiting Falling State");
    }

    public override void UpdateState()
    {
        if (_psm.IsMoving() && _psm.IsGrounded())
        {
            _psm.TransitionToState(EPlayerState.Moving);
        }
        else if (_psm.IsJumping())
        {
            _psm.TransitionToState(EPlayerState.Jumping);
        }
        else
        {
            move = Input.GetAxis("Horizontal");
            _psm.transform.Translate(Vector3.right * move * Time.deltaTime * moveSpeed);
        }
    }

    public override void FixedUpdateState() {}

    public override EPlayerState GetNextState()
    {
        if (_psm.IsMoving() && _psm.IsGrounded())
        {
            return EPlayerState.Moving;
        }
        else if (_psm.IsJumping())
        {
            return EPlayerState.Jumping;
        }
        else if (_psm.IsGrounded())
        {
            return EPlayerState.Idle;
        }
        return EPlayerState.Falling;
    }

    public override void OnTriggerEnter(Collider other) { }

    public override void OnTriggerStay(Collider other) { }

    public override void OnTriggerExit(Collider other) { }
}