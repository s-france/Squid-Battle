using UnityEngine.Audio;
using UnityEngine;
using System;
using System.Linq;
using System.Collections.Generic;

public class AudioManager : MonoBehaviour
{
    /*[SerializeField]*/ public Sound[] sounds;

    [HideInInspector] public bool initialized = false;

    //currently active/playing sounds
    [HideInInspector] public List<String> SoundsPlaying;


    public void Awake()
    {
        Init();
    }

    public void Init()
    {
        if (!initialized)
        {
            SoundsPlaying = new List<String>();

            foreach (Sound s in sounds)
            {
                s.source = gameObject.AddComponent<AudioSource>();
                s.source.clip = s.clip;
                s.source.volume = s.volume;
                s.source.pitch = s.pitch;
                s.source.loop = s.loop;
            }

            initialized = true;
        }

        

    }

    void Start()
    {
        Init();
    }

    //plays sound of name
    public void Play (string name)
    {
        //Debug.Log("playing sound: " + name);
        Sound s = Array.Find(sounds, sound => sound.name == name);

        if (s != null)
        {
            s.source.Play();
            SoundsPlaying.Append(name);
        }

        
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
            
            case "Parry":
                n = UnityEngine.Random.Range(1,4);
                break;

            default:
                n = -1;
                Debug.Log("ERROR: invalid audio type!!");
                return;
        }

        Play(type+n);
    }

    public void Stop (string name)
    {
        Sound s = Array.Find(sounds, sound => sound.name == name);        

        if(s != null)
        {
            s.source.Stop();

            SoundsPlaying.Remove(name);

            //Debug.Log("stopping sound: " + s.name);
            //Debug.Log("s.source = " + s.source.name);
        } else
        {
            Debug.Log("ERROR sound " + name + " not found!");
        }

    }

    public void StopAll()
    {
        foreach(Sound s in sounds)
        {
            Stop(s.name);
        }

    }
}
