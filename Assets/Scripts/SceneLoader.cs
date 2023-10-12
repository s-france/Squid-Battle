using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

//handles loading / switching between levels
public class SceneLoader : MonoBehaviour
{
    GameManager gm;
    CamController cc;

    void Awake()
    {
        Debug.Log("SCENE LOADER AWAKE!!");
        
        gm = GameObject.Find("GameManager").GetComponent<GameManager>();

        //lc.StartLevel();
    }

    public async void LoadScene(string sceneName)
    {
        gm.lc.EndLevel();
        

        var scene = SceneManager.LoadSceneAsync(sceneName);
        scene.allowSceneActivation = false;

        do {
            //loading stuff
            //TEMPORARY:
            Debug.Log("loading " + sceneName + "...");
            await Task.Delay(50);

        } while (scene.progress < 0.9f);

        scene.allowSceneActivation = true;

        //Debug.Log("assigning new lc");
        //gm.lc = GameObject.Find("LevelController").GetComponent<ILevelController>();
        //Debug.Log("starting level");
        //gm.lc.StartLevel();
    }

}
