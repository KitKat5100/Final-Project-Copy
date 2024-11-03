using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor; 

public class BOID : MonoBehaviour
{
    #region Test Values
    [SerializeField] private bool testBOID = false; 
    [SerializeField] private GameObject target; 
    [SerializeField] private float slowDownRadius = 1f; 
    #endregion

    #region Velocity
    [SerializeField] private float minSpeed = 7f; 
    [SerializeField] private float maxSpeed = 7f;
    [SerializeField] private float cruiseSpeed = 7f; 
    [SerializeField] private float baseCruiseCoefficient = 0.5f; 
    private float cruiseCoefficient; 
    private bool isOverCruise = false;
    private float viewCoefficient = 0.1f;  
    private float currentSpeed = 7; 
    private Vector3 acceleration = Vector3.forward; //Might not need this here 
    public Vector3 Acceleration{ //Desired move direction 
        get{
            return acceleration; 
        }
    }
    private Vector3 currentVelocity; 
    public Vector3 CurrentVelocity{
        get{
            return currentVelocity; 
        }
    }
    #endregion

    #region Rotation
    [SerializeField] private float maxRotation = 40f; 
    #endregion

    #region Sight Cones
    [SerializeField] private float baseViewRadius;
    private float viewRadius; 
     [Range(0, 360)] [SerializeField] private float viewAngle; 
    [SerializeField] private LayerMask targetMask; 
    [SerializeField] private LayerMask borderMask; 

    //Arrays for boids in sight
    private List<Transform> nearbyBOIDs = new List<Transform>(); //For the boids, used by cohesion
    private List<Transform> alignmentBOIDs = new List<Transform>(); //For the boids, used by alignment
    #endregion

    #region Core Vectors
    private Vector3 separation = Vector3.zero; 
    private Vector3 alignment = Vector3.zero;
    private Vector3 cohesion = Vector3.zero;
    private Vector3 collisionAvoidance = Vector3.zero; 

    [SerializeField] private float separationWeight = 1.0f; 
    [SerializeField] private float separationDistance = 1.0f; 
    private float alignmentDistance; 
    [SerializeField] private float alignmentWeight = 1.0f; 
    [SerializeField] private float cohesionWeight = 1.0f; 
    [SerializeField] private float collisionWeight = 1.0f; 
    [SerializeField] private float decayCoefficient = 2f;
    [SerializeField] private float collisionSteps = 20f;
    private Vector3 collisionDecayCoefficient = Vector3.zero; 
    #endregion


    #region Etc.
    [SerializeField] private int raycastAngle; 
    private int raycastNo; 
    private GameManager gameManager; 
    #endregion

    void Start() {   
        gameManager = GameManager.Instance;  //Get GameManager
        ReadValues();
        gameManager.AddBOID(this); 
        viewRadius = baseViewRadius; 
        cruiseCoefficient = baseCruiseCoefficient; 
        alignmentDistance = viewRadius/2; 
        currentSpeed = minSpeed; //So that the speed of the fish can change without losing it's base value
        raycastNo = 180/(int)raycastAngle; 
        currentVelocity = new Vector3(Random.Range(-0.5f, 0.5f), 0, Random.Range(-0.5f, 0.5f));
        currentVelocity.Normalize();   
    }

    void Update()
    {
        //Rotate Object
        if(acceleration != Vector3.zero){
            Quaternion toRotation = Quaternion.LookRotation(currentVelocity, Vector3.up);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, toRotation, maxRotation * Time.deltaTime);
        }
        
