using UnityEngine;

namespace CloudQuest
{
    /// <summary>
    /// The hazard a storm cloud carries. Only a cumulonimbus throws lightning,
    /// so growing one turns a tool into something the learner has to stand
    /// clear of. Every strike is announced first: the bolt brightens through
    /// the warning, so being hit is a decision rather than bad luck.
    /// </summary>
    [RequireComponent(typeof(Cloud))]
    public class LightningStrike : MonoBehaviour
    {
        [Tooltip("Renderer for the bolt, normally a child of the cloud.")]
        [SerializeField] private SpriteRenderer bolt;

        [Tooltip("Seconds between strikes.")]
        [SerializeField] private float interval = 3.5f;

        [Tooltip("Seconds of warning before a strike lands.")]
        [SerializeField] private float warning = 1f;

        [Tooltip("Seconds the bolt stays lit after it lands.")]
        [SerializeField] private float flash = 0.18f;

        [Tooltip("Width of the strike, in world units.")]
        [SerializeField] private float strikeWidth = 0.9f;

        [Tooltip("How far down the strike reaches.")]
        [SerializeField] private float reach = 8f;

        [Tooltip("Layers the strike can catch.")]
        [SerializeField] private LayerMask hitMask = ~0;

        private Cloud cloud;
        private float timer;
        private float flashUntil;

        private void Awake()
        {
            cloud = GetComponent<Cloud>();
            FitBoltToReach();
            Light(0f);
        }

        /// <summary>
        /// Stretches the bolt so what the learner sees is exactly how far the
        /// strike actually reaches. A hazard you must avoid has to be drawn
        /// where it really is.
        /// </summary>
        private void FitBoltToReach()
        {
            if (bolt == null || bolt.sprite == null)
            {
                return;
            }

            float nativeHeight = bolt.sprite.bounds.size.y;
            if (nativeHeight <= 0f)
            {
                return;
            }

            Vector3 scale = bolt.transform.localScale;
            scale.y = reach / nativeHeight;
            bolt.transform.localScale = scale;
        }

        private void Update()
        {
            // Only the storm cloud does this
            if (cloud.Type != CloudType.Cumulonimbus)
            {
                timer = 0f;
                Light(0f);
                return;
            }

            timer += Time.deltaTime;

            if (timer >= interval)
            {
                Strike();
                timer = 0f;
                return;
            }

            if (Time.time < flashUntil)
            {
                Light(1f);
            }
            else
            {
                float untilStrike = interval - timer;
                if (untilStrike <= warning && warning > 0f)
                {
                    // brightens as the strike approaches
                    Light(0.55f * (1f - untilStrike / warning));
                }
                else
                {
                    Light(0f);
                }
            }
        }

        private void Strike()
        {
            flashUntil = Time.time + flash;
            Light(1f);

            Vector2 size = new Vector2(strikeWidth, reach);
            Vector2 centre = new Vector2(transform.position.x,
                                         transform.position.y - reach * 0.5f);

            Collider2D[] hits = Physics2D.OverlapBoxAll(centre, size, 0f, hitMask);
            for (int i = 0; i < hits.Length; i++)
            {
                PlayerRespawn caught = hits[i].GetComponent<PlayerRespawn>();
                if (caught != null)
                {
                    caught.Respawn();
                }
            }
        }

        private void Light(float alpha)
        {
            if (bolt == null)
            {
                return;
            }

            Color c = bolt.color;
            c.a = alpha;
            bolt.color = c;
            bolt.enabled = alpha > 0f;
        }
    }
}
