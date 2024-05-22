using System.Collections.Generic;
using UnityEngine;

namespace TempleRun
{

    public class TileSpawner : MonoBehaviour
    {
        [SerializeField] private int tilesStart = 7;
        [SerializeField] private int minStraightTiles = 3;
        [SerializeField] private int maxStraightTiles = 15;
        [SerializeField] private GameObject castleTile;
        [SerializeField] private GameObject startingTile;
        [SerializeField] private List<GameObject> turnTiles;
        [SerializeField] private List<GameObject> obstacles;
        [SerializeField] private List<GameObject> gems;
        [SerializeField] private List<GameObject> jumps;

        private Vector3 currentTileLocation = Vector3.zero;
        private Vector3 currentTileDirection = Vector3.forward;
        private GameObject prevTile;

        private List<GameObject> currentTiles;
        private List<GameObject> currentObstacles;
        private List<GameObject> currentGems;
        private List<GameObject> currentJumps;

        private void Start()
        {
            currentTiles = new List<GameObject>();
            currentObstacles = new List<GameObject>();
            currentGems = new List<GameObject>();
            currentJumps = new List<GameObject>();

            Random.InitState(System.DateTime.Now.Millisecond);

            // Spawn the first straight tile without obstacle
            SpawnTile(castleTile.GetComponent<Tile>(), false, false);
            for(int i = 0; i < 1; i++) SpawnTile(startingTile.GetComponent<Tile>(), false, false);

            for (int i = 1; i < tilesStart - 1; i++)
            {
                SpawnRandomTile();
            }

            // Spawn the last straight tile without obstacle
            SpawnTile(startingTile.GetComponent<Tile>(), false);

            // Spawn a turn tile
            SpawnTile(RandomGameObjectFromList(turnTiles).GetComponent<Tile>());
        }

        private void SpawnTile(Tile tile, bool spawnObstacle = false, bool spawnGems = true)
        {
            Quaternion newTileRotation = tile.gameObject.transform.rotation * Quaternion.LookRotation(currentTileDirection, Vector3.up);
            prevTile = GameObject.Instantiate(tile.gameObject, currentTileLocation, newTileRotation);
            currentTiles.Add(prevTile);

            if (tile.type == TileType.STRAIGHT)
            {
                if (spawnObstacle)
                {
                    SpawnObstacle();
                }
                else if (Random.value <= 0.5f && spawnGems)
                {
                    SpawnGems();
                }
                currentTileLocation += Vector3.Scale(prevTile.GetComponent<Renderer>().bounds.size, currentTileDirection);
            }
            else if (tile.type == TileType.JUMP)
            {
                currentTileLocation += Vector3.Scale(prevTile.GetComponent<Renderer>().bounds.size, currentTileDirection);
            }
        }

        private void SpawnRandomTile()
        {
            if (Random.value <= 0.2f)
            {
                // Ensure a straight tile before a jump
                SpawnTile(startingTile.GetComponent<Tile>(), false);
                // 20% chance to spawn a jump tile without obstacle
                SpawnTile(RandomGameObjectFromList(jumps).GetComponent<Tile>(), false);
                // Ensure a straight tile after a jump
                SpawnTile(startingTile.GetComponent<Tile>(), false);
            }
            else
            {
                // 80% chance to spawn a straight tile
                bool spawnObstacle = Random.value <= 0.4f;
                SpawnTile(startingTile.GetComponent<Tile>(), spawnObstacle);
            }
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

            foreach (GameObject gem in currentGems)
            {
                Destroy(gem);
            }
        }

        public void addNewDirection(Vector3 direction)
        {
            currentTileDirection = direction;
            DeletePreviousTiles();

            Vector3 tilePlacementScale;
            if (prevTile.GetComponent<Tile>().type == TileType.SIDEWAYS)
            {
                tilePlacementScale = Vector3.Scale(prevTile.GetComponent<Renderer>().bounds.size / 2 + (Vector3.one * startingTile.GetComponent<BoxCollider>().bounds.size.z / 2), currentTileDirection);
            }
            else
            {
                tilePlacementScale = Vector3.Scale((prevTile.GetComponent<Renderer>().bounds.size - (Vector3.one * 2)) + (Vector3.one * startingTile.GetComponent<BoxCollider>().bounds.size.z / 2), currentTileDirection);
            }
            // añade 10 unidades de distancia a tilePlacementScale en la dirección actual
            tilePlacementScale += Vector3.Scale(Vector3.one * 16, currentTileDirection);
            currentTileLocation += tilePlacementScale;
            int currentPathLength = Random.Range(minStraightTiles, maxStraightTiles);

            // Spawn the first straight tile without obstacle
            SpawnTile(startingTile.GetComponent<Tile>(), false);

            for (int i = 1; i < currentPathLength - 1; i++)
            {
                SpawnRandomTile();
            }

            // Spawn the last straight tile without obstacle
            SpawnTile(startingTile.GetComponent<Tile>(), false);

            // Spawn a turn tile
            SpawnTile(RandomGameObjectFromList(turnTiles).GetComponent<Tile>());
        }

        private void SpawnObstacle()
        {
            GameObject obstacle = RandomGameObjectFromList(obstacles);
            Quaternion obstacleRotation = obstacle.gameObject.transform.rotation * Quaternion.LookRotation(currentTileDirection, Vector3.up);
            GameObject gameObject = Instantiate(obstacle, currentTileLocation, obstacleRotation);
            currentObstacles.Add(gameObject);
        }

        private void SpawnGems()
        {
            GameObject gem = RandomGameObjectFromList(gems);
            Quaternion gemRotation = gem.gameObject.transform.rotation * Quaternion.LookRotation(currentTileDirection, Vector3.up);

            GameObject gameObject = Instantiate(gem, currentTileLocation, gemRotation);

            currentGems.Add(gameObject);
        }

        private GameObject RandomGameObjectFromList(List<GameObject> list)
        {
            if (list.Count == 0) return null;
            return list[Random.Range(0, list.Count)];
        }
    }

}
