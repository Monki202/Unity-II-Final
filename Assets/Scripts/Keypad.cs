
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;

public class Keypad : MonoBehaviour
{
    [Header("Keypad Settings")]
    public string password = "1234";
    public string enteredpassword = "";
    public TMP_Text keypadDisplay;
    public int passDigits;

    [Header("Door Settings")]
    public Animator doorAnimator;

    [Header("UI Settings")]
    public GameObject firstSelectedButton;
    public float successDelay = 2f;

    private FPMovement movement;
    private MouseLook[] lookScripts;

    private bool completed = false;
    private bool isOpen = false;
    private bool checkingCode = false;

    void Awake()
    {
        passDigits = password.Length;
        enteredpassword = "";

        if (keypadDisplay != null)
            keypadDisplay.text = "Enter Code";
    }

    // Opens the keypad UI
    public void Open(GameObject player)
    {
        if (completed || isOpen)
            return;

        gameObject.SetActive(true);

        movement = player.GetComponent<FPMovement>();
        lookScripts = player.GetComponentsInChildren<MouseLook>();

        if (movement != null)
        {
            movement.ResetVerticalVelocity();
            movement.enabled = false;
        }

        foreach (MouseLook look in lookScripts)
        {
            if (look != null)
                look.enabled = false;
        }

        isOpen = true;
        checkingCode = false;

        enteredpassword = "";
        keypadDisplay.text = "Enter Code";

        // Enable mouse cursor for clicking UI buttons
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (firstSelectedButton == null)
        {
            Button button = GetComponentInChildren<Button>();

            if (button != null)
                firstSelectedButton = button.gameObject;
        }

        if (EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(null);

            if (firstSelectedButton != null)
                EventSystem.current.SetSelectedGameObject(firstSelectedButton);
        }
    }

    void Update()
    {
        if (!isOpen || checkingCode)
            return;

        // Close keypad with Escape
        if (EventSystem.current != null)
        {
            var uiInput =
                EventSystem.current.currentInputModule
                as InputSystemUIInputModule;

            if (uiInput != null &&
                uiInput.cancel != null &&
                uiInput.cancel.action.WasPerformedThisFrame())
            {
                Close();
                return;
            }
        }

        // Automatically check when all digits are entered
        if (enteredpassword.Length == passDigits)
        {
            StartCoroutine(CheckPassword());
        }
    }

    IEnumerator CheckPassword()
    {
        checkingCode = true;

        if (enteredpassword == password)
        {
            completed = true;

            // Display success message
            keypadDisplay.text = "Success!";

            // Start door opening animation
            if (doorAnimator != null)
            {
                doorAnimator.SetTrigger("OpenDoor");
            }

            // Keep Success! visible for 2 seconds
            yield return new WaitForSeconds(successDelay);

            Close();
        }
        else
        {
            // Incorrect password
            keypadDisplay.text = "Incorrect Passcode";

            yield return new WaitForSeconds(1f);

            enteredpassword = "";
            keypadDisplay.text = "Enter Code";

            checkingCode = false;
        }
    }

    // Close keypad using existing input event
    public void ShowCursor(InputAction.CallbackContext ctx)
    {
        if (ctx.performed && isOpen && !checkingCode)
            Close();
    }

    // Close keypad UI
    public void Close()
    {
        gameObject.SetActive(false);
    }

    void OnDisable()
    {
        if (!isOpen)
            return;

        isOpen = false;
        checkingCode = false;

        if (EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(null);

        // Hide and lock mouse cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Restore player movement
        if (movement != null)
        {
            movement.ResetVerticalVelocity();
            movement.enabled = true;
        }

        // Restore camera movement
        if (lookScripts != null)
        {
            foreach (MouseLook look in lookScripts)
            {
                if (look != null)
                    look.enabled = true;
            }
        }
    }

    // Called when clicking a keypad number button
    public void ButtonNumber(string btnNum)
    {
        if (!isOpen || checkingCode)
            return;

        if (enteredpassword.Length >= passDigits)
            return;

        enteredpassword += btnNum;

        keypadDisplay.text = enteredpassword;
    }
}
