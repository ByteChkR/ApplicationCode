using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;

[Serializable]
public class MovementVisualization
{
    public Material Material;
    public PlayerControllerState State;
}

[RequireComponent(typeof(Rigidbody)), RequireComponent(typeof(SpriteRenderer))]
public class PlayerController : MonoBehaviour
{
    private Rigidbody playerBody;
    private SpriteRenderer renderer;
    private Ray GroundRay => new Ray(transform.position, Vector3.down);

    public PlayerControllerState ControllerState { get; private set; } = PlayerControllerState.Idle;
    [Header("Ground Detection")]
    public LayerMask GroundMask;

    public bool IsGrounded => Physics.Raycast(GroundRay, out RaycastHit info, 1000, GroundMask) && info.distance < DistanceUntilUngrounded;
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
        Vector2 controllerForce = Vector2.zero;

        //The Diagram states that there is no transition from any state to runnning except Walking
        float runToggle = ControllerState == PlayerControllerState.Walking || ControllerState == PlayerControllerState.Running ? RunToggle.GetActivation() : 0;

        //Based on the information wether we are running or not we are selecting the right Speed Values.
        float mSpeedLeft = Math.Abs(runToggle) > 0.0001f ? RunSpeedLeft : WalkSpeedLeft;
        float mSpeedRight = Math.Abs(runToggle) > 0.0001f ? RunSpeedRight : WalkSpeedRight;

        //Get the Input from the InputBindings.
        float inputLeft = Left.GetActivation(); //Range [0, 1]
        float inputRight = Right.GetActivation(); //Range [0, 1]

        if (ControllerState == PlayerControllerState.Falling)
        {
            inputRight = inputLeft = 0;
        }

        controllerForce.x = -mSpeedLeft * inputLeft; //Move to the Reft
        controllerForce.x += mSpeedRight * inputRight; //Move to the Right

        bool grounded = IsGrounded;
        if (grounded && ControllerState == PlayerControllerState.Falling)
        {
            //Make sure we can transition from the Falling state back to the Idle State
            SetPlayerControllerState(PlayerControllerState.Idle);
        }

        //Set the state to walking if we are moving and not jumping/falling
        if (ControllerState != PlayerControllerState.Jumping && ControllerState != PlayerControllerState.Falling &&
            (Math.Abs(inputRight) > 0.0001f || Math.Abs(inputLeft) > 0.0001f))
        {
            SetPlayerControllerState(PlayerControllerState.Walking);
        }

        //When we are currently walking but the Run button was pressed, we are changing the state to Running
        if (ControllerState == PlayerControllerState.Walking && Mathf.Abs(runToggle) > 0.0001f)
        {
            SetPlayerControllerState(PlayerControllerState.Running);
        }


        float jump = Jump.GetActivation() * (grounded ? 1 : 0); //Multiply by 0 if grounded(No jump while not grounded).
        if (Math.Abs(jump) > 0.0001f)
        {
            SetPlayerControllerState(PlayerControllerState.Jumping);
            controllerForce.x *= ForwardJumpForce * jump; //Possibility to add a bit of extra Horizontal Velocity when jumping
            controllerForce.y = UpwardJumpForce * jump; //Upwards Motion
        }


        //Multiplying by delta time.
        controllerForce *= Time.deltaTime;

        //Applying the Force to the RigidBody
        Acceleration = controllerForce;
        playerBody.AddForce(controllerForce, ForceMode.Acceleration);

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

        //Debug.Log("Controller State: " + state);
    }


    public void SetPlayerControllerState(PlayerControllerState newState)
    {
        ControllerState = newState;
        //Set the material from the visualizations array.
        renderer.sharedMaterial = Visualization.First(x => x.State == newState).Material;
    }


}
