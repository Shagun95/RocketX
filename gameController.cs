using System;
using System.Collections;
using System.Collections.Generic;
using GoogleMobileAds.Api;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class gameController : MonoBehaviour
{

    /// <summary>
    /// mondo che sta usando ora
    /// </summary>
    public int WOLRD;

    public int record;

    public int money;

    private float timer;
    private float moneyForLabel;

    public getLabel recordLabel, rocketLevelLabel, tmpPlusLabel;

    public getLabel scoreText, moneyText;
    private int score;

    public GameObject recordPlacer;

    public soundManager audioManager;

    /// <summary>
    /// per ui
    /// </summary>
    public Sprite pauseSprite, playSprite;
    public Button pauseButton;


    /// <summary>
    /// Indica quando inizia il gioco
    /// </summary>
    public bool hasStarted;

    //public GameObject restartPanel;
    public Canvas menu;

    public int rocketChoise;

    public GameObject rocketChoosen;

    /// <summary>
    /// Livello rocket
    /// </summary>
    private int tmpRecord;

    #region attach

    public rocketController rkController;

    public GameObject rocketOrange, rocketBlack, classicRocket, FH, nasaRocket, esaRocket, starship;

    public getLabel fuelLabel, dragLabel, velocityLabel, overHetLabel, inclinLabel, altitudeLabel;

    public CameraShake cShake;

    public cameraFollow cFollow;

    public uiManager uimanager;

    public smokeController smoke;

    public Camera bkCamera;

    #endregion

    public Animator pointsAnimator;

    private bool startAnimate;
    private int newMoney;

    /// <summary>
    /// stringa per richiamare record a seconda di dove provengo
    /// </summary>
    private string recordPath;

    //banner
    private BannerView bannerView;
    private RewardedAd doubleUp;
    public Button rewardButton;
    private InterstitialAd interstitial;
    /// <summary>
    /// numero volte mostra interstetial
    /// </summary>
    private int intestTimes;

    // Start is called before the first frame update
    void Start()
    {
        initializeAds();
        score = 0;
        Application.targetFrameRate = 30;
        hasStarted = false;
        startAnimate = false;
        Time.timeScale = 1;
        timer = 0;
        //moneyText.text.text = PlayerPrefs.GetInt("money", 0).ToString();
        moneyForLabel = money;

        intestTimes = PlayerPrefs.GetInt("interst", 0);
        //RAZZO ATTUALMENTE SCELTO
        rocketChoise = PlayerPrefs.GetInt("ROCKET", 0);

        switch (WOLRD)
        {
            case 0:
                recordPath = "EARTH_RECORD";
                break;

            case 1:
                recordPath = "MARS_RECORD";
                break;
            case 2:
                recordPath = "VENUS_RECORD";
                break;
            case 3:
                recordPath = "EUROPA_RECORD";
                break;
            case 4:
                recordPath = "PANDORA_RECORD";
                break;
        }

        record = PlayerPrefs.GetInt(recordPath, 0);

        //record che userò per segnalare a video, prima almeno a 100 
        tmpRecord = record > 50 ? record : 50;
        recordPlacer.transform.position = new Vector3(-2, tmpRecord, -5);
        recordPlacer.GetComponent<TextMeshPro>().text = "- " + tmpRecord.ToString() ;
        money = PlayerPrefs.GetInt("money", 0);

        recordLabel.text.text = record.ToString();
        
        switch(rocketChoise)
        {
            case 0:
                rocketChoosen = Instantiate(rocketOrange);
                break;

            case 1:
                rocketChoosen = Instantiate(rocketBlack);
                break;
            case 2:
                rocketChoosen = Instantiate(classicRocket);
                break;
            case 3:
                rocketChoosen = Instantiate(FH);
                break;
            case 4:
                rocketChoosen = Instantiate(nasaRocket);
                break;
            case 5:
                rocketChoosen = Instantiate(esaRocket);
                break;
            case 6:
                rocketChoosen = Instantiate(starship);
                break;
        }
        rkController = rocketChoosen.GetComponent<rocketController>();
        rocketChoosen.transform.position = new Vector3(0, 0, 0);
        setRocket();
        
        //rocketLevelLabel.text.text = rkController.rocketLevel.ToString();
        
    }



    /// <summary>
    /// Aggiunge e setta tutti gli script del razzo
    /// </summary>
    private void setRocket()
    {
        if (!rocketChoosen.GetComponent<rocketController>())
        {
            rocketChoosen.AddComponent<rocketController>();
            rkController = rocketChoosen.GetComponent<rocketController>();
        }

        rkController.fuelLabel = fuelLabel;
        rkController.dragLabel = dragLabel;
        rkController.velocityLabel = velocityLabel;
        rkController.overHetLabel = overHetLabel;
        rkController.inclinLabel = inclinLabel;
        rkController.altitudeLabel = altitudeLabel;
        rkController.cShake = cShake;
        rkController.efManager.backCamera = bkCamera;

        cFollow.target = rocketChoosen;
        cFollow.rocket = rkController;

        rkController.gameController = this;
        uimanager.rC = rkController;

        smoke.rk = rkController;
        smoke.target = rocketChoosen;
    }


    // Update is called once per frame
    void Update()
    {
        //così inizia sempre da 0 il label di score
        if (rkController.altitude > score)
        {
            score = (int)rkController.altitude;
            scoreText.text.text = score.ToString();
        }

        if (score > record)
        {
            tmpRecord = score;
        }

        if (startAnimate)
        {
            audioManager.coinSoundPlay();
            if (moneyForLabel < newMoney)
            {
                moneyForLabel = Mathf.Lerp(money, newMoney, timer);
                timer += Time.deltaTime / 3;
            } else
            {
                startAnimate = false;
                timer = 0;
            }

            moneyText.text.text = moneyForLabel.ToString("F0");
        }


    }


    public void engineRunSound(bool play, float altitude, bool stop)
    {
        audioManager.manageEngineSound(play, rkController.rocketAudio, altitude, stop);
    }

    public void stopRocketEngine()
    {
        audioManager.stopEngine();
    }

    /// <summary>
    /// Gestisce gameOver
    /// </summary>
    public void gameOver()
    {
        audioManager.explosionSound();
        Debug.Log("GameOver");
        int moneyNow = PlayerPrefs.GetInt("money", 0);
        newMoney = money + (int)(score/5);
        PlayerPrefs.SetInt("money", money);
        PlayerPrefs.Save();
        StartCoroutine(showMenu());
    }

    /// <summary>
    /// Missione riuscita
    /// </summary>
    public void succed()
    {
        Debug.Log("OK");

        if (score > record)
        {
            //altra animazione
            PlayerPrefs.SetInt(recordPath, score);
            PlayerPrefs.Save();
        }
        int moneyNow = PlayerPrefs.GetInt("money", 0);
        newMoney = money + score * 3;
        PlayerPrefs.SetInt("money", money);
        PlayerPrefs.Save();


        StartCoroutine(showMenu());
    }

    /// <summary>
    /// bottone reinizia livello
    /// </summary>
    public void replayBt()
    {
        Destroy(gameObject);
        SceneManager.LoadScene(SceneManager.GetActiveScene().name, LoadSceneMode.Single);
    }

    /// <summary>
    /// Bottone menu
    /// </summary>
    public void menuBt()
    {
        bannerView.Hide();
        SceneManager.LoadScene("menu", LoadSceneMode.Single);
    }

    /// <summary>
    /// mostra il menu in ritardo
    /// </summary>
    /// <returns></returns>
    IEnumerator showMenu ()
    {
        
        yield return new WaitForSeconds(2);
        manageShowInterstetial();
        tmpPlusLabel.text.text = "+ " + (newMoney - money).ToString();
        menu.enabled = true;
        pointsAnimator.SetBool("start", true);
        //tmpPlusLabel.text.text = "+ " + (newMoney - money).ToString();
        //moneyText.text.text = money.ToString();
        StartCoroutine(increaseMoney());
        
    }

    IEnumerator increaseMoney()
    {
        yield return new WaitForSeconds(1);
        
        PlayerPrefs.SetInt("money", newMoney);
        PlayerPrefs.Save();
        
        startAnimate = true;
    }


    /// <summary>
    /// Bottone pausa
    /// </summary>
    public void pauseButtonClick()
    {
        //metto in pausa
        Debug.Log(Time.timeScale);
        if (Time.timeScale == 1)
        {
            pauseButton.image.sprite = playSprite;
            Time.timeScale = 0;
        } else
        {
            pauseButton.image.sprite = pauseSprite;
            Time.timeScale = 1;
        }
        
    }
    /// <summary>
    /// Richiamato quando guarda tutto video
    /// </summary>
    private void doubleCoins()
    {
        int doubl = newMoney - money; //quello che aggiungo
        money = newMoney;
        newMoney += doubl;

        PlayerPrefs.SetInt("money", newMoney);
        PlayerPrefs.Save();

        tmpPlusLabel.text.text = doubl.ToString();
        pointsAnimator.Play("tmpPlusLabel", 0);
        startAnimate = true;
    }


    #region ads
    private void initializeAds()
    {
        // Initialize the Google Mobile Ads SDK.
        //MobileAds.Initialize(initStatus => { }); spostato in scena iniziale

        // Create an empty ad request.
        AdRequest request = new AdRequest.Builder().Build();

        RequestBanner(request);
        loadReward(request);
        loadInterst(request);
    }


    private void RequestBanner(AdRequest req)
    {
        #if UNITY_ANDROID
                string adUnitId = "ca-app-pub-9220119611394678/6807329502";
        #elif UNITY_IPHONE
                    string adUnitId = "ca-app-pub-3940256099942544/2934735716";
        #endif

        // Create a 320x50 banner at the top of the screen.
        bannerView = new BannerView(adUnitId, AdSize.Banner, AdPosition.Bottom);

        

        // Load the banner with the request.
        bannerView.LoadAd(req);
    }

    private void loadReward(AdRequest req)
    {
        doubleUp = new RewardedAd("ca-app-pub-9220119611394678/7928839482");

        // Load the rewarded ad with the request.
        doubleUp.LoadAd(req);

        // Called when an ad request has successfully loaded.
        doubleUp.OnAdLoaded += HandleRewardedAdLoaded;
        // Called when an ad request failed to load.
        doubleUp.OnAdFailedToLoad += HandleRewardedAdFailedToLoad;
        // Called when an ad is shown.
        doubleUp.OnAdOpening += HandleRewardedAdOpening;
        // Called when an ad request failed to show.
        doubleUp.OnAdFailedToShow += HandleRewardedAdFailedToShow;
        // Called when the user should be rewarded for interacting with the ad.
        doubleUp.OnUserEarnedReward += HandleUserEarnedReward;
        // Called when the ad is closed.
        doubleUp.OnAdClosed += HandleRewardedAdClosed;
    }

    private void loadInterst(AdRequest req)
    {
        // Initialize an InterstitialAd.
        interstitial = new InterstitialAd("ca-app-pub-9220119611394678/8469903147");
        // Load the interstitial with the request.
        interstitial.LoadAd(req);
    }

    /// <summary>
    /// mostra interst 3 volte per sessione
    /// </summary>
    private void manageShowInterstetial()
    {
        Debug.Log(intestTimes);
        if(intestTimes % 3 == 0 && intestTimes != 0)
        {
            if (interstitial.IsLoaded())
            {
                interstitial.Show();
            }
            PlayerPrefs.SetInt("interst", 0);
        } else
        {
            intestTimes++;
            PlayerPrefs.SetInt("interst", intestTimes);
        }
        PlayerPrefs.Save();
    }

    #region reward

    public void HandleRewardedAdLoaded(object sender, EventArgs args)
    {
        MonoBehaviour.print("HandleRewardedAdLoaded event received");
    }

    public void HandleRewardedAdFailedToLoad(object sender, EventArgs args)
    {
        MonoBehaviour.print(
            "HandleRewardedAdFailedToLoad event received with message: "
                             + args.ToString());
    }

    public void HandleRewardedAdOpening(object sender, EventArgs args)
    {
        MonoBehaviour.print("HandleRewardedAdOpening event received");
    }

    public void HandleRewardedAdFailedToShow(object sender, AdErrorEventArgs args)
    {
        MonoBehaviour.print(
            "HandleRewardedAdFailedToShow event received with message: "
                             + args.Message);
    }

    public void HandleRewardedAdClosed(object sender, EventArgs args)
    {
        MonoBehaviour.print("HandleRewardedAdClosed event received");
        // Create an empty ad request.
        AdRequest request = new AdRequest.Builder().Build();
        // Load the rewarded ad with the request.
        doubleUp.LoadAd(request);
    }

    public void HandleUserEarnedReward(object sender, Reward args)
    {
        doubleCoins();
    }

    public void UserChoseToWatchAd()
    {
        if (doubleUp.IsLoaded())
        {
            rewardButton.interactable = false; //disabilito dopo che guarda video
            doubleUp.Show();
        }
    }

    #endregion


    #endregion
}
