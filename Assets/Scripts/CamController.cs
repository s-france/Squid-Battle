using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CamController : MonoBehaviour
{
    [SerializeField] private GameObject gameCam;


    // Start is called before the first frame update
    void Start()
    {
        GameCamOn();
    }

    public void StartCamOn()
    {
        //startCam.SetActive(true);
        gameCam.SetActive(false);
    }

    public void GameCamOn()
    {
        //startCam.SetActive(false);
        gameCam.SetActive(true);
    }
}
