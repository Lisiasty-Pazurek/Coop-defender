using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;


public class PlayerController : NetworkBehaviour
{   
    [SerializeField] public int playerHealth = 5;
    [SerializeField] private float movementSpeed = 5f;
    [SerializeField] private float rotationSpeed = 10f;
    [SerializeField] private float jumpForce = 6f;
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform bulletSpawnPoint;

    private Rigidbody rb;
    private Camera cam;
    [SerializeField] private bool isGrounded; //debugging

    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();

        // Get reference to main camera and set it to follow the player
        cam = Camera.main;

        // Disable cursor and hide it - not exactly necessary 
        // Cursor.lockState = CursorLockMode.Locked;
        // Cursor.visible = false;
    }

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void Update() 
    {
        // Check for shooting input
        if (Input.GetButtonDown("Fire1"))
        {
            CmdFire();
        }
    }

    private void FixedUpdate()
    {
        if (!isLocalPlayer)  { return;}

        // Get input from horizontal and vertical axes
        float moveHorizontal = Input.GetAxis("Horizontal");
        float moveVertical = Input.GetAxis("Vertical");

        // Calculate movement vector based on input and movement speed 
        Vector3 movement = new Vector3(moveHorizontal , 0f, moveVertical ) * movementSpeed;
        // make it relative to camera rotation
        movement = cam.transform.TransformDirection(movement);
        // Apply movement to rigidbody
        rb.MovePosition(transform.position + movement * Time.deltaTime);

        // Cast a ray from the camera to the mouse position
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);

        // Declare a RaycastHit variable to store information about the hit
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
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

        if (isGrounded && Input.GetKeyDown(KeyCode.Space))
        {
            // Apply jump force to rigidbody - nat workin yet
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }
    }

    [Command]
    public void CmdFire()
    {
        // Diff weapons = diff bullets. New class might be usefull, also.. shooting cooldown 
        // Instantiate bullet prefab at spawn point position and rotation
        GameObject bullet = Instantiate(bulletPrefab, bulletSpawnPoint.position, bulletSpawnPoint.rotation);

        // Apply force to bullet in the direction the player is facing
        bullet.GetComponent<Rigidbody>().AddForce(transform.forward * bullet.GetComponent<Bullet>().bulletSpeed);

        // Spawn bullet on clients
        NetworkServer.Spawn(bullet);
    }

    [Server]
    public void TakeDamage(int amount)
    {
        playerHealth -= amount;
    }
    
}