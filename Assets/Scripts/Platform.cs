using System.Collections;
using System.Collections.Generic;
//using System.Numerics;

//using System.Numerics;
using UnityEngine;

public class Platform : MonoBehaviour
{
    List<Collider2D> contacts;

    Vector2 prevPos;
    Vector2 diff = new Vector2(0,0);

    // Start is called before the first frame update
    void Start()
    {
        contacts = new List<Collider2D>();

        prevPos = transform.position;
    }

    // Update is called once per frame
    void Update()
    {

    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if(!contacts.Contains(col))
        {
            contacts.Add(col);
        }

    }

    void OnTriggerExit2D(Collider2D col)
    {
        contacts.Remove(col);

    }

    void FixedUpdate()
    {
        diff = (Vector2)transform.position - prevPos;

        foreach(Collider2D col in contacts)
        {
            col.transform.position += (Vector3)diff;
            
        }

        prevPos = transform.position;

    }

}
