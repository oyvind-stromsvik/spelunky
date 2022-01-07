using System.Collections.Generic;
using UnityEngine;
using Gizmos = Popcron.Gizmos;

namespace Spelunky {

    /// <summary>
    /// Generates our levels.
    ///
    /// TODO: Currently extremely wip. Only generates a single level and very badly.
    /// </summary>
    public class LevelGenerator : MonoBehaviour {

        public bool debug;

        public SpriteRenderer boundsStraight;
        public SpriteRenderer boundsCorner;

        public GameObject circle;
        public GameObject arrowRight;
        public GameObject arrowDown;
        public GameObject arrowLeft;

        public int roomsHorizontal = 4;
        public int roomsVertical = 4;

        [Header("Normal rooms")]
        public Room[] normalRooms;

        [Header("Special rooms")]
        public Room[] specialRooms;

        public Room[,] Rooms { get; private set; }
        public Tile[,] Tiles { get; private set; }

        // The width and height of the rooms in number of tiles.
        // I left these at the default Spelunky values so that it's easy to recreate the same rooms here if desirable.
        // NB: Changing these involves recreating all room prefabs.
        public const int RoomWidth = 10;
        public const int RoomHeight = 8;

        // The total pixel width and height of the level.
        public float LevelWidth {
            get { return RoomWidth * roomsHorizontal * Tile.Width; }
        }
        public float LevelHeight {
            get { return RoomHeight * roomsVertical * Tile.Height; }
        }

        private Dictionary<string, Tile> _tilePrefabs;
        private Dictionary<string, GameObject> _backgroundPrefabs;

        private Transform _boundsParent;
        private Transform _backgroundParent;
        private Transform _roomParent;
        private Transform _debugParent;

        private Vector2 _direction;
        private Vector2 _lastDirection;

        private Room firstRoom;
        private Room lastRoom;
        private Tile entrance;
        private Tile exit;

        private bool _hasSpawnedTrapRoom;
        private bool _hasSpawnedSacrificalAltar;

        public static LevelGenerator instance;

        private void Awake() {
            instance = this;

            Object[] resourcesTiles = Resources.LoadAll("Tiles/Prefabs", typeof(Tile));
            _tilePrefabs = new Dictionary<string, Tile>();
            foreach (Object resource in resourcesTiles) {
                Tile tile = (Tile) resource;
                _tilePrefabs.Add(tile.name, tile);
            }

            Object[] resourcesBackgrounds = Resources.LoadAll("Backgrounds/Prefabs", typeof(GameObject));
            _backgroundPrefabs = new Dictionary<string, GameObject>();
            foreach (Object resource in resourcesBackgrounds) {
                GameObject background = (GameObject) resource;
                _backgroundPrefabs.Add(background.name, background);
            }

            _boundsParent = GameObject.Find("_BOUNDS").GetComponent<Transform>();
            _backgroundParent = GameObject.Find("_BACKGROUND").GetComponent<Transform>();
            _roomParent = GameObject.Find("_ROOMS").GetComponent<Transform>();
            _debugParent = GameObject.Find("_DEBUG").GetComponent<Transform>();

            Rooms = new Room[roomsHorizontal, roomsVertical];
            Tiles = new Tile[roomsHorizontal * RoomWidth, roomsVertical * RoomHeight];
        }

        private void Start() {
            CreateLevel();
        }

        /// <summary>
        ///
        /// </summary>
        /// <exception cref="Exception"></exception>
        private void CreateLevel() {
            // 1. First create the main path from entrance to exit.
            CreateMainPathRooms();

            // 2. Then create any rooms not on the main path.
            CreateRemainingRooms();

            // 3. Setup the tiles (add variations, decorations etc.)
            InitializeTiles();
            SetupTiles();

            // 4. Create the indestructable bounds around the level.
            CreateLevelBounds();

            // 5. Create the background sprites.
            CreateBackground();

            // 6. Place the entrace and exit.
            PlaceEntranceAndExit();

            // 7. Spawn the player at the entrance.
            FindObjectOfType<GameManager>().SpawnPlayer(entrance.transform.position);
        }

        /// <summary>
        ///
        /// </summary>
        private void PlaceEntranceAndExit() {
            Tile tileToSpawnEntranceOn = firstRoom.GetSuitableEntranceOrExitTile();
            entrance = Instantiate(_tilePrefabs["Entrance"], tileToSpawnEntranceOn.transform.position + new Vector3(0, Tile.Height, 0), Quaternion.identity);

            Tile tileToSpawnExitOn = lastRoom.GetSuitableEntranceOrExitTile();
            exit = Instantiate(_tilePrefabs["Exit"], tileToSpawnExitOn.transform.position + new Vector3(0, Tile.Height, 0), Quaternion.identity);
        }

