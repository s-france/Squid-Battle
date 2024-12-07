using System.Collections;
using System.Collections.Generic;
//using System.Numerics;

//using System.Numerics;
using UnityEngine;
using UnityEngine.Splines;

public class Platform : MonoBehaviour
{
    bool started = false;
    List<Collider2D> contacts;

    Vector2 prevPos;
    Vector2 diff = Vector2.zero;

    // Start is called before the first frame update
    void Start()
    {
        contacts = new List<Collider2D>();


        prevPos = transform.position;

        StartCoroutine(DelayStart());
    }

    IEnumerator DelayStart()
    {
        int delay = 0;
        WaitForFixedUpdate fuWait = new WaitForFixedUpdate();
        
        while(delay < 4)
        {
            delay++;
            yield return fuWait;
        }

        started = true;
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

        if(started)
        {
            foreach(Collider2D col in contacts)
            {
                col.transform.position += (Vector3)diff;
                
            }
        }

        prevPos = transform.position;
        
    }

}
