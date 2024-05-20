using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NewBehaviourScript : MonoBehaviour
{
    public GameObject[] countDown;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip countdownClip;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        StartCoroutine(CountDown());
    }

    IEnumerator CountDown()
    {
        yield return new WaitForSeconds(0.5f);
        audioSource.clip = countdownClip;
        audioSource.Play();
        yield return new WaitForSeconds(0.5f);
        for (int i = 0; i < countDown.Length; i++)
        {
            countDown[i].SetActive(true);
            yield return new WaitForSeconds(1);
            countDown[i].SetActive(false);
        }
    }
}
