using UnityEngine.Audio;
using UnityEngine;
using System;

public class AudioManager : MonoBehaviour
{
    public Sound[] sounds;


    void Awake()
    {
        foreach (Sound s in sounds)
        {
            s.source = gameObject.AddComponent<AudioSource>();
            s.source.clip = s.clip;
            s.source.volume = s.volume;
            s.source.pitch = s.pitch;
            s.source.loop = s.loop;
        }
    }

    void Start()
    {
        //Play("BattleTheme");
    }

    //plays sound of name
    public void Play (string name)
    {
        Sound s = Array.Find(sounds, sound => sound.name == name);
        s.source.Play();
        Debug.Log("playing sound: " + name);
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
            default:
                n = -1;
                Debug.Log("ERROR: invalid audio type!!");
                break;
        }

        Play(type+n);
    }

    public void Stop (string name)
    {
        Sound s = Array.Find(sounds, sound => sound.name == name);
        s.source.Stop();
    }
}
