using System.Collections;
using System.Collections.Generic;
using UnityEngine;



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
}