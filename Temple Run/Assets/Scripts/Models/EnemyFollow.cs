using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyFollow : MonoBehaviour
{
    public GameObject Player;  // Referencia al jugador
    public float followDistance = 2.0f; // Distancia a la que el enemigo seguirá al jugador

    void Update()
    {
        // Calcula la dirección hacia el jugador
        Vector3 dir = (Player.transform.position - transform.position).normalized;

        // Mueve al enemigo a la posición del jugador menos la distancia de seguimiento
        transform.position = Player.transform.position - dir * followDistance;

        // Hace que el enemigo mire hacia el jugador
        transform.LookAt(Player.transform);
    }
}
