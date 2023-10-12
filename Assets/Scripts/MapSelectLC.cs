using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapSelectLC : MonoBehaviour, ILevelController
{
    GameManager gm;

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


    public void StartLevel()
    {
        Debug.Log("MapSelect STARTED!");
    }

    public void EndLevel()
    {

    }

    public void ResetLevel()
    {

    }

    public void OnPlayerJoin(int idx)
    {

    }

    public void SpawnPlayer(int idx)
    {

    }

    public List<Transform> GetSpawnPoints()
    {
        return null;
    }

    public int GetLevelType()
    {
        //map select level menu level type = 1
        return 1;
    }

    public GameObject GetGameManager()
    {
        return GameObject.Find("GameManager");
    }

    public void LoadMap(string sceneName)
    {
        Debug.Log("LOADING " + sceneName);
        GetGameManager().GetComponentInChildren<SceneLoader>().LoadScene(sceneName);
    }
}
