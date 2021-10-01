using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class soundManager : MonoBehaviour
{
    public AudioSource audioSource;
    public AudioClip explosion, engineSound, coinSound;
    public float volume;
    bool _stop;

    bool moneyIsPlaying;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        volume = PlayerPrefs.GetFloat("soundVolume", 0.5f);
        _stop = false;
        moneyIsPlaying = false;
    }

    public void explosionSound()
    {
        audioSource.PlayOneShot(explosion, volume);
    }

    public void manageEngineSound(bool play, AudioSource rocketAudio, float altitude, bool stop)
    {
        if (!_stop)
        {

            if (play)
            {
                if (!rocketAudio.isPlaying)
                {
                    rocketAudio.PlayOneShot(engineSound, (volume));
                }
                else
                {
                    if (altitude > 0)
                    {
                        rocketAudio.volume = (volume) / (altitude / 50);
                    } else
                    {
                        rocketAudio.volume = (volume);
                    }
                    
                }

                Debug.Log(rocketAudio.volume);
                Debug.Log("alt"+altitude);
            }
            else
            {
                rocketAudio.Stop();
            }
        } else
        {
            rocketAudio.Stop();
        }
        
    }

    public void stopEngine()
    {
        _stop = true;
    }

    public void coinSoundPlay()
    {
        if (!moneyIsPlaying)
        {
        moneyIsPlaying = true;
        audioSource.PlayOneShot(coinSound, volume);
        }
    }
}
