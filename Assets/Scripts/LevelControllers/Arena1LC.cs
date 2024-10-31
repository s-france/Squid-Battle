using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Unity.VisualScripting.FullSerializer;
using UnityEngine.UI;
using UnityEngine.InputSystem.UI;
using UnityEngine.InputSystem;
using TMPro;
using System;

public class Arena1LC : ArenaLevelController
{


    // Start is called before the first frame update
    public override void Awake()
    {
        base.Awake();

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public override IEnumerator ShrinkClock()
    {
        return base.ShrinkClock();
    }


    public override void StartLevel()
    {
        base.StartLevel();

    }


    public override void EndLevel()
    {
        base.EndLevel();

    }

    public override void SetUIColors(int idx)
    {
        base.SetUIColors(idx);

    }


    //shows results + mapvote after round ends
    public override void ShowResults()
    {
        base.ShowResults();

    }

    public override void PlayAgain()
    {
        base.PlayAgain();

    }


    public override void OnConfirm(int playerID, InputAction.CallbackContext ctx)
    {
        base.OnConfirm(playerID, ctx);
        
    }


    public override void OnBack(int playerID, InputAction.CallbackContext ctx)
    {
        base.OnBack(playerID, ctx);

    }

    public override void ReadyPlayer(int idx)
    {
        base.ReadyPlayer(idx);

    }

    public override void UnReadyPlayer(int idx)
    {
        base.UnReadyPlayer(idx);

    }



    //moves player to spawnpoint - in ArenaLevelController
    //public override void SpawnPlayer(int idx)

    public override int GetLevelType()
    {
        //battle arenas type = 10
        return 11;
    }
}
