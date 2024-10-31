using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Unity.VisualScripting.FullSerializer;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;
using TMPro;

public class Arena0LC : ArenaLevelController
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


    public override int GetLevelType()
    {
        //battle arenas type = 10
        return 10;
    }
}