        //Move Boid
        transform.Translate(Vector3.forward * currentSpeed * Time.deltaTime, Space.Self); 
    }

    void FixedUpdate(){
        FieldOfViewCheck(); 
        acceleration = Vector3.zero; 

        //TODO: Decrease the collision vector by a certain amount each frame OR make them seek and arrive and when they arrive they let go of the vector

        if(nearbyBOIDs.Count != 0){ //If there are neighbours to take into account
            //Get input vectors
            alignment = GetAlignment(); //this has to be first
            cohesion = GetCohesion(); 
            separation = GetSeparation(); 

            //Weight them 
            alignment *= alignmentWeight; 
            cohesion *= cohesionWeight;
            separation *= separationWeight; 

            //Add, normalize, and scale
            acceleration = alignment + cohesion + separation; 
        }

        CheckCollision(); 
        if(collisionAvoidance != Vector3.zero){
            collisionAvoidance -= collisionDecayCoefficient; 
        }
        acceleration += (collisionAvoidance * collisionWeight); 

        
        currentVelocity.Normalize();  //here so that last frame's velocity doesn't overtake things
        currentVelocity *= minSpeed; 
        
        currentVelocity += acceleration; 
        currentVelocity = ClampToSpeed(currentVelocity); 
        currentVelocity = ConvertTo2D(currentVelocity);  
        //Debug.Log("Current Velocity: " + currentVelocity.magnitude + ", Acceleration: " + acceleration.magnitude); 

        UpdateViewRadius(); 
    }

    #region Core Vectors
    private Vector3 GetAlignment(){
        Vector3 sum = new Vector3(0, 0, 0); 
        //Gets the summation of neighbour move directions, weighted by distance. 
        foreach (Transform neighbour in nearbyBOIDs){
            Vector3 neighbourVelocity = neighbour.gameObject.GetComponent<BOID>().CurrentVelocity; 
            float distance = Vector3.Distance(transform.position, neighbour.position);
            if(distance < alignmentDistance){
                            sum += neighbourVelocity/distance;
            }
        }

        sum = ClampToSpeed(sum); 
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
        return sum; 
    }
    #endregion

    #region Gizmos
    void OnDrawGizmos(){
        //DEBUGGING: draw where it's going
        Gizmos.color = new Color(0, 1, 0f, 0.3f); 
        Gizmos.DrawLine(transform.position, transform.position + (collisionAvoidance));

    }

    void OnDrawGizmosSelected(){
        //Draws their view cone
        Handles.color = new Color(1, 0, 0, 0.1f);
        Vector3 startCurve = Quaternion.AngleAxis(-viewAngle/2, Vector3.up) * transform.forward; 
        Handles.DrawSolidArc(transform.position, Vector3.up, startCurve, viewAngle, viewRadius); 

        //Draws which boids they can see
        // for(int i = 0; i < nearbyBOIDs.Count; i++){
        //     Gizmos.DrawLine(transform.position, nearbyBOIDs[i].position); 
        // }

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


    //Clamping to speed
    private Vector3 ClampToSpeed(Vector3 vector){
        float speed = vector.magnitude; 
        vector.Normalize(); 
        speed = Mathf.Clamp(speed, minSpeed, maxSpeed); 
        currentSpeed = speed;
        return ApplyCruiseSpeed(vector * speed); 
    }

    //Applying cruiseSpeed
    private Vector3 ApplyCruiseSpeed(Vector3 vector){
        float speed = vector.magnitude; 
        bool temp; //where we store whether or not we have achieved cruise speed

        if(vector.magnitude > cruiseSpeed){ //if greater than cruise speed
            temp = true;
            vector.Normalize(); 

            if (temp == isOverCruise){ //we are still over cruise
                cruiseCoefficient += baseCruiseCoefficient; //increase coefficient
            } else { //not anymore over cruise; 
                isOverCruise = !isOverCruise;
                cruiseCoefficient = baseCruiseCoefficient; 
            }
            return vector * (speed + (-cruiseCoefficient)); 

        } else if(vector.magnitude < cruiseSpeed){ //if less than cruise speed
            temp = false;
            vector.Normalize(); 

            if (temp == isOverCruise){ //we are still over cruise
                cruiseCoefficient += baseCruiseCoefficient; //increase coefficient
            } else { //not anymore over cruise; 
                isOverCruise = !isOverCruise;
                cruiseCoefficient = baseCruiseCoefficient; 
            }

            return vector * (speed + cruiseCoefficient); 
        } 

        cruiseCoefficient = baseCruiseCoefficient; 
        return vector; 
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

    //Implements dynamic viewRadius length based on school density 
    private void UpdateViewRadius(){
        float newRadius = baseViewRadius - (nearbyBOIDs.Count * viewCoefficient);  
        viewRadius = Mathf.Max(separationDistance, newRadius); 
    }


    //Collision detection, fanning raycasts until vacant path is found
    private void CheckCollision(){
        bool pathFound = true; 
        //Make rays here to see things easier
        Ray borderCheckRay = new Ray(transform.position, transform.forward * viewRadius); 

        RaycastHit centralHit; 
        //Checks central ray
        if(Physics.SphereCast(transform.position, 0.5f, transform.forward * viewRadius, out centralHit, viewRadius, borderMask)){
            Debug.DrawRay(transform.position, transform.forward * viewRadius, Color.red); //Draw central raycast in red
            
            //Things needed for the incoming loop
            pathFound = false; 
            bool isRight = true; 
            int count = 1; 
            Vector3 originalRay = transform.forward*viewRadius;  

            while(pathFound == false && count <= raycastNo){
                if(isRight){    
                    Vector3 newRay = Quaternion.Euler(0, raycastAngle * (count + 1/2), 0) * originalRay; 
                    pathFound = PerformRaycast(newRay); 
                    isRight = false; 
                } else if (!isRight) {
                    Vector3 newRay = Quaternion.Euler(0, -raycastAngle * (count + 1/2), 0) * originalRay; 
                    pathFound = PerformRaycast(newRay); 
                    isRight = true; 
                    count++; 
                }
            }
        }

    }

    private bool PerformRaycast(Vector3 toDirection){
        if(Physics.Raycast(transform.position, toDirection, viewRadius, borderMask)){ //If bumped into something ;-; 
            Debug.DrawRay(transform.position, toDirection, Color.red);  
            return false;
        }

        //Vacant Path found
        Vector3 target = toDirection;
        collisionAvoidance = target; 
        collisionDecayCoefficient = collisionAvoidance/collisionSteps; 
        Debug.DrawRay(transform.position, toDirection, Color.blue); 

        return true;  
    }

    #endregion

    #region Steering Behaviours
    private void Seek(){
        currentVelocity = target.transform.position - transform.position; 
        currentVelocity.Normalize(); 
    }

    private void Arrive(){
        Vector3 distance = target.transform.position - transform.position; 
        if(distance.magnitude < slowDownRadius){ //It is close enough to start slowing down
            acceleration = currentVelocity; 
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
        minSpeed = gameManager.minSpeed; 
        cruiseSpeed = gameManager.cruiseSpeed; 
        baseCruiseCoefficient = gameManager.cruiseCoefficient; 
        maxRotation = gameManager.maxRotation; 
        baseViewRadius = gameManager.viewRadius; 
        viewAngle = gameManager.viewAngle; 
        separationWeight = gameManager.separationWeight; 
        alignmentWeight = gameManager.alignmentWeight; 
        collisionWeight = gameManager.collisionWeight;
        decayCoefficient = gameManager.decayCoefficient; 
        cohesionWeight = gameManager.cohesionWeight; 
        separationDistance = gameManager.separationDistance; 
        raycastAngle = gameManager.raycastAngle; 
        collisionSteps = gameManager.collisionSteps; 
        viewCoefficient = gameManager.viewCoefficient; 
    }

    public void ReadEditableValues(){
        maxSpeed = gameManager.maxSpeed; 
        minSpeed = gameManager.minSpeed; 
        cruiseSpeed = gameManager.cruiseSpeed; 
        baseViewRadius = gameManager.viewRadius; 
        alignmentDistance = Mathf.Max(baseViewRadius/2, gameManager.separationDistance); 
        cohesionWeight = gameManager.cohesionWeight; 
        separationWeight = gameManager.separationWeight; 
        alignmentWeight = gameManager.alignmentWeight; 
        collisionWeight = gameManager.collisionWeight;
    }
    #endregion 
}
