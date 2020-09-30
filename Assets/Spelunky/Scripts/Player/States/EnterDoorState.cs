using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Spelunky {
    /// <summary>
    /// The state we're in when we're entering a door (exiting a level).
    /// </summary>
    public class EnterDoorState : State {

        public AudioClip enterDoorClip;

        public override bool CanEnter() {
            if (player._exitDoor == null) {
                return false;
            }

            return true;
        }

        public override void Enter() {
            StartCoroutine(EnterDoor());
        }

        private IEnumerator EnterDoor() {
            transform.position = new Vector2(player._exitDoor.transform.position.x + Tile.Width / 2f, player._exitDoor.transform.position.y);

            player.Visuals.animator.Play("EnterDoor", true);
            player.Visuals.animator.fps = 12;

            player.Audio.Play(enterDoorClip);

            Color color = player.Visuals.renderer.color;
            float animationLength = player.Visuals.animator.GetAnimationLength("EnterDoor");
            float t = 0;
            while (t <= animationLength) {
                t += Time.deltaTime;
                player.Visuals.renderer.color = Color.Lerp(color, Color.black, t.Remap(0f, animationLength, 0f, 1f));
                yield return null;
            }

            Scene scene = SceneManager.GetActiveScene();
            SceneManager.LoadScene(scene.name);
        }

        public override bool LockInput() {
            return true;
        }
    }
}
