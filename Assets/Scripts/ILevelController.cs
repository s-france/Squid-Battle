using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public interface ILevelController
{
    void StartLevel();
    void EndLevel();
    void ResetLevel();
    void OnPlayerJoin(int idx);
    void SpawnPlayer(int idx);

    GameObject GetGameManager();
    List<Transform> GetSpawnPoints();
    int GetLevelType();
}