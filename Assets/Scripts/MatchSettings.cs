using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MatchSettings : MonoBehaviour
{
    public int pointsToWin; //total points to win
    public bool[] ActiveItems; //toggled items
    [SerializeField] int[] DefaultItemProbs; //default Item Probability values
    [HideInInspector] public int[] ItemProbs; //Item Probabilities used in-game
    public int[] MapPool; //tracks map votes

    [HideInInspector] public int roundsPlayed; //number of rounds played


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

        roundsPlayed = 1;

        ItemProbs = new int[DefaultItemProbs.Length];

        //load default item probs
        int idx = 0;
        foreach(int p in DefaultItemProbs)
        {
            ItemProbs[idx] = p;
            idx++;
        }
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void VoteMap(int mapID)
    {
        MapPool[mapID]++;
    }

    public void UnVoteMap(int mapID)
    {
        if(MapPool[mapID] > 0)
        {
            MapPool[mapID]--;
        }
    }

    public int PickMap()
    {
        //store all votes
        int votes = 0;
        foreach (int m in MapPool)
        {
            votes += m;
        }


        //pick random value in votes
        float r = Random.value * votes;
        float bottom = 0;
        float top = 0;

        int mapID = 0;

        //find which map corresponds to r value
        foreach (int m in MapPool)
        {
            top += m;

            if(bottom <= r && r <= top)
            {
                return mapID;
            }
            
            mapID += 1;
            bottom += m;
        }

        return mapID;
        
    }

    //enables/disables item
    public void ToggleItem(int itemID)
    {
        ActiveItems[itemID] = !ActiveItems[itemID];

        if (ActiveItems[itemID] == false)
        {
            ItemProbs[itemID] = 0;
        } else //ADD THIS: functionality for saving player-set probabilities...
        {
            ItemProbs[itemID] = DefaultItemProbs[itemID];
        }
    }

}
