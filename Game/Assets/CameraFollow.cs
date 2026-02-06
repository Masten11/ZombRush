using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;  // Spelaren vi ska följa
    public Vector3 offset;    // Avståndet till spelaren (t.ex. bakom och ovanför)
    public float smoothSpeed = 0.125f; // Hur mjukt kameran följer (lägre = mjukare)

    void Start()
    {
        // Om du inte ställt in offset manuellt, räkna ut det baserat på startpositionen
        if (target != null)
        {
            offset = transform.position - target.position;
        }
    }

    void LateUpdate()
    {
        if (target == null) return;

        // Vi vill följa spelarens X (sida) och Z (framåt), 
        // men vi  vill ha en fast höjd eller följa Y mjukt.
        
        // Räkna ut var kameran borde vara
        Vector3 desiredPosition = target.position + offset;

        
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
        
        // Applicera positionen
        transform.position = smoothedPosition;
    }
}