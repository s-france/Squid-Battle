using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MapButton : Button
{

    ButtonMultiSelections selections;
    MatchSettings ms;

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();

        selections = GetComponent<ButtonMultiSelections>();
        ms = FindFirstObjectByType<MatchSettings>();


    }

    // Update is called once per frame
    void Update()
    {
        
    }


    public override void OnSelect(BaseEventData eventData)
    {
        base.OnSelect(eventData);

        //set token position to this button
        UITokenTracker token = eventData.currentInputModule.GetComponent<UITokenTracker>();
        int idx = token.idx;
        token.tokenPos.position = selections.positions[idx].position;
        token.confPos.position = selections.positions[idx+6].position;
        
        token.selectionID = selections.buttonID;

        Debug.Log("player " + idx + "OnSelect!");

    }

    void OnClick()
    {

    }

    public override void OnSubmit(BaseEventData eventData)
    {
        base.OnSubmit(eventData);

        //Debug.Log("Button Submitted!");
        

        UITokenTracker token = eventData.currentInputModule.GetComponent<UITokenTracker>();
        
        //set selected icon to correct color
        //selections.positions[6 + token.idx].GetComponent<Image>().sprite = ;
        
        
        //activate confirmed icon
        token.confPos.GetComponent<Image>().enabled = true;
        //selections.positions[6 + token.idx].gameObject.SetActive(true);

        //deactivate selecting icon
        token.tokenPos.GetComponent<Image>().enabled = false;

        //disable player selection
        eventData.currentInputModule.gameObject.SetActive(false);

        //vote for map
        ms.VoteMap(selections.buttonID);


    }

    public override void OnDeselect(BaseEventData eventData)
    {
        base.OnDeselect(eventData);



    }

}
