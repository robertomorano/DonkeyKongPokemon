using UnityEngine;

public class PlayerScrpit : MonoBehaviour
{
    private Rigidbody2D rb;
    private CapsuleCollider2D capsuleCollider;

    private readonly Collider2D[] overlaps = new Collider2D[4];
    private Vector2 direction;

    private bool isGrounded;
    private bool isClimbing;

    public float moveSpeed = 3f;
    public float jumpForce = 4f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        capsuleCollider = GetComponent<CapsuleCollider2D>();
    }

    // Update is called once per frame
    void Update()
    {
        CheckCollision();
    }

    private void CheckCollision()
    {
        isGrounded = false;
        isClimbing = false;

        float skinWidth = 0.1f;

        Vector2 size = capsuleCollider.bounds.size;
        size.y += skinWidth;
        size.x /= 2f;

        int amount = Physics2D.OverlapBoxNonAlloc(transform.position, size, 0f, overlaps);

        for (int i = 0; i < amount; i++)
        {
            GameObject hit = overlaps[i].gameObject;

            if (hit.layer == LayerMask.NameToLayer("Ground"))
            {
                isGrounded = hit.transform.position.y < (transform.position.y - 0.5f + skinWidth);

                Physics2D.IgnoreCollision(overlaps[i], capsuleCollider, !isGrounded);
            }
            else if (hit.layer == LayerMask.NameToLayer("Ladder"))
            {
                isClimbing = true;
            }
        }
    }
    private void setDirection()
    {
        if (isClimbing)
        {
            direction.y = Input.GetAxisRaw("Vertical") * moveSpeed;
        } else if (isGrounded && Input.GetButtonDown("Jump"))
        {
            
                direction = Vector2.up * jumpForce;
            }
        else
            {
                direction += Physics2D.gravity * Time.deltaTime;
            }

    }
}
