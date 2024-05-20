using UnityEngine;

public class PushableBox : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        // Verifica si el objeto con el que colisionamos es el jugador
        if (other.CompareTag("Player"))
        {
            // Calcula la direcci�n del empuje basado en la posici�n del jugador y la caja
            Vector3 pushDirection = other.transform.position - transform.position;
            pushDirection.y = 0f; // No empujar verticalmente

            // Aplica un impulso al Rigidbody de la caja
            GetComponent<Rigidbody>().AddForce(pushDirection.normalized * 5f, ForceMode.Impulse);
        }
    }
}
