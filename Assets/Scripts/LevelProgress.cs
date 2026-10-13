using UnityEngine;

namespace CloudQuest
{
    /// <summary>
    /// Which level this scene is, and therefore which clouds the learner may
    /// use. One cloud is introduced per level, so a level only offers what has
    /// already been taught plus the one new idea it is about.
    /// </summary>
    public class LevelProgress : MonoBehaviour
    {
        [Tooltip("Level number, counting from 1. Level 1 teaches cirrus; level 5 teaches cumulonimbus.")]
        [SerializeField] private int level = 1;

        public int Level { get { return level; } }

        /// <summary>The cloud this level is about.</summary>
        public CloudType NewCloud { get { return CloudCurriculum.IntroducedAt(level); } }

        /// <summary>Whether the learner has been taught this cloud yet.</summary>
        public bool Allows(CloudType type) { return CloudCurriculum.IsAvailable(type, level); }

        /// <summary>The line the level opens with.</summary>
        public string Lesson { get { return CloudCurriculum.LessonFor(level); } }
    }
}
