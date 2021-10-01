using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class cameraFollow : MonoBehaviour
{

    /// <summary>
    /// Il razzo
    /// </summary>
    public GameObject target;

    public rocketController rocket;

    private float delta;

    Camera main;

    public gameController gameC;

    private float _time;

    private float xPositionCamera;

    Vector3 screenPoint;
    bool onScreen;

    private bool firtstZoom;

    
    
    private float standardCameraFOV;


    

    // Start is called before the first frame update
    void Start()
    {
        main = Camera.main;
        delta = target.transform.position.y - main.transform.position.y;
        _time = 0;
        xPositionCamera = target.transform.position.x;
        firtstZoom = false;
        standardCameraFOV = 36f;
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = new Vector3(0, target.transform.position.y - delta, transform.position.z);

        //zoomManager();
        initialZoom();
        manageScreenCamera();
    }


    /// <summary>
    /// Gestisce vari zoom durante il gioco
    /// </summary>
    private void zoomManager()
    {
        if (gameC.hasStarted && _time<=1)
        {
            Camera.main.fieldOfView = Mathf.Lerp(38, 30, _time);
            _time += Time.deltaTime/5;
        }
    }

    /// <summary>
    /// Primo zoom camera
    /// </summary>
    private void initialZoom()
    {
        if (!firtstZoom && _time <= 1)
        {
            Camera.main.fieldOfView = Mathf.Lerp(50, 35, _time);
            _time += Time.deltaTime/2;
        } else
        {
            firtstZoom = true;
            _time = 0;
        }
    }


    private void manageScreenCamera()
    {
        Vector3 screenPoint = main.WorldToViewportPoint(target.transform.position);
        bool onScreen = screenPoint.x > 0.3 && screenPoint.x < 0.7;

        if (!onScreen && main.fieldOfView < 100)
        {
            main.fieldOfView += Time.deltaTime * 25;
            

            

        } else if (main.fieldOfView > standardCameraFOV)
        {
            //rocket.freezeXVelocity = false;
            bool gettingCloser = screenPoint.x > 0.4 && screenPoint.x < 0.6;
            if (gettingCloser)
            {
                main.fieldOfView -= Time.deltaTime * 15;
            }
        } 
    }


}
