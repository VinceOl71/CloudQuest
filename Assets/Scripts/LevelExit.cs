using UnityEngine;
using UnityEngine.SceneManagement;

namespace CloudQuest
{
    /// <summary>
    /// The end of a level. Reaching it finishes the level and moves on to the
    /// next one, which is what turns a scene into something that can be
    /// completed rather than merely walked around.
    /// </summary>
    public class LevelExit : MonoBehaviour
    {
        [Tooltip("How close the player has to get, in world units.")]
        [SerializeField] private float radius = 1.4f;

        [Tooltip("Layers the player can be on.")]
        [SerializeField] private LayerMask playerMask = ~0;

        [Tooltip("Load the next scene in Build Settings once the level is finished.")]
        [SerializeField] private bool advanceToNextLevel = true;

        [Tooltip("Seconds to hold on the finished level before moving on.")]
        [SerializeField] private float pauseBeforeAdvancing = 1.2f;

        private float advanceAt;

        public bool Reached { get; private set; }

        private void FixedUpdate()
        {
            if (!Reached)
            {
                Look();
                return;
            }

            if (advanceToNextLevel && Time.time >= advanceAt)
            {
                Advance();
            }
        }

        private void Look()
        {
            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, radius, playerMask);
            for (int i = 0; i < hits.Length; i++)
            {
                if (hits[i].GetComponent<PlayerMovement>() != null)
                {
                    Reached = true;
                    advanceAt = Time.time + pauseBeforeAdvancing;
                    return;
                }
            }
        }

        private void Advance()
        {
            int next = SceneManager.GetActiveScene().buildIndex + 1;
            if (next < SceneManager.sceneCountInBuildSettings)
            {
                SceneManager.LoadScene(next);
            }
            else
            {
                // last level: stay put rather than wrapping round to the start
                advanceToNextLevel = false;
            }
        }
    }
}
