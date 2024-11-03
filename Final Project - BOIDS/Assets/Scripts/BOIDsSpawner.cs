using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class BOIDsSpawner : MonoBehaviour
{
    [SerializeField] private int spawnNo; 
    [SerializeField] GameObject BOID; 
    int count = 0; 
    // Start is called before the first frame update
    void Start()
    {   
    }

    // Update is called once per frame
    void FixedUpdate()
    {
            float randomX = Random.Range(-9, 9); 
            float randomZ = Random.Range(-9, 9); 
            Instantiate(BOID,new Vector3(transform.position.x + randomX, transform.position.y, transform.position.z + randomZ), Quaternion.identity); 
            count++; 
            if(count == spawnNo){
                Destroy(this); 
            }
        
    }
}
