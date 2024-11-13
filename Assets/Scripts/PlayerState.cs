using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerState : MonoBehaviour
{
    public bool isRolling;
    public bool isDashing = false;
    public bool isJumping = false;
    public bool recoilingX = false;
    public bool lookingRight;

    public bool isHealing;

    public bool invincible = false;
}
