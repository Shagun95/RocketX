using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class smokeController : MonoBehaviour
{
    // Start is called before the first frame update

    public GameObject target;

    public rocketController rk;

    Vector2 newPosition;

    private ParticleSystem _smoke;

    void Start()
    {
        _smoke = gameObject.GetComponent<ParticleSystem>();
    }


    void Update()
    {
        newPosition = transform.position;
        newPosition.x = target.transform.position.x;

        transform.position = newPosition;

        //ottimizzabile
        if (rk.engineIsRunning && !rk._finished)
            _smoke.Play();
        else
            _smoke.Stop();


        
    }
}
