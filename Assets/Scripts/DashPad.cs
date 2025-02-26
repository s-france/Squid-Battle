using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DashPad : MonoBehaviour
{
    [SerializeField] float movePower;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    void OnTriggerEnter2D(Collider2D col)
    {
        if(LayerMask.LayerToName(col.gameObject.layer) == "Players")
        {
            col.transform.parent.position = transform.position;

            col.GetComponentInParent<PlayerController>().ApplyMove(-1, -transform.up, movePower);
        }


    }

}
