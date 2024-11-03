using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [SerializeField] protected float health;
    [SerializeField] protected float recoilLength;
    [SerializeField] protected float recoilFactor;
    [SerializeField] protected bool isRecoiling = false;

    protected float recoilTimer;


    //[SerializeField] protected PlayerController player;
    //[SerializeField] protected float speed;

    protected Rigidbody2D rb;
    protected virtual void Start()
    {
        
    }

    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        //player = PlayerController.Instance;
    }
    protected virtual void Update()
    {
        if (health <= 0)
        {
            Destroy(gameObject);
        }
        if (isRecoiling)
        {
            if (recoilTimer < recoilLength)
            {
                recoilTimer += Time.deltaTime;
            }
            else
            {
                isRecoiling = false;
                recoilTimer = 0;
            }
        }
    }

    public virtual void EnemyHit(float damage, Vector2 hitDirection, float hitForce)
    {
        health -= damage;
        
        if (!isRecoiling)
        {
            rb.AddForce(hitForce * recoilFactor * hitDirection);
            isRecoiling = true;
        }
    }
}
