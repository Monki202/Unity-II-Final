using UnityEngine;

public class UnlockCursor : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        Cursor.lockState = CursorLockMode.None;

        Cursor.visible = true;
    }

}
