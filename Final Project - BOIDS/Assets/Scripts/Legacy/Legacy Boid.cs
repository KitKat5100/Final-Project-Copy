using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor; 

public class LegacyBoid : MonoBehaviour
{
    #region Test Values
    [SerializeField] private bool testBOID = false; 
    [SerializeField] private GameObject target; 
    [SerializeField] private float slowDownRadius = 1f; 
    [SerializeField] private bool usePriorityBlending; 
    #endregion

    #region Velocity
    [SerializeField] private float maxSpeed = 1f;
    private float currentSpeed; 
    private Vector3 desiredVelocity = Vector3.forward; //Might not need this here 
    public Vector3 DesiredVelocity{ //Desired move direction 
        get{
            return desiredVelocity; 
        }
    }
    private Vector3 movDirection; //Current move direction
    public Vector3 MovDirection{
        get{
            return movDirection; 
        }
    }
    
    private Vector3 collisionAvoidance; 
    #endregion

    #region Rotation
    [SerializeField] private float maxRotation = 40f; 
    #endregion

    #region Sight Cones
    [SerializeField] private float viewRadius; [Range(0, 360)]
    [SerializeField] private float viewAngle; 

    [SerializeField] private LayerMask targetMask; 
    [SerializeField] private LayerMask borderMask; 

    //Arrays for boids in sight
    private List<Transform> nearbyBOIDs = new List<Transform>(); //For the boid
    //private List<float> BOIDDistance = new List<float>(); //For the distance
    #endregion

    #region Core Vectors
    private Vector3 separation = Vector3.zero; 
    private Vector3 alignment = Vector3.zero;
    private Vector3 cohesion = Vector3.zero;

    [SerializeField] private float separationWeight = 1.0f; 
    [SerializeField] private float separationDistance = 1.0f; 
    [SerializeField] private float alignmentWeight = 1.0f; 
    [SerializeField] private float cohesionWeight = 1.0f; 
    [SerializeField] private float decayCoefficient = 2f;
    #endregion


    #region Etc.
    [SerializeField] private float checkDistance = 1f; 
    [SerializeField] private float smallCheckDistance; 
    [SerializeField] private int raycastAngle; 
    private int raycastNo; 
    private GameManager gameManager; 
    #endregion

    void Awake(){
    }

    void OnEnable(){
    }

    void Start() {   
        gameManager = GameManager.Instance;  //Get GameManager
        ReadValues(); 

        currentSpeed = maxSpeed; //So that the speed of the fish can change without losing it's base value
        raycastNo = (int) 180/(int)viewAngle; 
        //raycastNo = (int) (viewAngle/2) / raycastAngle; //Determine how many collision avoidance raycasts we do based on fish's view radius 
        //Randomize starting movDirection; 
        movDirection = new Vector3(Random.Range(-0.5f, 0.5f), 0, Random.Range(-0.5f, 0.5f));
        movDirection.Normalize();   
    }

    void Update()
    {
        //Rotate Object
        if(desiredVelocity != Vector3.zero){
            Quaternion toRotation = Quaternion.LookRotation(desiredVelocity, Vector3.up);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, toRotation, maxRotation * Time.deltaTime);
        }
        
