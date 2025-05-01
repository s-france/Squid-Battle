using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class SoccerArenaLevelController : ArenaLevelController
{
    public List<Goal> Goals;

    // Start is called before the first frame update
    void Start()
    {
        int goalID = 0;

        //assign goal team IDs
        foreach (Team t in pm.TeamList)
        {
            //real team w/ players
            if(t.Players.Count > 0)
            {
                if(goalID >=2)
                {
                    Debug.Log("ERROR: too many teams for soccer! (>2)");
                }

                Goals[goalID].AssignTeam(t.idx);
                
                goalID++;
            }

        }
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    //processes goal scoring

    public void ScoreGoal(int goalTeamID, BallPlayerController ball)
    {
        //add score to otherTeam
        ///find team opposite from goalTeamID
        
        foreach( Team t in pm.TeamList)
        {
            //real team with players //not this goal's team i.e. other team
            if(t.Players.Count > 0 && t.idx != goalTeamID)
            {
                Debug.Log("Team" + t.idx + " scored on Team" + goalTeamID + "!");

                //award points
                t.score += 1;
                break;
            }
            
        }

        //avoid using conventional placements
        pm.placements.Clear();

        //reset round ////(if last ball??)
        ShowTeamResults();


    }


    public override void StartLevel()
    {
        FindFirstObjectByType<AudioManager>().Init();
        mv.InitMapVoteUI();

        Debug.Log("Starting LEvel arena");
        if(pm == null)
        {
            Debug.Log("PlayerManager NOT FOUND!!");
        }

        //move players to spawnpoints
        //enable player control
        //begin item spawns
        //battleStarted = true;

        Debug.Log("playing music!!!");
        //play music
        FindFirstObjectByType<AudioManager>().PlayRandom("BattleTheme");

        //move players to spawn positions
        foreach (PlayerConfig p in pm.PlayerList.Where(p => p.isActive))
        {
            //set ArenaLC
            p.playerScript.alc = this;

            pm.DeactivatePlayer(p.playerIndex);
            pm.ReactivatePlayer(p.playerIndex);

            //clear dummy list
            p.playerScript.Clones.Clear();

            //enable Player actions for gameplay instances
            p.input.SwitchCurrentActionMap("Player");

            Transform start = GetSpawnPoints()[p.playerIndex];
            if(p.team == 0)
            {
                start.position = SpawnPoints[p.playerIndex].position;
            } else if(p.team == 1)
            {
                start.position = SpawnPoints[6 + p.playerIndex].position;
            }
            
            p.input.gameObject.transform.position = start.position;

            p.isAlive = true;

            //prepare post-game UI:
            //assign players to UI input modules
            UIInputModules[p.playerIndex].gameObject.SetActive(true);
            UIInputModules[p.playerIndex].actionsAsset = pm.PlayerList[pm.PlayerList.FindIndex(player => player.playerIndex == p.playerIndex)].input.actions;
            pm.PlayerList[pm.PlayerList.FindIndex(player => player.playerIndex == p.playerIndex)].input.uiInputModule = UIInputModules[p.playerIndex];

            //set player UI colors
            SetUIColors(p.playerIndex);
        }
        
        
        //set team colors
        if(gm.gameMode ==1 || gm.gameMode ==2)
        {
            foreach (Team t in pm.TeamList)
            {
                if(t.Players.Count > 0)
                {
                    sb.SetColor(t.idx);
                }
            }
        }
        
        gm.battleStarted = true;
        Debug.Log("game started!");

        StartCoroutine(im.RandomSpawns(15));

        //not doing this anymore
        //StartCoroutine(ShrinkClock());

        //StartCoroutine(LaserClock());
        //StartCoroutine(KillPlaneClock());

        //pick random endgame routine
        //NEW endgame routines
        int r = Random.Range(1,3);
        switch (r)
        {
            case 1: //laser
                StartCoroutine(LaserClock());
                break;

            case 2: //killplanes
                StartCoroutine(KillPlaneClock());
                break;

            default:
                break;
        }



        //ITEM TESTING
        //itemMan.SpawnItem(2, itemMan.transform);
    }

}
