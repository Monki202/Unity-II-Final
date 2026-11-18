using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Audio;
using UnityEngine.InputSystem;

public class PlayerHealth : MonoBehaviour
{
    public Image healthBar;
    public float playerHealth = 100;
    public float currentHealth;
    PlayerInput playerInput;

    AudioSource audioSource;
    public AudioClip catchSound;
    // Start is called before the first frame update
    void Start()
    {
        playerInput = GetComponent<PlayerInput>();
        audioSource = GetComponent<AudioSource>();
        currentHealth = playerHealth;
    }

    public void TakeDamage(float damage)
    {
        if (currentHealth >= 0) {
            currentHealth -= damage;
            healthBar.fillAmount = currentHealth/playerHealth;
        }
        if(currentHealth <= 0)
        {
            StartCoroutine(Death());
        }
    }

    IEnumerator Death()
    {
        playerInput.enabled = false;
        audioSource.PlayOneShot(catchSound);
        yield return new WaitForSeconds(1.4f);
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
