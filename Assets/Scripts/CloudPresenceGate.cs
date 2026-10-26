using UnityEngine;

namespace CloudQuest
{
    /// <summary>
    /// A route that opens while a cloud of a particular type exists in the
    /// level. Used by the early levels, where the cloud being taught does not
    /// rain and so cannot be shown through its effect on water. The learner
    /// still has to put the right cloud in the right place, because a cloud
    /// only forms in the altitude band it belongs to.
    /// </summary>
    public class CloudPresenceGate : MonoBehaviour
    {
        [Tooltip("The learner's cloud controller.")]
        [SerializeField] private CloudController clouds;

        [Tooltip("Cloud type that opens this gate.")]
        [SerializeField] private CloudType required = CloudType.Cirrus;

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
            bool shouldBeOpen = HasRequiredCloud();
            if (shouldBeOpen == IsOpen)
            {
                return;
            }

            IsOpen = shouldBeOpen;
            Apply();
        }

        private bool HasRequiredCloud()
        {
            if (clouds == null)
            {
                return false;
            }

            System.Collections.Generic.IList<Cloud> active = clouds.ActiveClouds();
            for (int i = 0; i < active.Count; i++)
            {
                if (active[i] != null && active[i].Type == required)
                {
                    return true;
                }
            }

            return false;
        }

        private void Apply()
        {
            if (barrier != null)    barrier.enabled = !IsOpen;
            if (barrierArt != null) barrierArt.enabled = !IsOpen;
        }
    }
}
