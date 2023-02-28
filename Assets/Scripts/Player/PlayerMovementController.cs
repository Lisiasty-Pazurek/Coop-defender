using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;


public class PlayerMovementController : NetworkBehaviour
{   
    [Header ("Settings")]
    [SerializeField] [SyncVar (hook = nameof(ChangeHealth))] public int playerHealth = 5;
    [SerializeField] private float movementSpeed = 5f;
    [SerializeField] private float rotationSpeed = 6f;
    [SerializeField] private float reloadTime = .2f;

    [Header ("Prefabs")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform bulletSpawnPoint;

    [Header ("References")]
    public UIHandler uiHandler;
    private Rigidbody rb;
    private Camera cam;
//    public GameSession gameSession;
    public Animator pcAnimator;

    [Header("Attributes")]
    [SyncVar]public bool isAlive = false;    
    private float reloadCD;
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
        uiHandler = GameObject.FindObjectOfType<UIHandler>();
        // Disable cursor and hide it - not necessary, can be useful for nice looking one 
        // Cursor.lockState = CursorLockMode.Locked;
        // Cursor.visible = false;
    }

    public override void OnStartServer()
    {
        base.OnStartServer();
        //Get some basic references for rb and ui handler
        rb = GetComponent<Rigidbody>();
        pcAnimator = GetComponentInChildren<Animator>();

    }

    private void Update() 
    {        if (!isAlive || !isLocalPlayer)  { return;}

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
        }

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
        // Instantiate bullet prefab at spawn point position and rotation
        GameObject bullet = Instantiate(bulletPrefab, bulletSpawnPoint.position, bulletSpawnPoint.rotation);

        // Apply force to bullet in the direction the player is facing
        bullet.GetComponent<Rigidbody>().AddForce(transform.forward * bullet.GetComponent<Bullet>().bulletSpeed);
        bullet.GetComponent<Bullet>().shooter = this.gameObject;
        // Spawn bullet on clients
//        NetworkServer.Spawn(bullet);
        reloadCD = reloadTime;
        }

    }

    [ServerCallback]
    private void OnTriggerEnter(Collider other)
    {
        //Check if get hit by bullet
        if (other.GetComponent<Bullet>() != null)
        {
            Bullet bullet = other.GetComponent<Bullet>();
            if (bullet.canDamagePlayer)
            // Apply damage to player 
            TakeDamage(bullet.damageAmount);
        }
    }

    [Server]
    void TakeDamage(int amount)
    {
        if (!isAlive) {return;}
        playerHealth -= amount;
        if (playerHealth <= 0)
        {   
            StartCoroutine(PlayerWounded());
        }
    }

    // Death/healing/respawn
    [Server]
    public IEnumerator PlayerWounded()
    {
        isAlive = false;

        // need some visual feedback with animator but works as intended
        while (isAlive == false)
        {
            yield return new WaitForSeconds(6f); 
            
            isAlive = true;
        }

        playerHealth = 5;
        
        
    }


    [ServerCallback]
    public void ChangeHealth(int oldValue, int newValue)
    {
        if (!isLocalPlayer) {return;}
        uiHandler.ChangeHealth(playerHealth.ToString());
    }

    
}