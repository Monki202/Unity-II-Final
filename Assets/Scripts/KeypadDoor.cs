using UnityEngine;
using UnityEngine.InputSystem;

public class KeypadDoor : MonoBehaviour
{
    [Header("Keypad Settings")]
    public string password = "1234";
    public Animator doorAnimator;
    public Keypad keypadUI;

    [Header("Interaction Settings")]
    public GameObject interactionPrompt;

    private bool playerNearby = false;
    private bool keypadActive = false;

    private GameObject player;
    private KeypadDoor currentKeypad;

    private static KeypadDoor nearestKeypad;

    void Start()
    {
        if (interactionPrompt != null)
            interactionPrompt.SetActive(false);
    }

    void Update()
    {
        // Only the closest available keypad responds to E.
        if (nearestKeypad != this || !playerNearby)
            return;

        if (keypadActive)
            return;

        if (Keyboard.current != null &&
            Keyboard.current.eKey.wasPressedThisFrame)
        {
            Interact();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        FPMovement movement =
            other.GetComponentInParent<FPMovement>();

        if (movement == null)
            return;

        player = movement.gameObject;
        playerNearby = true;

        nearestKeypad = this;

        if (interactionPrompt != null)
            interactionPrompt.SetActive(true);
    }

    private void OnTriggerExit(Collider other)
    {
        FPMovement movement =
            other.GetComponentInParent<FPMovement>();

        if (movement == null || movement.gameObject != player)
            return;

        playerNearby = false;

        if (nearestKeypad == this)
            nearestKeypad = null;

        if (interactionPrompt != null)
            interactionPrompt.SetActive(false);
    }

    public void Interact()
    {
        if (!playerNearby || keypadActive)
            return;

        if (keypadUI == null || player == null)
            return;

        keypadActive = true;

        if (interactionPrompt != null)
            interactionPrompt.SetActive(false);

        keypadUI.Open(player, password, doorAnimator, this);
    }

    public void InteractionFinished(bool success)
    {
        keypadActive = false;

        if (success)
        {
            // Prevent the same keypad from being used again.
            enabled = false;

            if (interactionPrompt != null)
                interactionPrompt.SetActive(false);

            return;
        }

        if (playerNearby && interactionPrompt != null)
            interactionPrompt.SetActive(true);
    }

    private void OnDisable()
    {
        if (nearestKeypad == this)
            nearestKeypad = null;

        if (interactionPrompt != null)
            interactionPrompt.SetActive(false);
    }
}