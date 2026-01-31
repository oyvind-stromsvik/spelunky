using UnityEngine;

namespace Spelunky {

public class TestingChamber : MonoBehaviour {

    [SerializeField] LevelGenerator levelGenerator;
    [SerializeField] private Tile entrance;
    [SerializeField] private Tile exit;

    private void Awake() {
        levelGenerator.generateLevelOnStart = false;
        levelGenerator.entrance = entrance;
        levelGenerator.exit = exit;
    }
}
}
