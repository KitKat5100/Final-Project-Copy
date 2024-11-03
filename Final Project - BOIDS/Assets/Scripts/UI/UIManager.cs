using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [SerializeField] private GameObject parameterUI; 
    [SerializeField] private GameObject showParameter; 
    [SerializeField] private GameObject hideParameter; 

    //Sliders
    [SerializeField] private Slider separation; 
    [SerializeField] private Slider cohesion; 
    [SerializeField] private Slider alignment;
    [SerializeField] private Slider collisionAvoidance;  
    [SerializeField] private Slider cruiseSpeed; 
    [SerializeField] private Slider viewRadius; 

    private float minSpeedDiff; 
    private float maxSpeedDiff; 

    private float origAlignmentWeight; 
    private float origCohesionWeight; 
    private float origSeparationWeight; 
    private float origCollisionWeight; 
    private float origCruiseSpeed; 
    private float origViewRadius; 

    //Etc.
    private bool isInitialized = false; 
    private GameManager gameManager; 

    void Start() {
        gameManager = GameManager.Instance; 

        //Show and hide certain UI at the start
        parameterUI.SetActive(false);
        hideParameter.SetActive(false); 
        showParameter.SetActive(true); 
        SetValues();

        minSpeedDiff = Mathf.Abs(gameManager.minSpeed - gameManager.cruiseSpeed); 
        maxSpeedDiff = Mathf.Abs(gameManager.maxSpeed - gameManager.cruiseSpeed);
    }

    void Update() {
        
    }

    //TODO: Fill this up completely
    private void SetValues(){
        origSeparationWeight = gameManager.separationWeight; 
        origCohesionWeight = gameManager.cohesionWeight; 
        origAlignmentWeight = gameManager.alignmentWeight; 
        origCollisionWeight = gameManager.collisionWeight; 
        origCruiseSpeed = gameManager.cruiseSpeed; 
        origViewRadius = gameManager.viewRadius; 
        
        separation.value = origSeparationWeight;
        cohesion.value = origCohesionWeight; 
        alignment.value = origAlignmentWeight;
        collisionAvoidance.value = origCollisionWeight; 
        cruiseSpeed.value = origCruiseSpeed; 
        viewRadius.value = origViewRadius; 
    }

    public void ShowParameters(){
        parameterUI.SetActive(true); //Show main UI panel

        // Show/hide buttons
        showParameter.SetActive(false); 
        hideParameter.SetActive(true); 
    }

    public void HideParameters(){
        parameterUI.SetActive(false); //Show main UI panel

        // Show/hide buttons
        showParameter.SetActive(true); 
        hideParameter.SetActive(false); 
    }

    public void ValueChanged(){     
        gameManager.separationWeight = separation.value; 
        gameManager.cohesionWeight = cohesion.value; 
        gameManager.alignmentWeight = alignment.value; 
        gameManager.collisionWeight = collisionAvoidance.value; 
        gameManager.cruiseSpeed = cruiseSpeed.value; 
        gameManager.minSpeed = Mathf.Max((cruiseSpeed.value - minSpeedDiff), 1); 
        gameManager.maxSpeed = cruiseSpeed.value + maxSpeedDiff; 
        gameManager.viewRadius = viewRadius.value; 
        gameManager.ReadParameters(); 
    }

    public void ResetValues(){
        separation.value = origSeparationWeight; 
        cohesion.value = origCohesionWeight; 
        alignment.value = origAlignmentWeight;
        collisionAvoidance.value = origCollisionWeight; 
        cruiseSpeed.value = origCruiseSpeed; 
        viewRadius.value = origViewRadius;
        ValueChanged();  
    }

}
