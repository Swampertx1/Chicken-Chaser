using UnityEngine;

public class DestroyOnKeypress : MonoBehaviour
{
    // The Update method runs once per frame
    void Update()
    {
        // Check if the 'E' key is pressed down this frame
        if (Input.GetKeyDown(KeyCode.E))
        {
            // Create a ray from the mouse position
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            // Perform the raycast to see if it hits a collider
           
        }
    }
}