using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; 
using TMPro;


public class SliderValue : MonoBehaviour
{
    [SerializeField] Slider slider; 
    [SerializeField] private TMP_Text text; 
    
    void Awake(){
    }
    void Start()
    {
    }

    public void ReadValue(){
        float temp = slider.value; 
        text.text = temp.ToString("#.00"); 
    }
}
