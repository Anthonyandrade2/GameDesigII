using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class TankShooting : MonoBehaviourPunCallbacks, IPunObservable
{
    public int m_PlayerNumber = 1;       
    public Rigidbody m_Shell;            
    public Transform m_FireTransform;    
    public Slider m_AimSlider;           
    public AudioSource m_ShootingAudio;  
    public AudioClip m_ChargingClip;     
    public AudioClip m_FireClip;         
    public float m_MinLaunchForce = 15f; 
    public float m_MaxLaunchForce = 30f; 
    public float m_MaxChargeTime = 0.75f;

    
    private string m_FireButton;         
    private float m_CurrentLaunchForce;  
    private float m_ChargeSpeed;         
    private bool m_Fired;                


    public override void OnEnable()
    {
        if (PhotonNetwork.LocalPlayer.ActorNumber != m_PlayerNumber)
        {
            this.enabled = false;
            return;
        }
        m_CurrentLaunchForce = m_MinLaunchForce;
        m_AimSlider.value = m_MinLaunchForce;
    }


    private void Start()
    {
        m_FireButton = "Fire";

        m_ChargeSpeed = (m_MaxLaunchForce - m_MinLaunchForce) / m_MaxChargeTime;
    }
    

    private void Update()
    {
        if(PhotonNetwork.LocalPlayer.ActorNumber != m_PlayerNumber)
        {
            return;
        }


        // Track the current state of the fire button and make decisions based on the current launch force.
        m_AimSlider.value = m_MinLaunchForce;

        //if the current force is more than the max force or equal, and we haven't fired, then fire
        if(m_CurrentLaunchForce >= m_MaxLaunchForce && !m_Fired)
        {
            m_CurrentLaunchForce = m_MaxLaunchForce; //we can not fire more than the max
            Fire();
        }

        //if the fire button has been pressed this frame
        else if (Input.GetButtonDown(m_FireButton)){
            //reset the fire flaf
            m_Fired = false;
            m_CurrentLaunchForce = m_MinLaunchForce;

            //change and play changing clip
            m_ShootingAudio.clip = m_ChargingClip;
            m_ShootingAudio.Play();
        }

        else if (Input.GetButton(m_FireButton) && !m_Fired)
        {
            m_CurrentLaunchForce += m_ChargeSpeed * Time.deltaTime;
            //update the slider
            m_AimSlider.value = m_CurrentLaunchForce;
        }
        //otherwise, if we have lifted and not fired, fire.
        else if (Input.GetButtonUp(m_FireButton) && !m_Fired)
        {
            Fire();
        }

    }


    private void Fire()
    {
        // Instantiate and launch the shell.
        //set the flired flag so that you can only fire once
        m_Fired = true;
        //Create an instance of the shell and store a reference to it's rigidbody
        Rigidbody shellInstance = Instantiate(m_Shell, m_FireTransform.position, m_FireTransform.rotation) as Rigidbody;

        //set the shell velocity to the launce force in the FP forward direction
        shellInstance.velocity = m_CurrentLaunchForce * m_FireTransform.forward;

        //change the clip to the firing clip
        m_ShootingAudio.clip = m_FireClip;
        m_ShootingAudio.Play();

        //reset the launch force
        m_CurrentLaunchForce = m_MinLaunchForce;
    }
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(m_Fired);
        }
        else if (stream.IsReading)
        {
            m_Fired = (bool)stream.ReceiveNext();
        }
    }
}