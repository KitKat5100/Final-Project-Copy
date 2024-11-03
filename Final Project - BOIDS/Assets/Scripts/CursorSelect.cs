using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Events;

public class CursorSelect : MonoBehaviour
{
    private Vector2 mousePos; 
    private GameObject previousObject = null; 
    private Transform itemToMove = null; 
    private bool started = false; 
    Camera cam;
    [SerializeField] private LayerMask obstacles; 

    //Arrow Stuff
    private Arrow previousArrow; //The selected arrow
    [SerializeField] private GameObject goneArrowPos; //Where we hide the arrow  
    [SerializeField] private GameObject moveArrows; //Moving selected objects

    // Start is called before the first frame update
    void Start()
    {
        cam = Camera.main;
    }

    // Update is called once per frame
    void Update()
    {
        if(previousArrow != null){ //Move arrow and item 

            if(previousArrow.isXAxis){
                Vector3 oldPoint = cam.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, cam.nearClipPlane + (itemToMove.position.z/3)));
                mousePos = Input.mousePosition; 
                Vector3 newPoint = cam.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, cam.nearClipPlane + (itemToMove.position.z/3)));
                float difference = newPoint.x - oldPoint.x;
                difference *= 1.76f; 
                moveArrows.transform.position = new Vector3(moveArrows.transform.position.x + difference, moveArrows.transform.position.y, moveArrows.transform.position.z);
                itemToMove.position = new Vector3(itemToMove.position.x + difference, itemToMove.transform.position.y, itemToMove.transform.position.z); 
            } else {
                Vector3 oldPoint = cam.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, cam.nearClipPlane + (itemToMove.position.z/2)));
                mousePos = Input.mousePosition; 
                Vector3 newPoint = cam.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, cam.nearClipPlane + (itemToMove.position.z/2)));
                float difference = newPoint.z - oldPoint.z;
                difference *= 3.3f; 
                moveArrows.transform.position = new Vector3(moveArrows.transform.position.x, moveArrows.transform.position.y, moveArrows.transform.position.z + difference);
                itemToMove.position = new Vector3(itemToMove.position.x, itemToMove.transform.position.y, itemToMove.transform.position.z + difference); 
            }


        } else { //Not moving arrow and item
            if(previousObject != null){
                previousObject.SetActive(false); 
            }
            mousePos = new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0);
            Ray ray = cam.ScreenPointToRay(mousePos);

            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, 5000, obstacles)) {
                GameObject obstacle = hit.collider.gameObject; 
                if(obstacle.tag == "Arrow"){//hovering over an arrow 

                } else { //Hovering over an obstacle
                    obstacle = obstacle.transform.GetChild(0).gameObject; 
                    obstacle.SetActive(true); 
                    previousObject = obstacle;
                } 
            }
        }
    }

    public void OnClick(InputAction.CallbackContext context){
        if(context.performed){
            started = !started; //first call will always be true
        }

        if(started){ //On click down
            Ray ray = cam.ScreenPointToRay(mousePos);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, 5000, obstacles)) 
            {
                GameObject obstacle = hit.collider.gameObject; 

                if(obstacle.tag == "Arrow"){ //Once they select an arrow
                    Arrow arrowScript = obstacle.GetComponent<Arrow>(); 
                    previousArrow = arrowScript; 
                    mousePos = Input.mousePosition; 
                    arrowScript.OnSelect(); 
                } else {
                    previousArrow = null; 
                }
            } 
        } else if(!started){ //on click up 
            if(previousArrow != null){ //Once they deselect an arrow do nothing else
                previousArrow.OnDeselect(); 
                previousArrow = null; 
            } else {
                Ray ray = cam.ScreenPointToRay(mousePos);
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit, 5000, obstacles)) 
                {
                    GameObject obstacle = hit.collider.gameObject; 
                    itemToMove = obstacle.transform; 
                    obstacle = obstacle.transform.GetChild(1).gameObject; 
                    moveArrows.transform.position = obstacle.transform.position; 
                    
                } else { //Remove the move items arrow 
                    moveArrows.transform.position = goneArrowPos.transform.position; 
                }
            }
        }
    }
}
