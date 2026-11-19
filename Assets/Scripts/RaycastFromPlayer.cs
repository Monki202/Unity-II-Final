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


    //Player Controller
    public FPMovement playerController;




    // Start is called before the first frame update
    void Start()
    {
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


    }

    void buttonSound()
    {
        if (doorUnlocked)
        {
            audioSource.PlayOneShot(rightBuzzer);
        }
        else
        {
            audioSource.PlayOneShot(wrongBuzzer);
        }
    }
}