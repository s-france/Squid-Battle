using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UITokenTracker : MonoBehaviour
{
    [SerializeField] public int idx; //playerID basically
    [HideInInspector] public int selectionID = 0; //currently selected mapID
    [SerializeField] public Transform tokenPos;
    [SerializeField] public Transform confPos;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}