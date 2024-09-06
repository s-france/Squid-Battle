using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MatchSettingsLC : LevelController
{
    //game mode
    //items array
    //number of rounds
    //number of players
    //

    [SerializeField] Slider slider1;
    [SerializeField] SliderButton SliderButton1;


    // Start is called before the first frame update
    void Awake()
    {
        gm = GameObject.Find("GameManager").GetComponent<GameManager>();
        gm.lc = this;
        pm = FindFirstObjectByType<PlayerManager>();
        //pm = GetGameManager().GetComponentInChildren<PlayerManager>();
        //pm = GameObject.Find("PlayerManager").GetComponent<PlayerManager>();

        SpawnPoints = new List<Transform>();

        SpawnPoints.Add(transform.GetChild(0));
        SpawnPoints.Add(transform.GetChild(1));
        SpawnPoints.Add(transform.GetChild(2));
        SpawnPoints.Add(transform.GetChild(3));


        StartLevel();
        
    }


    // Update is called once per frame
    void Update()
    {
        
    }


    public override void StartLevel()
    {
        //basic menuLC stuff
        if(pm == null)
        {
            pm = FindFirstObjectByType<PlayerManager>();
        }

        //disable players joining
        pm.im.pim.DisableJoining();

        foreach (PlayerConfig p in pm.playerList)
        {
            pm.DeactivatePlayer(p.playerIndex);

            //pm.UnReadyPlayer(p.playerIndex);

            Transform spawn = this.gameObject.transform.GetChild(p.playerIndex);
            p.input.gameObject.transform.position = spawn.position;
            //}
            //else
            //{
            //    ui.HidePlayerUI(p.playerIndex);
            //}
        }
        

    }


    public override void EndLevel()
    {
        //run any kind of outro/transition stuff here
        Debug.Log("ending match settings menu");
    }


    public override void ResetLevel()
    {


    }


    public override int GetLevelType()
    {
        //MatchSettings type = 3
        return 3;
    }


    //need to fix this - shouldn't require hold to exit slider
    public override void OnBack()
    {
        base.OnBack();
        //if slider selected -> exit slider
        //else -> go to previous menu

        if(SliderButton1.sliderSelected)
        {
            SliderButton1.Select();
        } else
        {
            gm.sl.LoadScene("CharacterSelect");
        }


    }

    public void ToggleItem(int itemID)
    {
        gm.ms.ActiveItems[itemID] = !gm.ms.ActiveItems[itemID];
    }

    public void SetPointsToWin()
    {
        gm.ms.pointsToWin = (int)slider1.value;
    }
    




}
