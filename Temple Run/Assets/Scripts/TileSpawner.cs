using System.Collections.Generic;
using System.Data;
using UnityEngine;
using UnityEngine.ProBuilder.MeshOperations;

namespace TempleRun {

public class TileSpawner : MonoBehaviour
{
        [SerializeField] private int tilesStart = 10;
        [SerializeField] private int minStraightTiles = 3;
        [SerializeField] private int maxStraightTiles = 15;
        [SerializeField] private GameObject startingTile;
        [SerializeField] private List<GameObject> turnTiles;
        [SerializeField] private List<GameObject> obstacles;

        private Vector3 currentTileLocation = Vector3.zero;
        private Vector3 currentTileDirection = Vector3.forward;
        private GameObject prevTile;

        private List<GameObject> currentTiles;
        private List<GameObject> currentObstacles;

        private void Start()
        {
            currentTiles = new List<GameObject>();
            currentObstacles = new List<GameObject>();

            Random.InitState(System.DateTime.Now.Millisecond);

            for (int i = 0; i < tilesStart; i++)
            {
                SpawnTile(startingTile.GetComponent<Tile>());
            }

            SpawnTile(RandomGameObjectFromList(turnTiles).GetComponent<Tile>());
        }

        private void SpawnTile(Tile tile, bool spawnObstacle = false)
        {
            Quaternion newTileRotation = tile.gameObject.transform.rotation * Quaternion.LookRotation(currentTileDirection, Vector3.up);
            prevTile = GameObject.Instantiate(tile.gameObject, currentTileLocation, newTileRotation);
            currentTiles.Add(prevTile);

            if(spawnObstacle) SpawnObstacle();

            if (tile.type == TileType.STRAIGHT) currentTileLocation += Vector3.Scale(prevTile.GetComponent<Renderer>().bounds.size, currentTileDirection);
        }

        private void DeletePreviousTiles()
        {
            while (currentTiles.Count != 1)
            {
                GameObject tile = currentTiles[0];
                currentTiles.RemoveAt(0);
                Destroy(tile);
            }

            foreach (GameObject obstacle in currentObstacles)
            {
                Destroy(obstacle);
            }
        }

        public void addNewDirection(Vector3 direction)
        {
            currentTileDirection = direction;
            DeletePreviousTiles();

            Vector3 tilePlacementScale;
            if (prevTile.GetComponent<Tile>().type == TileType.SIDEWAYS) {
                tilePlacementScale = Vector3.Scale(prevTile.GetComponent<Renderer>().bounds.size /2 + (Vector3.one * startingTile.GetComponent<BoxCollider>().bounds.size.z /2), currentTileDirection);
            }
            else
            {
                tilePlacementScale = Vector3.Scale((prevTile.GetComponent<Renderer>().bounds.size - (Vector3.one * 2)) + (Vector3.one * startingTile.GetComponent<BoxCollider>().bounds.size.z / 2), currentTileDirection);
            }

            currentTileLocation += tilePlacementScale;
            int currentPathLenght = Random.Range(minStraightTiles, maxStraightTiles);
            for (int i = 0; i < currentPathLenght; i++)
            {
                SpawnTile(startingTile.GetComponent<Tile>(), (i == 0) ? false : true);
            }

            SpawnTile(RandomGameObjectFromList(turnTiles).GetComponent<Tile>());
        }

        private void SpawnObstacle ()
        {
            if (Random.value > 0.2f) return;

            GameObject obstacle = RandomGameObjectFromList(obstacles);
            Quaternion obstacleRotation = obstacle.gameObject.transform.rotation * Quaternion.LookRotation(currentTileDirection, Vector3.up);
            GameObject gameObject = Instantiate(obstacle, currentTileLocation, obstacleRotation);
            currentObstacles.Add(gameObject);
        }

        private GameObject RandomGameObjectFromList(List<GameObject> list)
        {
            if (list.Count == 0) return null;
            return list[Random.Range(0, list.Count)];
        }
}

}