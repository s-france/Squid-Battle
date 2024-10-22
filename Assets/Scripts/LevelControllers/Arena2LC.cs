using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Numerics;
//using UnityEngine.UIElements;
using Unity.Mathematics;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;
using TMPro;

public class Arena2LC : LevelController
{
    Transform Box1;
    Transform Box2;

    [SerializeField] float moveDistance;
    [SerializeField] float moveSpeed;
    int moveDirection1;
    int moveDirection2;
    float arenaPosition;
    bool forward;
    UnityEngine.Vector2 movePos;


    [SerializeField] List<UnityEngine.Vector2> ArenaShrinks;
    [SerializeField] float shrinkSpeed;
    Transform arena;

    //post-game UI stuff
    [SerializeField] Transform resultsScreen;
    [SerializeField] Transform mapVoteMenu;
    [SerializeField] Transform finalResultMenu;
    [SerializeField] InputSystemUIInputModule[] UIInputModules;
    [SerializeField] Image[] TokenSprites;
    [SerializeField] GameObject Map1Button;
    [SerializeField] GameObject ReturnButton;
    [SerializeField] TextMeshProUGUI nextRoundText;
    [SerializeField] Image winnerSprite;
    [SerializeField] TextMeshProUGUI winnerText;
    [SerializeField] ScoreBoard sb;


    // Start is called before the first frame update
    void Awake()
    {
        gm = GameObject.Find("GameManager").GetComponent<GameManager>();
        gm.lc = this;
        pm = gm.GetComponentInChildren<PlayerManager>();
        im = GameObject.Find("ItemManager").GetComponent<ItemManager>();

        im.gm = gm;

        SpawnPoints = new List<Transform>();

        SpawnPoints.Add(transform.GetChild(0));
        SpawnPoints.Add(transform.GetChild(1));
        SpawnPoints.Add(transform.GetChild(2));
        SpawnPoints.Add(transform.GetChild(3));
        SpawnPoints.Add(transform.GetChild(4));
        SpawnPoints.Add(transform.GetChild(5));
        
        Box1 = GameObject.Find("Box1").transform;
        Box2 = GameObject.Find("Box2").transform;
        arenaPosition = 0;
        moveDirection1 = 1;
        moveDirection2 = -1;
        forward = true;
        
        arena = GameObject.Find("Arena").transform;
        
        StartLevel();
    }

    // Update is called once per frame
    void Update()
    {
        MoveBox(1);
        MoveBox(2);
    }

    void MoveBox(int boxNum)
    {
        Transform box = null;
        int direction = 0;
        UnityEngine.Vector2 target = UnityEngine.Vector2.zero;
        UnityEngine.Vector2 start = UnityEngine.Vector2.zero;

        if(boxNum == 1)
        {
            box = Box1;
            direction = moveDirection1;
        } else if(boxNum == 2)
        {
            box = Box2;
            direction = moveDirection2;
        }

        start = new UnityEngine.Vector2(0, box.localPosition.y);
        target = new UnityEngine.Vector2(moveDistance*direction, box.localPosition.y);
        
        if(Mathf.Abs(box.localPosition.x) == Mathf.Abs(target.x) /*-.01*/)
        {
            forward = false;
        } else if(Mathf.Abs(box.localPosition.x) == Mathf.Abs(start.x))
        {
            forward = true;
        }

        if(forward)
        {
            box.localPosition = UnityEngine.Vector2.MoveTowards(box.localPosition, target, moveSpeed*Time.deltaTime);
        } else
        {
            box.localPosition = UnityEngine.Vector2.MoveTowards(box.localPosition, start, moveSpeed*Time.deltaTime);
        }
        
    }


    IEnumerator ShrinkClock()
    {
        float timer = 0;
        Debug.Log("COROUTINE STARTED");
        
        while(arena.localScale.magnitude > ArenaShrinks[2].magnitude)
        {
            if (timer >= 35 && arena.localScale.magnitude > ArenaShrinks[0].magnitude)
            {
                //Debug.Log("SHRINKING 0");
                arena.localScale = UnityEngine.Vector2.MoveTowards(arena.localScale, ArenaShrinks[0], shrinkSpeed * Time.deltaTime);
            } else if (timer >= 70 && arena.localScale.magnitude > ArenaShrinks[1].magnitude)
            {
                //Debug.Log("SHRINKING 1");
                arena.localScale = UnityEngine.Vector2.MoveTowards(arena.localScale, ArenaShrinks[1], shrinkSpeed * Time.deltaTime);
            } else if (timer >= 105 && arena.localScale.magnitude > ArenaShrinks[2].magnitude)
            {
                //Debug.Log("SHRINKING 2");
                arena.localScale = UnityEngine.Vector2.MoveTowards(arena.localScale, ArenaShrinks[2], shrinkSpeed * Time.deltaTime);
            }
            timer = timer + Time.deltaTime;
            yield return null;
        }
        Debug.Log("ENDING SHRINK COROUTINE");
        
    }


