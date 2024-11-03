using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//At the start of the scene all boids read their serialized fields from game manager, it's a bit shorter than changing one boid and applying it to the prefab 
public class GameManager : MonoBehaviour
{
    //Fields to be read
    [SerializeField] public GameObject target; 
    [SerializeField] public float slowDownRadius = 1f; 
    [SerializeField] public float maxSpeed; 
    [SerializeField] public float minSpeed; 
    [SerializeField] public float cruiseSpeed;
    [SerializeField] public float cruiseCoefficient; 
    [SerializeField] public float maxRotation = 300f; 
    [SerializeField] public float viewRadius; [Range(0, 360)]
    [SerializeField] public float viewAngle; 
    [SerializeField] public float separationWeight = 1.0f; 
    [SerializeField] public float collisionWeight = 1.0f; 
    [SerializeField] public float alignmentWeight = 1f; 
    [SerializeField] public float cohesionWeight = 1f; 
    [SerializeField] public float decayCoefficient = 2f; 
    [SerializeField] public float collisionSteps = 25f; 
    [SerializeField] public int raycastAngle; 
    [SerializeField] public float separationDistance = 1.0f; 
    [SerializeField] public float viewCoefficient = 0.3f; 

    //Etc

    public List<BOID> boids = new List<BOID>(); 

    // Singleton Stuff
    static private GameManager instance;
    static public GameManager Instance
    {
        get
        {
            if (instance == null)
            {
                Debug.LogError("There is no GameManager instance in the scene.");
            }
            return instance;
        }
    }

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            instance = this;
        }

        DontDestroyOnLoad(this.gameObject); 
    }

    public void ReadParameters(){
        foreach(BOID boid in boids){
            boid.ReadEditableValues(); 
        }
    }

    public void AddBOID(BOID newBoid){
        boids.Add(newBoid); 
    }

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
