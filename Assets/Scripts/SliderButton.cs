using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
//using UnityEngine.UIElements;

public class SliderButton : Button
{
    Slider slider;
    public bool sliderSelected;
    int[] sliderIntervals = {10, 15, 20, 25, 30};

    Text number;


    // Start is called before the first frame update
    void Start()
    {
        number = GetComponentInChildren<Text>();

        slider = GetComponentInChildren<Slider>();
        sliderSelected = false;
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SelectSlider()
    {
        sliderSelected = true;
        slider.interactable = true;
        slider.Select();
        
    }

    public void OnSliderValueChanged()
    {
        FindFirstObjectByType<AudioManager>().Play("UINav3");

        //update slider visuals
        int idx = 0;

        foreach (int i in sliderIntervals)
        {
            if(slider.value == i-1)
            {
                slider.value = sliderIntervals[idx-1];
                break;
            } else if(slider.value == i+1)
            {
                slider.value = sliderIntervals[idx+1];
                break;
            }

            idx++;
        }

        number.text = slider.value.ToString();
        
    }

    public override void OnSelect(BaseEventData eventData)
    {
        base.OnSelect(eventData);
        sliderSelected = false;
        slider.interactable = false;
    }

    public override void OnDeselect(BaseEventData eventData)
    {
        base.OnDeselect(eventData);
    }


}
