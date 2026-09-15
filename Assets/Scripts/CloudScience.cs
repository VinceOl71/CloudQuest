using System.Collections.Generic;

namespace CloudQuest
{
    /// <summary>The cloud types the learner works with. Grade 4 refresher content.</summary>
    public enum CloudType
    {
        Cirrus,
        Cumulus,
        Stratus,
        Nimbostratus,
        Cumulonimbus
    }

    /// <summary>What falls from a cloud, if anything.</summary>
    public enum Precipitation
    {
        None,
        Drizzle,
        Steady,
        Heavy
    }

    /// <summary>
    /// The height a cloud type occupies. A cloud can only be formed in its own
    /// band, which is what stops the learner putting a cirrus cloud at ground
    /// level and is the reason altitude has to be read off the level.
    /// </summary>
    public enum AltitudeBand
    {
        Low,
        Middle,
        High
    }

    /// <summary>
    /// The rules of the game world. Every fact here is a science fact, so the
    /// puzzle cannot be solved without applying the concept. Deliberately free
    /// of UnityEngine so it can be tested on its own.
    /// </summary>
    public static class CloudScience
    {
        public static Precipitation PrecipitationOf(CloudType type)
        {
            switch (type)
            {
                case CloudType.Cirrus:       return Precipitation.None;    // ice crystals, far too high to reach the ground
                case CloudType.Cumulus:      return Precipitation.None;    // fair weather
                case CloudType.Stratus:      return Precipitation.Drizzle; // light and intermittent
                case CloudType.Nimbostratus: return Precipitation.Steady;  // continuous, over a wide area
                case CloudType.Cumulonimbus: return Precipitation.Heavy;   // downpour, thunder and lightning
                default:                     return Precipitation.None;
            }
        }

        /// <summary>Water level a cloud adds per second, in world units.</summary>
        public static float RainfallRate(CloudType type)
        {
            switch (PrecipitationOf(type))
            {
                case Precipitation.Drizzle: return 0.15f;
                case Precipitation.Steady:  return 0.40f;
                case Precipitation.Heavy:   return 0.90f;
                default:                    return 0f;
            }
        }

        /// <summary>
        /// Only a downpour carries enough energy to wear rock down and move the
        /// loosened material, which is the Week 36 weathering and erosion idea.
        /// </summary>
        public static bool Erodes(CloudType type)
        {
            return PrecipitationOf(type) == Precipitation.Heavy;
        }

        public static AltitudeBand BandOf(CloudType type)
        {
            switch (type)
            {
                case CloudType.Cirrus:       return AltitudeBand.High;
                case CloudType.Cumulus:      return AltitudeBand.Low;
                case CloudType.Stratus:      return AltitudeBand.Low;
                case CloudType.Nimbostratus: return AltitudeBand.Middle;
                case CloudType.Cumulonimbus: return AltitudeBand.Low; // based low, but towers through every band
                default:                     return AltitudeBand.Low;
            }
        }

        /// <summary>
        /// Clouds that condense straight out of rising moist air. Stratus
        /// belongs here and does drizzle, because that is how it behaves. What
        /// is absent is the heavier rain: steady and downpour clouds have to be
        /// grown from these, which is the point the transformation mechanic
        /// teaches.
        /// </summary>
        public static bool CanFormDirectly(CloudType type)
        {
            return type == CloudType.Cumulus
                || type == CloudType.Stratus
                || type == CloudType.Cirrus;
        }

        /// <summary>
        /// How a cloud develops. Growth only, because clouds build upward as
        /// moist air keeps rising; to undo one the learner removes it instead.
        /// </summary>
        public static IEnumerable<CloudType> TransformationsFrom(CloudType type)
        {
            switch (type)
            {
                case CloudType.Cumulus:
                    yield return CloudType.Cumulonimbus; // heat and moisture drive it upward
                    break;
                case CloudType.Stratus:
                    yield return CloudType.Nimbostratus; // the layer thickens until it rains
                    break;
            }
        }

        public static bool CanTransform(CloudType from, CloudType to)
        {
            foreach (CloudType option in TransformationsFrom(from))
            {
                if (option == to)
                {
                    return true;
                }
            }

            return false;
        }

        public static string DisplayName(CloudType type)
        {
            return type.ToString();
        }

        /// <summary>Shown when the learner selects a cloud, in Grade 5 vocabulary.</summary>
        public static string Description(CloudType type)
        {
            switch (type)
            {
                case CloudType.Cirrus:
                    return "Thin and feathery, very high up. Made of ice crystals. It brings no rain, but it can mean the weather is about to change.";
                case CloudType.Cumulus:
                    return "Puffy and white with a flat bottom. A fair weather cloud. It brings no rain, but it can grow into a storm cloud.";
                case CloudType.Stratus:
                    return "A flat grey layer low in the sky. It brings drizzle, a light rain that falls slowly.";
                case CloudType.Nimbostratus:
                    return "A thick grey layer that blocks the sun. It brings steady rain over a wide area for a long time.";
                case CloudType.Cumulonimbus:
                    return "A tall, towering storm cloud. It brings heavy rain, thunder and lightning, and it can wear away rock.";
                default:
                    return string.Empty;
            }
        }
    }
}
