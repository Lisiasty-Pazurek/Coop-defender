using UnityEngine;
using Cinemachine;

// This sets up the scene cinemachine camera to borrow lobby menu camera and follow player gameobject

    public class PlayerCamera : MonoBehaviour
    {
        private Camera mainCam;
 //       private GameObject gameCamera; // not needed for now
        private CinemachineVirtualCamera cineCam;

        public void Start ()
        {
            SetupPlayerCamera();
        }

        public void SetupPlayerCamera()
        {
            Debug.Log("Setting up camera controllers");
            mainCam = Camera.main;
                cineCam = mainCam.GetComponent<CinemachineVirtualCamera>();
                mainCam.orthographic = false;
                cineCam.Follow = this.transform;
                Debug.Log("cinemachine cam follow set up for local client");
        }

    }

