using UnityEngine;
using System.Collections;

public class WallScript : MonoBehaviour {

    public Rigidbody2D WallBody;

    Level level;
    public void SetLevel(Level level, int x, int y, int direction)
    {
        this.level = level;
        this.X = x;
        this.Y = y;
        this.Direction = direction;
    }

    [HideInInspector]
    public int X;
    [HideInInspector]
    public int Y;
    [HideInInspector]
    public int Direction;
   // [HideInInspector]
   // public Vector3 OriginalPosition;

    public void OnStartPushWall(Vector2 normal)
    {
        level.OnStartPushWall(this, normal);
    }

    public void OnStopPushWall()
    {
        level.OnStopPushWall(this);
    }

    // Use this for initialization
    void Start () {
       // OriginalPosition = transform.position;

    }

    // Update is called once per frame
    void Update () {
	
	}
}
