using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class uiManager : MonoBehaviour
{
    // Start is called before the first frame update
    public rocketController rC;
    public Image tank;
    public Image overHeat;

    public getLabel speedLabel;
    public getLabel heatLabel;

    float fuelAmount;

    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        fuelAmount = rC.fuel / 100;

        tank.fillAmount = fuelAmount;

        overHeat.fillAmount = rC.overheating/100;

        heatLabel.text.text = rC.overheating.ToString("F0") + "%";
    }
}
