using UnityEngine;

public class ZombieFollower : MonoBehaviour
{
    public Transform player;
    public float followDistance = 14f;
    public float xFollowSpeed = 12f;

    public bool followPlayerY = true;

    private Animator zombieAnim;
    private PlayerMovement playerMove;

    void Awake()
    {
        zombieAnim = GetComponentInChildren<Animator>();
        if (player != null)
            playerMove = player.GetComponent<PlayerMovement>();
    }

    void FixedUpdate()
    {
        if (player == null || playerMove == null) return;

        // Follow position
        Vector3 pos = transform.position;
        pos.z = player.position.z - followDistance;
        pos.x = Mathf.Lerp(pos.x, player.position.x, Time.fixedDeltaTime * xFollowSpeed);
        pos.y = followPlayerY ? player.position.y : pos.y;

        transform.position = pos;
        transform.rotation = player.rotation;

        // Force correct animation state
        if (zombieAnim != null)
            zombieAnim.SetBool("isGrounded", playerMove.IsGrounded);
    }
}