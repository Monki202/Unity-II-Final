using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;

public class Keypad : MonoBehaviour
{
    [Header("UI Settings")]
    public TMP_Text keypadDisplay;
    public GameObject firstSelectedButton;

    [Header("Settings")]
    public float successDelay = 2f;

    private string password;
    private string enteredpassword = "";

    private Animator doorAnimator;
    private KeypadDoor activeKeypad;

    private FPMovement movement;
    private MouseLook[] lookScripts;

    private bool completed = false;
    private bool isOpen = false;
    private bool checkingCode = false;

    void Awake()
    {
        if (keypadDisplay != null)
            keypadDisplay.text = "Enter Code";
    }

    public void Open(
        GameObject player,
        string newPassword,
        Animator newDoorAnimator,
        KeypadDoor sourceKeypad)
    {
        if (isOpen)
            return;

        password = newPassword;
        doorAnimator = newDoorAnimator;
        activeKeypad = sourceKeypad;

        enteredpassword = "";
        completed = false;
        checkingCode = false;

        movement = player.GetComponent<FPMovement>();
        lookScripts = player.GetComponentsInChildren<MouseLook>();

        gameObject.SetActive(true);

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

        if (keypadDisplay != null)
            keypadDisplay.text = "Enter Code";

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
                EventSystem.current.SetSelectedGameObject(
                    firstSelectedButton);
        }
    }

    void Update()
    {
        if (!isOpen || checkingCode)
            return;

        // Close the keypad with Escape.
        if (Keyboard.current != null &&
            Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            Close();
            return;
        }

        if (enteredpassword.Length == password.Length)
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

            if (keypadDisplay != null)
                keypadDisplay.text = "Success!";

            if (doorAnimator != null)
                doorAnimator.SetTrigger("OpenDoor");

            yield return new WaitForSeconds(successDelay);

            Close();
        }
        else
        {
            if (keypadDisplay != null)
                keypadDisplay.text = "Incorrect Passcode";

            yield return new WaitForSeconds(1f);

            enteredpassword = "";

            if (keypadDisplay != null)
                keypadDisplay.text = "Enter Code";

            checkingCode = false;
        }
    }

    public void ButtonNumber(string btnNum)
    {
        if (!isOpen || checkingCode)
            return;

        if (enteredpassword.Length >= password.Length)
            return;

        enteredpassword += btnNum;

        if (keypadDisplay != null)
            keypadDisplay.text = enteredpassword;
    }

    public void Close()
    {
        if (!isOpen)
            return;

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

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (movement != null)
        {
            movement.ResetVerticalVelocity();
            movement.enabled = true;
        }

        if (lookScripts != null)
        {
            foreach (MouseLook look in lookScripts)
            {
                if (look != null)
                    look.enabled = true;
            }
        }

        if (activeKeypad != null)
        {
            activeKeypad.InteractionFinished(completed);
            activeKeypad = null;
        }
    }
}