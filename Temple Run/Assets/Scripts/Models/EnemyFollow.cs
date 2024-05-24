using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyFollow : MonoBehaviour
{
    public GameObject Player;  // Referencia al jugador
    public float followDistance = 8.0f; // Distancia a la que el enemigo seguirá al jugador

    private bool isFollowing = true; // Bandera para controlar si el enemigo sigue al jugador

    [SerializeField] private Animator zombie1; // Referencia al primer zombie
    [SerializeField] private Animator zombie2; // Referencia al segundo zombie
    [SerializeField] private Animator zombie3;

    void Update()
    {
        if (isFollowing)
        {
            // Calcula la dirección hacia el jugador
            Vector3 dir = (Player.transform.position - transform.position).normalized;

            // Mueve al enemigo a la posición del jugador menos la distancia de seguimiento
            transform.position = Player.transform.position - dir * followDistance;

            // Hace que el enemigo mire hacia el jugador
            transform.LookAt(Player.transform);
        }
    }

    public void Acercar()
    {
        StartCoroutine(TransitionDistance(4.0f, 1.0f));
    }

    public void Alejar()
    {
        StartCoroutine(TransitionDistance(10.0f, 1.5f));
    }

    public void Comer()
    {
        // Dejar de seguir al jugador
        StopAllCoroutines(); // Detiene cualquier corrutina en ejecución, incluyendo el seguimiento del jugador
        isFollowing = false; // Marca que el enemigo ya no debe seguir al jugador

        // Iniciar la corrutina para avanzar 5 unidades hacia adelante
        StartCoroutine(Advance(11.0f, 1.0f)); // 1.0f es la duración de la transición
    }

    private IEnumerator TransitionDistance(float targetDistance, float duration)
    {
        float initialDistance = followDistance;
        float elapsedTime = 0.0f;

        while (elapsedTime < duration)
        {
            followDistance = Mathf.Lerp(initialDistance, targetDistance, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        followDistance = targetDistance;
    }

    public void AtacarAnimacion()
    {
        zombie1.Play("zombieAttack");
        zombie2.Play("zombieAttack");
        zombie3.Play("zombieAttack");
    }

    private IEnumerator Advance(float distance, float duration)
    {
        Vector3 initialPosition = transform.position;
        Vector3 targetPosition = transform.position + transform.forward * distance;
        float elapsedTime = 0.0f;

        while (elapsedTime < duration)
        {
            transform.position = Vector3.Lerp(initialPosition, targetPosition, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        AtacarAnimacion();
        transform.position = targetPosition;
    }
}
