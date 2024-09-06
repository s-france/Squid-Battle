using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ModeButton : Button
{
    ModeSelectLC lc;
    Transform desc;


    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
        lc = FindFirstObjectByType<ModeSelectLC>();
        desc = transform.Find("Desc");
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public override void OnSelect(BaseEventData eventData)
    {
        base.OnSelect(eventData);

        //
        desc.gameObject.SetActive(true);

    }

    public override void OnDeselect(BaseEventData eventData)
    {
        base.OnDeselect(eventData);

        desc.gameObject.SetActive(false);

    }


}
