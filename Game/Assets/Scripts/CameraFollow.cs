using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Target & Movement")]
    public Transform target;  // The player we want to look at
    public float smoothSpeed = 10f; // How fast the camera catches up (higher = faster)

    [Header("Cinematic Intro Settings")]
    public Vector3 frontOffset = new Vector3(0f, 3f, 8f); // Position in front of the player's face
    public Vector3 frontRotation = new Vector3(10f, 180f, 0f); // Looking backwards at the player
    public float introDuration = 3f; // How long the camera stays in front

    [Header("Lighting")]
    public Light introFillLight; // Temporary light to illuminate the player's face during the intro

    // These store the "normal" view based on where you placed the camera in the Unity Editor
    private Vector3 normalOffset;    
    private Quaternion normalRotation;
    
    private float timer = 0f;

    void Start()
    {
        if (target != null)
        {
            // 1. Take a snapshot of where the camera is right now in the Editor
            normalOffset = transform.position - target.position;
            normalRotation = transform.rotation;

            // 2. Instantly teleport the camera to the front so the very first frame is the intro view
            transform.position = target.position + frontOffset;
            transform.rotation = Quaternion.Euler(frontRotation);
        }
    }

    // We use LateUpdate for cameras so it moves AFTER the player moves. This prevents stuttering!
    void LateUpdate()
    {
        if (target == null) return;

        timer += Time.deltaTime; // Keep track of how much time has passed

        Vector3 desiredPosition;
        Quaternion desiredRotation;

        // --- STATE 1: THE INTRO ---
        if (timer < introDuration)
        {
            // Set our target to be in front of the player
            desiredPosition = target.position + frontOffset;
            desiredRotation = Quaternion.Euler(frontRotation);
            
            // Keep the cinematic face light turned on
            if (introFillLight != null) introFillLight.enabled = true;
        }
        // --- STATE 2: NORMAL GAMEPLAY ---
        else
        {
            // Set our target to be the normal behind-the-back view we saved earlier
            desiredPosition = target.position + normalOffset;
            desiredRotation = normalRotation;
            
            // Turn off the cinematic face light so it doesn't mess up the level lighting
            if (introFillLight != null) introFillLight.enabled = false;
        }

        // --- SMOOTH MOVEMENT ---
        // Lerp smoothly slides the position from where we are, to where we want to be
        transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
        
        // Slerp smoothly rotates the camera to the new angle
        transform.rotation = Quaternion.Slerp(transform.rotation, desiredRotation, smoothSpeed * Time.deltaTime);
    }
}