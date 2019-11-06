using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMotionAttribute : MonoBehaviour
{
    [Header("行走速度")]
    public float WalkSpeed = 1f;

    [Header("行走加速度")]
    public float WalkAcceleration = 10f;

    [Header("奔跑速度")]
    public float RunSpeed = 5f;

    [Header("奔跑加速度")]
    public float RunAcceleration = 10f;

    [Header("重力")]
    public float Gravity = 1f;

    [Header("跳跃力")]
    public float JumpForce = 1f;

}
