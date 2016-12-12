using UnityEngine;
using System.Collections;

public class InnerWall : MonoBehaviour {

    public Color HighlightColor;
    public Sprite GlowSprite;

    private SpriteRenderer renderer2;
    private Color originalColor;
    private Sprite originalSprite;

    public WallScript Parent;

    [HideInInspector]
    public Vector3 OriginalPosition;

    // Use this for initialization
    void Awake() {
        renderer2 = GetComponent<SpriteRenderer>();
        originalColor = renderer2.color;
        originalSprite = renderer2.sprite;
    }

    void Start()
    {
        OriginalPosition = this.transform.position;
    }

    void OnCollisionEnter2D(Collision2D coll)
    {
        ContactPoint2D contact = coll.contacts[0];
        
        Parent.OnStartPushWall(contact.normal);
    }

    void OnCollisionExit2D(Collision2D coll)
    {
        Parent.OnStopPushWall();
    }

    public void Highlight(bool highlight)
    {
        if (highlight)
        {
            renderer2.sprite = GlowSprite;
            renderer2.color = HighlightColor;
        }
        else
        {
            renderer2.sprite = originalSprite;
            renderer2.color = originalColor;
        }
    }

    // Update is called once per frame
    void Update () {
	
	}
}
