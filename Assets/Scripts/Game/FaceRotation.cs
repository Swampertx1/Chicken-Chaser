using Unity.Netcode;
using UnityEngine;

namespace Game
{
    public class FaceRotation : NetworkBehaviour
    {
        private Rigidbody rb;
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Awake()
        {
            rb = GetComponent<Rigidbody>();
        }

        // Update is called once per frame
        void FixedUpdate()
        {
            rb.rotation = Quaternion.LookRotation(rb.linearVelocity, Vector3.right);
        }

        
    }
}
