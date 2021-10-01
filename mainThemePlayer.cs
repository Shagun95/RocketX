using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class mainThemePlayer : MonoBehaviour
{
    public AudioSource _audioSource;
    public menuManager controller;
    private void Awake()
    {
        DontDestroyOnLoad(transform.gameObject);
        _audioSource = GetComponent<AudioSource>();
    }

    private void Start()
    {
        _audioSource.loop = true;
        _audioSource.volume = controller.musicVolume;
        _audioSource.PlayDelayed(3);
    }

    public void changeVolume(float value)
    {
        _audioSource.volume = value;
    }

}
