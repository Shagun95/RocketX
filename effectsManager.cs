using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class effectsManager : MonoBehaviour
{

    public Camera backCamera;

    public rocketController rContr;
    
    public SpriteRenderer noseHeat;
    bool noseHeatActive;

    /// <summary>
    /// effetto reentry
    /// </summary>
    public ParticleSystem reentryfireEffect;

    float TimeNoseCone;

    Color initColor, endColor;

    private float _inclination; //todo sarebbe da mettere nel rocketcontroller

    /// <summary>
    /// Serve al nosecone per capire se si può accendere
    /// </summary>
    private bool _allowNoseCone;

    /// <summary>
    /// Richiamo da rocket quando è finito per spegnere gli effetti
    /// </summary>
    public bool finished;

    void Start()
    {
        noseHeatActive = false;
        TimeNoseCone = 0;

        initColor = backCamera.backgroundColor;
        endColor = new Color(0, 0.1073113f, 0.2641509f);
        _allowNoseCone = true;
        finished = false;
    }

    void Update()
    {

        if (!finished)
        {
            noseCOneManager();

            reentryHeatManager();

            _inclination = WrapAngle(rContr.inclination);

            //effetto spazio andando in su
            backCamera.backgroundColor = Color.Lerp(initColor, endColor, rContr.altitude / 600);
        }

        


    }

    /// <summary>
    /// Gestisce effetto nosecone in ascesa
    /// </summary>
    void noseCOneManager()
    {

        //se è inclinato, o ha superato l'atmosfera spengo e non faccio accendere
        if (_inclination < -2 || _inclination >2 || rContr.altitude > 412)
        {
            if (noseHeatActive)
            {
                //spegne
                StartCoroutine(switchOffNoseCone());
            }

            _allowNoseCone = false;

        } else if (!_allowNoseCone)
        {
            //altrimentri, rendo possibile
            _allowNoseCone = true;
        }

        if (_allowNoseCone)
        {
            if (!noseHeatActive && rContr.velocity > 20 && rContr.altitude < 1000)
            {
                //accendi
                StartCoroutine(switchOnNoseCone());
            }
            else if (noseHeatActive && rContr.velocity < 20)

            {
                //spegne
                StartCoroutine(switchOffNoseCone());
                
            }
        }

    }

    /// <summary>
    /// Metto come coroutine perchè deve sempre essere completato accende
    /// </summary>
    /// <returns></returns>
    IEnumerator switchOnNoseCone()
    {
        if (TimeNoseCone < 1)
        {
            noseHeat.color = new Color(1f, 1f, 1f, TimeNoseCone);
            TimeNoseCone += Time.deltaTime / 2;
        }
        else
        {
            noseHeatActive = true;
            TimeNoseCone = 0;
            yield break;
        }
    }

    /// <summary>
    /// Metto come coroutine perchè deve sempre essere completato spegne
    /// </summary>
    /// <returns></returns>
    IEnumerator switchOffNoseCone()
    {
        if (TimeNoseCone < 1 && (1 - TimeNoseCone >= 0))
        {
            noseHeat.color = new Color(1f, 1f, 1f, 1 - TimeNoseCone);
            TimeNoseCone += Time.deltaTime / 2;
        }
        else
        {
            noseHeatActive = false;
            TimeNoseCone = 0;
            yield break;
        }
    }

    void reentryHeatManager()
    {
        
        if ((_inclination < -5 || _inclination > 5) && rContr.altitude >20 && rContr.drag > 0.10 && rContr.velocity < 0 && rContr.altitude<412)
        {
            
            reentryfireEffect.Play();
        } else
        {
            reentryfireEffect.Stop();
        }
    }

    private static float WrapAngle(float angle)
    {
        angle %= 360;
        if (angle > 180)
            return angle - 360;

        return angle;
    }

}