using System.Collections.Generic;
using UnityEngine;

namespace Spelunky {

    public class Room : MonoBehaviour {
        public Vector2 index;

        public bool drawGrid;

        public bool top, right, down, left;

        private void OnDrawGizmos() {
            if (!drawGrid) {
                return;
            }

            for (int x = 0; x <= LevelGenerator.instance.RoomWidth; x++) {
                Gizmos.DrawRay(new Vector3(transform.position.x + x * Tile.Width, transform.position.y, 0), Vector3.up * LevelGenerator.instance.RoomHeight * Tile.Height);
            }

            for (int y = 0; y <= LevelGenerator.instance.RoomHeight; y++) {
                Gizmos.DrawRay(new Vector3(transform.position.x, transform.position.y + y * Tile.Height, 0), Vector3.right * LevelGenerator.instance.RoomWidth * Tile.Width);
            }
        }

        public Tile[] GetRoomTiles() {
            List<Tile> roomTiles = new List<Tile>();

            for (int x = (int) index.x * LevelGenerator.instance.RoomWidth; x < (int) (index.x + 1) * LevelGenerator.instance.RoomWidth; x++)
            for (int y = (int) index.y * LevelGenerator.instance.RoomHeight; y < (int) (index.y + 1) * LevelGenerator.instance.RoomHeight; y++) {
                // No tile.
                if (LevelGenerator.instance.Tiles[x, y] == null) {
                    continue;
                }

                roomTiles.Add(LevelGenerator.instance.Tiles[x, y]);
            }

            return roomTiles.ToArray();
        }

        public Tile GetSuitableEntranceOrExitTile() {
            Tile[] roomTiles = GetRoomTiles();
            List<Tile> suitableTiles = new List<Tile>();

            foreach (Tile tile in roomTiles) {
                // TODO: We only want to spawn the exit on a "normal" tile. Find a better and more generic solution for determining this.
                if (tile.name.Contains("Dirt") == false) {
                    continue;
                }

                // If there is an empty space above the tile we can spawn an exit here.
                // But make sure we don't try to spawn an exit out of bounds or so far
                // up it's on the bottom of the room above us.
                int yPositionToCheck = tile.y + 1;
                int roomMaxYPosition = (int) (index.y + 1) * LevelGenerator.instance.RoomHeight - 1;
                if (yPositionToCheck < roomMaxYPosition && yPositionToCheck < LevelGenerator.instance.Tiles.GetLength(1) - 1 && LevelGenerator.instance.Tiles[tile.x, yPositionToCheck] == null) {
                    suitableTiles.Add(tile);
                }
            }

            return suitableTiles.Count > 0 ? suitableTiles[Random.Range(0, suitableTiles.Count)] : null;
        }
    }

}