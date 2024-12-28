using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ImpactParticle : MonoBehaviour
{
    public ParticleSystem partSys;

    // Start is called before the first frame update
    void Start()
    {
        partSys = GetComponent<ParticleSystem>();
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void PlayImpactParticles(Vector2 direction)
    {
        Rotate(direction);
        partSys.Play();
    }



    void Rotate(Vector2 direction)
    {
        float angle = Vector2.SignedAngle(Vector2.up, direction);
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);

    }
}
