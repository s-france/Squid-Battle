using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoBack : MonoBehaviour
{
    [SerializeField] string previousScene;
    bool pressed = false;
    float timer = 0;

    bool done = false;

    // Start is called before the first frame update
    void Start()
    {
        done = false;
        
    }

    // Update is called once per frame
    void Update()
    {
        //pressed = Input.GetKeyDown(KeyCode.JoystickButton0);
        //pressed = Input.GetKey(KeyCode.JoystickButton1);
        //pressed = Input.GetButton("Jump");

        if(Input.GetButtonDown("Jump"))
        {
            pressed = true;
        }

        if(Input.GetButtonUp("Jump"))
        {
            pressed = false;
        }

        if(pressed)
        {
            //Debug.Log("Back PRESSED!!!");
            timer += Time.deltaTime;
        } else
        {
            timer = 0;
        }

        if(timer >= .5f && !done)
        {
            done = true;
            Back();
        }
        
    }

    void Back()
    {
        SceneLoader sl = Object.FindFirstObjectByType<SceneLoader>();

        sl.LoadScene(previousScene);
    }

    

}

