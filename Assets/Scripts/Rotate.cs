using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

//rotates an object over time
public class Rotate : MonoBehaviour
{
    [SerializeField] float speed;
    [SerializeField] int direction; //1 = clockwise, -1 = counterclockwise
    Vector3 rotation;

    // Start is called before the first frame update
    void Start()
    {
        if(direction == 1)
        {
            rotation = new Vector3(0,0, speed * Time.fixedDeltaTime);
        } else if(direction == -1)
        {
            rotation = new Vector3(0,0, -speed * Time.fixedDeltaTime);
        }
        
    }

    // Update is called once per frame
    void Update()
    {

        
    }

    void FixedUpdate()
    {
        transform.Rotate(rotation);
    }
}
