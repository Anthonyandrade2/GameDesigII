using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
namespace MyGame
{
    public class Launcher : MonoBehaviourPunCallbacks
    {
        #region Private Serializable Fields
        [Tooltip("the max people who can join a room before a new one is created")]
        [SerializeField] private byte maxPlayersPerRoom = 4;
        [SerializeField] GameObject controlPanel;
        [SerializeField] GameObject progressLabel;


        #endregion
        #region Private Fields
        private string gameVersion = "0.1";
        private bool isConnecting;
        #endregion
        private void Awake()
        {
            //the master will call LoadScene and all the Clients will sync to that scene
            PhotonNetwork.AutomaticallySyncScene = true;
        }
        // Start is called before the first frame update
        void Start()
        {
            progressLabel.SetActive(false);
            controlPanel.SetActive(true);
        }

        #region Public Methods
        public void Connect()
        {
            if (PhotonNetwork.IsConnected)
            {
                PhotonNetwork.JoinRandomRoom();
            }
            else
            {
                PhotonNetwork.ConnectUsingSettings();
                PhotonNetwork.GameVersion = gameVersion;
            }
        }
        public void LoadArena()
        {
            PhotonNetwork.LoadLevel(1); //Scene/Main
        }
        #endregion
        #region Call Bakcs
        public override void OnConnectedToMaster()
        {
            if (isConnecting)
            {
                Debug.Log("I have connected");
                PhotonNetwork.JoinRandomRoom();
                isConnecting = false;
            }
            
        }
        public override void OnDisconnected(DisconnectCause cause)
        {
            Debug.Log("I have disconnected");
            progressLabel.SetActive(false);
            controlPanel.SetActive(true);
            isConnecting = false;
        }
        public override void OnJoinRandomFailed(short returnCode, string message)
        {
            Debug.Log("Failed to join a random room");
            //create room to join
            var room = new RoomOptions();
            room.MaxPlayers = maxPlayersPerRoom;
            PhotonNetwork.CreateRoom(null, room);
        }
        public override void OnJoinedRoom()
        {
            Debug.Log("Joined Room");
            LoadArena();
        }

        #endregion
    }
}
