using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StaminaWheel : MonoBehaviour
{
    [SerializeField] Slider wheel;
    PlayerController3 pc;

    // Start is called before the first frame update
    void Start()
    {
        pc = GetComponentInParent<PlayerController3>();
    }

    // Update is called once per frame
    void Update()
    {
        wheel.value = 1-(pc.oobTimer/pc.oobLifespan);

        if(wheel.value == 1)
        {
            wheel.gameObject.SetActive(false);
        } else if(pc.isAlive)
        {
            wheel.gameObject.SetActive(true);
        }

    }
}
