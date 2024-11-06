using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ScoreBoard : MonoBehaviour
{
    GameManager gm;
    PlayerManager pm;
    //[SerializeField] Transform mapVote;
    [SerializeField] Transform finalResult;
    [SerializeField] Transform[] scores;


    // Start is called before the first frame update
    void Start()
    {
        gm = FindFirstObjectByType<GameManager>();
        pm = FindFirstObjectByType<PlayerManager>();

        //init slider scale to max points
        foreach(Transform s in scores)
        {
            s.GetComponentInChildren<Slider>().maxValue = gm.ms.pointsToWin;
        }
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    public void SetColor(int playerID)
    {
        Debug.Log("SetColor playerID: " + playerID);

        if(pm == null)
        {
            pm = FindFirstObjectByType<PlayerManager>();
        }

        //set player icon color
        scores[playerID].GetComponent<Image>().sprite = pm.playerList[playerID].playerScript.spriteSet[0];

        //set progress bar color
        //FINISH THIS!!!!
    }

    public void SetScores()
    {
        foreach(PlayerConfig p in pm.playerList)
        {
            Debug.Log("P" + p.playerIndex + " score: " + p.score);
            //show score
            scores[p.playerIndex].gameObject.SetActive(true);

            scores[p.playerIndex].GetComponentInChildren<Slider>().value = p.score;
            scores[p.playerIndex].GetComponentInChildren<TextMeshProUGUI>().text = p.score.ToString();



        }
    }





}
