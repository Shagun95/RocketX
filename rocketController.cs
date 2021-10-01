using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class rocketController : MonoBehaviour
{

    public bool debug;


    #region definizioni
    public float fuel, drag, velocity, overheating, inclination, altitude;

    //cancellerai
    public getLabel fuelLabel, dragLabel, velocityLabel, overHetLabel, inclinLabel, altitudeLabel;

    public GameObject engine;

    /// <summary>
    /// Riferimento effetti
    /// </summary>
    public effectsManager efManager;

    /// <summary>
    /// Controllo gioco
    /// </summary>
    public gameController gameController;

    /// <summary>
    /// Fuoco del razzo
    /// </summary>
    public ParticleSystem fireEmitter;

    /// <summary>
    /// Per effetto esplosione
    /// </summary>
    public ParticleSystem explosionEffect, smokeExplosion;

    /// <summary>
    /// Per effetto shake
    /// </summary>
    public CameraShake cShake;


    private Rigidbody _rb;

    /// <summary>
    /// Velocity ma con valore sempre positivo, per fare i calcoli
    /// </summary>
    public float adjustedVelocity;

    /// <summary>
    /// Posizione "di terra"
    /// </summary>
    private float _groundY;

    /// <summary>
    /// potenza del thrust
    /// </summary>
    public float thrust;

    /// <summary>
    /// Utilizzo privato per gestire calcolo
    /// </summary>
    private float _currentThrust;

    /// <summary>
    /// dichiaro qui così non devo reinserire ogni volta
    /// </summary>
    private float _absInclination;

    /// <summary>
    /// Tap iniziale calcolo per forza thrust
    /// </summary>
    private Vector2 _initTouch;

    /// <summary>
    /// Per vedere se engine è dritto rispetto razzo
    /// </summary>
    private bool engineIsUpright;

    /// <summary>
    /// Salvo la rotazione in su del motore
    /// </summary>
    Quaternion engineUprightQuaternion;

    /// <summary>
    /// Indica se il mototre è in azione
    /// </summary>
    public bool engineIsRunning;

    /// <summary>
    /// Mi serve al checkground per capire se è atterrato e non rientrare più nella func
    /// </summary>
    public bool _finished;

    /// <summary>
    /// Utilizzo nel camera shake per farlo avviare solo una volta
    /// </summary>
    private bool _stillOnGround;

    /// <summary>
    /// 
    /// </summary>
    private bool _decreaseXVelocity;

    public bool freezeXVelocity;

    public float limitcamera;

    /// <summary>
    /// Livello tank benzina (1 è massimo)
    /// </summary>
    private float LevelTank;

    public int rocketLevel;
    #endregion

    public AudioSource rocketAudio;

    // Start is called before the first frame update
    void Start()
    {
        _rb = GetComponent<Rigidbody>();
        _groundY = transform.position.y;
        altitude = 0;
        _currentThrust = thrust;

        engineUprightQuaternion = engine.transform.localRotation;
        engineIsRunning = false;
        _finished = false;
        _stillOnGround = true;

        _decreaseXVelocity = false;
        freezeXVelocity = false;

        limitcamera = 25;

        //todo prendi level tank
        LevelTank = 10;
        //livello rocket todo potrebbe rompere gioco
        rocketLevel = PlayerPrefs.GetInt(gameController.rocketChoise.ToString() + "level", 0);
        LevelTank = 10 - rocketLevel;
        rocketAudio = gameObject.AddComponent<AudioSource>();
        rocketAudio.loop = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (!_finished)
        {
            updateVariables();
            updateLabels();
            checkOverHeathing();

            engineIsUpright = engine.transform.localRotation == engineUprightQuaternion;
        }


        //todo guarda se puoi spostare dove c'è addvelocity
        if (transform.position.x < -limitcamera)
        {
            if (_rb.velocity.x < 0)
            {
                Vector3 pos = transform.position;
                pos.x = -limitcamera;
                transform.position = pos;
            }
        }
        else if (transform.position.x > limitcamera)
        {
            if (_rb.velocity.x >0) {
                Vector3 pos = transform.position;
                pos.x = limitcamera;
                transform.position = pos;
            }
        }


        //gestione suono razzo
        if (engineIsRunning)
        {
            gameController.engineRunSound(true, altitude, true);
        } else
        {
            gameController.engineRunSound(false, altitude, true);
        }
        

    }

    private void FixedUpdate()
    {
        if (!_finished)
        {
            liftUp();

            checkGround();

            checkAirDrag();

            if (_decreaseXVelocity)
                decreaseXAxisVelocity();

            if (Input.touchCount == 0)
            {
                //purtroppo devo ricontrollare se sta usando touch
                fireEmitter.Stop();
                resetEngineRotation();
                engineIsRunning = false;

            }
            else if (!fireEmitter.isPlaying && fuel >= 0)
            {
                fireEmitter.Play();
                engineIsRunning = true;
            }



            if (fuel <= 0 && fireEmitter.isPlaying)
            {
                fireEmitter.Stop();
                engineIsRunning = false;
            }
        }
        
    }

    private void liftUp()
    {
        if ((Input.touches.Length > 0 || debug) && fuel > 0 && !_finished)
        {
            Touch touch = Input.GetTouch(0);

            if (_stillOnGround)
            {
                //si attiva solo la prima volta quando decolla
                cShake.startShake(3, 0.3f);
                if (transform.position.y > _groundY + 5)
                {
                    _stillOnGround = false;
                    gameController.hasStarted = true;
                }

            }

            if (touch.phase == TouchPhase.Began)
            {
                _initTouch = touch.position;

                fireEmitter.Play();

                engineIsRunning = true;

                


            } else if (touch.phase == TouchPhase.Moved || touch.phase == TouchPhase.Stationary)
            {

                thrustManager();

                engineIsRunning = true;

                //calcolo e porto su (SOLO PORTARE SU o GIU)

                Vector2 dir = transform.up.y > 0 ? Vector2.up : Vector2.down;
                _rb.AddForce((dir * _currentThrust * Time.timeScale * Time.deltaTime));


                Vector3 edgeforce = transform.up;
                edgeforce.y = 0;
                
                    

                if(fuel > 0) fuel -=5 * Time.deltaTime; //todo capire


                if ((transform.position.x < -limitcamera && transform.up.x < 0) || (transform.position.x > limitcamera && transform.up.x > 0))
                {


                    Vector3 velocity = _rb.velocity;
                    
                    velocity.x = 0;
                    edgeforce.x = 0;
                    _rb.velocity = velocity;
                    
                }

                //risolto bug giro all'inizio
                if (gameController.hasStarted)
                {
                    _rb.AddForce(edgeforce * Time.deltaTime * 1000f);
                    transform.Rotate(getAngle(touch.position.x));
                }
                


            }
            else if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
            {
                engineIsRunning = false;
                _currentThrust = thrust;
                fireEmitter.Stop();

                _decreaseXVelocity = true;

                
            }
        }

    }





    /// <summary>
    /// Diminuisce x velocity per quando ruota il razzo
    /// </summary>
    /// <returns></returns>
    private void  decreaseXAxisVelocity()
    {
        if (_rb.velocity.x == 0)
        {
            _decreaseXVelocity = false;
        } else
        {
            Vector3 velocity = _rb.velocity;

            if (velocity.x > 0)
            {
                velocity.x -= Time.deltaTime;
            } else
            {
                velocity.x += Time.deltaTime;
            }
            
            if (velocity.x >= 1f || velocity.x <= -1f)
            {
                _rb.velocity = velocity;
            } else
            {
                velocity.x = 0;
                _rb.velocity = velocity;
            }

        }
    }


    /// <summary>
    /// Rimette la rotazione del razzo in su quando non lo sta utilizzando
    /// </summary>
    private void resetEngineRotation()
    {
        if (!engineIsUpright && !engineIsRunning)
        {
            engine.transform.localRotation = Quaternion.Lerp(engine.transform.localRotation, engineUprightQuaternion, Time.time *0.03f);
        }
    }

    

    /// <summary>
    /// a seconda di quanto è 'distante' la posizione touch da più o meno forza
    /// </summary>
    /// <param name="currentY"></param>
    /// <returns></returns>
    private float getForce(float currentY)
    {
        float force;

        force = (_initTouch.y - currentY) / 80;
        if (force > 5) force = 5;
        if (force < 0) force = 0;
        return force;
    }

    private Vector3 getAngle(float currentX)
    {
        Vector3 rot;
        float angle;
        angle = (currentX - _initTouch.x) / 200;
        rot = new Vector3(0, 0, angle);


        //se ruota più di 3 o meno di 3 è strano
        if (!((engine.transform.localRotation.z > 0.3 && angle > 0)||(engine.transform.localRotation.z < -0.3 && angle<0)))
        {
            engine.transform.Rotate(rot, Space.Self);
        }


        return rot;
    }

    /// <summary>
    /// Aggiorna i parametri e le applica al razzo
    /// </summary>
    private void updateVariables()
    {
        //calcolo velocità, sempre positiva anche in discesa
        velocity = _rb.velocity.y;
        

        adjustedVelocity = velocity;
        if (adjustedVelocity < 0) adjustedVelocity *= -1;
        //inclinazione
        inclination = transform.rotation.eulerAngles.z;

        //controllo altitudine
        altitude = transform.position.y +0.6f;

        //calcolo drag (rallenta in base a inclinazione)
        if (altitude < 300)
        {
            _rb.drag = drag * (adjustedVelocity / 100);//todo test (dipende da velocità)
        }
        

        //calcolo angular drag (rallenta rotazione in base a velocità)
        //_rb.angularDrag = velocity/100; //todo test

    }


    /// <summary>
    /// Aggiorna tutte le label
    /// </summary>
    private void updateLabels()
    {
        //fuelLabel.text.text = fuel.ToString("F2");
        //dragLabel.text.text = _rb.drag.ToString("F2");
        //velocityLabel.text.text = velocity.ToString("F2");
        //overHetLabel.text.text = overheating.ToString("F0"); //LO CONTROLLO UN UI MANAGER
        //inclinLabel.text.text = inclination.ToString("F2");
        altitudeLabel.text.text = altitude.ToString("F0");
    }

    /// <summary>
    /// Controlla quando cade a terra
    /// </summary>
    private void checkGround()
    {
        if (gameController.hasStarted && transform.position.y < _groundY && !_finished)
        {
            //spengo il motore se sta ancora andando
            switchEffectOff();

            _finished = true;
            float actualInclination = WrapAngle(inclination);
            //controllo velocità e verifico se touchdown riuscito
            if (adjustedVelocity <= 15 && (actualInclination > -10 && actualInclination < 10)) //todo controlla se 5 velocià accettabile e anche angolazione
            {
                //missione riuscita
                gameController.stopRocketEngine();
                gameController.succed();
            } else
            {
                explode();
            }
        }


        
    }

    static float WrapAngle(float angle)
    {
        angle %= 360;
        if (angle > 180)
            return angle - 360;

        return angle;
    }

    /// <summary>
    /// Esplode e gameover
    /// </summary>
    private void explode()
    {
        gameController.stopRocketEngine();
        switchEffectOff();

        _finished = true;
        _rb.constraints = RigidbodyConstraints.FreezeAll;
        explosionEffect.Play();
        smokeExplosion.Play();
        cShake.startShake(1.5f, 0.5f);
        GetComponent<Renderer>().enabled = false;
        engine.GetComponent<Renderer>().enabled = false;
        gameObject.GetComponent<effectsManager>().noseHeat.GetComponent<Renderer>().enabled = false;
        
        gameController.gameOver();

        
    }

    /// <summary>
    /// In caduta libera, a seconda di inclinazione rallenta
    /// </summary>
    private void checkAirDrag()
    {

        _absInclination = Mathf.Abs(inclination);

        //se supera 270, basta togliere 180 per farlo tornare nel "comportamento standard" degli altri giri
        if (_absInclination > 270) _absInclination -= 180;

        if (_absInclination < 90)
        {
            //se inclinazione tra 0 e 90 basta che divido e funziona il calcolo
            drag = _absInclination / 90;
        }
        else
        {
            //altrimenti ricavo in modo che dipenda da una fattore di 90, funziona fino a 270°
            drag = (Mathf.Abs(90 - (_absInclination - 90))) / 90;

        }
    }

    private void checkRotationDrag()
    {

    }


    private void thrustManager()
    {
        if (_currentThrust < 100)
        _currentThrust += 0.005f;
    }


    /// <summary>
    /// Controlla se se c'è overheat per potenza razzo o caduta libera
    /// </summary>
    private void checkOverHeathing()
    {
        if (overheating > 0)
        {
            overheating -= 10f * Time.deltaTime;
        }

        //overheating += (drag * 3) * Time.deltaTime;

        if (engineIsRunning)
        {
            overheating += 22f * Time.deltaTime;
        }

        if (overheating >= 100)
        {
            explode();
        }
    }

    /// <summary>
    /// Spegne tutti gli effetti per quando il gioco finisce
    /// </summary>
    private void switchEffectOff()
    {
        fireEmitter.Stop();
        efManager.finished = true;
        efManager.reentryfireEffect.Stop();
        rocketAudio.volume = 0;
        
    }



    //Controllo collisioni coin e terreno
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log(other.name);
    }

}
