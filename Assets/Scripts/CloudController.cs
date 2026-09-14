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
        [Tooltip("World Y below which the sky counts as low level.")]
        [SerializeField] private float lowBandCeiling = 2f;

        [Tooltip("World Y below which the sky counts as middle level; above it is high level.")]
        [SerializeField] private float middleBandCeiling = 5f;

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

            if (keyboard.digit1Key.wasPressedThisFrame) TryForm(CloudType.Cumulus);
            if (keyboard.digit2Key.wasPressedThisFrame) TryForm(CloudType.Stratus);
            if (keyboard.digit3Key.wasPressedThisFrame) TryForm(CloudType.Cirrus);
            if (keyboard.eKey.wasPressedThisFrame)      TryGrowNearest();
            if (keyboard.qKey.wasPressedThisFrame)      TryRemoveNearest();
        }

        /// <summary>Where a new cloud would go: ahead of the player, at their facing.</summary>
        public Vector2 PlacementPoint()
        {
            float facing = (spriteRenderer != null && spriteRenderer.flipX) ? -1f : 1f;
            return new Vector2(transform.position.x + placeAhead * facing, transform.position.y);
        }

        public AltitudeBand BandAt(float worldY)
        {
            if (worldY < lowBandCeiling)    return AltitudeBand.Low;
            if (worldY < middleBandCeiling) return AltitudeBand.Middle;
            return AltitudeBand.High;
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

            Vector2 point = PlacementPoint();

            // A cloud only forms at the height its type belongs to
            if (BandAt(point.y) != CloudScience.BandOf(type))
            {
                return false;
            }

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

        /// <summary>Clouds currently in the level, for a puzzle to inspect.</summary>
        public System.Collections.Generic.IList<Cloud> ActiveClouds()
        {
            return clouds;
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
