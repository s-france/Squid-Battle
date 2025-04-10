using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;



public class LevelController : MonoBehaviour
{
    [HideInInspector] public GameManager gm;
    [HideInInspector] public ItemManager im;

    [HideInInspector] public PlayerManager pm;
    [HideInInspector] public List<Transform> SpawnPoints;


    public virtual void StartLevel()
    {

    }


    public virtual void EndLevel()
    {

    }


    public virtual void ResetLevel()
    {

    }

    public virtual void OnConfirm(int playerID, InputAction.CallbackContext ctx)
    {

        return;
    }

    public virtual void OnBack(int playerID, InputAction.CallbackContext ctx)
    {
        
        return;
    }


    public virtual void OnPlayerJoin(int idx)
    {
        
    }

    public virtual void OnPlayerLeave(int idx)
    {

        return;
    }

    public virtual void OnPlayerReconnect(int idx)
    {
        return;
    }


    public virtual void SpawnPlayer(int idx)
    {

    }

    public virtual void ReadyPlayer(int idx)
    {

        return;
    }

    public virtual void UnReadyPlayer(int idx)
    {
        return;
    }

    //used in battle arenas to show score + initiate next round map vote
    public virtual void ShowResults()
    {
        return;
    }

    public virtual void ShowTeamResults()
    {
        return;
    }


    public virtual int GetLevelType()
    {
        print("no level type specified!");
        return -1;
    }

    //for setting individual player's UI colors
    public virtual void SetUIColors(int idx) //idx = playerID
    {

        return;
    }


    public GameObject GetGameManager()
    {
        return GameObject.Find("GameManager");
    }


    public List<Transform> GetSpawnPoints()
    {
        return SpawnPoints;
    }

    public void LoadScene(string sceneName)
    {
        gm.sl.LoadScene(sceneName);
    }
}