using UnityEngine;

namespace Spelunky {

    [RequireComponent(typeof(SpriteRenderer))]
    public class Tile : MonoBehaviour {

        [Tooltip("The probably for this tile to spawn. Setting this to 25 means the tile is 75% likely to be removed when the level is generated.")]
        [Range(0, 100)] public int spawnProbability = 100;

        public int x { get; private set; }
        public int y { get; private set; }

        private SpriteRenderer _spriteRenderer;

        [Header("For dynamic tile graphics")]
        public bool hasDecorations;
        public GameObject[] decorationUp;
        public GameObject[] decorationRight;
        public GameObject[] decorationDown;
        public GameObject[] decorationLeft;
        public Sprite[] alternatives;
        public Sprite[] spriteUp;
        public Sprite[] spriteDown;
        public Sprite[] spriteUpDown;

        // Tile width and height. I left these at the default Spelunky values.
        // NOTE: Changing these involves redoing all sprite assets.
        public const int Width = 16;
        public const int Height = 16;

        private void Awake() {
            _spriteRenderer = GetComponent<SpriteRenderer>();
        }

        /// <summary>
        /// Initializes a tile in the level.
        ///
        /// This gives it coordinates, names it according to it's coordinates for easier debugging and most importantly
        /// adds it to our tiles array for easier manipulation. For example to check if there is a tile above any given
        /// tile we can just do y + 1 to find out if there is an entry in the array.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public void InitializeTile(int x, int y) {
            this.x = x;
            this.y = y;
            gameObject.name = gameObject.name + " [" + x + "," + y + "]";
            LevelGenerator.instance.Tiles[x, y] = this;
        }

        /// <summary>
        /// Setup a tile in the level.
        ///
        /// This changes its sprite depending on the surrounding tiles as well as creates the decorations overhangs for
        /// the tile.
        /// </summary>
        public void SetupTile() {
            // Not a dirt tile.
            // NOTE: Currently we only setup dirt tiles.
            if (!hasDecorations) {
                return;
            }

            // Initialize our checks as false which means 'don't change anything'.
            bool up = false;
            bool right = false;
            bool down = false;
            bool left = false;

            // Check if we have dirt tiles in any of the 4 directions.
            if (y < LevelGenerator.instance.Tiles.GetLength(1) - 1 && (LevelGenerator.instance.Tiles[x, y + 1] == null || !LevelGenerator.instance.Tiles[x, y + 1].hasDecorations)) {
                up = true;
            }

            if (x < LevelGenerator.instance.Tiles.GetLength(0) - 1 && (LevelGenerator.instance.Tiles[x + 1, y] == null || !LevelGenerator.instance.Tiles[x + 1, y].hasDecorations)) {
                right = true;
            }

            if (y > 0 && (LevelGenerator.instance.Tiles[x, y - 1] == null || !LevelGenerator.instance.Tiles[x, y - 1].hasDecorations)) {
                down = true;
            }

            if (x > 0 && (LevelGenerator.instance.Tiles[x - 1, y] == null || !LevelGenerator.instance.Tiles[x - 1, y].hasDecorations)) {
                left = true;
            }

            // Add decorations and change tile graphics depending on our surroundings.
            if (up) {
                if (Random.value < 0.1f) {
                    decorationUp[1].SetActive(true);
                }
                else {
                    decorationUp[0].SetActive(true);
                }

                _spriteRenderer.sprite = spriteUp[0];
            }

            if (down) {
                decorationDown[0].SetActive(true);
                _spriteRenderer.sprite = spriteDown[0];
            }

            if (up && down) {
                _spriteRenderer.sprite = spriteUpDown[0];
            }

            if (left) {
                if (Random.value < 0.5f) {
                    decorationLeft[1].SetActive(true);
                }
                else {
                    decorationLeft[0].SetActive(true);
                }
            }

            if (right) {
                decorationRight[0].SetActive(true);
            }

            if (!up && !down) {
                if (Random.value < 0.1f) {
                    _spriteRenderer.sprite = alternatives[0];
                }
            }
        }

        /// <summary>
        /// Removes a tile from the level and destroys the game object for the tile.
        /// </summary>
        public void Remove() {
            LevelGenerator.instance.Tiles[x, y] = null;
            Destroy(gameObject);
        }

        public static Vector3 GetPositionOfCenterOfNearestTile(Vector3 position) {
            int x = Mathf.FloorToInt(Mathf.Abs(position.x) / Width) * Width + Mathf.RoundToInt(Width / 2f);
            int y = Mathf.FloorToInt(Mathf.Abs(position.y) / Height) * Height + Mathf.RoundToInt(Height / 2f);
            return new Vector3(x, y, 0);
        }

        public static Vector3 GetPositionOfLowerLeftOfNearestTile(Vector3 position) {
            int x = Mathf.FloorToInt(Mathf.Abs(position.x) / Width) * Width;
            int y = Mathf.FloorToInt(Mathf.Abs(position.y) / Height) * Height;
            return new Vector3(x, y, 0);
        }

    }

}