        /// <summary>
        ///
        /// </summary>
        private void CreateMainPathRooms() {
            Vector2 currentIndex = new Vector2(Random.Range(0, Rooms.GetLength(0)), Rooms.GetLength(1) - 1);
            PickRandomDirection();

            firstRoom = null;
            lastRoom = null;
            bool stopGeneration = false;
            while (stopGeneration == false) {
                Vector2 indexToCheck = new Vector2((int) currentIndex.x + (int) _direction.x, (int) currentIndex.y + (int) _direction.y);
                // Out of bounds.
                if (indexToCheck.x < 0 || indexToCheck.x >= Rooms.GetLength(0)) {
                    _lastDirection = _direction;
                    _direction = Vector2.down;
                }
                // Reached the bottom row.
                else if (indexToCheck.y < 0) {
                    if (firstRoom == null) {
                        _lastDirection = Vector2.zero;
                    }

                    _direction = Vector2.zero;

                    Room roomToSpawn = FindSuitableRoom(currentIndex);
                    if (roomToSpawn == null) {
                        Debug.LogError("No suitable main path room found. Trying to find any room instead.");
                        roomToSpawn = FindAnyRoom();
                    }

                    if (roomToSpawn == null) {
                        Debug.LogError("No room found at all!");
                    }

                    Room spawnedRoom = SpawnRoom(roomToSpawn, currentIndex);
                    if (firstRoom == null) {
                        firstRoom = spawnedRoom;
                    }

                    InstantiateDirectionArrow(currentIndex);
                    currentIndex = indexToCheck;

                    stopGeneration = true;

                    lastRoom = spawnedRoom;
                }
                // Found an empty slot.
                else if (Rooms[(int) indexToCheck.x, (int) indexToCheck.y] == null) {
                    if (firstRoom == null) {
                        _lastDirection = Vector2.zero;
                    }

                    Room roomToSpawn = FindSuitableRoom(currentIndex);
                    if (roomToSpawn == null) {
                        Debug.LogError("No suitable main path room found. Trying to find any room instead.");
                        roomToSpawn = FindAnyRoom();
                    }

                    if (roomToSpawn == null) {
                        Debug.LogError("No room found at all!");
                    }

                    Room spawnedRoom = SpawnRoom(roomToSpawn, currentIndex);
                    if (firstRoom == null) {
                        firstRoom = spawnedRoom;
                    }

                    InstantiateDirectionArrow(currentIndex);
                    currentIndex = indexToCheck;

                    PickRandomDirection();
                }
                // If all else fails try again with a different direction.
                else {
                    PickRandomDirection();
                }
            }
        }

