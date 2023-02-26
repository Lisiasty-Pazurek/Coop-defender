using UnityEngine;
using UnityEngine.UI;

namespace MirrorBasics {

    public class UIPlayer : MonoBehaviour {

        [SerializeField] Text text;
        Player player;

        public void SetPlayer (Player player) {
            this.player = player;
            text.text =  player.playerName;
        }

        public void SetName () 
        {
            player.playerName = text.text;
        }

    }
}