using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public static PlayerController Instance;
    
    [Header("Player state")]
    public PlayerState playerState;
    //Input
    private float horizontalInput;
    [SerializeField] private float playerSpeed;

    [Header("Roll")]
    //Rolling
    private float rollDuration = 0.5f;
    private float rollCurrentTime;
    [SerializeField] private float rollForce;

    [Header("Dash")]
    //Dashing
    private bool canDash = true;
    private bool dashed;
    private float gravity;
    [SerializeField] private float dashSpeed;
    [SerializeField] private float dashTime;
    [SerializeField] private float dashCooldown;

    [Header("Attack")]
    //Attacking
    private int currentAttack = 0;
    private float timeSinceAttack;
    [SerializeField] private float damage = 1f;
    [SerializeField] Transform SideAttackTransform;
    [SerializeField] Vector2 SideAttackArea;
    [SerializeField] LayerMask attackableLayer;

    [Header("Jump")]
    //Double Jump
    [SerializeField] private float jumpForce;
    private int airJumpCounter = 0;
    [SerializeField] private int maxAirJump;

    [Header("Recoil")]
    //Recoil
    [SerializeField] int recoilXStep = 5;
    [SerializeField] float recoilXSpeed = 100;
    int stepsXRecoiled;

    [Header("Heal")]
    //Health
    public int health;
    public int maxHealth;
    private float healTimer;
    [SerializeField] float timeToHeal;
    public HealController healController;

    [Header("Mana")]
    //Mana
    [SerializeField] float mana;
    [SerializeField] float manaDrainSpeed;
    [SerializeField] float manaGain;
    public ManaController manaController;

    private Rigidbody2D playerRb;
    private Animator playerAnimation;
    private BoxCollider2D playerBC;

    void Start()
    {
        playerRb = GetComponent<Rigidbody2D>();
        playerAnimation = GetComponent<Animator>();
        playerBC = GetComponent<BoxCollider2D>();

        gravity = playerRb.gravityScale;

        Health = maxHealth;
        healController.SetMaxHealth(Health);

        Mana = mana;
        manaController.SetMaxMana(Mana);
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    void Update()
    {
        // Set speed when player in the air
        playerAnimation.SetFloat("AirSpeedY", playerRb.velocity.y);

        // Increase timer that controls attack combo
        timeSinceAttack += Time.deltaTime;

        // Increase timer that checks roll duration
        if (playerState.isRolling)
            rollCurrentTime += Time.deltaTime;
        // Disable rolling if timer extends duration
        if (rollCurrentTime > rollDuration)
        {
            playerState.isRolling = false;
            rollCurrentTime = 0f;
        }
        //Check if player land on ground
        Grounded();
        horizontalInput = Input.GetAxis("Horizontal");
        Flip();
        Move();
        Jump();
        Attack();
        Roll();
        StartDash();
        Heal();
    }
    private void FixedUpdate()
    {
        Recoil();
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(SideAttackTransform.position, SideAttackArea);
    }
    private void Grounded()
    {
        if (playerState.isOnGround)
        {
            playerAnimation.SetBool("Grounded", true);
            airJumpCounter = 0;
            dashed = false;
            playerState.isJumping = false;
        }
        else
        {
            playerAnimation.SetBool("Grounded", false);
        }
    }
    private void Flip()
    {
        if (horizontalInput > 0)
        {
            transform.localScale = new Vector2(1, transform.localScale.y); 
            playerState.lookingRight = true;
        }
        else if (horizontalInput < 0)
        {
            transform.localScale = new Vector2(-1, transform.localScale.y);
            playerState.lookingRight = false;
        }
    }
    private void Move()
    {
        if (!playerState.isRolling && !playerState.isDashing && !playerState.isHealing)
        {
            playerRb.velocity = new Vector2(horizontalInput * playerSpeed, playerRb.velocity.y);    
        }

        if (horizontalInput != 0)
        {
            playerAnimation.SetFloat("Speed", 1);
        }
        else
        {
            playerAnimation.SetFloat("Speed", 0);

        }
    }
    private void Jump()
    {
        if (Input.GetKeyUp(KeyCode.Space) && playerRb.velocity.y > 0)
        {
            playerAnimation.SetTrigger("Jump");
            playerRb.velocity = new Vector2(playerRb.velocity.x, 0);
        }

        if (Input.GetKeyDown(KeyCode.Space) && playerState.isOnGround && !playerState.isRolling && !playerState.isHealing)
        {
            playerState.isJumping = true;
            playerState.isOnGround = false;
            playerAnimation.SetTrigger("Jump");
            playerRb.velocity = new Vector2(playerRb.velocity.x, jumpForce);
        }
        else if (!playerState.isOnGround && airJumpCounter < maxAirJump && Input.GetKeyDown(KeyCode.Space))
        {
            airJumpCounter++;
            playerAnimation.SetTrigger("Jump");
            playerRb.velocity = new Vector2(playerRb.velocity.x, jumpForce);
        }
    }
    void Attack()
    {
        if (Input.GetMouseButtonDown(0) && timeSinceAttack > 0.5f && !playerState.isRolling && !playerState.isHealing)
        {
            currentAttack++;
            if (currentAttack > 3)
                currentAttack = 1;

            if (timeSinceAttack > 1f)
                currentAttack = 1;

            playerAnimation.SetTrigger("Attack" + currentAttack);
            timeSinceAttack = 0f;

            Hit(SideAttackTransform, SideAttackArea, ref playerState.recoilingX, recoilXSpeed);
        }
    }
    void Hit(Transform attackTransform, Vector2 attackArea, ref bool recoilDir, float recoilStrength)
    {
        Collider2D[] objectsToHit = Physics2D.OverlapBoxAll(attackTransform.position, attackArea, 0, attackableLayer);
        if (objectsToHit.Length > 0)
        {
            recoilDir = true;
        }
        for (int i = 0; i < objectsToHit.Length; i++)
        {
            if (objectsToHit[i].GetComponent<Enemy>() != null)
            {
                objectsToHit[i].GetComponent<Enemy>().EnemyHit(damage, 
                    (-transform.position + objectsToHit[i].transform.position).normalized, 
                    recoilStrength);
                if (objectsToHit[i].CompareTag("Enemy"))
                {
                    Mana += manaGain;
                    manaController.SetMana(Mana);
                }
            } 
            
        }
    }
    void Recoil()
    {
        if (playerState.recoilingX)
        {
            if (playerState.lookingRight)
            {
                playerRb.velocity = new Vector2(-recoilXSpeed, 0);
            }
            else playerRb.velocity = new Vector2(recoilXSpeed, 0);
        }

        //Stop Recoil
        if (playerState.recoilingX && stepsXRecoiled < recoilXStep)
            stepsXRecoiled++;
        else StopRecoilX();
    }
    void StopRecoilX()
    {
        stepsXRecoiled = 0;
        playerState.recoilingX = false;
    }
    public void TakeDamage(float damage)
    {
        Health -= Mathf.RoundToInt(damage);
        healController.SetHealth(Health);
        StartCoroutine(StopTakingDamage());
    }
    IEnumerator StopTakingDamage()
    {
        playerState.invincible = true;
        playerAnimation.SetTrigger("Hurt");
        yield return new WaitForSeconds(1f);
        playerState.invincible = false;
    }
    public int Health
    {
        get { return health; }
        set
        {
            if (health != value)
            {
                health = Mathf.Clamp(value, 0, maxHealth);
            }
        }
    }
    IEnumerator IFrame()
    {
        playerState.invincible = true;
        yield return new WaitForSeconds(rollDuration);
        playerState.invincible = false;
    }
    void Heal()
    {
        if (Input.GetMouseButton(1) && Health < maxHealth && !playerState.isRolling && Mana > 0 && !playerState.isDashing && !playerState.isJumping)
        {
            playerRb.velocity = Vector3.zero;
            playerState.isHealing = true;
            healTimer += Time.deltaTime;
            playerAnimation.SetBool("Heal", true);
            
            if (healTimer >= timeToHeal)
            {
                Health++;
                healController.SetHealth(Health);
                healTimer = 0;
            }

            Mana -= Time.deltaTime * manaDrainSpeed;
            manaController.SetMana(Mana);
        }
        else
        {
            playerState.isHealing = false;
            playerAnimation.SetBool("Heal", false);
            healTimer = 0;
        }
    }
    float Mana
    {
        get { return mana; }
        set
        {
            if (mana != value)
            {
                mana = Mathf.Clamp(value, 0, 1);
            }
        }
    }
    private void Roll()
    {
        if (Input.GetKeyDown(KeyCode.LeftShift) && Mana >= 0.3f && playerState.isOnGround && !playerState.isRolling && !playerState.isDashing && !playerState.isHealing)
        {
            playerState.isRolling = true;
            playerAnimation.SetTrigger("Roll");
            Mana -= 0.3f;
            manaController.SetMana(Mana);
            StartCoroutine(IFrame());
            playerRb.velocity = new Vector2(rollForce * transform.localScale.x, 0);
        }
    }
    private void StartDash()
    {
        if (Input.GetKeyDown(KeyCode.LeftControl) && canDash && !dashed && !playerState.isRolling && !playerState.isHealing)
        {
            StartCoroutine(Dash());
            dashed = true;
        }
    }
    IEnumerator Dash()
    {
        canDash = false;
        playerState.isDashing = true;
        playerAnimation.SetTrigger("Dash");
        playerRb.gravityScale = 0;
        playerRb.velocity = new Vector2(transform.localScale.x * dashSpeed, 0);
        yield return new WaitForSeconds(dashTime);
        playerRb.gravityScale = gravity;
        playerState.isDashing = false;
        yield return new WaitForSeconds(dashCooldown);
        canDash = true;
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            playerState.isOnGround = true;
        }
    }
}
