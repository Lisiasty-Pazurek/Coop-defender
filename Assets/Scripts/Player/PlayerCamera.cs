using UnityEngine;
using Cinemachine;
using Mirror;

// This sets up the scene cinemachine camera to borrow lobby menu camera and follow player gameobject

    public class PlayerCamera : NetworkBehaviour
    {
        private Camera mainCam;
        private CinemachineVirtualCamera cineCam;

        public override void OnStartLocalPlayer ()
        {
            SetupPlayerCamera();
        }

        public void SetupPlayerCamera()
        {
            Debug.Log("Setting up camera controllers");
            mainCam = Camera.main;
            cineCam = mainCam.GetComponent<CinemachineVirtualCamera>();
//          mainCam.orthographic = false; // let's change it through inspector, not necessary for now 
            cineCam.Follow = this.transform;
            Debug.Log("cinemachine cam follow set up for local client");
        }

        [Client]
        private void Update() 
        {
            // Camera distance and rotation
            if (Input.GetKeyDown(KeyCode.Q))
            {
//                cineCam.
            }
            if (Input.GetKeyDown(KeyCode.E))
            {
//                cineCam.
            }
            if (Input.mouseScrollDelta != null)
            {
//                cineCam.               
            }

        }


    }

