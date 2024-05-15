using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class CollectCoin : MonoBehaviour
{
    public AudioSource coinFX;
    private void OnTriggerEnter(Collider other)
    {
        coinFX.Play();
        ControlCoin.coinCount++;
        Destroy(gameObject);
    }
}
