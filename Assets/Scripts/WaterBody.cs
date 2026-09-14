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

        private SpriteRenderer spriteRenderer;
        private BoxCollider2D box;
        private float baseHeight;
        private Vector3 baseCentre;
        private float rise;
        private float risenThisStep;

        /// <summary>How far the surface currently sits above its starting level.</summary>
        public float Rise { get { return rise; } }

        public bool IsFull { get { return rise >= fullThreshold; } }

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
