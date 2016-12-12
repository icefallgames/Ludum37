using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour {

    private Rigidbody2D rigid;

    public GameObject Visual;
    public AudioSource FootstepsAudio;

    private Animator anim;
    private int SpeedId;
    private int IsPushingId;

    public float SpringForcePushTx = 10f;

    SoundManager soundManager;
    Level level;

    void Start()
    {
        level = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Level>();
        soundManager = level.GetSoundManager();

        Debug.Assert(soundManager != null);
    }

    // Use this for initialization
    void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        anim = Visual.GetComponent<Animator>();
        SpeedId = Animator.StringToHash("Speed");
        IsPushingId = Animator.StringToHash("IsPushing");
    }

    void OnCollisionEnter2D(Collision2D coll)
    {
        // We'll wait till we have enough force
        // anim.SetBool(IsPushingId, true);
    }

    private bool canSayWeFailed = false;

    private float disableTime = 0f;

    void OnCollisionStay2D(Collision2D coll)
    {
        SpringJoint2D spring = coll.gameObject.GetComponent<SpringJoint2D>();
        if (spring != null)
        {
            bool isPushing = spring.GetReactionForce(Time.deltaTime).magnitude > SpringForcePushTx;
            anim.SetBool(IsPushingId, isPushing);
            if (isPushing)
            {
                canSayWeFailed = true;
                soundManager.PlayPushing();
            }
            else
            {
                soundManager.StopPushing();
                if (canSayWeFailed && level.FailedToPushForReason())
                {
                    canSayWeFailed = false;
                    soundManager.PlayFail();
                }
            }
        }
    }

    // Disable input briefly so that we don't go flying into next wall.
    public void DisableBriefly()
    {
        disableTime = 0.5f;

        // But let's actually do better:
        rigid.isKinematic = true;
    }

    public void HackForceCollisionExit()
    {
        anim.SetBool(IsPushingId, false);
        soundManager.StopPushing();
    }
    void OnCollisionExit2D(Collision2D coll)
    {
        anim.SetBool(IsPushingId, false);
        soundManager.StopPushing();
    }

    // Update is called once per frame
    void Update ()
    {
        disableTime -= Time.deltaTime;

        FootstepsAudio.volume = rigid.velocity.magnitude * 0.3f;

        anim.SetFloat(SpeedId, rigid.velocity.magnitude);
    }

    private bool disablePlayer = false;
    public void DisablePlayer()
    {
        disablePlayer = true;
    }

    public float ForceScaler = 1f;

    void FixedUpdate()
    {
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        if (disablePlayer || (disableTime > 0f))
        {
            h = 0;
            v = 0;
        }

        if (disableTime <= 0f)
        {
            // But let's actually do better:
            rigid.isKinematic = false;
        }

        rigid.AddForce(new Vector2(h, v) * ForceScaler, ForceMode2D.Impulse);

        Vector2 direction = new Vector2(h, v);
        if (direction.magnitude > Mathf.Epsilon)
        {
            direction = direction.normalized;
            Visual.transform.localRotation = Quaternion.FromToRotation(new Vector3(0, 1, 0), new Vector3(direction.x, direction.y, 0));
        }

    }
}
