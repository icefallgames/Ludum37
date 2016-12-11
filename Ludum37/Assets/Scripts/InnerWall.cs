using UnityEngine;
using System.Collections;

public class InnerWall : MonoBehaviour {

    private SpriteRenderer renderer2;
    private Color originalColor;

    public WallScript Parent;

    [HideInInspector]
    public Vector3 OriginalPosition;

    // Use this for initialization
    void Start () {
        renderer2 = GetComponent<SpriteRenderer>();
        originalColor = renderer2.color;
        OriginalPosition = this.transform.position;
}

    void OnCollisionEnter2D(Collision2D coll)
    {
        Parent.OnStartPushWall();
    }

    void OnCollisionExit2D(Collision2D coll)
    {
        Parent.OnStopPushWall();
    }

    public void SetColor(Color? color)
    {
        if (color.HasValue)
        {
            renderer2.color = color.Value;
        }
        else
        {
            renderer2.color = originalColor;
        }
    }

    // Update is called once per frame
    void Update () {
	
	}
}
