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
    //[SerializeField] Transform finalResult;
    [SerializeField] Transform[] scores;
    [SerializeField] Image[] sliderFills;


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


    //sets color of score meter at idx
    public void SetColor(int idx)
    {
        Debug.Log("SetColor scoreboardIDX: " + idx);

        if(pm == null)
        {
            pm = FindFirstObjectByType<PlayerManager>();
        }
        if(gm == null)
        {
            gm = FindFirstObjectByType<GameManager>();
        }

        //teams
        if(gm.gameMode == 1)
        {
            //set team icon color
            scores[idx].GetComponent<Image>().sprite = pm.PlayerList[0].playerScript.SpriteSet[0];
            scores[idx].GetComponent<Image>().color = pm.teamColors[idx];

            //set progress bar color
            sliderFills[idx].color = pm.teamColors[idx];

        } else //FFA
        {
            //set player icon color
            scores[idx].GetComponent<Image>().sprite = pm.PlayerList[idx].playerScript.SpriteSet[0];
            scores[idx].GetComponent<Image>().color = pm.PlayerList[idx].playerScript.sr.color;

            //set progress bar color
            sliderFills[idx].color = pm.PlayerList[idx].playerScript.sr.color;
        }

        
    }

    public void SetScores()
    {
        if(gm.gameMode == 1) //Teams
        {
            foreach (Team t in pm.TeamList)
            {
                //real team w players
                if(t.Players.Count > 0)
                {
                    Debug.Log("Team" + t.idx + " score: " + t.score);

                    //show score
                    scores[t.idx].gameObject.SetActive(true);
                    
                    //set values
                    scores[t.idx].GetComponentInChildren<Slider>().value = t.score;
                    scores[t.idx].GetComponentInChildren<TextMeshProUGUI>().text = t.score.ToString();
                }
            }

        } else //FFA
        {
             foreach(PlayerConfig p in pm.PlayerList)
            {
                Debug.Log("P" + p.playerIndex + " score: " + p.score);
                //show score
                scores[p.playerIndex].gameObject.SetActive(true);

                scores[p.playerIndex].GetComponentInChildren<Slider>().value = p.score;
                scores[p.playerIndex].GetComponentInChildren<TextMeshProUGUI>().text = p.score.ToString();

            }

        }

       

    }





}
