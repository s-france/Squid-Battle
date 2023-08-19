using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InkObj : MonoBehaviour
{
    Rigidbody2D rb;

    float activeTimer;
    float upTime;

    [SerializeField] float maxSize;
    [SerializeField] float maxUpTime;
    [SerializeField] float maxChargeTime;

    // Start is called before the first frame update
    void Start()
    {
        activeTimer = 0;
        upTime = maxUpTime;
    }

    // Update is called once per frame
    void Update()
    {
        //spinning animation
        transform.Find("Sprites").Rotate(0,0, 10 * Time.deltaTime);

        //this should be its own func / coroutine
        activeTimer += Time.deltaTime;
        if (activeTimer >= upTime)
        {
            Destroy(this.gameObject);
        }
    }


    public void Deploy(float chargetime)
    {
        //TWEAK THIS
        upTime = Mathf.Clamp(chargetime * 10, 13, 25);

        float size = Mathf.Clamp(chargetime, 0, maxChargeTime);
        transform.localScale += new Vector3(size, size, 0);

        rb = this.GetComponent<Rigidbody2D>();
        rb.AddForce(-transform.up * 20, ForceMode2D.Impulse);
    }

}
