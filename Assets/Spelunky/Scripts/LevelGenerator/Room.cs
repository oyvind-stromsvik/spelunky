using System.Collections.Generic;
using UnityEngine;
using Gizmos = Popcron.Gizmos;

namespace Spelunky {

    /// <summary>
    ///
    /// </summary>
    public class Room : MonoBehaviour {

        public Vector2 index;

        [HideInInspector]
        public bool debug;

        public bool top, right, down, left;

        /// <summary>
        ///
        /// </summary>
        private void Update() {
            if (!debug) {
                return;
            }

            for (int x = 0; x <= LevelGenerator.RoomWidth; x++) {
                Gizmos.Line(
                    new Vector3(transform.position.x + x * Tile.Width, transform.position.y, 0),
                    new Vector3(transform.position.x + x * Tile.Width, transform.position.y, 0) + Vector3.up * LevelGenerator.RoomHeight * Tile.Height,
                    new Color(1, 1, 1, 0.3f)
                );
            }

            for (int y = 0; y <= LevelGenerator.RoomHeight; y++) {
                Gizmos.Line(
                    new Vector3(transform.position.x, transform.position.y + y * Tile.Height, 0),
                    new Vector3(transform.position.x, transform.position.y + y * Tile.Height, 0) + Vector3.right * LevelGenerator.RoomWidth * Tile.Width,
                    new Color(1, 1, 1, 0.3f)
                );
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        private Tile[] GetRoomTiles() {
            List<Tile> roomTiles = new List<Tile>();

            for (int x = (int)index.x * LevelGenerator.RoomWidth; x < (int)(index.x + 1) * LevelGenerator.RoomWidth; x++) {
                for (int y = (int)index.y * LevelGenerator.RoomHeight; y < (int)(index.y + 1) * LevelGenerator.RoomHeight; y++) {
                    // No tile.
                    if (LevelGenerator.instance.Tiles[x, y] == null) {
                        continue;
                    }

                    roomTiles.Add(LevelGenerator.instance.Tiles[x, y]);
                }
            }

            return roomTiles.ToArray();
        }

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        public Tile GetSuitableEntranceOrExitTile() {
            Tile[] roomTiles = GetRoomTiles();
            List<Tile> suitableTiles = new List<Tile>();

            foreach (Tile tile in roomTiles) {
                // TODO: We only want to spawn the exit on a "normal" tile. Find a better and more generic solution for determining this.
                if (tile.name.Contains("Dirt") == false) {
                    continue;
                }

                // If there is an empty space above the tile we can spawn a door here, but make sure we don't try to
                // spawn a door out of bounds or so far up it's on the bottom of the room above us.
                int yPositionToCheck = tile.y + 1;
                int roomMaxYPosition = (int) (index.y + 1) * LevelGenerator.RoomHeight - 1;
                if (yPositionToCheck < roomMaxYPosition && yPositionToCheck < LevelGenerator.instance.Tiles.GetLength(1) - 1 && LevelGenerator.instance.Tiles[tile.x, yPositionToCheck] == null) {
                    suitableTiles.Add(tile);
                }
            }

            return suitableTiles.Count > 0 ? suitableTiles[Random.Range(0, suitableTiles.Count)] : null;
        }
    }

}
