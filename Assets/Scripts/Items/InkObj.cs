using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InkObj : MonoBehaviour
{
    GameManager gm;

    Rigidbody2D rb;

    float activeTimer;
    float upTime;

    [SerializeField] float maxSize;
    [SerializeField] float maxUpTime;
    [SerializeField] float maxChargeTime;

    // Start is called before the first frame update
    void Start()
    {
        gm = GameObject.Find("GameManager").GetComponent<GameManager>();

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
        if (activeTimer >= upTime || !gm.battleStarted)
        {
            Destroy(this.gameObject);
        }
    }


    public void Deploy(float chargetime)
    {
        //TWEAK THIS
        upTime = Mathf.Clamp(chargetime * 10, 13, 25);

        float size = 1 + (maxSize * (chargetime/maxChargeTime));
        transform.localScale += new Vector3(size, size, 1);

        rb = this.GetComponent<Rigidbody2D>();
        rb.AddForce(transform.up * 10, ForceMode2D.Impulse);
    }

}
