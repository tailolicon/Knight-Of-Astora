using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Skeleton : Enemy
{
    protected override void Start()
    {
        rb.gravityScale = 12f;
    }
    protected override void Awake()
    {
        base.Awake();
    }
    protected override void Update()
    {
        
    }

    /*public override void EnemyHit(float damage)
    {
        base.EnemyHit(damage);
    }*/
}
