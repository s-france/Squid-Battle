using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class ItemFollow : MonoBehaviour
{
    [SerializeField] Transform target;
    [SerializeField] float followSpeed;
    [SerializeField] float accelRate;

    // Start is called before the first frame update
    void Start()
    {
        if(transform.parent != null)
        {
            transform.SetParent(null);
        }
        
    }

    // Update is called once per frame
    void Update()
    {
        float acceleration = accelRate * (transform.position - target.position).magnitude;


        transform.position = Vector2.MoveTowards(transform.position, target.position, followSpeed * acceleration * Time.deltaTime);
    }


}
