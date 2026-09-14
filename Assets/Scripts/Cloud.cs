using UnityEngine;

namespace CloudQuest
{
    /// <summary>
    /// One cloud in the world. It knows its type, shows it, and rains on
    /// whatever sits in the column beneath it. All the science it obeys lives
    /// in CloudScience, so the behaviour here is only the delivery of it.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public class Cloud : MonoBehaviour
    {
        [Tooltip("How far below the cloud its rain can reach, in world units.")]
        [SerializeField] private float rainReach = 12f;

        [Tooltip("Layers a cloud may rain on.")]
        [SerializeField] private LayerMask rainMask = ~0;

        [Header("Artwork")]
        [SerializeField] private Sprite cirrusArt;
        [SerializeField] private Sprite cumulusArt;
        [SerializeField] private Sprite stratusArt;
        [SerializeField] private Sprite nimbostratusArt;
        [SerializeField] private Sprite cumulonimbusArt;

        private SpriteRenderer spriteRenderer;

        public CloudType Type { get; private set; }

        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        public void SetType(CloudType type)
        {
            if (spriteRenderer == null)
            {
                spriteRenderer = GetComponent<SpriteRenderer>();
            }

            Type = type;
            name = "Cloud (" + CloudScience.DisplayName(type) + ")";

            Sprite art = SpriteFor(type);
            if (art != null)
            {
                // The artwork already carries the shape and shading of the type
                spriteRenderer.sprite = art;
                spriteRenderer.color = Color.white;
            }
            else
            {
                // Nothing assigned yet: fall back to a tint and rough
                // proportions so the type still reads instead of vanishing
                spriteRenderer.color = TintFor(type);
                transform.localScale = ScaleFor(type);
            }
        }

        private Sprite SpriteFor(CloudType type)
        {
            switch (type)
            {
                case CloudType.Cirrus:       return cirrusArt;
                case CloudType.Cumulus:      return cumulusArt;
                case CloudType.Stratus:      return stratusArt;
                case CloudType.Nimbostratus: return nimbostratusArt;
                case CloudType.Cumulonimbus: return cumulonimbusArt;
                default:                     return null;
            }
        }

        /// <summary>Grows the cloud, if the science allows that step.</summary>
        public bool TryTransformTo(CloudType target)
        {
            if (!CloudScience.CanTransform(Type, target))
            {
                return false;
            }

            SetType(target);
            return true;
        }

        private void FixedUpdate()
        {
            float rate = CloudScience.RainfallRate(Type);
            if (rate <= 0f)
            {
                return;
            }

            float amount = rate * Time.fixedDeltaTime;
            bool erosive = CloudScience.Erodes(Type);

            // A tall thin column under the cloud, so rain lands on whatever is
            // directly below it rather than on the whole level
            Vector2 size = new Vector2(spriteRenderer.bounds.size.x, rainReach);
            Vector2 centre = new Vector2(transform.position.x,
                                         transform.position.y - rainReach * 0.5f);

            Collider2D[] hits = Physics2D.OverlapBoxAll(centre, size, 0f, rainMask);
            for (int i = 0; i < hits.Length; i++)
            {
                IRainTarget target = hits[i].GetComponent<IRainTarget>();
                if (target != null)
                {
                    target.ReceiveRain(Type, amount, erosive);
                }
            }
        }

        /// <summary>Fallback colouring, used only when no sprite is assigned.</summary>
        private static Color TintFor(CloudType type)
        {
            switch (type)
            {
                case CloudType.Cirrus:       return new Color(0.95f, 0.97f, 1.00f);
                case CloudType.Cumulus:      return new Color(1.00f, 1.00f, 1.00f);
                case CloudType.Stratus:      return new Color(0.76f, 0.79f, 0.83f);
                case CloudType.Nimbostratus: return new Color(0.55f, 0.58f, 0.63f);
                case CloudType.Cumulonimbus: return new Color(0.35f, 0.38f, 0.45f);
                default:                     return Color.white;
            }
        }

        /// <summary>
        /// Fallback proportions, used only when no sprite is assigned: cirrus
        /// wide and thin, cumulonimbus tall and heavy.
        /// </summary>
        private static Vector3 ScaleFor(CloudType type)
        {
            switch (type)
            {
                case CloudType.Cirrus:       return new Vector3(2.0f, 0.35f, 1f);
                case CloudType.Cumulus:      return new Vector3(1.2f, 0.90f, 1f);
                case CloudType.Stratus:      return new Vector3(2.2f, 0.55f, 1f);
                case CloudType.Nimbostratus: return new Vector3(2.4f, 0.90f, 1f);
                case CloudType.Cumulonimbus: return new Vector3(1.6f, 2.20f, 1f);
                default:                     return Vector3.one;
            }
        }
    }
}
