using UnityEngine;
using System.Collections;

public class VialGlow : MonoBehaviour {


    public Color Bright = new Color(1, 0.3f, 1);
    public Color Dark = new Color(0.7f, 0, 0.7f);
    public float CycleTime = 3f;

    private SpriteRenderer sr;
    private float currentCycle;

    // Use this for initialization
    void Start () {
        sr = GetComponent<SpriteRenderer>();
        UpdateGlow();
        currentCycle = Random.Range(0, CycleTime);
    }

    void UpdateGlow()
    {
        sr.color = Color.Lerp(Bright, Dark, 0.5f * (Mathf.Sin((currentCycle / CycleTime) * Mathf.PI * 2.0f) + 1f));
    }

    // Update is called once per frame
    void Update ()
    {
        currentCycle += Time.deltaTime;
        currentCycle %= CycleTime;
        UpdateGlow();
    }
}
