using UnityEngine;

namespace Spelunky {

    /// <summary>
    /// Place this in a testing scene to bypass procedural level generation.
    /// Hand-place your tiles, entities, entrance/exit in the scene and this will
    /// configure GameManager to scan for existing content instead of generating it.
    /// </summary>
    public class TestingChamber : MonoBehaviour {

        [SerializeField] private GameManager gameManager;
        [SerializeField] private LevelGenerator levelGenerator;
        [SerializeField] private Tile entrance;
        [SerializeField] private Tile exit;

        private void Awake() {
            // Tell GameManager to use existing scene content instead of generating
            gameManager.useExistingSceneContent = true;

            // Set the entrance and exit tiles that were hand-placed in the scene
            levelGenerator.entrance = entrance;
            levelGenerator.exit = exit;
        }
    }

}
