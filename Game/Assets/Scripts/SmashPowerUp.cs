using UnityEngine;

public class SmashPowerUp : MonoBehaviour
{
    public float spinSpeed = 100f;

    void Update()
    {
        transform.Rotate(Vector3.up * spinSpeed * Time.deltaTime);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerMovement player = other.GetComponent<PlayerMovement>();
            
            if (player != null)
            {
                // Turn on Invincibility!
                player.ActivateSmashMode();
                Destroy(gameObject);
            }
        }
    }
}