    public override void StartLevel()
    {
        //FindObjectOfType<AudioManager>().Stop("MenuTheme1");

        Debug.Log("Starting LEvel arena0");
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
        FindObjectOfType<AudioManager>().PlayRandom("BattleTheme");

        //move players to spawn positions
        foreach (PlayerConfig p in pm.playerList.Where(p => p.isActive))
        {
            pm.DeactivatePlayer(p.playerIndex);
            pm.ReactivatePlayer(p.playerIndex);

            //enable Player actions for gameplay instances
            p.input.SwitchCurrentActionMap("Player");

            Transform start = GetSpawnPoints()[p.playerIndex];
            p.input.gameObject.transform.position = start.position;

            p.isAlive = true;

            //prepare post-game UI:
            //assign players to UI input modules
            UIInputModules[p.playerIndex].gameObject.SetActive(true);
            UIInputModules[p.playerIndex].actionsAsset = pm.playerList[pm.playerList.FindIndex(player => player.playerIndex == p.playerIndex)].input.actions;
            pm.playerList[pm.playerList.FindIndex(player => player.playerIndex == p.playerIndex)].input.uiInputModule = UIInputModules[p.playerIndex];

            //activate player token UI
            TokenSprites[p.playerIndex].enabled = true;
            //TokenSprites[p.playerIndex+6].enabled = false;
            //initialize token position to Map1
            TokenSprites[p.playerIndex].transform.position = Map1Button.GetComponent<ButtonMultiSelections>().positions[p.playerIndex].position;
            TokenSprites[p.playerIndex+6].transform.position = Map1Button.GetComponent<ButtonMultiSelections>().positions[p.playerIndex+6].position;

            //set player UI colors
            SetUIColors(p.playerIndex);
        }

        //REWORK
        //camCon.GameCamOn();

        gm.battleStarted = true;
        Debug.Log("game started!");

        //REWORK
        StartCoroutine(im.RandomSpawns(15));

        StartCoroutine(ShrinkClock());

        //ITEM TESTING
        //itemMan.SpawnItem(2, itemMan.transform);
    

    

    }


    public override void EndLevel()
    {
        //deactivate all players
        //declare winner somewhere
        //load start menu

        Debug.Log("stopping music!!!");
        //turn off battle music
        FindFirstObjectByType<AudioManager>().Stop("BattleTheme1");
        FindFirstObjectByType<AudioManager>().Stop("BattleTheme2");
        FindFirstObjectByType<AudioManager>().Stop("BattleTheme3");

        //not really needed anymore
        /*
        foreach (GameObject obj in GameObject.FindGameObjectsWithTag("Despawns"))
        {
            Destroy(obj);
        }
        */

        gm.battleStarted = false;

        gm.sl.LoadScene("StartMenu");

    }

    public override void SetUIColors(int idx)
    {
        //base.SetUIColors(idx);

        //set token color
        TokenSprites[idx].sprite = pm.playerList[idx].playerScript.spriteSet[0];

        //set confirmed token colors
        TokenSprites[idx+6].sprite = pm.playerList[idx].playerScript.spriteSet[2];

        //set scoreboard colors
        sb.SetColor(idx);

    }


