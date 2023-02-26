using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;


public class PlayerMovementController : NetworkBehaviour
{   
    [Header ("Settings")]
    [SerializeField] [SyncVar] public int playerHealth = 5;
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

    private float reloadCD;
    public bool isReady = false;
    private bool isGrounded; //something is bugged

    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();

        // Get reference to main camera and set it to follow the player
        cam = Camera.main;
        FindObjectOfType<GameSession>().playerController = this;
        pcAnimator =GetComponent<Animator>();

        // Disable cursor and hide it - not necessary, can be useful for nice looking one 
        // Cursor.lockState = CursorLockMode.Locked;
        // Cursor.visible = false;
    }

    private void Start()
    {
        //Get some basic references for rb and ui handler
        rb = GetComponent<Rigidbody>();
        uiHandler = GameObject.FindObjectOfType<UIHandler>();
    }

    private void Update() 
    {
        // Check for shooting input
        if (Input.GetButtonDown("Fire1") && isReady)
        {
            CmdFire();
        }

        reloadCD -= Time.deltaTime;
    }

    private void FixedUpdate()
    {
        if (!isLocalPlayer || !isReady)  { return;}

        // Get input from horizontal and vertical axes
        float moveHorizontal = Input.GetAxis("Horizontal");
        float moveVertical = Input.GetAxis("Vertical");

        // Calculate movement vector based on input and movement speed 
        Vector3 movement = new Vector3(moveHorizontal , 0f, moveVertical ) * movementSpeed;
        // make it relative to camera rotation
        movement = cam.transform.TransformDirection(movement);

        //cheesy workaround for animation, looks good aaand need lot more work
        pcAnimator.SetFloat("Speed", Vector3.Dot(gameObject.transform.forward, movement));

        // Apply movement to rigidbody, looks bad works good
//        rb.MovePosition(transform.position + movement * Time.deltaTime);

        // Cast a ray from the camera to the mouse position
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);

        // Declare a RaycastHit variable to store information about the hit
        RaycastHit hit;



        if (Physics.Raycast(ray, out hit, 100f, 7))
        {
            // Get the direction to the hit point
            Vector3 rotation = hit.point - transform.position;
            // Ignore the y axis to avoid tilting the character
            rotation.y = 0;

            Quaternion targetRotation =  Quaternion.LookRotation(rotation);

            // Apply rotation to player
            rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRotation, rotationSpeed * Time.deltaTime));

        }

        // Check if player is on the ground
        isGrounded = Physics.Raycast(transform.position, Vector3.down, 1.1f);

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
        NetworkServer.Spawn(bullet);
        reloadCD = reloadTime;
        }

    }

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
        if (!isReady) {return;}
        playerHealth -= amount;
        uiHandler.ChangeHealth(playerHealth.ToString());
        if (playerHealth <= 0)
        {   
            StartCoroutine(PlayerWounded());
        }
    }

    // Death/healing/respawn
    [Server]
    public IEnumerator PlayerWounded()
    {
        isReady = false;

        // need some visual feedback with animator but works as intended
        while (isReady == false)
        {
            yield return new WaitForSeconds(6f); 
            
            isReady = true;
        }

        playerHealth = 5;
        uiHandler.ChangeHealth(playerHealth.ToString());
        
    }

    
}