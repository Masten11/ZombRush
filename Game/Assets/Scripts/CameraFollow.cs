using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Target & Movement")]
    public Transform target;  
    public float smoothSpeed = 10f; // Make sure this is 10 or higher in the Inspector!

    [Header("Cinematic Intro Settings")]
    public Vector3 frontOffset = new Vector3(0f, 3f, 8f); 
    public Vector3 frontRotation = new Vector3(10f, 180f, 0f); 
    public float introDuration = 3f; 

    // --- NEW: Reference to your temporary camera light ---
    [Header("Lighting")]
    public Light introFillLight; 
    // ---------------------------------------------------

    private Vector3 normalOffset;    
    private Quaternion normalRotation;
    
    private float timer = 0f;

    void Start()
    {
        if (target != null)
        {
            normalOffset = transform.position - target.position;
            normalRotation = transform.rotation;

            transform.position = target.position + frontOffset;
            transform.rotation = Quaternion.Euler(frontRotation);
        }
    }

    void LateUpdate()
    {
        if (target == null) return;

        timer += Time.deltaTime;

        Vector3 desiredPosition;
        Quaternion desiredRotation;

        if (timer < introDuration)
        {
            // We are in the intro
            desiredPosition = target.position + frontOffset;
            desiredRotation = Quaternion.Euler(frontRotation);
            
            // --- NEW: Keep the light ON ---
            if (introFillLight != null) introFillLight.enabled = true;
        }
        else
        {
            // We are in normal gameplay
            desiredPosition = target.position + normalOffset;
            desiredRotation = normalRotation;
            
            // --- NEW: Turn the light OFF ---
            if (introFillLight != null) introFillLight.enabled = false;
        }

        transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
        transform.rotation = Quaternion.Slerp(transform.rotation, desiredRotation, smoothSpeed * Time.deltaTime);
    }
}