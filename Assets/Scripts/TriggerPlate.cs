using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerPlate : MonoBehaviour
{
    public RaycastFromPlayer raycastScript;
    [SerializeField]
    string objTag;

    public Animator animator;

    private AudioSource audioSource;
    public AudioClip click;

    public bool buttonPressed = false;

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }


    private void OnTriggerStay(Collider other)
    {
        if (!buttonPressed)
        {
            audioSource.PlayOneShot(click);
        }
        buttonPressed = true;
        animator.SetFloat("UpDown", -1);
        if (other.gameObject.name == objTag)
        {
            if (other.gameObject.name == "RedBox")
            {
                raycastScript.redBox = true;
            }
            if (other.gameObject.name == "BlueBox")
            {
                raycastScript.blueBox = true;
            }
            if (other.gameObject.name == "GreenBox")
            {
                raycastScript.greenBox = true;
            }

        }
        
    }

    private void OnTriggerExit(Collider other)
    {
        buttonPressed = false;
        animator.SetFloat("UpDown", 2);
        if (other.gameObject.name == objTag)
        {
            if (other.gameObject.name == "RedBox")
            {
                raycastScript.redBox = false;
            }
            if (other.gameObject.name == "BlueBox")
            {
                raycastScript.blueBox = false;
            }
            if (other.gameObject.name == "GreenBox")
            {
                raycastScript.greenBox = false;
            }

        }
    }
}
