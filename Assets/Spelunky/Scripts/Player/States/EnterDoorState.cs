using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Spelunky {

    /// <summary>
    /// The state we're in when we're entering a door (exiting a level).
    /// </summary>
    public class EnterDoorState : State {

        public AudioClip enterDoorClip;

        public override bool CanEnterState() {
            if (player._exitDoor == null) {
                return false;
            }

            return true;
        }

        public override void EnterState() {
            StartCoroutine(EnterDoor());
        }

        public override void ChangePlayerVelocity(ref Vector2 velocity) {
            velocity = Vector2.zero;
        }

        public override bool LockInput() {
            return true;
        }

        private IEnumerator EnterDoor() {
            transform.position = new Vector2(player._exitDoor.transform.position.x + Tile.Width / 2f, player._exitDoor.transform.position.y);

            player.Visuals.animator.Play("EnterDoor");

            player.Audio.Play(enterDoorClip);

            Color color = player.Visuals.renderer.color;
            float animationLength = player.Visuals.animator.GetAnimationLength("EnterDoor");
            float t = 0;
            while (t <= animationLength) {
                t += Time.deltaTime;
                player.Visuals.renderer.color = Color.Lerp(color, Color.black, t.Remap(0f, animationLength, 0f, 1f));
                yield return null;
            }

            // TODO: This is just temporary. We should load the next level here, while obviously saving character stats.
            Scene scene = SceneManager.GetActiveScene();
            SceneManager.LoadScene(scene.name);
        }

    }

}
