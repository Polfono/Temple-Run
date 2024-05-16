using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;
using UnityEngine.UI;   

public class DistanceControl : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI distanceText;
    public void UpdateDistance(int score)
    {
        distanceText.text = score.ToString();
    }
}