        //Move object
        transform.Translate(Vector3.forward * currentSpeed * Time.deltaTime, Space.Self); 
    }

    void FixedUpdate(){
        FieldOfViewCheck(); 
        CheckCollision(); 

        if(nearbyBOIDs.Count != 0){ //If there are neighbours to take into account
            //Get input vectors
            alignment = GetAlignment(); 
            cohesion = GetCohesion(); 
            separation = GetSeparation(); 

            if(usePriorityBlending){
                desiredVelocity = collisionAvoidance; 

                if(desiredVelocity.magnitude <= maxSpeed){ //not above threshold
                    desiredVelocity += separation; 

                    if(desiredVelocity.magnitude <= maxSpeed){
                        desiredVelocity += cohesion; //add cohesion
                        if(desiredVelocity.magnitude <= maxSpeed){ //not above threshold
                            alignment.Normalize(); 
                            alignment *= (maxSpeed - desiredVelocity.magnitude);
                            desiredVelocity += alignment; //add alignment
                        }
                    }
                }

                if(desiredVelocity.magnitude >= maxSpeed){ //Clip the vector if its above maxSpeed
                    desiredVelocity.Normalize(); 
                    desiredVelocity = desiredVelocity * maxSpeed; 
                }

                if(testBOID){  
                }

            } else {
                //Weight them 
                alignment *= alignmentWeight; 
                cohesion *= cohesionWeight;
                separation *= separationWeight; 

                if(testBOID){
                Debug.Log("Alignment Vector: " + alignment.magnitude);     
                Debug.Log("Cohesion Vector: " + cohesion.magnitude);    
                Debug.Log("Separation Vector: " + separation.magnitude);    
                }

                //Add, normalize, and scale
                desiredVelocity = alignment + cohesion + separation; 
                desiredVelocity.Normalize(); 
                desiredVelocity *= maxSpeed; 
            }
            movDirection = desiredVelocity; 
        } else { 
            desiredVelocity = movDirection; 
        }
 
            desiredVelocity = ConvertTo2D(desiredVelocity); 

    }

    #region Core Vectors
    private Vector3 GetAlignment(){
        Vector3 sum = new Vector3(0, 0, 0); 
        int count = 0; 

        //Gets the summation of neighbour move directions, weighted by distance. 
        foreach (Transform neighbour in nearbyBOIDs){
            Vector3 neighbourVelocity = neighbour.gameObject.GetComponent<BOID>().CurrentVelocity; //MovDirection is already normalized
            float distance = Vector3.Distance(transform.position, neighbour.position);
            sum += neighbourVelocity/distance;
            count ++; 
        }

        //Normalize, and scale it
        //Debug.Log("Alignment magnitude: " + sum.magnitude); 
        sum.Normalize(); 
        sum = sum * maxSpeed;

        return sum; 
    }

    //TODO: Make it predict whether or not it is actually going to collide with the nearby boid. Only factor it into the average if it will. 
    private Vector3 GetSeparation(){
        Vector3 sum = new Vector3(0, 0, 0); 

        //Gets the summation of opposite neighbour directions, weighted by distance
        foreach (Transform neighbour in nearbyBOIDs){
            Vector3 neighbourDir = neighbour.position - transform.position; //vector from this to neighbour
            float distance = neighbourDir.magnitude; 

            if(distance < separationDistance) { //Needed because separation check has to have a smaller radius than cohesion. 
            //Calculate the strength of repulsion using inverse square law
            
            float strength = Mathf.Min(decayCoefficient/(distance * distance), maxSpeed);
            neighbourDir.Normalize(); 
            sum = sum + (-neighbourDir * strength);
            }
        }

        //Normalize and scale it
        //Debug.Log("Separation Magnitude" + sum.magnitude);

        return sum; 
    }

    private Vector3 GetCohesion(){
        Vector3 sum = new Vector3(0, 0, 0); 

        //Gets the summation of vector to neighbours, weighted by distance 
        foreach (Transform neighbour in nearbyBOIDs){
            Vector3 neighbourDir = neighbour.position - transform.position; //vector from this to neighbour
            float distance = neighbourDir.magnitude;
            //Calculate the strength of attraction using inverse square law

            float strength = Mathf.Min(decayCoefficient/(distance * distance), maxSpeed);
            neighbourDir.Normalize(); 
            sum = sum + (neighbourDir * strength);
        }

        //Normalize and scale it
        //Debug.Log("Cohesion Magnitude: " + sum.magnitude);

        return sum; 
    }
    #endregion

    #region Gizmos
    void OnDrawGizmos(){
        //DEBUGGING: draw where it's going
        Gizmos.color = new Color(0, 0, 1f, 0.3f); 
        Gizmos.DrawLine(transform.position, transform.position + (movDirection));

        Gizmos.color = new Color(0, 1f, 0, 0.3f); 
        Gizmos.DrawLine(transform.position, transform.position + (collisionAvoidance));

        //Draws FoV for looking at other boids
        // #if UNITY_EDITOR
        // Handles.color = new Color(1, 0, 0, 0.2f);
        // Vector3 startCurve = Quaternion.AngleAxis(-viewAngle/2, Vector3.up) * transform.forward; 
        // Handles.DrawSolidArc(transform.position, Vector3.up, startCurve, viewAngle, viewRadius); 
        // #endif
    }

    void OnDrawGizmosSelected(){
        //Draws their view cone
        Handles.color = new Color(1, 0, 0, 0.1f);
        Vector3 startCurve = Quaternion.AngleAxis(-viewAngle/2, Vector3.up) * transform.forward; 
        Handles.DrawSolidArc(transform.position, Vector3.up, startCurve, viewAngle, viewRadius); 

        //Draws which boids they can see
        for(int i = 0; i < nearbyBOIDs.Count; i++){
            Gizmos.DrawLine(transform.position, nearbyBOIDs[i].position); 
        }

        // //Draws Alignment Vector 
        // Gizmos.color = Color.red;
        // Gizmos.DrawLine(transform.position, transform.position + alignment);

        //Draws separation Vector; 
        Gizmos.color = Color.green;
        Gizmos.DrawLine(transform.position, transform.position + (collisionAvoidance));
    }
    #endregion

    #region Non-Automatic Functions

    private Vector3 ConvertTo2D(Vector3 vector){
        return new Vector3(vector.x, 0, vector.z); 
    }
    private void FieldOfViewCheck(){
        nearbyBOIDs.Clear(); //Clear previous neighbours
        Collider[] rangeChecks = Physics.OverlapSphere(transform.position, viewRadius, targetMask); //Define sight cone

        if(rangeChecks.Length > 1){ //If we found anything
            foreach (Collider newBoid in rangeChecks){
                Transform target = newBoid.transform; 
                if(target != transform){ //if it's not me 
                    Vector3 targetDir = (target.position - transform.position); //Direction to target
                    targetDir.Normalize(); 
                    if(Vector3.Angle(transform.forward, targetDir) < viewAngle/2){ //If they are within the view cone
                        nearbyBOIDs.Add(target); //They are seen, Yippee!
                    } 
                }
            }
        }
    }


    //Collision detection, fanning raycasts until vacant path is found
    private void CheckCollision(){
        bool pathFound = true; 
        //Make rays here to see things easier
        Ray borderCheckRay = new Ray(transform.position, transform.forward * checkDistance); 

        //Debug Draws
        Debug.DrawRay(transform.position, transform.forward * checkDistance); //draw central raycast

        RaycastHit centralHit; 
        //Checks central ray
        if(Physics.Raycast(borderCheckRay, out centralHit, checkDistance, borderMask)){
            Debug.DrawRay(transform.position, transform.forward * checkDistance, Color.red); //Draw central raycast in red
            
            //Things needed for the incoming loop
            pathFound = false; 
            bool isRight = true; 
            int count = 1; 
            Vector3 originalRay = transform.forward*checkDistance;  

            while(pathFound == false || count <= raycastNo){
                if(isRight){    
                    Vector3 newRay = Quaternion.Euler(0, raycastAngle * (count + 1/2), 0) * originalRay; 
                    pathFound = PerformRaycast(newRay); 
                    isRight = false; 
                } else {
                    Vector3 newRay = Quaternion.Euler(0, -raycastAngle * (count + 1/2), 0) * originalRay; 
                    pathFound = PerformRaycast(newRay); 
                    isRight = true; 
                    count++; 
                }
            }

            // //Rotate the raycast, set to a fixed number for now 
            // for(int i = 1; i <= raycastNo; i++){
                
            //     if(isRight){
            //         Vector3 newRay = Quaternion.Euler(0, raycastAngle * (i + 1/2), 0) * originalRay; 
            //         pathFound = PerformRaycast(newRay);
            //         isRight = false; 
            //         i--; 
            //     } else {
            //         Vector3 newRay = Quaternion.Euler(0, -raycastAngle * (i + 1/2), 0) * originalRay; 
            //         PerformRaycast(newRay); 
            //         //Debug.DrawRay(transform.position, newRay, Color.blue); 
            //         isRight = true; 
            //     }
            // }
        }

    }

    private bool PerformRaycast(Vector3 toDirection){
        if(Physics.Raycast(transform.position, toDirection, checkDistance, borderMask)){ //If bumped into something ;-; 
            Debug.DrawRay(transform.position, toDirection, Color.red);  
            return false;
        }

        //Vacant Path found
        Vector3 target = toDirection - transform.position; 
        movDirection = target; 
        Debug.DrawRay(transform.position, toDirection, Color.blue); 
        return true;  
    }

    //TODO: Do I still need this? 
    private void GetTarget(RaycastHit raycast, Transform originTransform){
        Vector3 target = raycast.point + (raycast.normal * checkDistance);
        // Vector3 target = originTransform.position + (originTransform.forward * checkDistance); 
        target = (target - transform.position) * 3; 
        collisionAvoidance = target; 
    }
    #endregion

    #region Steering Behaviours
    private void Seek(){
        movDirection = target.transform.position - transform.position; 
        movDirection.Normalize(); 
    }

    private void Arrive(){
        Vector3 distance = target.transform.position - transform.position; 
        if(distance.magnitude < slowDownRadius){ //It is close enough to start slowing down
            desiredVelocity = movDirection; 
            currentSpeed = maxSpeed * (distance.magnitude/slowDownRadius);

            //Possibly add something that triggers once it's arrived
        }
    }
    #endregion

    #region Etc. 
    //Used to read serialized field from GameManager at the start of the scene. 
    public void ReadValues(){
        target = gameManager.target;
        slowDownRadius = gameManager.slowDownRadius; 
        maxSpeed = gameManager.maxSpeed; 
        maxRotation = gameManager.maxRotation; 
        viewRadius = gameManager.viewRadius; 
        viewAngle = gameManager.viewAngle; 
        separationWeight = gameManager.separationWeight; 
        alignmentWeight = gameManager.alignmentWeight; 
        cohesionWeight = gameManager.cohesionWeight; 
        separationDistance = gameManager.separationDistance; 
    }
    #endregion 
}
