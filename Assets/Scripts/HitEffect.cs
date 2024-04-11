using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class HitEffect : MonoBehaviour
{
    [SerializeField] ParticleSystem[] parts;
  

    // Start is called before the first frame update
    void Start()
    {
        
        
    }

    public void Init(float size, float lifetime)
    {
        //pick random sprite
        int idx = Random.Range(0,4);
        
        Debug.Log("HIT PARTICLE RENDERING!!");
        parts[idx].Stop();

        var main = parts[idx].main;
        //main.startSize = size;
        main.startLifetime = lifetime;
        var rend = parts[idx].GetComponent<ParticleSystemRenderer>();

        parts[idx].Play();
        
        Destroy(gameObject, lifetime +Time.deltaTime);
    }


    // Update is called once per frame
    void Update()
    {
        
    }
}
