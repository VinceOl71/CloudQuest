using UnityEngine;

namespace CloudQuest
{
    /// <summary>
    /// Puts the player back at the start of the level when a hazard catches
    /// them. Getting it wrong costs a little time rather than ending the game,
    /// which suits a learner who is meant to experiment.
    /// </summary>
    public class PlayerRespawn : MonoBehaviour
    {
        [Tooltip("Where to reappear. Falls back to wherever the player started the scene.")]
        [SerializeField] private Transform spawnPoint;

        [Tooltip("Seconds of safety after respawning, so a hazard cannot catch them repeatedly.")]
        [SerializeField] private float safeSeconds = 1.5f;

        private Rigidbody2D body;
        private Vector3 startedAt;
        private float safeUntil;

        /// <summary>How many times this level has caught the learner out.</summary>
        public int Respawns { get; private set; }

        private void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            startedAt = transform.position;
        }

        /// <summary>Returns false when the player was still in their safe window.</summary>
        public bool Respawn()
        {
            if (Time.time < safeUntil)
            {
                return false;
            }

            transform.position = spawnPoint != null ? spawnPoint.position : startedAt;

            if (body != null)
            {
                body.linearVelocity = Vector2.zero;
            }

            safeUntil = Time.time + safeSeconds;
            Respawns++;
            return true;
        }
    }
}
