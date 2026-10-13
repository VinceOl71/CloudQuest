namespace CloudQuest
{
    /// <summary>
    /// Anything in a level that rain acts on. Implemented by the terrain
    /// pieces a puzzle is built from, so a cloud never needs to know what it
    /// is raining on.
    /// </summary>
    public interface IRainTarget
    {
        /// <param name="cloud">The cloud the rain came from.</param>
        /// <param name="amount">World units of rainfall this step.</param>
        /// <param name="erosive">True when the rain is heavy enough to wear rock away.</param>
        void ReceiveRain(CloudType cloud, float amount, bool erosive);
    }
}
