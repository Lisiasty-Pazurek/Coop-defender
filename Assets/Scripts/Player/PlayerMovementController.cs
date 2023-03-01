using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;


public class PlayerMovementController : NetworkBehaviour
{   
    [Header ("Settings")]
    [SerializeField] private float movementSpeed = 5f;
    [SerializeField] private float rotationSpeed = 6f;
    [SerializeField] private float reloadTime = .4f;

    [Header ("Prefabs")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform bulletSpawnPoint;

    [Header ("References")]
    public Health health;
    private Rigidbody rb;
    private Camera cam;
//    public GameSession gameSession;
    public Animator pcAnimator;

    [Header("Attributes")]

    [SerializeField] [SyncVar]public float reloadCD; // for debugging - going to pass it to bullet
    private bool isGrounded; //something is bugged

    [Header("Debugging")]
    [Tooltip("plz readonly values")]
    public Vector3 movement;
    public Vector3 rotation;

    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();
        // Get reference to main camera and set it to follow the player
        cam = Camera.main;
        FindObjectOfType<GameSession>().playerController = this;
        pcAnimator = GetComponentInChildren<Animator>();
        rb = GetComponent<Rigidbody>();
        health = GetComponent<Health>();


    }

    public override void OnStartServer()
    {
        base.OnStartServer();
        //Get some basic references 
        rb = GetComponent<Rigidbody>();
        health = GetComponent<Health>();
        pcAnimator = GetComponentInChildren<Animator>();

    }

    private void Update() 
    {       if (!health.isAlive || !isLocalPlayer)  { return;}

            RotateTowardsCursor();

            // Get input from horizontal and vertical axes
            float moveHorizontal = Input.GetAxis("Horizontal");
            float moveVertical = Input.GetAxis("Vertical");

            // Calculate movement vector based on input and movement speed 
            movement = new Vector3(moveHorizontal, 0f, moveVertical) * movementSpeed;
            // make it relative to camera rotation
            movement = cam.transform.TransformDirection(movement);



        //cheesy workaround for animation, looks better but need lot more work
        // Vector3.Dot is nice to get input on axis relative to pointer and character rotation
        pcAnimator.SetFloat("Speed", Vector3.Dot(gameObject.transform.forward, movement));
        pcAnimator.SetFloat("Direction", Vector3.Dot(gameObject.transform.right, movement));

        // Apply movement to rigidbody, looks bad works good
        rb.MovePosition(transform.position + movement * Time.deltaTime);

        // Check if player is on the ground
        isGrounded = Physics.Raycast(transform.position, Vector3.down, 1.1f);

        // Check for shooting input
        if (Input.GetButtonDown("Fire1") )
        {
            CmdFire();
            Debug.Log("pew pew");
            reloadCD = reloadTime;        // wy not work? - if it is here - becasue scipt is set up to sync from client to server
        }

        // prolly should change logic to make it server side
        reloadCD -= Time.deltaTime;
    }


    [Client]
    void RotateTowardsCursor()
    {
        // Cast a ray from the camera to the mouse position
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);

        // Declare a RaycastHit variable to store information about the hit
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 100f, 7))
        {
            // Get the direction to the hit point
            rotation = hit.point - transform.position;
            // Ignore the y axis to avoid tilting the character
            rotation.y = 0;

            Quaternion targetRotation =  Quaternion.LookRotation(rotation);

            // Apply rotation to player
            rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRotation, rotationSpeed * Time.deltaTime));
        }

    }


    [Command]
    public void CmdFire()
    {
        if (reloadCD <= 0) 
        {
            // Diff weapons = diff bullets. New class might be usefull, also.. shooting cooldown 
            Shoot();
        }

    }

    [Server]
    public void Shoot ()
    {        
//        Debug.Log("reloading for: " + reloadCD + "/" + reloadTime);
        // Instantiate bullet prefab at spawn point position and rotation        
        GameObject bullet = Instantiate(bulletPrefab, bulletSpawnPoint.position, bulletSpawnPoint.rotation);
        // Spawn bullet on server and clients <- dont do this as networked object, RPC visuals without rb/network identity etc.
        NetworkServer.Spawn(bullet);
        bullet.GetComponent<Bullet>().shooter = this.gameObject;
        // Apply force to bullet in the direction the player is facing
        bullet.GetComponent<Rigidbody>().AddForce(transform.forward * bullet.GetComponent<Bullet>().bulletSpeed);
    }




    private void OnTriggerEnter(Collider other)
    {
        //Check if get hit by bullet
        if (other.GetComponent<Bullet>() != null)
        {
            Bullet bullet = other.GetComponent<Bullet>();
            if (bullet.canDamagePlayer)
            // Apply damage to player 
            health.TakeDamage(bullet.damageAmount);
        }
    }
    
}