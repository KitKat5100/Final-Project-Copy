using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BOIDExit : MonoBehaviour
{
    [SerializeField] private GameObject boidPrefab; 
    private float colliderX; 
    private float colliderY; 
    private float colliderZ; 

    private float boidX; 
    private float boidY; 
    private float boidZ; 

    private BoxCollider boxCollider; 
    private BoxCollider boidCollider;

    void OnTriggerExit(Collider other){
        //Grab current position of BOID
        Vector3 displacement = other.gameObject.transform.position; 
        float newX = displacement.x; 
        float newY = displacement.y;
        float newZ = displacement.z;

        //Check if outside of Z bounds
        if(newZ + boidZ > colliderZ){
            newZ = -(newZ - boidZ); 
        } else if (newZ - boidZ < -colliderZ){
            newZ = -(newZ + boidZ);
        }

        //Check if outside of X bounds
        if(newX + boidX > colliderX){
            newX = -(newX - boidZ);  
        } else if (newX - boidX < -colliderX){
            newX = -(newX + boidZ);  
        }

        //Check if outside of Y bounds 
        //TODO: Change this to the more complicated version 
        if(newY + boidY > colliderY || newY - boidY < -colliderY){
            newY = -newY; 
        }
        
        //Change the object position. 
        other.gameObject.transform.position = new Vector3(newX, newY, newZ); 
    }

    // Start is called before the first frame update
    void Start()
    {
        //Get size of collider
        boxCollider = this.gameObject.GetComponent<BoxCollider>(); 
        colliderX = boxCollider.size.x/2; 
        colliderY = boxCollider.size.y/2; 
        colliderZ = boxCollider.size.z/2;

        //Get size of the colider on the boid. 
        boidCollider = boidPrefab.GetComponent<BoxCollider>();
        boidX = boidCollider.size.x * 2; 
        boidY = boidCollider.size.y * 2;
        boidZ = boidCollider.size.z * 2; 
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
