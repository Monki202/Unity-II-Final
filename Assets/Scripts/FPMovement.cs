
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

// Automatically adds CharacterController component to GameObject
[RequireComponent(typeof(CharacterController))]
public class FPMovement : MonoBehaviour
{
    // Movement settings
    public float speed = 10f;
    public float sprintSpeed = 16f;

    float h, v;
    private bool isSprinting = false;

    public Vector2 MovementInput => new Vector2(h, v);

    // Jump and gravity settings
    public float gravity = -9.8f;
    public float jumpStrength = 9.0f;

    float velocity;
    float gravityMultiplier = 3.0f;

    CharacterController controller;

    // Reset vertical velocity
    public void ResetVerticalVelocity()
    {
        velocity = 0f;
    }

    // Cache the controller before input callbacks run
    void Awake()
    {
        controller = GetComponent<CharacterController>();
    }

    // Update is called once per frame
    void Update()
    {
        // Determine current movement speed
        float currentSpeed = isSprinting ? sprintSpeed : speed;

        // Calculate horizontal movement
        float moveX = h * currentSpeed;
        float moveZ = v * currentSpeed;

        Vector3 movement = new Vector3(moveX, 0, moveZ);

        // Prevent faster diagonal movement
        movement = Vector3.ClampMagnitude(movement, currentSpeed);

        // Apply gravity
        if (IsGrounded() && velocity < 0)
        {
            velocity = -1;
        }
        else
        {
            velocity += gravity * gravityMultiplier * Time.deltaTime;
        }

        // Apply vertical velocity
        movement.y = velocity;

        // Convert movement to frame-based movement
        movement *= Time.deltaTime;

        // Move relative to player direction
        movement = transform.TransformDirection(movement);

        // Move the player
        controller.Move(movement);
    }

    // Movement input (WASD)
    public void MoveInput(InputAction.CallbackContext ctx)
    {
        h = ctx.ReadValue<Vector2>().x;
        v = ctx.ReadValue<Vector2>().y;
    }

    // Sprint input (Left Shift)
    public void Sprint(InputAction.CallbackContext ctx)
    {
        if (ctx.performed)
        {
            isSprinting = true;
        }
        else if (ctx.canceled)
        {
            isSprinting = false;
        }
    }

    // Jump input
    public void Jump(InputAction.CallbackContext ctx)
    {
        if (!enabled || !IsGrounded())
        {
            return;
        }

        if (ctx.performed)
        {
            velocity = jumpStrength;
        }
    }

    // Check if player is grounded
    bool IsGrounded()
    {
        return controller.isGrounded;
    }
}
