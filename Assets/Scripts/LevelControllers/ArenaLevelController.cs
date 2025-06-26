using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Rendering;


//parent LevelController class for all battle arena LevelControllers
public class ArenaLevelController : LevelController
{
    //Arena stuff
    [SerializeField] Transform Laser;
    [SerializeField] Transform KillPlanes;
    [SerializeField] Transform SoccerBall;
    [SerializeField] List<Vector2> ArenaShrinks;
    [SerializeField] float shrinkSpeed;
    Transform arena;
    Transform arenaAnchor;

    //post-game UI stuff
    [SerializeField] Transform resultsScreen;
    [HideInInspector] public MapVote mv;
    [SerializeField] Transform finalResultMenu;
    public InputSystemUIInputModule[] UIInputModules;
    

    [SerializeField] GameObject ReturnButton;
    
    [SerializeField] Image winnerSprite;
    [SerializeField] TextMeshProUGUI winnerText;
    public ScoreBoard sb;
    [SerializeField] Transform ready2StartUI;

    [HideInInspector] public bool roundOver = false;



    public virtual void Awake()
    {
        gm = GameObject.Find("GameManager").GetComponent<GameManager>();
        gm.lc = this;
        pm = gm.GetComponentInChildren<PlayerManager>();
        im = GameObject.Find("ItemManager").GetComponent<ItemManager>();
        im.gm = gm;

        //soccer init
        if(gm.gameMode == 2 && SoccerBall != null)
        {
            Debug.Log("Playing Soccer!");
            //SoccerBall.GetComponent<BallPlayerController>().Init();
        }

        mv = resultsScreen.GetComponentInChildren<MapVote>();

        SpawnPoints = new List<Transform>();

        SpawnPoints.Add(transform.GetChild(0));
        SpawnPoints.Add(transform.GetChild(1));
        SpawnPoints.Add(transform.GetChild(2));
        SpawnPoints.Add(transform.GetChild(3));
        SpawnPoints.Add(transform.GetChild(4));
        SpawnPoints.Add(transform.GetChild(5));

        if(gm.gameMode == 2)
        {
            SpawnPoints.Add(transform.GetChild(6));
            SpawnPoints.Add(transform.GetChild(7));
            SpawnPoints.Add(transform.GetChild(8));
            SpawnPoints.Add(transform.GetChild(9));
            SpawnPoints.Add(transform.GetChild(10));
            SpawnPoints.Add(transform.GetChild(11));
        }


        arena = GameObject.Find("Arena").transform;
        arenaAnchor = GameObject.Find("ArenaAnchor").transform;

        //arenaAnchor.transform.localScale = arena.transform.localScale;
        
        StartLevel();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public virtual IEnumerator ShrinkClock()
    {
        float timer = 0;
        //Debug.Log("COROUTINE STARTED");
        
        while(arena.localScale.magnitude > ArenaShrinks[2].magnitude)
        {
            if (timer >= 25 && arena.localScale.magnitude > ArenaShrinks[0].magnitude)
            {
                //Debug.Log("SHRINKING 0");
                arena.localScale = Vector2.MoveTowards(arena.localScale, ArenaShrinks[0], shrinkSpeed * Time.deltaTime);
                arenaAnchor.localScale = Vector2.MoveTowards(arenaAnchor.localScale, ArenaShrinks[0], shrinkSpeed * Time.deltaTime);
            } else if (timer >= 45 && arena.localScale.magnitude > ArenaShrinks[1].magnitude)
            {
                //Debug.Log("SHRINKING 1");
                arena.localScale = Vector2.MoveTowards(arena.localScale, ArenaShrinks[1], shrinkSpeed * Time.deltaTime);
                arenaAnchor.localScale = Vector2.MoveTowards(arenaAnchor.localScale, ArenaShrinks[1], shrinkSpeed * Time.deltaTime);
            } else if (timer >= 60 && arena.localScale.magnitude > ArenaShrinks[2].magnitude)
            {
                //Debug.Log("SHRINKING 2");
                arena.localScale = Vector2.MoveTowards(arena.localScale, ArenaShrinks[2], shrinkSpeed * Time.deltaTime);
                arenaAnchor.localScale = Vector2.MoveTowards(arenaAnchor.localScale, ArenaShrinks[2], shrinkSpeed * Time.deltaTime);
            }
            timer = timer + Time.deltaTime;
            yield return null;
        }
        Debug.Log("ENDING SHRINK COROUTINE");
        
    }

    public virtual IEnumerator LaserClock()
    {
        float timer = 0;

        while (timer < 45)
        {
            timer += Time.deltaTime;
            yield return null;
        }

        Debug.Log("STARTING LASER");
        Laser.gameObject.SetActive(true);

    }

    public virtual IEnumerator KillPlaneClock()
    {
        float timer = 0;

        while (timer < 45)
        {
            timer += Time.deltaTime;
            yield return null;
        }

        Debug.Log("STARTING KILLPLANES");
        KillPlanes.gameObject.SetActive(true);
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

    //not currently used for anything
    //could include stopping gameplay coroutines after round ends
    public override void EndLevel()
    {
        base.EndLevel();
        
    }

    public override void SetUIColors(int idx)
    {
        //base.SetUIColors(idx);

        mv.SetUIColors(idx);

        //set scoreboard colors
        sb.SetColor(idx);
    }


    //shows results for team battle 
    public override void ShowTeamResults()
    {
        //signal round has ended
        roundOver = true;

        FindFirstObjectByType<AudioManager>().StopAll();

        //assign points
        pm.placements.Reverse();
        int idx = 0;
        foreach(int i in pm.placements)
        {
            Debug.Log("placements[idx] = " + i);

            switch (idx)
            {
                case 0:
                {
                    //headhunters
                    if(gm.ms.scoreFormat == 0)
                    {
                        pm.TeamList[i].score += 1; //1st +1
                    
                    //survival
                    } else if(gm.ms.scoreFormat == 1)
                    {
                        pm.TeamList[i].score += 3; //1st +3
                    }
                    break;
                }
                case 1:
                {
                    if(gm.ms.scoreFormat == 1)
                    {
                        pm.TeamList[i].score += 2; //2nd +2
                    }
                    break;
                }
                case 2:
                {
                    if(gm.ms.scoreFormat == 1)
                    {
                        pm.TeamList[i].score += 1; //3rd +1
                    }
                    break;
                }
                default:
                {
                    break;
                }
            }

            idx++;
        }

        pm.placements.Clear();

        //update scoreboard
        //(team version)
        sb.SetScores();

        resultsScreen.gameObject.SetActive(true);

        //team tie check
        //check for ties
        List<Team> winners = pm.TeamList.FindAll(t => t.score >= gm.ms.pointsToWin);
        
        
        int max = 0;
        int count = 0;

        foreach(Team t in winners)
        {
            if(t.score > max)
            {
                max = t.score;
                count = 1;
            } else if(t.score == max)
            {
                count ++;
            }
        }


        //if there is one winner show end screen
        if(pm.TeamList.Exists(t => t.score >= gm.ms.pointsToWin) && count ==1)
        {

            int winner = -1;// = pm.playerList.Find(p => p.score >= gm.ms.pointsToWin).playerIndex;

            foreach(Team t in winners)
            {
                if(t.score >= max)
                {
                    winner = t.idx;
                    break;
                }
            }
            
            //set winner team sprite
            winnerSprite.sprite = pm.PlayerList[0].playerScript.SpriteSet[0];
            winnerSprite.color = pm.teamColors[pm.TeamList[winner].color];
            
            //set winning team text
            winnerText.text = "Team " + winner + " Wins!";

            mv.DeactivateMapVoteMenu();
            
            finalResultMenu.gameObject.SetActive(true);

            //switch P1 input to menu
            pm.PlayerList[0].input.SwitchCurrentActionMap("Menu");
            //P1 select results menu
            UIInputModules[0].GetComponent<MultiplayerEventSystem>().SetSelectedGameObject(ReturnButton);


        } else
        {
            finalResultMenu.gameObject.SetActive(false);
            
            mv.ActivateMapVoteMenu();

        }
    }


    
    //shows results + mapvote after round ends
    public override void ShowResults()
    {
        //signal round has ended
        roundOver = true;

        FindFirstObjectByType<AudioManager>().StopAll();


        //assign points
        pm.placements.Reverse();
        int idx = 0;
        foreach(int i in pm.placements)
        {
            Debug.Log("placements[idx] = " + i);

            switch (idx)
            {
                case 0:
                {
                    //headhunters
                    if(gm.ms.scoreFormat == 0)
                    {
                        //MAKE THIS AN OPTION (not default)
                        //pm.PlayerList[i].score += 1; //1st +1
                    //survival
                    } else if(gm.ms.scoreFormat == 1)
                    {
                        pm.PlayerList[i].score += 3; //1st +3
                    }
                    break;
                }
                case 1:
                {
                    if(gm.ms.scoreFormat == 1)
                    {
                        pm.PlayerList[i].score += 2; //2nd +2
                    }
                    break;
                }
                case 2:
                {
                    if(gm.ms.scoreFormat == 1)
                    {
                        pm.PlayerList[i].score += 1; //3rd +1
                    }
                    break;
                }
                default:
                {
                    break;
                }
            }

            idx++;
        }

        pm.placements.Clear();

        //update scoreboard
        sb.SetScores();

        resultsScreen.gameObject.SetActive(true);

        //check for ties
        List<PlayerConfig> winners = pm.PlayerList.FindAll(p => p.score >= gm.ms.pointsToWin);
        int max = 0;
        int count = 0;

        foreach(PlayerConfig p in winners)
        {
            if(p.score > max)
            {
                max = p.score;
                count = 1;
            } else if(p.score == max)
            {
                count ++;
            }
        }


        //if there is one winner show end screen
        if(pm.PlayerList.Exists(p => p.score >= gm.ms.pointsToWin) && count ==1)
        {

            int winner = -1;// = pm.playerList.Find(p => p.score >= gm.ms.pointsToWin).playerIndex;

            foreach(PlayerConfig p in winners)
            {
                if(p.score >= max)
                {
                    winner = p.playerIndex;
                    break;
                }
            }
            
            //set winner sprite
            winnerSprite.sprite = pm.PlayerList.Find(p => p.playerIndex == winner).playerScript.SpriteSet[0];
            winnerSprite.color = pm.PlayerList.Find(p => p.playerIndex == winner).playerScript.sr.color;
            //set winner text
            winnerText.text = "Player " + winner + " Wins!";

            mv.DeactivateMapVoteMenu();
            
            finalResultMenu.gameObject.SetActive(true);

            //switch P1 input to menu
            pm.PlayerList[0].input.SwitchCurrentActionMap("Menu");
            //P1 select results menu
            UIInputModules[0].GetComponent<MultiplayerEventSystem>().SetSelectedGameObject(ReturnButton);


        } else
        {
            finalResultMenu.gameObject.SetActive(false);
            
            mv.ActivateMapVoteMenu();

        }

    }

    public virtual void PlayAgain()
    {
        //reset game stats
        gm.ResetGame();


        //deactivate finalResults
        finalResultMenu.gameObject.SetActive(false);
        
        mv.ActivateMapVoteMenu();
    }


    public override void OnConfirm(int playerID, InputAction.CallbackContext ctx)
    {
        base.OnConfirm(playerID, ctx);

        if(ctx.performed)
        {
            //start game if all players ready
            if(pm.PlayerList.TrueForAll(p => (p.isReady || !p.isActive)) && pm.PlayerList.Count(p => p.isActive) > 1)
            {
                //assign value randomly selected from gm.ms.MapPool
                int map = gm.ms.PickMap();

                Debug.Log("picked map" + map);

                gm.StartMatch(map);
                return;
            }


            if(!pm.PlayerList[playerID].isReady)
            {
                pm.ReadyPlayer(playerID); //calls lc.ReadyPlayer
            }
        }
    }


    public override void OnBack(int playerID, InputAction.CallbackContext ctx)
    {
        //base.OnBack(ctx);

        if(ctx.started)
        {
            if (pm.PlayerList[playerID].isReady)
            {
                FindFirstObjectByType<AudioManager>().Play("UINav2");
                pm.UnReadyPlayer(playerID);
            }
        }
        
        if (ctx.performed)
        {
            //nothing to do here rn
        }
    }


    public override void ReadyPlayer(int idx)
    {
        base.ReadyPlayer(idx);

        //allow starting game if all active players are ready && more than 1 player
        if(pm.PlayerList.TrueForAll(p => (p.isReady || !p.isActive)) && pm.PlayerList.Count(p => p.isActive) > 1)
        {
            //show ready to start UI
            ready2StartUI.gameObject.SetActive(true);

        }else
        {
            ready2StartUI.gameObject.SetActive(false);
        }

    }


    public override void UnReadyPlayer(int idx)
    {
        base.UnReadyPlayer(idx);

        //reactivate player's mapselect token
        UITokenTracker token = UIInputModules[idx].GetComponent<UITokenTracker>();

        //cancel map vote
        gm.ms.UnVoteMap(token.selectionID);
        
        //deactivate confirmed icon
        token.confPos.GetComponent<Image>().enabled = false;
        //selections.positions[6 + token.idx].gameObject.SetActive(true);

        //reactivate selecting icon
        token.tokenPos.GetComponent<Image>().enabled = true;

        //reenable player selection
        UIInputModules[idx].gameObject.SetActive(true);


        //allow starting game if all active players are ready && more than 1 player
        if(pm.PlayerList.TrueForAll(p => (p.isReady || !p.isActive)) && pm.PlayerList.Count(p => p.isActive) > 1)
        {
            //show ready to start UI
            ready2StartUI.gameObject.SetActive(true);

        }else
        {
            ready2StartUI.gameObject.SetActive(false);
        }
    }


    public override void OnPlayerJoin(int idx)
    {
        //players should not be able to join during game
        //deactivate until next round
    }

    //moves player to spawnpoint
    public override void SpawnPlayer(int idx)
    {
        pm.PlayerList[pm.PlayerList.FindIndex(p => p.playerIndex == idx)].input.gameObject.transform.position = SpawnPoints[idx].position;
    }

    public override int GetLevelType()
    {
        //print("generic ArenaLC leveltype = 10");
        return 10;
    }


}
