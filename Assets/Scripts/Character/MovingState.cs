using System;
using UnityEngine;


public class MovingState : BaseState<EPlayerState>
{
    private PlayerStateMachine _psm;
    private float move;
    private Rigidbody2D rb;

    public MovingState(PlayerStateMachine playerStateMachine) : base(EPlayerState.Moving)
    {
        _psm = playerStateMachine;
    }

    public override void EnterState()
    {
        rb = _psm.GetComponent<Rigidbody2D>();
        _psm.ResetAnimation();
        Debug.Log("Entering Moving State");
    }

    public override void ExitState()
    {
        Debug.Log("Exiting Moving State");
    }

    public override void UpdateState()
    {
        if (!_psm.IsMoving())
        {
            _psm.TransitionToState(EPlayerState.Idle);
        }
        else if (_psm.IsFalling())
        {
            _psm.TransitionToState(EPlayerState.Falling);
        }
        else if (_psm.IsJumping())
        {
            _psm.TransitionToState(EPlayerState.Jumping);
        }
        else
        {
            move = Input.GetAxis("Horizontal");
            rb.velocity = new Vector2(move * _psm.moveSpeed, rb.velocity.y);
            //_psm.transform.Translate(Vector3.right * move * Time.deltaTime * _psm.moveSpeed);
        }
    }

    public override void FixedUpdateState() {}

    public override EPlayerState GetNextState()
    {
        if (!_psm.IsMoving() && !_psm.IsJumping() && !_psm.IsFalling())
        {
            Debug.Log("Return");
            return EPlayerState.Idle;
        }
        else if (_psm.IsJumping())
        {
            return EPlayerState.Jumping;
        }

        return EPlayerState.Moving;
    }

    public override void OnTriggerEnter(Collider other) { }

    public override void OnTriggerStay(Collider other) { }

    public override void OnTriggerExit(Collider other) { }
}