    //shows results + mapvote after round ends
    public override void ShowResults()
    {
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
                    pm.playerList[i].score += 3; //1st +3
                    break;
                }
                case 1:
                {
                    pm.playerList[i].score += 2; //2nd +2
                    break;
                }
                case 2:
                {
                    pm.playerList[i].score += 1; //3rd +1
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

        //if there is a winner show end screen
        if(pm.playerList.Exists(p => p.score >= gm.ms.pointsToWin))
        {
            int winner = pm.playerList.Find(p => p.score >= gm.ms.pointsToWin).playerIndex;
            
            //set winner sprite
            winnerSprite.sprite = pm.playerList.Find(p => p.playerIndex == winner).playerScript.spriteSet[0];
            //set winner text
            winnerText.text = "Player " + winner + " Wins!";

            mapVoteMenu.gameObject.SetActive(false);
            finalResultMenu.gameObject.SetActive(true);

            //switch P1 input to menu
            pm.playerList[0].input.SwitchCurrentActionMap("Menu");
            //P1 select results menu
            UIInputModules[0].GetComponent<MultiplayerEventSystem>().SetSelectedGameObject(ReturnButton);


        } else //show map vote
        {
            //update next round text
            nextRoundText.text = "Round " + ++gm.ms.roundsPlayed + " Incoming!";

            finalResultMenu.gameObject.SetActive(false);
            mapVoteMenu.gameObject.SetActive(true);

            foreach(PlayerConfig p in pm.playerList)
            {
                //switch to menu input action map
                p.input.SwitchCurrentActionMap("Menu");

                //initialize token position to Map1
                TokenSprites[p.playerIndex].transform.position = Map1Button.GetComponent<ButtonMultiSelections>().positions[p.playerIndex].position;
                TokenSprites[p.playerIndex+6].transform.position = Map1Button.GetComponent<ButtonMultiSelections>().positions[p.playerIndex+6].position;
            }

        }

    }

    public void PlayAgain()
    {
        //reset game stats
        gm.ResetGame();


        //deactivate finalResults
        finalResultMenu.gameObject.SetActive(false);
        //activate MapVote
        mapVoteMenu.gameObject.SetActive(true);

        foreach(PlayerConfig p in pm.playerList)
            {
                //switch to menu input action map
                p.input.SwitchCurrentActionMap("Menu");

                UIInputModules[p.playerIndex].GetComponent<MultiplayerEventSystem>().SetSelectedGameObject(Map1Button);
                
                //initialize token position to Map1
                TokenSprites[p.playerIndex].transform.position = Map1Button.GetComponent<ButtonMultiSelections>().positions[p.playerIndex].position;
                TokenSprites[p.playerIndex+6].transform.position = Map1Button.GetComponent<ButtonMultiSelections>().positions[p.playerIndex+6].position;
            }
    }


    public override void OnConfirm(int playerID, InputAction.CallbackContext ctx)
    {
        base.OnConfirm(playerID, ctx);

        if(ctx.performed)
        {
            //start game if all players ready
            if(pm.playerList.TrueForAll(p => (p.isReady || !p.isActive)) && pm.playerList.Count(p => p.isActive) > 1)
            {
                //assign value randomly selected from gm.ms.MapPool
                int map = gm.ms.PickMap();

                gm.StartMatch(map);
            }


            if(!pm.playerList[playerID].isReady)
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
            if (pm.playerList[playerID].isReady)
            {
                pm.UnReadyPlayer(playerID);
            }
        }
        
        if (ctx.performed)
        {
            //nothing to do here rn
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
        if(pm.playerList.TrueForAll(p => (p.isReady || !p.isActive)) && pm.playerList.Count(p => p.isActive) > 1)
        {
            //show ready to start UI
            //COME BACK AND RESOLVE THIS!!!!!!!!!
            //ready2StartUI.gameObject.SetActive(true);

        }else
        {
            //ready2StartUI.gameObject.SetActive(false);
        }
    }


    //REWORK:  LevelController.EndBattle should call this -> leads back to start menu
    //declare winner and end/reset game
    public void ResetGame(int winnerIdx)
    {
        


        


        

        //reactivate() all isactive players
        //move players to spawn points
        
    }


    public override void ResetLevel()
    {
        //idk
    }


    public override void OnPlayerJoin(int idx)
    {
        //deactivate until next round
    }


    //moves player to spawnpoint
    public override void SpawnPlayer(int idx)
    {
        pm.playerList[pm.playerList.FindIndex(p => p.playerIndex == idx)].input.gameObject.transform.position = SpawnPoints[idx].position;
    }

    /*
    public GameObject GetGameManager()
    {
        return GameObject.Find("GameManager");
    }

    public List<Transform> GetSpawnPoints()
    {
        return SpawnPoints;
    }
    */

    public override int GetLevelType()
    {
        //battle arenas type = 10
        return 12;
    }
}
