using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class starController : MonoBehaviour
{

    /// <summary>
    /// accendo le stelle sopra l'atmosfera
    /// </summary>
    ///

    SpriteRenderer stars;
    Color empty, full;

    Camera mcamera;

    // Start is called before the first frame update
    void Start()
    {
        stars = GetComponent<SpriteRenderer>();
        empty = stars.color;

        full = empty;
        full.a = 1;
        mcamera = Camera.main;
    }

    // Update is called once per frame
    void Update()
    {
        stars.color = Color.Lerp(empty, full, (mcamera.transform.position.y - 430)/100);
    }
}
