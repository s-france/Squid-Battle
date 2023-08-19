using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEditor;

public class ShotObj : MonoBehaviour
{
    float activeTimer;
    Rigidbody2D rb;
    [SerializeField] float shotStrength;
    [SerializeField] float maxChargeTime;
    // Start is called before the first frame update
    void Start()
    {
        rb = this.GetComponent<Rigidbody2D>();
        activeTimer = 0;
    }

    // Update is called once per frame
    void Update()
    {
        activeTimer += Time.deltaTime;
    }

    public void Shoot(float chargeTime)
    {
        rb = this.GetComponent<Rigidbody2D>();
        rb.AddForce(-transform.up * shotStrength * Math.Clamp(chargeTime, .5f, maxChargeTime), ForceMode2D.Impulse);
    }
}
