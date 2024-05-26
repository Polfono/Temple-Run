using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyFollow : MonoBehaviour
{
    public GameObject Player;  // Reference to the player
    public float followDistance = 8.0f; // Distance at which the enemy will follow the player
    public float smoothSpeed = 0.125f; // Velocidad de suavizado del movimiento

    private bool isFollowing = true; // Flag to control if the enemy is following the player

    [SerializeField] private Animator zombie; // Reference to the first zombie

    private Queue<Vector3> playerPositions = new Queue<Vector3>(); // Queue to store player's positions
    private float positionInterval = 0.001f; // Time interval between position records
    private float nextPositionTime = 0.0f; // Time to record the next position

    void Update()
    {
        if (isFollowing)
        {
            // Registrar la posición del jugador a intervalos
            if (Time.time >= nextPositionTime)
            {
                Vector3 playerPosition = Player.transform.position;
                playerPosition.y = 0; // Ignorar la componente y
                playerPositions.Enqueue(playerPosition);
                nextPositionTime = Time.time + positionInterval;
            }

            // Mantener la distancia de seguimiento eliminando posiciones antiguas
            while (playerPositions.Count > 0)
            {
                Vector3 playerPosition = playerPositions.Peek();
                playerPosition.y = 0; // Ignorar la componente y

                Vector3 currentPosition = Player.transform.position;
                currentPosition.y = 0; // Ignorar la componente y

                if (Vector3.Distance(playerPosition, currentPosition) <= followDistance)
                {
                    break;
                }

                playerPositions.Dequeue();
            }

            // Mover al enemigo a la posición más antigua registrada
            if (playerPositions.Count > 0)
            {
                Vector3 targetPosition = playerPositions.Peek();
                targetPosition.y = transform.position.y; // Mantener la componente y actual del enemigo

                transform.position = Vector3.Lerp(transform.position, targetPosition, smoothSpeed);
                transform.LookAt(new Vector3(Player.transform.position.x, transform.position.y, Player.transform.position.z)); // Ignorar la componente y al mirar
            }
        }
    }



    public void Acercar()
    {
        StartCoroutine(TransitionDistance(4.0f, 1.0f));
    }

    public void Alejar()
    {
        StartCoroutine(TransitionDistance(12.0f, 1.5f));
    }

    public void Comer()
    {
        // Stop following the player
        StopAllCoroutines(); // Stop any running coroutines
        isFollowing = false; // Mark that the enemy should no longer follow the player

        // Start the coroutine to move forward 11 units
        StartCoroutine(Advance(11.0f, 1.0f)); // 1.0f is the duration of the transition
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
        zombie.Play("zombieAttack");
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
