using UnityEngine;

namespace CloudQuest
{
    /// <summary>
    /// A body of water that rises as rain falls into it, and falls again as
    /// that water evaporates. The Week 37 water cycle competency: what goes up
    /// comes down, and what comes down goes back up.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public class WaterBody : MonoBehaviour, IRainTarget
    {
        [Tooltip("Highest the surface may rise above its starting level, in world units.")]
        [SerializeField] private float maxRise = 4f;

        [Tooltip("World units the level drops per second when no rain is falling.")]
        [SerializeField] private float evaporationRate = 0.05f;

        [Tooltip("Rise at or above which the level counts as full, for a puzzle to check.")]
        [SerializeField] private float fullThreshold = 3.5f;

        [Tooltip("Rise at or above which the route is flooded and cannot be waded.")]
        [SerializeField] private float floodDepth = 2.5f;

        [Tooltip("Layers that can drown in deep water.")]
        [SerializeField] private LayerMask swimmerMask = ~0;

        private SpriteRenderer spriteRenderer;
        private BoxCollider2D box;
        private float baseHeight;
        private Vector3 baseCentre;
        private float rise;
        private float risenThisStep;

        /// <summary>How far the surface currently sits above its starting level.</summary>
        public float Rise { get { return rise; } }

        public bool IsFull { get { return rise >= fullThreshold; } }

        /// <summary>Deep enough that the route through it is closed.</summary>
        public bool IsFlooded { get { return rise >= floodDepth; } }

        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            box = GetComponent<BoxCollider2D>();
            baseHeight = spriteRenderer.size.y;
            baseCentre = transform.position;
        }

        public void ReceiveRain(CloudType cloud, float amount, bool erosive)
        {
            risenThisStep += amount;
        }

        private void FixedUpdate()
        {
            if (risenThisStep > 0f)
            {
                rise += risenThisStep;
                risenThisStep = 0f;
            }
            else
            {
                rise -= evaporationRate * Time.fixedDeltaTime;
            }

            rise = Mathf.Clamp(rise, 0f, maxRise);
            ApplyLevel();
            DrownAnyoneIn();
        }

        /// <summary>
        /// Shallow water is waded through; once it is deep enough the route is
        /// flooded, which is what makes over-watering a level a mistake rather
        /// than simply slower.
        /// </summary>
        private void DrownAnyoneIn()
        {
            if (!IsFlooded)
            {
                return;
            }

            Bounds area = spriteRenderer.bounds;
            Collider2D[] hits = Physics2D.OverlapBoxAll(area.center, area.size, 0f, swimmerMask);
            for (int i = 0; i < hits.Length; i++)
            {
                PlayerRespawn caught = hits[i].GetComponent<PlayerRespawn>();
                if (caught != null)
                {
                    caught.Respawn();
                }
            }
        }

        /// <summary>
        /// Grows the water upward from its bed, so the surface rises while the
        /// bottom stays put.
        /// </summary>
        private void ApplyLevel()
        {
            float height = baseHeight + rise;
            spriteRenderer.size = new Vector2(spriteRenderer.size.x, height);
            transform.position = new Vector3(baseCentre.x,
                                             baseCentre.y + (height - baseHeight) * 0.5f,
                                             baseCentre.z);

            if (box != null)
            {
                box.size = new Vector2(box.size.x, height);
            }
        }
    }
}
