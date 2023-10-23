using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

//handles loading / switching between levels
public class SceneLoader : MonoBehaviour
{
    bool loading;
    GameManager gm;
    CamController cc;

    void Awake()
    {
        Debug.Log("SCENE LOADER AWAKE!!");
        
        gm = GetComponentInParent<GameManager>();
        loading = false;

        //lc.StartLevel();
    }

    public async void LoadScene(string sceneName)
    {
        if(!loading)
        {
            loading = true;

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
            loading = false;

            //this doesn't work. Cool idea though
            //Debug.Log("assigning new lc");
            //gm.lc = GameObject.Find("LevelController").GetComponent<ILevelController>();
            //Debug.Log("starting level");
            //gm.lc.StartLevel();
        }
    }

}
