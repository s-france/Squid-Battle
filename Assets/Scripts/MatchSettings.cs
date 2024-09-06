using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MatchSettings : MonoBehaviour
{
    public int pointsToWin;
    public bool[] ActiveItems;

    int roundsPlayed;


    // Start is called before the first frame update
    void Start()
    {
        //load default settings - this is editable in inspector
        /*
        pointsToWin = 10;

        ItemsProbabilities[0] = .9f; //shot
        ItemsProbabilities[1] = .9f; //ink
        ItemsProbabilities[2] = .9f; //wall
        ItemsProbabilities[3] = .9f; //warp
        ItemsProbabilities[4] = .9f; //rewind
        ItemsProbabilities[5] = .1f; //grow
        */
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
