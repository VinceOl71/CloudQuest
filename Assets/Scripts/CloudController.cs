using UnityEngine;
using UnityEngine.InputSystem;

namespace CloudQuest
{
    /// <summary>
    /// The learner's side of the cloud mechanic: form a cloud, grow it, or
    /// clear it away. Every rule it enforces is a science rule from
    /// CloudScience, so a puzzle cannot be brute forced without the concept.
    /// </summary>
    public class CloudController : MonoBehaviour
    {
        [Header("Placement")]
        [Tooltip("Prefab used for a cloud. Needs a SpriteRenderer and a Cloud component.")]
        [SerializeField] private Cloud cloudPrefab;

        [Tooltip("How far in front of the player a cloud is placed.")]
        [SerializeField] private float placeAhead = 3f;

        [Tooltip("Most clouds allowed at once. Keeps a puzzle from being solved by flooding it.")]
        [SerializeField] private int maxClouds = 3;

        [Header("Altitude bands")]
        [Tooltip("Height low cloud forms at.")]
        [SerializeField] private float lowBandY = 1f;

        [Tooltip("Height middle cloud forms at.")]
        [SerializeField] private float middleBandY = 3f;

        [Tooltip("Height high cloud forms at.")]
        [SerializeField] private float highBandY = 5f;

        [Header("Progression")]
        [Tooltip("Limits the learner to the clouds this level has taught. Leave empty to allow every type.")]
        [SerializeField] private LevelProgress progress;

        [Header("Selection")]
        [Tooltip("Radius searched for the cloud a grow or clear key acts on.")]
        [SerializeField] private float reachRadius = 6f;

        private readonly System.Collections.Generic.List<Cloud> clouds =
            new System.Collections.Generic.List<Cloud>();

        private SpriteRenderer spriteRenderer;

        /// <summary>Raised whenever the set of clouds changes, for puzzles and UI.</summary>
        public System.Action CloudsChanged;

        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        private void Update()
        {
            Keyboard keyboard = Keyboard.current;
            if (keyboard == null)
            {
                return;
            }

            // Number keys follow the order the clouds were taught, so key 1 is
            // always the first cloud the learner met rather than a fixed type
            System.Collections.Generic.IList<CloudType> keys = FormableClouds();
            if (keys.Count > 0 && keyboard.digit1Key.wasPressedThisFrame) TryForm(keys[0]);
            if (keys.Count > 1 && keyboard.digit2Key.wasPressedThisFrame) TryForm(keys[1]);
            if (keys.Count > 2 && keyboard.digit3Key.wasPressedThisFrame) TryForm(keys[2]);

            if (keyboard.eKey.wasPressedThisFrame) TryGrowNearest();
            if (keyboard.qKey.wasPressedThisFrame) TryRemoveNearest();
        }

        /// <summary>
        /// Where a new cloud goes: ahead of the player, at the height its own
        /// type belongs to. Cirrus appears far overhead and stratus close to
        /// the ground, so the learner sees the altitude of each type rather
        /// than having to have climbed to it first.
        /// </summary>
        public Vector2 PlacementPoint(CloudType type)
        {
            float facing = (spriteRenderer != null && spriteRenderer.flipX) ? -1f : 1f;
            return new Vector2(transform.position.x + placeAhead * facing, HeightOf(type));
        }

        public float HeightOf(CloudType type)
        {
            switch (CloudScience.BandOf(type))
            {
                case AltitudeBand.High:   return highBandY;
                case AltitudeBand.Middle: return middleBandY;
                default:                  return lowBandY;
            }
        }

        public bool TryForm(CloudType type)
        {
            if (cloudPrefab == null || clouds.Count >= maxClouds)
            {
                return false;
            }

            // Only clouds that condense on their own can be placed directly
            if (!CloudScience.CanFormDirectly(type))
            {
                return false;
            }

            // and only ones this level has taught
            if (!IsTaught(type))
            {
                return false;
            }

            Vector2 point = PlacementPoint(type);
            Cloud cloud = Instantiate(cloudPrefab, point, Quaternion.identity);
            cloud.SetType(type);
            clouds.Add(cloud);

            if (CloudsChanged != null) CloudsChanged();
            return true;
        }

        public bool TryGrowNearest()
        {
            Cloud cloud = NearestCloud();
            if (cloud == null)
            {
                return false;
            }

            foreach (CloudType target in CloudScience.TransformationsFrom(cloud.Type))
            {
                // a cloud cannot be grown into one the learner has not met yet
                if (!IsTaught(target))
                {
                    continue;
                }

                if (cloud.TryTransformTo(target))
                {
                    if (CloudsChanged != null) CloudsChanged();
                    return true;
                }
            }

            return false;
        }

        public bool TryRemoveNearest()
        {
            Cloud cloud = NearestCloud();
            if (cloud == null)
            {
                return false;
            }

            clouds.Remove(cloud);
            Destroy(cloud.gameObject);

            if (CloudsChanged != null) CloudsChanged();
            return true;
        }

        /// <summary>
        /// The clouds this level lets the learner place directly, in the order
        /// they were taught. Drives the number keys and what the HUD lists.
        /// </summary>
        public System.Collections.Generic.IList<CloudType> FormableClouds()
        {
            System.Collections.Generic.List<CloudType> formable =
                new System.Collections.Generic.List<CloudType>();

            int upto = progress != null ? progress.Level : CloudCurriculum.LevelCount;
            foreach (CloudType type in CloudCurriculum.AvailableAt(upto))
            {
                if (CloudScience.CanFormDirectly(type))
                {
                    formable.Add(type);
                }
            }

            return formable;
        }

        /// <summary>Clouds currently in the level, for a puzzle to inspect.</summary>
        public System.Collections.Generic.IList<Cloud> ActiveClouds()
        {
            return clouds;
        }

        /// <summary>With no LevelProgress assigned, every cloud is allowed.</summary>
        private bool IsTaught(CloudType type)
        {
            return progress == null || progress.Allows(type);
        }

        private Cloud NearestCloud()
        {
            Cloud best = null;
            float bestDistance = reachRadius;

            for (int i = clouds.Count - 1; i >= 0; i--)
            {
                if (clouds[i] == null)
                {
                    clouds.RemoveAt(i);   // destroyed elsewhere
                    continue;
                }

                float distance = Vector2.Distance(transform.position, clouds[i].transform.position);
                if (distance <= bestDistance)
                {
                    bestDistance = distance;
                    best = clouds[i];
                }
            }

            return best;
        }
    }
}
