using TMPro;
using UnityEngine;

public class getLabel : MonoBehaviour
{

    public TextMeshProUGUI text;

    // Use this for initialization
    void Start()
    {
        text = GetComponent<TMPro.TextMeshProUGUI>();
    }
}
