using System;
using UnityEngine;

public class IdleState : BaseState<EPlayerState>
{
    private PlayerStateMachine _psm;

    public IdleState(PlayerStateMachine playerStateMachine) : base(EPlayerState.Idle)
    {
        _psm = playerStateMachine;
    }

    public override void EnterState()
    {
        _psm.ResetAnimation();
        Debug.Log("Entering Idle State");
    }

    public override void ExitState()
    {
        //Debug.Log("Exiting Idle State");
    }

    public override void UpdateState()
    {
        if (_psm.IsMoving())
        {
            _psm.TransitionToState(EPlayerState.Moving);
        }
        else if (_psm.IsJumping())
        {
            _psm.TransitionToState(EPlayerState.Jumping);
        }
    }

    public override void FixedUpdateState() {}

    public override EPlayerState GetNextState()
    {
        if (_psm.IsMoving())
        {
            return EPlayerState.Moving;
        }
        else if (_psm.IsJumping())
        {
            return EPlayerState.Jumping;
        }

        return EPlayerState.Idle;
    }

    public override void OnTriggerEnter(Collider other) { }

    public override void OnTriggerStay(Collider other) { }

    public override void OnTriggerExit(Collider other) { }
}