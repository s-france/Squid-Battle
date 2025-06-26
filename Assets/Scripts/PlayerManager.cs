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
    [HideInInspector] public List<PlayerConfig> PlayerList {get; private set;}
    
    [HideInInspector] public int colorsCount;
    int[] TakenColors;

    public  List<Team> TeamList;

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
    public Color[] teamColors;


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

        PlayerList = new List<PlayerConfig>();

        //init teams list
        TeamList = new List<Team>(); 
        for (int i = 0; i <6; i++) //max number of teams=6
        {
            TeamList.Add(new Team(i, i));
        }


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
        if(TakenColors[PlayerList[idx].color] > 0)
        {
            TakenColors[PlayerList[idx].color]--;
        }

        PlayerList[idx].color = color;

        TakenColors[PlayerList[idx].color]++;

        //updates player's UI colors
        gm.lc.SetUIColors(idx);


    }


    //adds player to team
    public void SetPlayerTeam(int playerIdx, int teamIdx)
    {
        //exit if player is already on this team
        if(PlayerList[playerIdx].team == teamIdx)
        {
            return;
        }


        if(PlayerList[playerIdx].team != -1)
        {
            //remove player from current team
            Debug.Log("P" + playerIdx + " left team" + TeamList[PlayerList[playerIdx].team].idx);
            TeamList[PlayerList[playerIdx].team].Players.Remove(PlayerList[playerIdx].playerScript);
        }
        
        //add to new team
        PlayerList[playerIdx].team = teamIdx;
        TeamList[teamIdx].Players.Add(PlayerList[playerIdx].playerScript);

        Debug.Log("P" + playerIdx + " joined team " + teamIdx);

        //update UI colors
        gm.lc.SetUIColors(playerIdx);

        //update team colors
        PlayerList[playerIdx].playerScript.ChangeColor(PlayerList[playerIdx].playerScript.colorID);
    }

    public void SetPlayerTeamDefault(int playerIdx)
    {
        //exit if player already has a team
        if(PlayerList[playerIdx].team != -1)
        {
            return;
        }

        //find best team to add
        int teamCount = 0;
        foreach(Team t in TeamList)
        {
            Debug.Log("Team" + t.idx + " Count: " + t.Players.Count);

            if(t.Players.Count > 0)
            {
                teamCount++;
            }
        }

        Debug.Log("TeamCount: " + teamCount);

        //join new team
        if(teamCount <= 1)
        {
            foreach(Team t in TeamList)
            {
                if(t.Players.Count == 0)
                {
                    Debug.Log("P" + playerIdx + " joining new team" + t.idx);
                    SetPlayerTeam(playerIdx, t.idx);
                    break;
                }
            }
        } else //join smallest team
        {
            Team smallest = TeamList[0];
            //find first non-empty team
            foreach(Team t in TeamList)
            {
                if(t.Players.Count > 0)
                {
                    smallest = t;
                    break;
                }
            }

            //find smallest team
            foreach(Team t in TeamList)
            {
                if(t.Players.Count > 0 && t.Players.Count < smallest.Players.Count)
                {
                    smallest = t;
                }
            }
            //join smallest
            Debug.Log("P" + playerIdx + " joining smallest team" + smallest.idx);
            SetPlayerTeam(playerIdx, smallest.idx);
        }
    }

    //not doing this anymore do not use
    public void SetTeamColor(int teamIdx, int color)
    {
        if(TakenColors[TeamList[teamIdx].color] > 0)
        {
            TakenColors[TeamList[teamIdx].color]--;
        }

        TeamList[teamIdx].color = color;

        TakenColors[TeamList[teamIdx].color]++;

        //set all team players' colors
        foreach (PlayerController pc in TeamList[teamIdx].Players)
        {
            SetPlayerColor(pc.idx, color);
        }
    }


    //finds first available colorID after/including idx
    //LeftRight = -1 -> search to the left
    //LefRight = 1 -> searchto the right
    public int FindFirstAvailableColorID(int idx, int leftRight)
    {
        if(leftRight != -1 && leftRight != 1)
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
            result += leftRight;

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

    //finds first available team ID after/including teamIdx
    //LeftRight = -1 -> search to the left
    //LefRight = 1 -> searchto the right
    public int FindFirstAvailableTeam(int teamIdx, int leftRight)
    {
        if(leftRight != -1 && leftRight != 1)
        {
            Debug.Log("ERROR: leftRight input must be = -1 or 1");
            return -1;
        }


        //team to avoid joining
        //only used in case where only 2 teams
        int avoidTeam = -1;

        //count teams
        int teamsCount = 0;
        foreach(Team t in TeamList)
        {
            //this is a non-empty team
            if(t.Players.Count > 0)
            {
                //count teams
                teamsCount++;
            }
        }

        if(teamsCount <=2)
        {
            foreach(Team t in TeamList)
            {
                //this is a non-empty team
                if(t.Players.Count > 0)
                {
                    //avoid joining other team if it would be the only team
                    if(teamsCount <= 2 && TeamList[teamIdx-leftRight].Players.Count <= 1 && t.idx != (teamIdx - leftRight))
                    {
                        avoidTeam = t.idx;
                    } else if(t.idx != (teamIdx - leftRight))
                    {
                        avoidTeam = -1;
                    }
                }
            }
        }
        

        Debug.Log("avoidTeam: " + avoidTeam);


        int result = teamIdx;

        //regular team battle use all 6 teams
        if(gm.gameMode == 1)
        {
            //wrap team search at 6 team IDs
            if(result < 0)
            {
                result = 5;
            } else if(result > 5)
            {
                result = 0;
            }

            //loop through teams starting at idx
            //until valid team is found 
            while(TeamList[result].idx == avoidTeam)
            {
                result += leftRight;

                //wrap around
                if(result < 0)
                {
                    result = 5;
                } else if(result > 5)
                {
                    result = 0;
                }
            }

        //soccer use only 2 teams
        } else if(gm.gameMode == 2)
        {
            //wrap team search at 6 team IDs
            if(result < 0)
            {
                result = 1;
            } else if(result > 1)
            {
                result = 0;
            }

            //loop through teams starting at idx
            //until valid team is found 
            while(TeamList[result].idx == avoidTeam)
            {
                result += leftRight;

                //wrap around
                if(result < 0)
                {
                    result = 1;
                } else if(result > 1)
                {
                    result = 0;
                }
            }
        }

        

        //return idx of next valid team
        return result;
    }


    public void ReadyPlayer(int idx)
    {
        PlayerList[idx].isReady = true;
        Debug.Log("player" + PlayerList[idx].playerIndex + " is ready: " + PlayerList[idx].isReady);

        //handled in CharacterSelectLC
        gm.lc.ReadyPlayer(idx);
    }


    public void UnReadyPlayer(int idx)
    {
        PlayerList[idx].isReady = false;

        //handled in CharacterSelectLC
        gm.lc.UnReadyPlayer(idx);
    }


    public void ReactivatePlayer(int idx)
    {
        PlayerList[idx].playerScript.Reactivate();
        //playerList[idx].input.ActivateInput();
        PlayerList[idx].isAlive = true;
        PlayerList[idx].playerScript.isAlive = true;
        //playerList[idx].isInBounds = true;
        PlayerList[idx].playerScript.isInBounds = true;

        


        Debug.Log("player" + idx + " reactivated!");
    }

    public void DeactivatePlayer(int idx)
    {
        //disable and hide gameobject
        PlayerList[idx].playerScript.Deactivate();
        UnReadyPlayer(idx);

        //playerList[idx].input.DeactivateInput();
        PlayerList[idx].isReady = false;
        PlayerList[idx].isAlive = false;
        PlayerList[idx].playerScript.isAlive = false;
        //playerList[idx].isInBounds = true;
        PlayerList[idx].playerScript.isInBounds = true;

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

        PlayerList[idx].playerScript.Clones.Clear();

        //track player's placement
        //Teams
        if(gm.gameMode == 1)
        {
            //if all team members are dead -> add to placements
            if(TeamList[PlayerList[idx].team].Players.Count(p => p.isAlive)<=0)
            {
                placements.Add(PlayerList[idx].team);
            }

        } else //FFA
        {
            placements.Add(idx);
        }

        //assign points (headhunters)
        if(gm.ms.scoreFormat == 0)
        {
            //killed by another player
            if(idx != killCredit)
            {
                //teams
                if(gm.gameMode ==1)
                {
                    //check for team kill
                    if(PlayerList[killCredit].team != PlayerList[idx].team)
                    {
                        PlayerList[killCredit].score += 2;
                        TeamList[PlayerList[killCredit].team].score += 2;
                    } else //punish team kills
                    {
                        //EDIT: Nevermind! -too harsh for default -should be toggleable
                        /*
                        TeamList[PlayerList[killCredit].team].score -= 1;
                        if(PlayerList[idx].score < 0)
                        {
                            PlayerList[idx].score = 0;
                        }
                        
                        if(TeamList[PlayerList[killCredit].team].score < 0)
                        {
                            TeamList[PlayerList[killCredit].team].score = 0;
                        }
                        */
                    }


                } else //FFA
                {
                    PlayerList[killCredit].score += 2;

                }
                
                
            } else //self destruct
            {
                //EDIT: not punishing SD by default -make it an option
                /*
                PlayerList[idx].score -= 1;
                if(PlayerList[idx].score < 0)
                {
                    PlayerList[idx].score = 0;
                }
                */

                /* should we really punish SD in team battle?
                //team scoring
                if(gm.gameMode == 1)
                {
                    Teams[playerList[killCredit].team].score -= 1;

                    if(Teams[playerList[killCredit].team].score < 0)
                    {
                        Teams[playerList[killCredit].team].score = 0;
                    }
                }
                */

            }
        }
        
        //check for endofgame conditions
        if(gm.gameMode == 1)
        {
            //if only one remaining team alive end/reset the game
            if (TeamList.Count(t => (t.Players.Count>0 && t.Players.Count(p => p.isAlive)>0)) == 1 && gm.battleStarted)
            {
                int winner = PlayerList[PlayerList.FindIndex(p => p.isAlive)].team;
                placements.Add(winner);
                Debug.Log("winner: " + winner);


                //open results screen + map select
                gm.lc.ShowTeamResults();

                //gm.lc.EndLevel();
            }

        } else //FFA
        {
            //if only one remaining player alive end/reset the game
            if (PlayerList.Count(p => p.isAlive) == 1 && gm.battleStarted)
            {
                int winner = PlayerList.FindIndex(p => p.isAlive);
                placements.Add(winner);
                Debug.Log("winner: " + winner);


                //open results screen + map select
                //(assigns placement points also)
                gm.lc.ShowResults();

                //gm.lc.EndLevel();
            }

        }
        
    }


    //outdated ignore
    public IEnumerator PlayerKillClock(int idx, float timer)
    {
        float clock = 0;
        
        while(clock < timer && !PlayerList[idx].playerScript.isInBounds)
        {
            //add dying animation here
            
            
            clock += Time.deltaTime;
            yield return null;
        }

        if(!PlayerList[idx].playerScript.isInBounds)
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
        team = -1;
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

    public int team {get; set;} //team idx
    public int color {get; set;} //color idx
    public int score {get; set;}
}

public class Team
{
    public Team(int teamID, int teamColor)
    {
        idx = teamID;
        color = teamColor;
        Players = new List<PlayerController>();
        score = 0;
    }

    public int idx {get; set;} //team idx
    public int captain {get; set;} //team captain
    public int color {get; set;} //team color
    public int score {get; set;} //score
    public List<PlayerController> Players {get; set;} //team members

}