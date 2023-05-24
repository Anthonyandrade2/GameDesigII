using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;
using Photon.Realtime;

public class GameManager : MonoBehaviourPunCallbacks
{
    public int m_NumRoundsToWin = 5;        
    public float m_StartDelay = 3f;         
    public float m_EndDelay = 3f;           
    public CameraControl m_CameraControl;   
    public TextMeshProUGUI m_MessageText;              
    public GameObject m_TankPrefab;         
    public TankManager[] m_Tanks;
    public Button StartbButton;


    private int m_RoundNumber;              
    private WaitForSeconds m_StartWait;     
    private WaitForSeconds m_EndWait;       
    private TankManager m_RoundWinner;
    private TankManager m_GameWinner;

    private void Start()
    {
        DontDestroyOnLoad(this);
        StartbButton.onClick.AddListener(StartGame);
        StartbButton.gameObject.SetActive(PhotonNetwork.IsMasterClient);
    }


    private void StartGame()
    {
        StartbButton.gameObject.SetActive(false);
        m_StartWait = new WaitForSeconds(m_StartDelay);
        m_EndWait = new WaitForSeconds(m_EndDelay);

        SpawnAllTanks();
        SetCameraTargets();

        StartCoroutine(GameLoop());
    }


    private void SpawnAllTanks()
    {
        //only let the master spawn the tanks
        if (!PhotonNetwork.IsMasterClient) return;
        for (int i = 0; i < m_Tanks.Length; i++)
        {
            m_Tanks[i].m_Instance =
                PhotonNetwork.Instantiate(m_TankPrefab.name, m_Tanks[i].m_SpawnPoint.position, m_Tanks[i].m_SpawnPoint.rotation) as GameObject;
            m_Tanks[i].m_Instance.GetComponent<PhotonView>().TransferOwnership(PhotonNetwork.LocalPlayer);

            m_Tanks[i].m_PlayerNumber = i + 1;
            m_Tanks[i].Setup();
        }
    }


    private void SetCameraTargets()
    {
        Transform[] targets = new Transform[m_Tanks.Length];

        for (int i = 0; i < targets.Length; i++)
        {
            targets[i] = m_Tanks[i].m_Instance.transform;
        }

        m_CameraControl.m_Targets = targets;
    }


    private IEnumerator GameLoop()
    {
        yield return StartCoroutine(RoundStarting());
        yield return StartCoroutine(RoundPlaying());
        yield return StartCoroutine(RoundEnding());

        if (m_GameWinner != null)
        {
            LeaveRoom();
        }
        else
        {
            StartCoroutine(GameLoop());
        }
    }


    private IEnumerator RoundStarting()
    {
        yield return m_StartWait;
        //reset all tanks
        ResetAllTanks();
        //disable all tank controls
        DisableTankControl();
        //Set Camera Position and size
        m_CameraControl.SetStartPositionAndSize();
        //Increment round number
        m_RoundNumber++;
        //set the message UI text
        m_MessageText.text = "ROUND" + m_RoundNumber;
        yield return m_StartWait;
    }


    private IEnumerator RoundPlaying()
    {
        //enable all tank controlls
        EnableTankControl();
        //empty message ui text
        m_MessageText.text = string.Empty;
        //wait for one tank left
        while (!OneTankLeft())
        {
            yield return null; //come back after every frame and check
        }
        
    }


    private IEnumerator RoundEnding()
    {
        //Disable all tank controls
        DisableTankControl();
        //clear existing winner and get the round winner
        m_RoundWinner = null;
        m_RoundWinner = GetRoundWinner();
        //todo: Add the winner increment
        if(m_RoundWinner != null)
        {
            m_RoundWinner.m_Wins++;
        }
        //check for a game winner
        m_GameWinner = GetGameWinner();
        //calculate message UI text and show it
        string message = EndMessage();
        m_MessageText.text = message;
        yield return m_EndWait;
    }


    private bool OneTankLeft()
    {
        int numTanksLeft = 0;

        for (int i = 0; i < m_Tanks.Length; i++)
        {
            if (m_Tanks[i].m_Instance.activeSelf)
                numTanksLeft++;
        }

        return numTanksLeft <= 1;
    }


    private TankManager GetRoundWinner()
    {
        for (int i = 0; i < m_Tanks.Length; i++)
        {
            if (m_Tanks[i].m_Instance.activeSelf)
                return m_Tanks[i];
        }

        return null;
    }


    private TankManager GetGameWinner()
    {
        for (int i = 0; i < m_Tanks.Length; i++)
        {
            if (m_Tanks[i].m_Wins == m_NumRoundsToWin)
                return m_Tanks[i];
        }

        return null;
    }


    private string EndMessage()
    {
        string message = "DRAW!";

        if (m_RoundWinner != null)
            message = m_RoundWinner.m_ColoredPlayerText + " WINS THE ROUND!";

        message += "\n\n\n\n";

        for (int i = 0; i < m_Tanks.Length; i++)
        {
            message += m_Tanks[i].m_ColoredPlayerText + ": " + m_Tanks[i].m_Wins + " WINS\n";
        }

        if (m_GameWinner != null)
            message = m_GameWinner.m_ColoredPlayerText + " WINS THE GAME!";

        return message;
    }


    private void ResetAllTanks()
    {
        for (int i = 0; i < m_Tanks.Length; i++)
        {
            m_Tanks[i].Reset();
        }
    }


    private void EnableTankControl()
    {
        for (int i = 0; i < m_Tanks.Length; i++)
        {
            m_Tanks[i].EnableControl();
        }
    }


    private void DisableTankControl()
    {
        for (int i = 0; i < m_Tanks.Length; i++)
        {
            m_Tanks[i].DisableControl();
        }
    }
    private void LoadArena()
    {
        if (!PhotonNetwork.IsMasterClient)
        {
            Debug.Log("Trying to load but we are not the Master");
            return;
        }
        PhotonNetwork.LoadLevel(1);
    }

    #region Photon CallBacks
    public override void OnLeftRoom()
    {
        SceneManager.LoadScene(0);
        
    }
    public override void OnPlayerEnteredRoom(Player other)
    {
        Debug.Log("Player has entered the arena: " + other.NickName);
    }
    public override void OnPlayerLeftRoom(Player other)
    {
        Debug.Log("Player has left the arena: " + other.NickName);
    }
    public void LeaveRoom()
    {
        PhotonNetwork.LeaveRoom();
    }
    #endregion
}