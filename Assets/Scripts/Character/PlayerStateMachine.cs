using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EPlayerState 
{
    Idle,
    Moving,
    Jumping,
    Falling,
}

public class PlayerStateMachine : MonoBehaviour
{
    public Sprite IdleSprite;
    public Sprite MovingSprite;
    public Sprite JumpingSprite;
    public Sprite FallingSprite;

    protected Dictionary<EPlayerState, BaseState<EPlayerState>> States = new Dictionary<EPlayerState, BaseState<EPlayerState>>();
    protected Dictionary<EPlayerState, Sprite> Sprites = new Dictionary<EPlayerState, Sprite>();
    protected BaseState<EPlayerState> CurrentState;
    protected SpriteRenderer spriteRenderer;

    public float moveSpeed = 3f;

    protected bool IsTransitioningState = false;

    private bool grounded;

    // animations
    public Animator anim;
    private bool playingAnimation = false;

    private void Awake()
    {
        anim = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        States[EPlayerState.Idle] = new IdleState(this);
        States[EPlayerState.Moving] = new MovingState(this);
        States[EPlayerState.Jumping] = new JumpingState(this);
        States[EPlayerState.Falling] = new FallingState(this);

        Sprites[EPlayerState.Idle] = IdleSprite;
        Sprites[EPlayerState.Moving] = MovingSprite;
        Sprites[EPlayerState.Jumping] = JumpingSprite;
        Sprites[EPlayerState.Falling] = FallingSprite;

        CurrentState = States[EPlayerState.Idle];
    }

    void Start()
    {
        CurrentState.EnterState();
    }

    void Update()
    {
        EPlayerState nextStateKey = CurrentState.GetNextState();

        if(!IsTransitioningState && nextStateKey.Equals(CurrentState.StateKey))
        {
            CurrentState.UpdateState();
        }
        else if(!IsTransitioningState && !nextStateKey.Equals(CurrentState.StateKey))
        {
            TransitionToState(nextStateKey);
        }
        
    }

    void FixedUpdate()
    {
        CurrentState.FixedUpdateState();
    }

    public void TransitionToState(EPlayerState stateKey)
    {
        IsTransitioningState = true;
        CurrentState.ExitState();
        Debug.Log($"{CurrentState.StateKey} -> {stateKey}");
        CurrentState = States[stateKey];
        CurrentState.EnterState();
        IsTransitioningState = false;
    }

    // Implement other methods as needed, such as checking for movement or jumps.
    public bool IsMoving()
    {
        // Implement logic to check if the player is moving
        return Input.GetAxis("Horizontal") != 0;
    }

    public bool IsGrounded()
    {
        return grounded;
    }

    public bool IsJumping()
    {
        // Implement logic to check if the player is jumping
        if(Input.GetButtonDown("Jump") && IsGrounded())
        {
            grounded = false; // do not put this in OnCollisionExit because queues and stuff
            return true;
        }
        return false;
    }

    public bool IsFalling()
    {
        // Implement logic to check if the player is falling
        return /*!IsGrounded() &&*/ GetComponent<Rigidbody2D>().velocity.y < 0;
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        if(other.gameObject.CompareTag("Ground"))
        {
            grounded = true;
        }
    }

    private void OnCollisionExit2D(Collision2D other)
    {
        if(other.gameObject.CompareTag("Ground"))
        {
            grounded = false;
        }
    }

    public bool AnimationHasLooped()
    {
        return anim.GetCurrentAnimatorStateInfo(0).normalizedTime > 1 && !anim.IsInTransition(0);
    }
    
    // animator controller params need to be set in parent thread because fuck you
    public void SetBool(Animator animator, string name, bool value)
    {
        animator.SetBool(name, value);
    }


    public void ResetAnimation()
    {
        
        SetBool(anim, "IsCharging", false);
        SetBool(anim, "IsFullCharge", false);
        SetBool(anim, "HasJumped", false);
        SetBool(anim, "IsFlying", false);
        SetBool(anim, "IsFalling", false);
    }

    void OnTriggerEnter(Collider other)
    {
        CurrentState.OnTriggerEnter(other);
    }

    void OnTriggerStay(Collider other)
    {
        CurrentState.OnTriggerStay(other);
    }

    void OnTriggerExit(Collider other)
    {
        CurrentState.OnTriggerExit(other);
    }

    public void SetSprite(Sprite sprite)
    {
        this.GetComponent<SpriteRenderer>().sprite = sprite;
    }
}