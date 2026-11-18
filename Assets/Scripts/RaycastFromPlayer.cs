using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class RaycastFromPlayer : MonoBehaviour
{
    public float raycastDistance = 5.0f;
    bool holdingItem = false;
    GameObject heldObj;

    public bool redBox = false;
    public bool blueBox = false;
    public bool greenBox = false;

    public GameObject doorButton;
    public Animator leftDoor;
    public Animator rightDoor;
    bool buttonPressed = false;
    bool doorUnlocked = false;
    MeshRenderer hitObj;
    public GameObject messageBox;

    //Extra UI Elements
    public Image pickupHandOpen;
    public Image interactHandOpen;
    public Image pickupHandClosed;
    public TMP_Text pickupPrompt;
    public TMP_Text buttonPrompt;

    //Audio 
    private AudioSource audioSource;
    public AudioClip wrongBuzzer;
    public AudioClip rightBuzzer;
    public GameObject Confetti;
    public GameObject VoiceCompletion;

    public Animator Glados;

    //Player Controller
    public FPMovement playerController;




    // Start is called before the first frame update
    void Start()
    {
        Glados = GameObject.Find("GLaDOS").GetComponent<Animator>();
        playerController = GetComponentInParent<FPMovement>();
        audioSource = GetComponent<AudioSource>();
        pickupHandClosed.enabled = false;
        interactHandOpen.enabled = false;
        pickupHandOpen.enabled = false;
        pickupPrompt.text = "";
        buttonPrompt.text = "";
    }

    // Update is called once per frame
    void Update()
    {

        Debug.DrawRay(transform.position, transform.forward * raycastDistance, Color.green);


        if (redBox && blueBox && greenBox)
        {
            doorButton.GetComponent<Renderer>().material.color = Color.green;
            doorUnlocked = true;
        }
        else
        {
            doorButton.GetComponent<Renderer>().material.color = Color.red;
            doorUnlocked = false;
        }

        // Clear the previous highlight and prompt.
        if (hitObj != null)
        {
            //Get rid of hand when not looking at an object
            pickupPrompt.text = "";
            pickupHandOpen.enabled = false;
            interactHandOpen.enabled = false;
            buttonPrompt.text = "";

            hitObj.materials[1].SetFloat("_Scale", 1.0f);
            hitObj = null;
        }

        //pickupPrompt.text = "";

        //// Check what the player is looking at now.
        RaycastHit hit;

        if (Physics.Raycast(transform.position, transform.forward,
            out hit, raycastDistance))
        {


            bool canPickUp = hit.collider.CompareTag("PickupItem")
                             && !holdingItem;

            bool canUseDoor = hit.collider.CompareTag("DoorButton")
                              && doorUnlocked;

            if (canPickUp)
            {
                hitObj = hit.collider.GetComponent<MeshRenderer>();
                hitObj.materials[1].SetFloat("_Scale", 1.03f);
                pickupPrompt.text = "LMB to Pickup";
                pickupHandOpen.enabled = true;
            }

            if(canUseDoor && hit.collider.CompareTag("DoorButton"))
            {
                hitObj = hit.collider.GetComponent<MeshRenderer>();
                hitObj.materials[1].SetFloat("_Scale", 1.03f);
                buttonPrompt.text = "E to Press Button";
                interactHandOpen.enabled = true;
            }

        }

    }

    public void PickupItem(InputAction.CallbackContext ctx)
    {
        if (ctx.performed)
        {
            RaycastHit hit;

            if (Physics.Raycast(transform.position, transform.forward, out hit, raycastDistance))
            {
                Debug.Log(hit.collider.name);

                if (hit.collider.gameObject.name == "GreenBox")
                {
                    pickupHandOpen.enabled = false;
                    pickupHandClosed.enabled = true;

                    hit.collider.GetComponent<PickupObj>().Pickup();
                    heldObj = hit.collider.gameObject;
                    holdingItem = true;

                    playerController.jumpStrength = 0f;
                    playerController.speed *= 0.2f;
                }

                if (hit.collider.CompareTag("PickupItem") && hit.collider.gameObject.name != "GreenBox")
                {
                        //Close hand when grabbing
                        pickupHandOpen.enabled = false;
                        pickupHandClosed.enabled = true;

                        hit.collider.GetComponent<PickupObj>().Pickup();
                        heldObj = hit.collider.gameObject;
                        holdingItem = true;
                }



            }
        }
        if (ctx.canceled)
        {
            if (holdingItem)
            {

                playerController.jumpStrength = 10f;
                playerController.speed = 10f;

                //Open hand when dropping
                pickupHandClosed.enabled = false;
                heldObj.GetComponent<PickupObj>().Pickup();
                holdingItem = false;
                heldObj = null;
            }
        }
    }

    public void InteractableObject(InputAction.CallbackContext ctx)
    {
        if (ctx.performed)
        {

            RaycastHit hit;

            if (Physics.Raycast(transform.position, transform.forward, out hit, raycastDistance))
            {
                

                if(hit.collider.CompareTag("DoorButton"))
                {
                    buttonSound();
                }
                if (hit.collider.CompareTag("DoorButton") && doorUnlocked && !buttonPressed)
                {
                    buttonPressed = true;
                    Confetti.SetActive(true);
                    Glados.SetTrigger("Talk");
                    VoiceCompletion.SetActive(true);
                    leftDoor.SetTrigger("OpenDoor");
                    rightDoor.SetTrigger("OpenDoor");
                }

            }
        }
    }

    void buttonSound()
    {
        if(doorUnlocked)
        {
            audioSource.PlayOneShot(rightBuzzer);
        }
        else
        {
            audioSource.PlayOneShot(wrongBuzzer);
        }
    }
}
