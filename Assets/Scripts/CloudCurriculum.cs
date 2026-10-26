using System.Collections.Generic;

namespace CloudQuest
{
    /// <summary>
    /// The teaching order. One cloud is introduced per level, and a level only
    /// offers what has been taught, so the learner meets a single new idea at a
    /// time rather than the whole set at once.
    ///
    /// The order runs from the cloud that does nothing to the one that does the
    /// most: no rain, then no rain but it can grow, then drizzle, then steady
    /// rain, then the storm that floods, erodes and throws lightning.
    /// </summary>
    public static class CloudCurriculum
    {
        private static readonly CloudType[] Order =
        {
            CloudType.Cirrus,       // level 1: high and thin, leaves the ground dry
            CloudType.Cumulus,      // level 2: fair weather, and it can be grown
            CloudType.Stratus,      // level 3: the first rain, only a drizzle
            CloudType.Nimbostratus, // level 4: steady rain, grown from stratus
            CloudType.Cumulonimbus  // level 5: the storm, grown from cumulus
        };

        public static int LevelCount { get { return Order.Length; } }

        /// <summary>The cloud a level teaches. Levels are numbered from 1.</summary>
        public static CloudType IntroducedAt(int level)
        {
            return Order[Clamp(level) - 1];
        }

        /// <summary>Whether a cloud has been taught by the given level.</summary>
        public static bool IsAvailable(CloudType type, int level)
        {
            int upto = Clamp(level);
            for (int i = 0; i < upto; i++)
            {
                if (Order[i] == type)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>Everything the learner may use at this level.</summary>
        public static IEnumerable<CloudType> AvailableAt(int level)
        {
            int upto = Clamp(level);
            for (int i = 0; i < upto; i++)
            {
                yield return Order[i];
            }
        }

        /// <summary>The one line a level opens with.</summary>
        public static string LessonFor(int level)
        {
            CloudType type = IntroducedAt(level);
            return "New cloud: " + CloudScience.DisplayName(type) + ". "
                 + CloudScience.Description(type);
        }

        private static int Clamp(int level)
        {
            if (level < 1) return 1;
            if (level > Order.Length) return Order.Length;
            return level;
        }
    }
}
