
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class KeypadDoor : MonoBehaviour
{
    [Header("Interaction Settings")]
    public float raycastDistance = 5.0f;

    [Header("Keypad Settings")]
    public Keypad keypad;

    [Header("UI Settings")]
    public TMP_Text interactionPrompt;

    [Header("Player Reference")]
    public GameObject player;

    void Start()
    {
        // Automatically find the player if not assigned
        if (player == null)
        {
            FPMovement movement =
                GetComponentInParent<FPMovement>();

            if (movement != null)
                player = movement.gameObject;
        }

        // Hide interaction prompt at start
        if (interactionPrompt != null)
            interactionPrompt.text = "";
    }

    void Update()
    {
        // Draw raycast for debugging
        Debug.DrawRay(
            transform.position,
            transform.forward * raycastDistance,
            Color.green
        );

        // Clear interaction prompt
        if (interactionPrompt != null)
            interactionPrompt.text = "";

        RaycastHit hit;

        // Check what the player is looking at
        if (Physics.Raycast(
            transform.position,
            transform.forward,
            out hit,
            raycastDistance))
        {
            // Check if object is a keypad
            if (hit.collider.CompareTag("Keypad"))
            {
                // Display interaction prompt
                if (interactionPrompt != null)
                    interactionPrompt.text = "Press E to Use Keypad";
            }
        }
    }

    // Called when player presses E
    public void InteractableObject(
        InputAction.CallbackContext ctx)
    {
        if (!ctx.performed)
            return;

        RaycastHit hit;

        // Check if player is looking at keypad
        if (Physics.Raycast(
            transform.position,
            transform.forward,
            out hit,
            raycastDistance))
        {
            if (hit.collider.CompareTag("Keypad"))
            {
                // Open keypad UI
                if (keypad != null && player != null)
                {
                    keypad.Open(player);
                }
            }
        }
    }
}
