using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TeamCharacterSelectLC : CharacterSelectLC
{
    public Color[] teamColors;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public override void OnPlayerJoin(int idx)
    {
        Debug.Log("PLAYER" + idx + " JOINED FROM LC");

        SpawnPlayer(idx);

        pm.PlayerList[idx].input.SwitchCurrentActionMap("Menu");
        //Debug.Log("Action Map: " + pm.playerList[idx].input.currentActionMap);

        //Character Select UI stuff

        //assign a team
        pm.SetPlayerTeamDefault(idx);

        //set colors
        SetUIColors(idx);

        //set UI to be active
        ActivatePlayerUI(idx);
    }

    public override void ActivatePlayerUI(int idx)
    {
        base.ActivatePlayerUI(idx);

    }


    public override void SetUIColors(int idx)
    {
        base.SetUIColors(idx);

        Transform[] PlayerUI = PlayersUI[idx];

        //set background team color
        PlayerUI[8].GetComponent<Image>().color = teamColors[pm.TeamList[pm.PlayerList[idx].team].color + 1];
        //set hat color
        PlayerUI[9].GetComponent<Image>().color = pm.teamColors[pm.TeamList[pm.PlayerList[idx].team].color];

    }
}
