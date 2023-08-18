using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class ShotObj : MonoBehaviour
{
    Rigidbody2D rb;
    [SerializeField] float shotStrength;
    [SerializeField] float maxChargeTime;
    // Start is called before the first frame update
    void Start()
    {
        rb = this.GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Shoot(float chargeTime)
    {
        rb = this.GetComponent<Rigidbody2D>();
        rb.AddForce(-transform.up * shotStrength * Math.Clamp(chargeTime, .5f, maxChargeTime), ForceMode2D.Impulse);
    }
}
