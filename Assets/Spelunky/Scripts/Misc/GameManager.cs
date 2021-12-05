using UnityEngine;

namespace Spelunky {

    public class GameManager : Singleton<GameManager> {
        public Player player;
        public CameraFollow playerCamera;

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