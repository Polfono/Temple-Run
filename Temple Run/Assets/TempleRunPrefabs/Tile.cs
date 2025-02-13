using UnityEngine;


namespace TempleRun {

public enum TileType {
    STRAIGHT,
    LEFT,
    RIGHT,
    SIDEWAYS,
    JUMP
}

/// <summary>
/// Defines the attributes of a tile.
/// </summary>
public class Tile : MonoBehaviour
{
    public TileType type;
    public Transform pivot;
}

}