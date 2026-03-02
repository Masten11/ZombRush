using UnityEngine;

public class SmashablePlank : MonoBehaviour
{
    private Rigidbody rb;
    public float breakForce = 10f; // Extra push to make it look cool

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        
        // Ensure it starts frozen so it stands perfectly straight
        rb.isKinematic = true; 
    }

    void OnCollisionEnter(Collision col)
    {
        // Check if the thing hitting us is the player
        if (col.gameObject.CompareTag("Player"))
        {
            // Turn physics back on instantly!
            rb.isKinematic = false;

            // Optional: Give it a nice push in the direction the player is running
            Vector3 pushDirection = col.contacts[0].point - transform.position;
            pushDirection = -pushDirection.normalized; // Push away from the impact point
            
            rb.AddForce(pushDirection * breakForce, ForceMode.Impulse);
        }
    }
}