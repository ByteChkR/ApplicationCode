using System;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(Rigidbody)), RequireComponent(typeof(SpriteRenderer))]
public class PlayerController : MonoBehaviour
{
    private Rigidbody playerBody;
    private SpriteRenderer renderer;
    private Ray GroundRayRight => new Ray(transform.position + Vector3.right * 0.5f, Vector3.down);
    private Ray GroundRayLeft => new Ray(transform.position + Vector3.left * 0.5f, Vector3.down);
    private Ray GroundRayCenter => new Ray(transform.position, Vector3.down);

    public PlayerControllerState ControllerState { get; private set; } = PlayerControllerState.Idle;
    [Header("Ground Detection")]
    public LayerMask GroundMask;

    public bool IsGrounded
    {
        get
        {
            //Check all Rays if any hit the ground.
            RaycastHit hL, hR, hC;
            if (Physics.Raycast(GroundRayCenter, out hC, 1000, GroundMask) && 
                hC.distance < DistanceUntilUngrounded)
            {
                return true;
            }

            if (Physics.Raycast(GroundRayLeft, out hL, 1000, GroundMask) &&
                hL.distance < DistanceUntilUngrounded)
            {
                return true;
            }

            return Physics.Raycast(GroundRayRight, out hR, 1000, GroundMask) &&
                   hR.distance < DistanceUntilUngrounded;
        }
    }

    public Vector2 Velocity => playerBody.velocity;
    public Vector2 Acceleration { get; private set; }

    [Header("Input Bindings")]
    public InputBinding Left;
    public InputBinding Right;
    public InputBinding RunToggle;
    public InputBinding Jump;

    [Header("Movement Visualization")] public MovementVisualization[] Visualization;

    [Header("Movement Parameters")]
    public float WalkSpeedLeft;
    public float WalkSpeedRight;
    public float RunSpeedLeft;
    public float RunSpeedRight;
    public float ForwardJumpForce;
    public float UpwardJumpForce;

    //The Distance from the Origin to the Ground at which the player is marked as Grounded again
    public float DistanceUntilUngrounded;



    // Start is called before the first frame update
    private void Start()
    {
        renderer = GetComponent<SpriteRenderer>();
        playerBody = GetComponent<Rigidbody>();
    }


    // Update is called once per frame
    private void Update()
    {
        //Create Horizontal Input
        Vector2 controllerForce = CreateMovementInput(out bool running);
        //Create Vertical Input
        controllerForce = CreateJumpInput(controllerForce, out bool jumped);
        //Multiplying by delta time.
        controllerForce *= Time.deltaTime;

        //Update the State Machine
        SetStateMachine(jumped, controllerForce, running);

        //Applying the Force to the RigidBody
        Acceleration = controllerForce;
        playerBody.AddForce(controllerForce, ForceMode.Acceleration);
    }


    public void SetPlayerControllerState(PlayerControllerState newState)
    {
        ControllerState = newState;
        //Set the material from the visualizations array.
        renderer.sharedMaterial = Visualization.First(x => x.State == newState).Material;
    }

    #region State Machine

    public void SetStateMachine(bool jumped, Vector2 currentAccel, bool running)
    {

        if (jumped)
        {
            SetPlayerControllerState(PlayerControllerState.Jumping);
        }
        bool grounded = IsGrounded;
        if (grounded && ControllerState == PlayerControllerState.Falling)
        {
            //Make sure we can transition from the Falling state back to the Idle State
            SetPlayerControllerState(PlayerControllerState.Idle);
        }

        //Set the state to walking if we are moving and not jumping/falling
        if (ControllerState != PlayerControllerState.Jumping && ControllerState != PlayerControllerState.Falling &&
            (Math.Abs(currentAccel.x) > 0.0001f || Math.Abs(currentAccel.y) > 0.0001f))
        {
            SetPlayerControllerState(PlayerControllerState.Walking);
        }

        //When we are currently walking but the Run button was pressed, we are changing the state to Running
        if (ControllerState == PlayerControllerState.Walking && running)
        {
            SetPlayerControllerState(PlayerControllerState.Running);
        }

        //When we are not grounded and we are over the apex(velocity.y <= 0) we are falling
        if (!grounded && playerBody.velocity.y <= 0)
        {
            SetPlayerControllerState(PlayerControllerState.Falling);
        }

        //In case we are not jumping and the speed is 0 we transition back into idle state.
        if (playerBody.velocity.sqrMagnitude < 0.000001f && ControllerState != PlayerControllerState.Jumping && ControllerState != PlayerControllerState.Falling)
        {
            SetPlayerControllerState(PlayerControllerState.Idle);
        }

    }

    #endregion

    #region Movement Computation

    private Vector2 CreateJumpInput(Vector2 baseInput, out bool jumped)
    {
        jumped = false;
        float jump = Jump.GetActivation() * (IsGrounded ? 1 : 0); //Multiply by 0 if grounded(No jump while not grounded).
        if (Math.Abs(jump) > 0.0001f)
        {
            jumped = true;
            baseInput.x *= ForwardJumpForce * jump; //Possibility to add a bit of extra Horizontal Velocity when jumping
            baseInput.y = UpwardJumpForce * jump; //Upwards Motion
        }

        return baseInput;
    }

    private Vector2 CreateMovementInput(out bool running)
    {
        Vector2 controllerForce = Vector2.zero;

        //The Diagram states that there is no transition from any state to runnning except Walking
        float runToggle = ControllerState == PlayerControllerState.Walking || ControllerState == PlayerControllerState.Running ? RunToggle.GetActivation() : 0;
        running = Math.Abs(runToggle) > 0.0001f;
        //Based on the information wether we are running or not we are selecting the right Speed Values.
        float mSpeedLeft = running ? RunSpeedLeft : WalkSpeedLeft;
        float mSpeedRight = running ? RunSpeedRight : WalkSpeedRight;

        //Get the Input from the InputBindings.
        float inputLeft = Left.GetActivation(); //Range [0, 1]
        float inputRight = Right.GetActivation(); //Range [0, 1]

        if (ControllerState == PlayerControllerState.Falling)
        {
            inputRight = inputLeft = 0;
        }

        controllerForce.x = -mSpeedLeft * inputLeft; //Move to the Reft
        controllerForce.x += mSpeedRight * inputRight; //Move to the Right
        return controllerForce;
    }

    #endregion

}
