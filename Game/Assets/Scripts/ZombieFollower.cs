using UnityEngine;
using System.Collections;

public class ZombieFollower : MonoBehaviour
{
    public Transform player;
    public float followDistance = 14f;
    public float xFollowSpeed = 12f;
    public bool followPlayerY = true;

    [Header("Kill Sequence")]
    public float killRushSpeed = 18f;
    public float stopDistance = 1.6f;
    public float attackDelay = 0.05f;
    public float gameOverDelay = 1.5f;

    private Animator zombieAnim;
    private PlayerMovement playerMove;
    private PlayerAudio playerAudio;
    private bool isKillingPlayer = false;

    void Awake()
    {
        zombieAnim = GetComponentInChildren<Animator>();

        if (player != null)
        {
            playerMove = player.GetComponent<PlayerMovement>();
            playerAudio = player.GetComponent<PlayerAudio>();
        }
    }

    void Start()
    {
        if (zombieAnim != null)
        {
            zombieAnim.ResetTrigger("Attack");
            zombieAnim.Play("run", 0, 0f);
        }
    }

    void FixedUpdate()
    {
        if (player == null || playerMove == null || isKillingPlayer) return;

        Vector3 pos = transform.position;
        pos.z = player.position.z - followDistance;
        pos.x = Mathf.Lerp(pos.x, player.position.x, Time.fixedDeltaTime * xFollowSpeed);
        pos.y = followPlayerY ? player.position.y : pos.y;

        transform.position = pos;
        transform.rotation = player.rotation;

        if (zombieAnim != null)
            zombieAnim.SetBool("isGrounded", playerMove.IsGrounded);
    }

    public void StartKillSequence()
    {
        if (isKillingPlayer) return;
        StartCoroutine(KillSequence());
    }

    private IEnumerator KillSequence()
    {
        isKillingPlayer = true;

        while (player != null && Vector3.Distance(transform.position, player.position) > stopDistance)
        {
            Vector3 target = player.position;
            target.y = transform.position.y;

            transform.position = Vector3.MoveTowards(transform.position, target, killRushSpeed * Time.deltaTime);
            yield return null;
        }

        yield return new WaitForSeconds(attackDelay);

        if (zombieAnim != null)
        {
            zombieAnim.ResetTrigger("Attack");
            zombieAnim.SetTrigger("Attack");
        }

        yield return new WaitForSeconds(gameOverDelay);

        if (playerAudio != null)
            playerAudio.LoadGameOver();
    }
}