using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour {

    private Rigidbody2D rigid;

	// Use this for initialization
	void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
    }
	
	// Update is called once per frame
	void Update ()
    {
	
	}

    void FixedUpdate()
    {
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        rigid.AddForce(new Vector2(h, v), ForceMode2D.Impulse);
    }
}
