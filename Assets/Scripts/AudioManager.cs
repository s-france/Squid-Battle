using UnityEngine.Audio;
using UnityEngine;
using System;

public class AudioManager : MonoBehaviour
{
    /*[SerializeField]*/ public Sound[] sounds;


    public void Awake()
    {
        foreach (Sound s in sounds)
        {
            s.source = gameObject.AddComponent<AudioSource>();
            s.source.clip = s.clip;
            s.source.volume = s.volume;
            s.source.pitch = s.pitch;
            s.source.loop = s.loop;
        }

        //Play("MenuTheme1");
        
    }

    void Start()
    {
        //Play("BattleTheme");
    }

    //plays sound of name
    public void Play (string name)
    {
        if(name == "Menu")
        {
            return;
        }

        Debug.Log("playing sound: " + name);
        Sound s = Array.Find(sounds, sound => sound.name == name);
        s.source.Play();
    }

    //plays random sound from designated type
    public void PlayRandom (string type)
    {
        int n;

        switch (type)
        {
            case "Impact":
                n = UnityEngine.Random.Range(1, 4);
                break;

            case "Move":
                n = UnityEngine.Random.Range(1, 8);
                break;

            case "BattleTheme":
                n = UnityEngine.Random.Range(1,4);
                break;

            default:
                n = -1;
                Debug.Log("ERROR: invalid audio type!!");
                break;
        }

        Play(type+n);
    }

    public void Stop (string name)
    {
        if(name == "Menu")
        {
            return;
        }

        Sound s = Array.Find(sounds, sound => sound.name == name);        

        if(s == null)
        {
            Debug.Log("ERROR sound not found!");
        }

        s.source.Stop();

        Debug.Log("stopping sound: " + s.name);
        Debug.Log("s.source = " + s.source.name);
    }
}
