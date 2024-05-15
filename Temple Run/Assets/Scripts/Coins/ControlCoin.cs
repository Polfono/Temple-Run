using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class ControlCoin : MonoBehaviour
{
    public static int coinCount;
    public GameObject coinCountDisplay;
    void Update()
    {
        coinCountDisplay.GetComponent<TextMeshProUGUI>().text = "" + coinCount;
    }
}
