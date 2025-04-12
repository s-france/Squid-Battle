using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KillPlanes : MonoBehaviour
{
    [SerializeField] float progressionTime;
    [SerializeField] Transform leftPlane;
    [SerializeField] Transform rightPlane;
    float startPos; //starting x position for killplanes
    float endPos; //end x position

    Vector3 leftPos;
    Vector3 rightPos;

    float timer = 0;

    // Start is called before the first frame update
    void Start()
    {
        startPos = Mathf.Abs(leftPlane.localPosition.x);

        endPos = Mathf.Abs(leftPlane.localScale.x/2);

        leftPos = leftPlane.localPosition;
        rightPos = rightPlane.localPosition;
        leftPos.x = -startPos;
        rightPos.x = startPos;

        leftPlane.localPosition = leftPos;
        rightPlane.localPosition = rightPos;

        //randomize rotation
        float rotation = Random.Range(0f,360f);
        transform.Rotate(0,0,rotation);
    }

    void FixedUpdate()
    {
        timer+= Time.fixedDeltaTime;

        //position progression
        leftPos.x = Mathf.Lerp(-startPos, -endPos, timer/progressionTime);
        rightPos.x = Mathf.Lerp(startPos, endPos, timer/progressionTime);

        leftPlane.localPosition = leftPos;
        rightPlane.localPosition = rightPos;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
