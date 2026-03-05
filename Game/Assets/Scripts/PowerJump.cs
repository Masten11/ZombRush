using UnityEngine;

public class PowerJump : MonoBehaviour
{
    public float spinSpeed = 100f;

    void Update()
    {
        // Makes the power-up spin beautifully in the air
        transform.Rotate(Vector3.up * spinSpeed * Time.deltaTime);
    }

    void OnTriggerEnter(Collider other)
    {
        // Check if the object crashing into us is the Player
        if (other.CompareTag("Player"))
        {
            PlayerMovement player = other.GetComponent<PlayerMovement>();
            
            if (player != null)
            {
                // Tell the player to turn on the power-up!
                player.ActivateDoubleJump();

                // Destroy the power-up so it disappears from the road
                Destroy(gameObject);
            }
        }
    }
}