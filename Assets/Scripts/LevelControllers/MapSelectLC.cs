using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapSelectLC : LevelController
{
    //GameManager gm;

    // Start is called before the first frame update
    void Awake()
    {
        gm = GameObject.Find("GameManager").GetComponent<GameManager>();
        gm.lc = this;

        StartLevel();
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    public override void StartLevel()
    {
        Debug.Log("MapSelect STARTED!");
    }

    public override void EndLevel()
    {
        Debug.Log("Ending LevelSelect!");
        FindFirstObjectByType<AudioManager>().Stop("Menu");
    }

    public override void ResetLevel()
    {

    }

    public override void OnPlayerJoin(int idx)
    {

    }

    public override void SpawnPlayer(int idx)
    {

    }

    /*
    public List<Transform> GetSpawnPoints()
    {
        return null;
    }
    */

    public override int GetLevelType()
    {
        //map select level menu level type = 3
        return 3;
    }

    /*
    public GameObject GetGameManager()
    {
        return GameObject.Find("GameManager");
    }
    */

    public void LoadMap(string sceneName)
    {
        //EndLevel();
        Debug.Log("LOADING " + sceneName);
        GetGameManager().GetComponentInChildren<SceneLoader>().LoadScene(sceneName);
    }
}
