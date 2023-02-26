using Mirror;
using UnityEngine;
using UnityEngine.UI;

namespace MirrorBasics {
    public class AutoHostClient : MonoBehaviour {

        [SerializeField] NetworkManager networkManager;
        [SerializeField] InputField serverAddress;

        void Start () {
            if (Application.isBatchMode) { 
                Debug.Log ($"=== Server Build ===");
                networkManager.StartServer();
            }

        }

        public void JoinLocal () {
            networkManager.networkAddress = "localhost";
            networkManager.StartClient ();
        }

        public void JoinServer () {
            networkManager.networkAddress = "45.147.248.224";
            networkManager.StartClient ();
        }

        public void JoinCustomServer () {
            networkManager.StartClient ();
        }

        public void StartServer()
        {
            networkManager.StartServer();
        }

        public void SetServerAddress()
        {
            networkManager.networkAddress = serverAddress.text.ToString();
        }

    }
}