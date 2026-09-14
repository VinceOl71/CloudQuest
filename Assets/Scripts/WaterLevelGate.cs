using UnityEngine;

namespace CloudQuest
{
    /// <summary>
    /// A route that opens only while the water sits in a particular band.
    /// The band is the whole point: too little rain and it stays shut, too much
    /// and it shuts again, so the learner has to pick the cloud that produces
    /// the right amount rather than the most.
    /// </summary>
    public class WaterLevelGate : MonoBehaviour
    {
        [Tooltip("The water this gate watches.")]
        [SerializeField] private WaterBody water;

        [Tooltip("Lowest water level that opens the gate.")]
        [SerializeField] private float opensAtLeast = 1.5f;

        [Tooltip("Highest water level that still leaves it open.")]
        [SerializeField] private float opensAtMost = 3f;

        [Tooltip("Collider removed while the gate is open.")]
        [SerializeField] private Collider2D barrier;

        [Tooltip("Artwork hidden while the gate is open.")]
        [SerializeField] private SpriteRenderer barrierArt;

        public bool IsOpen { get; private set; }

        private void Start()
        {
            IsOpen = false;
            Apply();
        }

        private void FixedUpdate()
        {
            bool shouldBeOpen = water != null
                             && water.Rise >= opensAtLeast
                             && water.Rise <= opensAtMost;

            if (shouldBeOpen == IsOpen)
            {
                return;
            }

            IsOpen = shouldBeOpen;
            Apply();
        }

        private void Apply()
        {
            if (barrier != null)    barrier.enabled = !IsOpen;
            if (barrierArt != null) barrierArt.enabled = !IsOpen;
        }
    }
}
