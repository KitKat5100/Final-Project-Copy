using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; 
using TMPro;
public class InputValue : MonoBehaviour
{
    [SerializeField] Slider slider; 
    [SerializeField] private TMP_InputField text; 
    
    void Awake(){
    }
    void Start()
    {
    }

    public void ValueTyped(){
        if(text.text != ""){
            slider.value = float.Parse(text.text); 
        } else {
            ReadValue(); 
        }
    }

    public void ReadValue(){
        float temp = slider.value; 
        if (temp >= 1){
            text.text = temp.ToString("#.0");
        } else {
            text.text = temp.ToString("0.#0"); 
        }
    }
}
