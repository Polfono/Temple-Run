using UnityEngine;
using Cinemachine;
using System.Collections;

public class CameraSwitch : MonoBehaviour
{
    [SerializeField] private GameObject parametros;

    void Start()
    {
        // Inicia la corrutina para cambiar la cámara después de 3 segundos.
        StartCoroutine(ChangeCameraAfterDelay(3.0f));
    }

    IEnumerator ChangeCameraAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        ActivateVirtualCamera();
    }

    void ActivateVirtualCamera()
    {
        GetComponent<CinemachineVirtualCamera>().enabled = true;
        parametros.SetActive(true);
    }
}
