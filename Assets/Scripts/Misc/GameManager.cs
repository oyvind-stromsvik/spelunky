using TwiiK.Utility;
using UnityEngine;

namespace Spelunky {

    public class GameManager : Singleton<GameManager> {

        [Header("Player")]
        public Player player;
        public CameraFollow playerCamera;

        [Header("Level")]
        [Tooltip("Reference to the LevelGenerator in the scene")]
        public LevelGenerator levelGenerator;

        [Tooltip("When true, skips procedural generation and scans the scene for existing entities")]
        public bool useExistingSceneContent;

        // Sub-managers for the centralized game loop
        public PlatformManager PlatformManager { get; private set; }
        public EntityManager EntityManager { get; private set; }
        public TimerManager TimerManager { get; private set; }

        public bool IsGameOver { get; private set; }

        public override void Awake() {
            base.Awake();
            CreateSubManagers();
        }

        private void Start() {
            InitializeLevel();
        }

        private void CreateSubManagers() {
            // Create child GameObjects for each sub-manager
            GameObject platformManagerObj = new GameObject("PlatformManager");
            platformManagerObj.transform.SetParent(transform);
            PlatformManager = platformManagerObj.AddComponent<PlatformManager>();

            GameObject entityManagerObj = new GameObject("EntityManager");
            entityManagerObj.transform.SetParent(transform);
            EntityManager = entityManagerObj.AddComponent<EntityManager>();

            GameObject timerManagerObj = new GameObject("TimerManager");
            timerManagerObj.transform.SetParent(transform);
            TimerManager = timerManagerObj.AddComponent<TimerManager>();
        }

        private void InitializeLevel() {
            if (levelGenerator == null) {
                Debug.LogError("GameManager: No LevelGenerator assigned!");
                return;
            }

            if (useExistingSceneContent) {
                // Testing mode: scan for existing entities and register them
                ScanAndRegisterExistingEntities();
            } else {
                // Normal mode: procedurally generate the level
                levelGenerator.GenerateLevel();
            }

            // Always setup the level (tiles, bounds, background)
            levelGenerator.SetupLevel();

            if (!useExistingSceneContent) {
                // Place entrance/exit after setup (needs initialized tiles to find placement spots)
                levelGenerator.PlaceEntranceAndExit();
            }

            // Spawn the player at the entrance (unless one already exists in testing mode)
            Player existingPlayer = FindObjectOfType<Player>();
            if (existingPlayer != null) {
                // Player already exists in the scene, just set up the camera
                CameraFollow existingCam = FindObjectOfType<CameraFollow>();
                if (existingCam != null) {
                    existingCam.Initialize(existingPlayer);
                    existingPlayer.cam = existingCam;
                } else {
                    // Spawn camera for existing player
                    CameraFollow camInstance = Instantiate(playerCamera, existingPlayer.transform.position, Quaternion.identity);
                    camInstance.Initialize(existingPlayer);
                    existingPlayer.cam = camInstance;
                }
            } else {
                // Spawn a new player at the entrance
                SpawnPlayer(levelGenerator.entrance.transform.position);
            }
        }

        private void ScanAndRegisterExistingEntities() {
            // Find and register all MovingPlatforms
            MovingPlatform[] platforms = FindObjectsOfType<MovingPlatform>();
            foreach (MovingPlatform platform in platforms) {
                PlatformManager.Register(platform);
            }
            Debug.Log($"GameManager: Registered {platforms.Length} existing MovingPlatforms");

            // Find and register all ITickable entities
            // Note: MonoBehaviours that implement ITickable
            MonoBehaviour[] allBehaviours = FindObjectsOfType<MonoBehaviour>();
            int entityCount = 0;
            foreach (MonoBehaviour behaviour in allBehaviours) {
                if (behaviour is ITickable tickable && !(behaviour is MovingPlatform)) {
                    EntityManager.Register(tickable);
                    entityCount++;
                }
            }
            Debug.Log($"GameManager: Registered {entityCount} existing entities");
        }

        private void Update() {
            if (IsGameOver) {
                return;
            }

            // ===== 1. INPUT PHASE =====
            // Read input, update directional input
            EntityManager.EarlyTick();

            // ===== 2. PRE-PHYSICS PHASE =====
            // MovingPlatforms set externalDelta on riders
            PlatformManager.Tick();

            // ===== 3. PHYSICS PHASE =====
            // State machine updates, velocity calculations, entity movement
            EntityManager.Tick();

            // ===== 4. POST-PHYSICS PHASE =====
            // Player.HandleEnemyOverlaps(), State.ChangePlayerVelocityAfterMove()
            EntityManager.LateTick();

            // ===== 5. TIMER PHASE =====
            // Process all active timers
            TimerManager.Tick();
        }

        public void HandlePlayerDeath(Player player) {
            Debug.Log("GameManager: Player has died, handling game over...");
            
            if (IsGameOver) {
                return;
            }

            IsGameOver = true;

            int score = 0;
            if (player != null && player.Inventory != null) {
                score = player.Inventory.goldAmount;
            }
            
            Debug.Log($"GameManager: Player score at death: {score}");

            GameOverUI.ShowGameOver(score);
        }

        public void SpawnPlayer(Vector3 position) {
            // Bump us half a tile to the right so we're in the center of the entrance.
            Player playerInstance = Instantiate(player, position + new Vector3(8, 0, 0), Quaternion.identity);
            // Bump the camera half a tile up as well so it's in the correct spot right away.
            CameraFollow camInstance = Instantiate(playerCamera, position + new Vector3(8, 8, 0), Quaternion.identity);
            camInstance.Initialize(playerInstance);
            playerInstance.cam = camInstance;
        }

    }

}
