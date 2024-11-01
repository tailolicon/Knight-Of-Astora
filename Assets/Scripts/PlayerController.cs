using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private float horizontalInput;
    private int facingDirection;

    private bool isOnGround;
    private bool isBlocking;
    private bool isRolling;
    private float rollDuration = 0.5f;
    private float rollCurrentTime;

    private int currentAttack = 0;
    private float timeSinceAttack;

    //Player attributes
    [SerializeField] private float jumpForce = 3f;
    [SerializeField] private float playerSpeed = 5;
    [SerializeField] private float rollForce = 6.5f;
    //readonly int hitPoints = 5;
    //public float stamina = 10;


    private Rigidbody2D playerRb;
    private Animator playerAnimation;

    // Start is called before the first frame update
    void Start()
    {
        playerRb = GetComponent<Rigidbody2D>();
        playerAnimation = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        PlayerMovement();
    }

    private void PlayerMovement()
    {
        // Set speed when player in the air
        playerAnimation.SetFloat("AirSpeedY", playerRb.velocity.y);

        // Increase timer that controls attack combo
        timeSinceAttack += Time.deltaTime;

        // Increase timer that checks roll duration
        if (isRolling)
            rollCurrentTime += Time.deltaTime;
        // Disable rolling if timer extends duration
        if (rollCurrentTime > rollDuration)
            isRolling = false;

        //Check if player land on ground
        if (isOnGround)
        {
            playerAnimation.SetBool("Grounded", true);
        }
        else
        {
            playerAnimation.SetBool("Grounded", false);
        }

        //Handle Input Movement
        horizontalInput = Input.GetAxis("Horizontal");

        if (horizontalInput > 0)
        {
            GetComponent<SpriteRenderer>().flipX = false;
            facingDirection = 1;
        }
        if (horizontalInput < 0)
        {
            GetComponent<SpriteRenderer>().flipX = true;
            facingDirection = -1;
        }

        // Move
        if (!isRolling && !isBlocking)
        {
            playerRb.velocity = new Vector2(horizontalInput * playerSpeed, playerRb.velocity.y);
            //transform.Translate(new Vector2(horizontalInput * playerSpeed * Time.deltaTime, 0));
        }

        // Run
        if (horizontalInput != 0)
        {
            playerAnimation.SetFloat("Speed", 1);
        }
        else
        {
            playerAnimation.SetFloat("Speed", 0);

        }

        // Jump
        if(Input.GetKeyUp(KeyCode.Space) && playerRb.velocity.y > 0)
        {
            playerRb.velocity = new Vector2(playerRb.velocity.x, 0);
        }

        if (Input.GetKeyDown(KeyCode.Space) && isOnGround)
        {

            isOnGround = false;
            playerAnimation.SetTrigger("Jump");
            playerRb.velocity = new Vector2(playerRb.velocity.x, jumpForce);
            
        }

        // Attack
        if (Input.GetMouseButtonDown(0) && timeSinceAttack > 0.35f && !isRolling)
        {
            currentAttack++;
            if (currentAttack > 3)
                currentAttack = 1;

            if (timeSinceAttack > 1f)
                currentAttack = 1;

            playerAnimation.SetTrigger("Attack" + currentAttack);

            timeSinceAttack = 0f;
        }

        // Roll
        if (Input.GetKeyDown(KeyCode.LeftShift) && isOnGround && !isRolling)
        {
            isRolling = true;
            playerAnimation.SetTrigger("Roll");
            playerRb.velocity = new Vector2(facingDirection * rollForce, playerRb.velocity.y);
            //transform.Translate(new Vector2(rollForce * Time.deltaTime * facingDirection, 0));
        }

        // Block
        if (Input.GetMouseButtonDown(1) && isOnGround && !isRolling)
        {
            isBlocking = true;
            playerRb.velocity = Vector2.zero;
            playerAnimation.SetTrigger("Block");
            playerAnimation.SetBool("IdleBlock", true);
        }
        else if (Input.GetMouseButtonUp(1))
        {
            isBlocking = false;
            playerAnimation.SetBool("IdleBlock", false);
        }
            

    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isOnGround = true;
        }
    }
}