        /// <summary>
        ///
        /// </summary>
        private void CreateRemainingRooms() {
            for (int x = 0; x < Rooms.GetLength(0); x++) {
                for (int y = 0; y < Rooms.GetLength(1); y++) {
                    Vector2 currentIndex = new Vector2(x, y);
                    if (Rooms[(int)currentIndex.x, (int)currentIndex.y] == null) {
                        Room roomToSpawn = null;
                        if (!_hasSpawnedTrapRoom && Random.value < 0.1f) {
                            _hasSpawnedTrapRoom = true;
                            roomToSpawn = specialRooms[0];
                        }
                        else if (!_hasSpawnedSacrificalAltar && Random.value < 0.1f) {
                            _hasSpawnedSacrificalAltar = true;
                            roomToSpawn = specialRooms[1];
                        }
                        else {
                            roomToSpawn = FindAnyRoom();
                        }

                        SpawnRoom(roomToSpawn, currentIndex);
                        if (debug) {
                            Instantiate(circle, CurrentPosition(currentIndex, true), Quaternion.identity, _debugParent);
                        }
                    }
                }
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="roomToSpawn"></param>
        /// <param name="currentIndex"></param>
        /// <returns></returns>
        private Room SpawnRoom(Room roomToSpawn, Vector2 currentIndex) {
            Room roomInstance = Instantiate(roomToSpawn, CurrentPosition(currentIndex), Quaternion.identity, _roomParent);
            roomInstance.name = "Room [" + currentIndex.x + "," + currentIndex.y + "]";
            roomInstance.index = currentIndex;
            roomInstance.debug = debug;
            Rooms[(int)currentIndex.x, (int)currentIndex.y] = roomInstance;
            return roomInstance;
        }

        /// <summary>
        ///
        /// </summary>
        private void PickRandomDirection() {
            _lastDirection = _direction;
            if (Random.value < 0.8f) {
                if (Random.value < 0.5f) {
                    _direction = Vector2.right;
                }
                else {
                    _direction = Vector2.left;
                }
            }
            else {
                _direction = Vector2.down;
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="arrow"></param>
        /// <returns></returns>
        private static Vector3 CurrentPosition(Vector2 currentIndex, bool arrow = false) {
            if (arrow) {
                return new Vector3(currentIndex.x * RoomWidth * Tile.Width + RoomWidth * Tile.Width / 2f, currentIndex.y * RoomHeight * Tile.Height + RoomHeight * Tile.Height / 2f, 0);
            }

            return new Vector3(currentIndex.x * RoomWidth * Tile.Width, currentIndex.y * RoomHeight * Tile.Height, 0);
        }

        /// <summary>
        ///
        /// </summary>
        private void InstantiateDirectionArrow(Vector2 currentIndex) {
            if (!debug) {
                return;
            }

            if (_direction == Vector2.right) {
                Instantiate(arrowRight, CurrentPosition(currentIndex, true), Quaternion.identity, _debugParent);
            }
            else if (_direction == Vector2.left) {
                Instantiate(arrowLeft, CurrentPosition(currentIndex, true), Quaternion.identity, _debugParent);
            }
            else {
                Instantiate(arrowDown, CurrentPosition(currentIndex, true), Quaternion.identity, _debugParent);
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        private Room FindSuitableRoom(Vector2 currentIndex) {
            bool top = _lastDirection == Vector2.down || _direction == Vector2.up;
            bool right = _lastDirection == Vector2.left || _direction == Vector2.right;
            bool down = _direction == Vector2.down;
            // TODO: This doesn't work. Don't spawn rooms with opening down if we're at the bottom.
            if (currentIndex.y == 0) {
                down = false;
            }

            bool left = _lastDirection == Vector2.right || _direction == Vector2.left;
            List<Room> suitableRooms = new List<Room>();
            foreach (Room room in normalRooms) {
                if (top && !room.top ||
                    right && !room.right ||
                    down && !room.down ||
                    left && !room.left) {
                    continue;
                }

                suitableRooms.Add(room);
            }

            return suitableRooms.Count > 0 ? suitableRooms[Random.Range(0, suitableRooms.Count)] : null;
        }

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        private Room FindAnyRoom() {
            return normalRooms[Random.Range(0, normalRooms.Length)];
        }

        /// <summary>
        /// Create an indestructable boundary around the level.
        /// </summary>
        private void CreateLevelBounds() {
            // Straights.
            SpriteRenderer boundsTop = Instantiate(boundsStraight, new Vector3(0, LevelHeight + 48, 0), Quaternion.identity, _boundsParent);
            boundsTop.size = new Vector2(LevelWidth, 64);
            boundsTop.GetComponent<BoxCollider2D>().size = new Vector2(LevelWidth, 48);
            boundsTop.GetComponent<BoxCollider2D>().offset = new Vector2(LevelWidth / 2f, 24);
            boundsTop.transform.localScale = new Vector3(1, -1, 1);
            SpriteRenderer boundsRight = Instantiate(boundsStraight, new Vector3(LevelWidth + 48, 0, 0), Quaternion.identity, _boundsParent);
            boundsRight.size = new Vector2(LevelHeight, 64);
            boundsRight.GetComponent<BoxCollider2D>().size = new Vector2(LevelHeight, 48);
            boundsRight.GetComponent<BoxCollider2D>().offset = new Vector2(LevelHeight / 2f, 24);
            boundsRight.transform.localRotation = Quaternion.Euler(0, 0, 90);
            SpriteRenderer boundsBottom = Instantiate(boundsStraight, new Vector3(0, -48, 0), Quaternion.identity, _boundsParent);
            boundsBottom.size = new Vector2(LevelWidth, 64);
            boundsBottom.GetComponent<BoxCollider2D>().size = new Vector2(LevelWidth, 48);
            boundsBottom.GetComponent<BoxCollider2D>().offset = new Vector2(LevelWidth / 2f, 24);
            SpriteRenderer boundsLeft = Instantiate(boundsStraight, new Vector3(-48, LevelHeight, 0), Quaternion.identity, _boundsParent);
            boundsLeft.size = new Vector2(LevelHeight, 64);
            boundsLeft.GetComponent<BoxCollider2D>().size = new Vector2(LevelHeight, 48);
            boundsLeft.GetComponent<BoxCollider2D>().offset = new Vector2(LevelHeight / 2f, 24);
            boundsLeft.transform.localRotation = Quaternion.Euler(0, 0, -90);

            // Corners.
            SpriteRenderer boundsCornerTopLeft = Instantiate(boundsCorner, new Vector3(0, LevelHeight + 48, 0), Quaternion.identity, _boundsParent);
            boundsCornerTopLeft.transform.localRotation = Quaternion.Euler(0, 0, 180);
            SpriteRenderer boundsCornerTopRight = Instantiate(boundsCorner, new Vector3(LevelWidth + 48, LevelHeight, 0), Quaternion.identity, _boundsParent);
            boundsCornerTopRight.transform.localRotation = Quaternion.Euler(0, 0, 90);
            SpriteRenderer boundsCornerBottomRight = Instantiate(boundsCorner, new Vector3(LevelWidth, -48, 0), Quaternion.identity, _boundsParent);
            boundsCornerBottomRight.transform.localRotation = Quaternion.Euler(0, 0, 0);
            SpriteRenderer boundsCornerBottomLeft = Instantiate(boundsCorner, new Vector3(-48, 0, 0), Quaternion.identity, _boundsParent);
            boundsCornerBottomLeft.transform.localRotation = Quaternion.Euler(0, 0, -90);

            // Fill the rest. 2 layers of just corners outside the inner "frame".
        }

        /// <summary>
        /// Just fill the background of the level.
        /// </summary>
        private void CreateBackground() {
            for (int y = 0; y < RoomHeight * roomsVertical * Tile.Height; y += 64) {
                for (int x = 0; x < RoomWidth * roomsHorizontal * Tile.Width; x += 64) {
                    Instantiate(
                        _backgroundPrefabs["Background"],
                        new Vector3(x, y, 0),
                        Quaternion.identity,
                        _backgroundParent
                    );

                    if (Random.value < 0.1f) {
                        if (Random.value < 0.5f) {
                            Instantiate(
                                _backgroundPrefabs["BackgroundDecal"],
                                new Vector3(x + Random.Range(-16, 16), y + Random.Range(-16, 16), 0),
                                Quaternion.identity,
                                _backgroundParent
                            );
                        }
                        else {
                            Instantiate(
                                _backgroundPrefabs["BackgroundDecal_2"],
                                new Vector3(x + Random.Range(-16, 16), y + Random.Range(-16, 16), 0),
                                Quaternion.identity,
                                _backgroundParent
                            );
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Initialize all the tiles in the level.
        /// </summary>
        private void InitializeTiles() {
            // Find all tiles in the level.
            Tile[] tempTiles = FindObjectsOfType<Tile>();
            foreach (Tile tile in tempTiles) {
                // Check if we should remove the tile.
                if (tile.spawnProbability <= Random.Range(0, 100)) {
                    Destroy(tile.gameObject);
                    continue;
                }

                // Otherwise initialize the tile.
                int x = (int) tile.transform.position.x / Tile.Width;
                int y = (int) tile.transform.position.y / Tile.Height;
                tile.InitializeTile(x, y);
                tile.debug = debug;
            }
        }

        /// <summary>
        /// Loop through and setup all the tiles in the level.
        ///
        /// This gives the correct sprite and decorations etc.
        /// </summary>
        private void SetupTiles() {
            for (int x = 0; x < Tiles.GetLength(0); x++) {
                for (int y = 0; y < Tiles.GetLength(1); y++) {
                    // No tile.
                    if (Tiles[x, y] == null) {
                        continue;
                    }

                    Tiles[x, y].SetupTile();
                }
            }
        }

        /// <summary>
        /// Remove tiles from the level.
        /// </summary>
        /// <param name="tilesToRemove"></param>
        public void RemoveTiles(Tile[] tilesToRemove) {
            // Find the bounds of the tiles to remove while we remove the specified tiles.
            int minX = int.MaxValue;
            int maxX = -1;
            int minY = int.MaxValue;
            int maxY = -1;
            foreach (Tile tile in tilesToRemove) {
                if (tile.x < minX) {
                    minX = tile.x;
                }

                if (tile.x > maxX) {
                    maxX = tile.x;
                }

                if (tile.y < minY) {
                    minY = tile.y;
                }

                if (tile.y > maxY) {
                    maxY = tile.y;
                }

                // Remove the specified tile.
                tile.Remove();
            }

            // Expand the bounds by 1...
            minX--;
            maxX++;
            minY--;
            maxY++;

            // But ensure we stay within the level bounds.
            if (minX < 0) {
                minX = 0;
            }
            if (maxX >= Tiles.GetLength(0)) {
                maxX = Tiles.GetLength(0) - 1;
            }
            if (minY < 0) {
                minY = 0;
            }
            if (maxY >= Tiles.GetLength(1)) {
                maxY = Tiles.GetLength(1) - 1;
            }

            // Setup the tiles surrounding the tiles we just removed using the bounds we've just founds, so that the
            // affected tiles get the correct sprites and decorations now that their neighbor tiles are gone.
            for (int x = minX; x <= maxX; x++) {
                for (int y = minY; y <= maxY; y++) {
                    // No tile.
                    if (Tiles[x, y] == null) {
                        continue;
                    }

                    Tiles[x, y].SetupTile();
                }
            }
        }

    }

}
