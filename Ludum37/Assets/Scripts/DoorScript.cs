using UnityEngine;
using System.Collections;

public class DoorScript : MonoBehaviour {

    Level level;

    public bool IsOpen = true;
    public AudioSource DoorSound;

    public void SetIsOpen(bool open)
    {
        if (IsOpen != open)
        {
            IsOpen = open;
            if (IsOpen)
            {
                DoorSound.Play();
            }
        }
    }

    private SpriteRenderer sr;
    public Sprite OpenSprite;
    public Sprite ClosedSprite;

	// Use this for initialization
	void Start ()
    {
        sr = GetComponent<SpriteRenderer>();
        level = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Level>();

    }
	
	// Update is called once per frame
	void Update ()
    {
        sr.sprite = IsOpen ? OpenSprite : ClosedSprite;

    }

    // I'm using Stay instead of Enter, because we warp the player and they end up on the door.
    void OnTriggerStay2D(Collider2D coll)
    {
        if (IsOpen)
        {
            if (coll.tag == "Player")
            {
                level.OnFoundDoor();
            }
        }
    }
}
