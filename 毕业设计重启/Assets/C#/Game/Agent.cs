using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Agent : MonoBehaviour
{
    [Header("运动参数")]
    public float maxSpeed = 2f;
    public float maxForce = 20f;
    public float mass = 0.5f;

    [Header("属性")]
    public int ap = 10;
    public int Hp = 100;

    [Header("当前状态")]
    public Vector3 velocity = Vector3.zero;
    public Vector3 acceleration = Vector3.zero;
}
