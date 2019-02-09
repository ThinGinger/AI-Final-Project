using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class Enemy : MonoBehaviour
{
    //AI Stuff
    public NavMeshAgent agent;
    public GameObject target;
    public Rigidbody targetRB;
    public enum State { CHASE, SHOOT, FLEE };
    State state;

    public float m_StartingHealth = 100f;               // The amount of health each tank starts with.
    public Slider m_Slider;                             // The slider to represent how much health the tank currently has.
    public Image m_FillImage;                           // The image component of the slider.
    public Color m_FullHealthColor = Color.green;       // The color the health bar will be when on full health.
    public Color m_ZeroHealthColor = Color.red;         // The color the health bar will be when on no health.
    public GameObject m_ExplosionPrefab;                // A prefab that will be instantiated in Awake, then used whenever the tank dies.


    private AudioSource m_ExplosionAudio;               // The audio source to play when the tank explodes.
    private ParticleSystem m_ExplosionParticles;        // The particle system the will play when the tank is destroyed.
    private float m_CurrentHealth;                      // How much health the tank currently has.
    private bool m_Dead;                               // Has the tank been reduced beyond zero health yet?

    //enemy bullets
    public GameObject shell;
    private bool m_Fired;
    private Rigidbody m_Shell;
    public Transform m_FireTransform;

    Vector3 velocity = Vector3.zero;
    Vector3 orientation = Vector3.up;



    // Use this for initialization
    void Start ()
    {

	}
	
	// Update is called once per frame
	void Update ()
    {

        if(m_CurrentHealth > m_StartingHealth /2)
        {
            if (Vector3.Distance(transform.position, target.transform.position) < 30)
            {
                state = State.SHOOT;
            }
            else
            {
                state = State.CHASE;
            }
        }
        else
        {
            state = State.FLEE;
        }
        


        switch (state)
        {
            case State.CHASE:
                AI_Chase();
                break;

            case State.SHOOT:
                AI_Shoot();
                break;

            case State.FLEE:
                AI_Flee();
                break;

            default:
                break;
        }

    }

    private void Awake()
    {
        // Instantiate the explosion prefab and get a reference to the particle system on it.
        m_ExplosionParticles = Instantiate(m_ExplosionPrefab).GetComponent<ParticleSystem>();

        // Get a reference to the audio source on the instantiated prefab.
        m_ExplosionAudio = m_ExplosionParticles.GetComponent<AudioSource>();

        // Disable the prefab so it can be activated when it's required.
        m_ExplosionParticles.gameObject.SetActive(false);

        // Update the health slider's value and color.
        SetHealthUI();
    }


    private void OnEnable()
    {
        // When the tank is enabled, reset the tank's health and whether or not it's dead.
        m_CurrentHealth = m_StartingHealth;
        m_Dead = false;
    }


    public void TakeDamage(float amount)
    {
        // Reduce current health by the amount of damage done.
        m_CurrentHealth -= amount;

        // Change the UI elements appropriately.
        SetHealthUI();

        // If the current health is at or below zero and it has not yet been registered, call OnDeath.
        if (m_CurrentHealth <= 0f && !m_Dead)
        {
            OnDeath();
        }
    }


    public void SetHealthUI()
    {
        // Set the slider's value appropriately.
        m_Slider.value = m_CurrentHealth;

        // Interpolate the color of the bar between the choosen colours based on the current percentage of the starting health.
        m_FillImage.color = Color.Lerp(m_ZeroHealthColor, m_FullHealthColor, m_CurrentHealth / m_StartingHealth);
    }


    private void OnDeath()
    {
        // Set the flag so that this function is only called once.
        m_Dead = true;

        // Move the instantiated explosion prefab to the tank's position and turn it on.
        m_ExplosionParticles.transform.position = transform.position;
        m_ExplosionParticles.gameObject.SetActive(true);

        // Play the particle system of the tank exploding.
        m_ExplosionParticles.Play();

        // Play the tank explosion sound effect.
        m_ExplosionAudio.Play();

        // Turn the tank off.
        gameObject.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)//healthpack
    {
        if (m_CurrentHealth < m_StartingHealth && other.gameObject.tag == "Health")
        {
            m_CurrentHealth = m_StartingHealth;
            SetHealthUI();
            Destroy(other.gameObject);
        }
    }

    private void FireCalc()
    {
        m_Shell = shell.GetComponent<Rigidbody>();
        //relative velocity calculation
        Vector3 relativeV = (target.GetComponent<Rigidbody>().velocity - velocity);
        //relative distance
        Vector3 relativeD = (target.transform.position - transform.position);
        //time to close
        float timeToClose = (relativeD.magnitude / relativeV.magnitude);
        //calculate predicted position
        Vector3 predictedPos = target.transform.position + target.GetComponent<Rigidbody>().velocity * timeToClose;

        //get desired direction
        Vector3 direction = predictedPos - transform.position;
        //calculate velocity
        velocity = direction.normalized * agent.speed*3;
        //update position
        Fire(velocity);
        
       
    }

    private void Fire(Vector3 v)
    {
        // Set the fired flag so only Fire is only called once.
        if (!m_Fired)
        {
            // Create an instance of the shell and store a reference to it's rigidbody.
            Rigidbody shellInstance = Instantiate(m_Shell, m_FireTransform.position, m_FireTransform.rotation) as Rigidbody;
            // launch the shell
            shellInstance.velocity = v;
            shellInstance.useGravity = false;
            m_Fired = true;
            StartCoroutine(ShotTimer(1));
        }
    }

    IEnumerator ShotTimer(float time)
    {
        yield return new WaitForSeconds(time);

        m_Fired = false;
    }

    public GameObject FindClosestHealth()
    {
        GameObject[] gos;
        gos = GameObject.FindGameObjectsWithTag("Health");
        GameObject closest = null;
        float distance = Mathf.Infinity;
        Vector3 position = transform.position;
        foreach (GameObject go in gos)
        {
            Vector3 diff = go.transform.position - position;
            float curDistance = diff.sqrMagnitude;
            if (curDistance < distance)
            {
                closest = go;
                distance = curDistance;
            }
        }
        return closest;
    }

    private void AI_Chase()
    {
        agent.isStopped = false;
        agent.SetDestination(target.transform.position);
    }

    private void AI_Shoot()
    {
        agent.isStopped = true;
        transform.LookAt(target.transform);
        FireCalc();

    }

    private void AI_Flee()
    {
        agent.isStopped = false;


        if (Vector3.Distance(target.transform.position, this.transform.position) > 15)
        {
            agent.SetDestination(FindClosestHealth().transform.position);
        }
        else
        {
            Vector3 fleeDirection = (transform.position - target.transform.position);
            Vector3.Normalize(fleeDirection);
            agent.SetDestination(fleeDirection);
        }
       
    }
}