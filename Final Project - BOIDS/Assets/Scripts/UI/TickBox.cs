using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; 
using TMPro;

public class TickBox : MonoBehaviour
{
    [SerializeField] private bool isTicked = true; 
    [SerializeField] private GameObject checkIcon; 
    [SerializeField] private Slider slider; 

    private GameManager gameManager; 
    private float oldValue = 0; 
    void Start(){
        gameManager = GameManager.Instance; 
        checkIcon.SetActive(isTicked); 
    }

    //Function that's called when the button is clicked. Toggles/untoggles the check icon and changes whether the slider is interactable
    public void OnClick(){
        isTicked = !isTicked; 
        checkIcon.SetActive(isTicked); 
        slider.enabled = isTicked; 
    }

    public void SetTrue(){
        isTicked = true; 
        checkIcon.SetActive(true); 
        slider.enabled = true; 
    }

    public void ToggleSeparation(){
        if(!isTicked){//Set to not because it's switched by OnClick() before this function is called
            oldValue = gameManager.separationWeight; 
            gameManager.separationWeight = 0; 
            gameManager.ReadParameters(); 
        } else {
            gameManager.separationWeight = oldValue; 
            gameManager.ReadParameters(); 
        }
    }

    public void ToggleCohesion(){
        if(!isTicked){//Set to not because it's switched by OnClick() before this function is called
            oldValue = gameManager.cohesionWeight; 
            gameManager.cohesionWeight = 0; 
            gameManager.ReadParameters(); 
        } else {
            gameManager.cohesionWeight = oldValue; 
            gameManager.ReadParameters(); 
        }
    }

    public void ToggleAlignment(){
        if(!isTicked){//Set to not because it's switched by OnClick() before this function is called
            oldValue = gameManager.alignmentWeight; 
            gameManager.alignmentWeight = 0; 
            gameManager.ReadParameters(); 
        } else {
            gameManager.alignmentWeight = oldValue; 
            gameManager.ReadParameters(); 
        }
    }

    public void ToggleCollision(){
        if(!isTicked){//Set to not because it's switched by OnClick() before this function is called
            oldValue = gameManager.collisionWeight; 
            gameManager.collisionWeight = 0; 
            gameManager.ReadParameters(); 
        } else {
            gameManager.collisionWeight = oldValue; 
            gameManager.ReadParameters(); 
        }
    }
}
