using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private Animator anim;
    [SerializeField] private SpriteRenderer playerSprite;
    [SerializeField] private int speed;
    [SerializeField] private float jumpForce;

    [Header("Collision Info")]
    [SerializeField] private float groundCheckDistance;
    [SerializeField] private float gravityScale = 1.0f;

    public static float globalGravity = -9.81f;

    private PlayerControls playerControls;
    private Rigidbody rb;
    private Vector3 movement;

    [Header("Grass Encounters")]
    [SerializeField] private LayerMask grassLayer;
    [SerializeField] private int stepsInGrass;
    [SerializeField] private int minStepsToEncounter;
    [SerializeField] private int maxStepsToEncounter;
    private const float timePerStep = 0.5f;
    private bool movingInGrass;
    private float stepTimer;
    private int stepsToEncounter;


    private const string IS_MOVE_PARAM = "isMoving";
    private const string BATTLE_SCENE = "BattleScene";

    // Start is called before the first frame update
    private void Awake()
    {
        playerControls = new PlayerControls();
        CalculateStepsToNextEncounter();
    }
    private void OnEnable()
    {
        playerControls.Enable();
    }

    private void Start()
    {
        rb = gameObject.GetComponent<Rigidbody>();
        anim = GetComponentInChildren<Animator>();
        groundCheckDistance = GetComponent<Collider>().bounds.extents.y;
    }

    // Update is called once per frame
    void Update()
    {
        Movement();
        Jump();
        isGrounded();
        AnimatorControllers();

    }

    private void FixedUpdate()
    {
        rb.MovePosition(transform.position + movement * speed * Time.fixedDeltaTime);
        Vector3 gravity = globalGravity * gravityScale * Vector3.up;
        rb.AddForce(gravity, ForceMode.Acceleration);

        Collider[] colliders = Physics.OverlapSphere(transform.position, 1, grassLayer);
        movingInGrass = colliders.Length != 0 && movement != Vector3.zero;

        if (movingInGrass == true)
        {
            stepTimer += Time.fixedDeltaTime;
            if (stepTimer > timePerStep)
            {
                stepsInGrass ++;
                stepTimer = 0;

                if (stepsInGrass >= stepsToEncounter)
                {
                    SceneManager.LoadScene(BATTLE_SCENE);
                }

                // Check to see if we have reached an encounter
                // Transition into scene
            }
        }
    }

    private bool isGrounded()
    {
        return Physics.Raycast(transform.position, -Vector3.up, groundCheckDistance + 0.1f);
    }

    private void Jump()
    {
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded())
        {
            rb.velocity = new Vector3(rb.velocity.x, jumpForce, rb.velocity.z);
        }
    }

    private void AnimatorControllers()
    {
        anim.SetBool("isGrounded", isGrounded());
        anim.SetFloat("yVelocity", rb.velocity.y);

    }

    private void Movement()
    {
        float x = playerControls.Player.Move.ReadValue<Vector2>().x;
        float z = playerControls.Player.Move.ReadValue<Vector2>().y;

        movement = new Vector3(x, 0, z).normalized;

        anim.SetBool(IS_MOVE_PARAM, movement!=Vector3.zero);

        if (x != 0 && x < 0)
        {
            playerSprite.flipX = true;
        }
        if (x != 0 && x > 0)
        {
            playerSprite.flipX = false;
        }

    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawLine(transform.position, new Vector3(transform.position.x, transform.position.y - groundCheckDistance));
    }

    private void CalculateStepsToNextEncounter()
    {
        stepsToEncounter = Random.Range(minStepsToEncounter, maxStepsToEncounter);
    }
}
