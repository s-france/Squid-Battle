using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Laser : MonoBehaviour
{
    [SerializeField] Collider2D Col;
    [SerializeField] SpriteRenderer LaserSR;
    [SerializeField] SpriteRenderer WarningSR;

    public bool started = false; //started by LevelController
    [HideInInspector] public bool firing = false;

    [SerializeField] Vector2 maxPos;
    [SerializeField] Vector2 minPos;


    [SerializeField] float freqProgressionTime;
    [SerializeField] float startFrequency;
    [SerializeField] float endFrequency;
    float frequency;

    [SerializeField] float warningTime;
    [SerializeField] float activeTime;

    float timer = 0;

    // Start is called before the first frame update
    void Start()
    {
        frequency = startFrequency;
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void FixedUpdate()
    {
        if (started)
        {
            timer += Time.fixedDeltaTime;

            //frequency progression
            frequency = Mathf.Lerp(startFrequency, endFrequency, timer/freqProgressionTime);
            Debug.Log(frequency);

            //fire at freq rate
            if (timer % frequency <= .1f && !firing)
            {
                StartCoroutine(FireLaser());

            }


        }
    }

    Vector2 PickPos()
    {
        float x = Random.Range(minPos.x, maxPos.x);
        float y = Random.Range(minPos.y, maxPos.y);

        return new Vector2(x, y);
    }

    void RandomRotation()
    {
        
    }

    IEnumerator FireLaser()
    {
        WaitForFixedUpdate fuWait = new WaitForFixedUpdate();

        firing = true;
        float time = 0;

        //set random position + rotation
        transform.position = PickPos();
        transform.Rotate(0, 0, Random.Range(0, 360));

        //enable warning
        WarningSR.enabled = true;

        while (time <= warningTime)
        {
            time += Time.fixedDeltaTime;
            yield return fuWait;
        }

        //activate laser
        LaserSR.enabled = true;
        Col.enabled = true;

        time = 0;
        while (time <= activeTime)
        {
            time += Time.fixedDeltaTime;
            yield return fuWait;
        }

        //deactivate all
        WarningSR.enabled = false;
        LaserSR.enabled = false;
        Col.enabled = false;

        firing = false;
    }

}
