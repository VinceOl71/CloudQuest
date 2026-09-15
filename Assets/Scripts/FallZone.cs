using UnityEngine;

namespace CloudQuest
{
    /// <summary>
    /// The bottom of the level. Falling into a chasm puts the player back at
    /// the start rather than dropping them out of the world.
    /// </summary>
    public class FallZone : MonoBehaviour
    {
        [Tooltip("World Y at or below which the player has fallen out of the level.")]
        [SerializeField] private float below = -7f;

        [Tooltip("Width of the level, so the check covers all of it.")]
        [SerializeField] private float width = 60f;

        [SerializeField] private LayerMask playerMask = ~0;

        private void FixedUpdate()
        {
            Vector2 centre = new Vector2(transform.position.x, below - 2f);
            Collider2D[] hits = Physics2D.OverlapBoxAll(centre, new Vector2(width, 4f), 0f, playerMask);
            for (int i = 0; i < hits.Length; i++)
            {
                PlayerRespawn fallen = hits[i].GetComponent<PlayerRespawn>();
                if (fallen != null)
                {
                    fallen.Respawn();
                }
            }
        }
    }
}
