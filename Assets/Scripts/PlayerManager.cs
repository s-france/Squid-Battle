using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;




public class PlayerManager : MonoBehaviour
{
    GameManager gm;
    [HideInInspector] public InputManager im;

    //REWORK!!!
    [HideInInspector] public List<PlayerConfig> playerList {get; private set;}
    
    [HideInInspector] public int colorsCount;
    int[] TakenColors;

    /*
    public Sprite[] blueSprites;
    public Sprite[] cyanSprites;
    public Sprite[] greenSprites;
    public Sprite[] orangeSprites;
    public Sprite[] pinkSprites;
    public Sprite[] purpSprites;
    public Sprite[] redSprites;
    public Sprite[] yellowSprites;
    */

    public Sprite[] playerSprites;
    public Color[] playerColors;


    //[HideInInspector] public int winnerIdx;

    [HideInInspector] public List<int> placements;
    //i.e. placements[0] = last place, placements[playercount] = first place


    // Start is called before the first frame update
    void Awake()
    {
        
        
    }

    //use this instead of Awake()
    public void Init()
    {
        gm = GetComponentInParent<GameManager>();
        im = GetComponentInChildren<InputManager>();

        playerList = new List<PlayerConfig>();
        

        /*
        //build sprite array
        
        //build sprite array
        playerSprites[0] = blueSprites;
        playerSprites[1] = cyanSprites;
        playerSprites[2] = greenSprites;
        playerSprites[3] = orangeSprites;
        playerSprites[4] = pinkSprites;
        playerSprites[5] = purpSprites;
        playerSprites[6] = redSprites;
        playerSprites[7] = yellowSprites;
        */

        colorsCount = playerColors.Length - 1;

        TakenColors = new int[colorsCount+1];
        Array.Clear(TakenColors, 0, TakenColors.Length);

        im.Init();

        placements = new List<int>();
        placements.Clear();
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    //sets player color
    public void SetPlayerColor(int idx, int color)
    {
        if(TakenColors[playerList[idx].color] > 0)
        {
            TakenColors[playerList[idx].color]--;
        }

        playerList[idx].color = color;

        TakenColors[playerList[idx].color]++;

        //updates player's UI colors
        gm.lc.SetUIColors(idx);

    }


    //finds first available colorID after/including idx
    //LeftRight = -1 -> search to the left
    //LefRight = 1 -> searchto the right
    public int FindFirstAvailableColorID(int idx, int LeftRight)
    {
        if(LeftRight != -1 && LeftRight != 1)
        {
            Debug.Log("ERROR: LeftRight input must be = -1 or 1");
            return -1;
        }
        
        int result = idx;

        if(result < 0)
        {
            result = colorsCount;
        } else if(result > colorsCount)
        {
            result = 0;
        }

        
        //loop through colors starting at idx
        while(TakenColors[result] > 0)
        {
            result += LeftRight;

            if(result < 0)
            {
                result = colorsCount;
            } else if(result > colorsCount)
            {
                result = 0;
            }
        }

        //return first "available" color
        return result;
    }


    public void ReadyPlayer(int idx)
    {
        playerList[idx].isReady = true;
        Debug.Log("player" + playerList[idx].playerIndex + " is ready: " + playerList[idx].isReady);

        //handled in CharacterSelectLC
        gm.lc.ReadyPlayer(idx);
    }


    public void UnReadyPlayer(int idx)
    {
        playerList[idx].isReady = false;

        //handled in CharacterSelectLC
        gm.lc.UnReadyPlayer(idx);
    }


    public void ReactivatePlayer(int idx)
    {
        playerList[idx].playerScript.Reactivate();
        //playerList[idx].input.ActivateInput();
        playerList[idx].isAlive = true;
        playerList[idx].playerScript.isAlive = true;
        //playerList[idx].isInBounds = true;
        playerList[idx].playerScript.isInBounds = true;

        


        Debug.Log("player" + idx + " reactivated!");
    }

    public void DeactivatePlayer(int idx)
    {
        //disable and hide gameobject
        playerList[idx].playerScript.Deactivate();
        UnReadyPlayer(idx);

        //playerList[idx].input.DeactivateInput();
        playerList[idx].isReady = false;
        playerList[idx].isAlive = false;
        playerList[idx].playerScript.isAlive = false;
        //playerList[idx].isInBounds = true;
        playerList[idx].playerScript.isInBounds = true;

        if(gm.lc.GetLevelType() == 0)
        {
            //UIController ui = GameObject.Find("MenuUI").GetComponent<UIController>();
            //ui.HidePlayerUI(idx);
        }
        


        Debug.Log("player " + idx + " deactivated!");
    }


    //kills player - ONLY CALL THIS WHEN gameStarted == true
    //idx: player dying, killCredit player that killed them
    public void KillPlayer(int idx, int killCredit)
    {
        DeactivatePlayer(idx); //sets isalive = false
        FindFirstObjectByType<AudioManager>().Play("Fall");

        playerList[idx].playerScript.Clones.Clear();

        //track player's placement
        placements.Add(idx);

        //assign points (headhunters)
        if(gm.ms.scoreFormat == 0)
        {
            if(idx != killCredit)
            {
                playerList[killCredit].score += 2;
            } else
            {
                playerList[idx].score -= 1;
                if(playerList[idx].score < 0)
                {
                    playerList[idx].score = 0;
                }
            }
        }
        
        //if only one remaining player alive end/reset the game
        if (playerList.Count(p => p.isAlive) == 1 && gm.battleStarted)
        {
            int winner = playerList.FindIndex(p => p.isAlive);
            placements.Add(winner);
            Debug.Log("winner: " + winner);


            //NEW STUFF HERE
            //open results screen + map select
            gm.lc.ShowResults();

            //gm.lc.EndLevel();
        }
    }


    //outdated ignore
    public IEnumerator PlayerKillClock(int idx, float timer)
    {
        float clock = 0;
        
        while(clock < timer && !playerList[idx].playerScript.isInBounds)
        {
            //add dying animation here
            
            
            clock += Time.deltaTime;
            yield return null;
        }

        if(!playerList[idx].playerScript.isInBounds)
        {
            KillPlayer(idx, -1);
        }

    }


    



}

public class PlayerConfig
{
    public PlayerConfig(PlayerInput pi)
    {
        input = pi;
        playerIndex = pi.playerIndex;
        playerScript = pi.GetComponentInParent<PlayerController>();
        playerManager = UnityEngine.Object.FindFirstObjectByType<PlayerManager>();
        isReady = false;
        isActive = true;
        isAlive = true;
        playerScript.isInBounds = true;
        score = 0;
        color = playerManager.FindFirstAvailableColorID(pi.playerIndex, 1);
    }

    public PlayerController playerScript {get; set;} //player prefab object
    public PlayerManager playerManager {get; set;}
    public PlayerInput input {get; set;}
    public int playerIndex {get; set;}
    public bool isReady {get; set;}
    public bool isActive {get; set;}
    public bool isAlive {get; set;}
    //public bool isInBounds {get; set;}
    public int color {get; set;} //color idx
    public int score {get; set;}
}