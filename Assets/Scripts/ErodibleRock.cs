using UnityEngine;

namespace CloudQuest
{
    /// <summary>
    /// A rock surface that only heavy rain wears away, opening the path behind
    /// it. The Week 36 weathering and erosion competency: drizzle will not do
    /// it, so the learner has to grow the cloud that will.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public class ErodibleRock : MonoBehaviour, IRainTarget
    {
        [Tooltip("Rainfall that has to land on this rock before it gives way.")]
        [SerializeField] private float rainToErode = 3f;

        [Tooltip("Fades the rock out as it wears down, so progress is visible.")]
        [SerializeField] private bool fadeWhileEroding = true;

        private SpriteRenderer spriteRenderer;
        private Collider2D solid;
        private float absorbed;

        /// <summary>0 to 1, for a progress indicator or a puzzle to read.</summary>
        public float Progress
        {
            get { return rainToErode <= 0f ? 1f : Mathf.Clamp01(absorbed / rainToErode); }
        }

        public bool HasEroded { get; private set; }

        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            solid = GetComponent<Collider2D>();
        }

        public void ReceiveRain(CloudType cloud, float amount, bool erosive)
        {
            // Light rain wets the rock but carries nothing away
            if (!erosive || HasEroded)
            {
                return;
            }

            absorbed += amount;

            if (fadeWhileEroding)
            {
                Color c = spriteRenderer.color;
                c.a = 1f - Progress;
                spriteRenderer.color = c;
            }

            if (absorbed >= rainToErode)
            {
                Erode();
            }
        }

        private void Erode()
        {
            HasEroded = true;
            spriteRenderer.enabled = false;

            if (solid != null)
            {
                solid.enabled = false;
            }
        }
    }
}
