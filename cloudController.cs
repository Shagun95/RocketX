using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class cloudController : MonoBehaviour
{
    /// <summary>
    /// true = left
    /// </summary>
    public bool direction;
    public float speed;
    public float secToChange;
    float timer;

    Vector2 dir;
    // Start is called before the first frame update
    void Start()
    {
        dir = direction ? Vector2.left : Vector2.right;
        timer = 0;
    }

    // Update is called once per frame
    void Update()
    {
        transform.Translate(dir * Time.timeScale * Time.deltaTime * speed, Space.World);


        timer += Time.deltaTime;
        if (timer >= secToChange)
        {
            timer = 0;
            dir = dir == Vector2.left ? Vector2.right : Vector2.left;
        }
    }
}
