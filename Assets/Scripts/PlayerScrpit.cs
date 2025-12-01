using UnityEngine;
using System;

public class Player : MonoBehaviour
{
    private float horizontal;
    private float vertical;

    [Header("Movimiento")]
    [SerializeField] private float moveSpeed = 6f;
    [SerializeField] private float jumpStrength = 8f;

    private Vector2 direction;
    private bool grounded;
    private bool climbing;

    private Rigidbody2D rb;
    private CapsuleCollider2D capsuleCollider;
    private Animator animator;

    private int Vida = 3;
    private readonly Collider2D[] overlapHits = new Collider2D[4];

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        capsuleCollider = GetComponent<CapsuleCollider2D>();
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        LeerInput();
        DetectarColisiones();
        MoverJugador();
        ActualizarAnimator();
    }

    void FixedUpdate()
    {
        rb.MovePosition(rb.position + direction * Time.fixedDeltaTime);
    }

    private void LeerInput()
    {
        horizontal = Input.GetAxis("Horizontal");
        vertical = Input.GetAxis("Vertical");
    }

    private void DetectarColisiones()
    {
        grounded = false;
        climbing = false;

        float skinWidth = 0.1f;

        Vector2 size = capsuleCollider.bounds.size;
        size.y += skinWidth;
        size.x /= 2f;

        int cantidad = Physics2D.OverlapBoxNonAlloc(transform.position, size, 0f, overlapHits);

        for (int i = 0; i < cantidad; i++)
        {
            GameObject hit = overlapHits[i].gameObject;

            if (hit.layer == LayerMask.NameToLayer("Ground"))
            {
                grounded = hit.transform.position.y < transform.position.y - 0.45f;
                Physics2D.IgnoreCollision(overlapHits[i], capsuleCollider, !grounded);
            }

            if (hit.layer == LayerMask.NameToLayer("Ladder"))
            {
                climbing = true;
            }
        }
    }

    private void MoverJugador()
    {
        // Movimiento horizontal
        direction.x = horizontal * moveSpeed;

        if (climbing)
        {
            direction.y = vertical * moveSpeed;
        }
        else if (grounded && Input.GetButtonDown("Jump"))
        {
            direction = Vector2.up * jumpStrength;
        }
        else
        {
            direction += Physics2D.gravity * Time.deltaTime;
        }

        // Limitar caída al tocar el suelo
        if (grounded && direction.y < -1f)
            direction.y = -1f;

        // Girar sprite
        if (horizontal > 0f)
            transform.localScale = new Vector3(1, 1, 1);
        else if (horizontal < 0f)
            transform.localScale = new Vector3(-1, 1, 1);
    }

    private void ActualizarAnimator()
    {
        animator.SetBool("Running", horizontal != 0f);
        animator.SetBool("Climbing", climbing);
        animator.SetBool("jumping", !grounded && !climbing);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Obstacle"))
        {
            Vida--;
            
            if (Vida <= 0)
            {
                Debug.Log("Jugador ha muerto");
                
            }
        }
    }
}
