using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Soccer Goal
public class Goal : MonoBehaviour
{
    [SerializeField] SoccerArenaLevelController slc; //soccerlc
    [HideInInspector] public int teamID; //team associated with this goal

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    //assigns team + sets goal to team color
    public void AssignTeam(int idx)
    {
        //set teamID
        teamID = idx;

        //Set team color
        //ADD THIS!!!!

    }

    void OnTriggerEnter2D(Collider2D col)
    {
        Debug.Log("Goal" + teamID + " collision!");
        //ball collision
        if(col.GetComponentInParent<BallPlayerController>() != null)
        {
            
            //score goal
            slc.ScoreGoal(teamID, col.GetComponentInParent<BallPlayerController>());


            //reset?
        }

        
    }
}
