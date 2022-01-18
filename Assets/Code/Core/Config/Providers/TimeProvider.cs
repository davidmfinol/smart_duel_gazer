namespace Code.Core.Config.Providers
{
    public interface ITimeProvider
    {
        float SceneRunTime { get; }
        float TimeSinceLastFrame { get; }
    }

    public class TimeProvider : ITimeProvider
    {
        public float SceneRunTime => UnityEngine.Time.time;
        public float TimeSinceLastFrame => UnityEngine.Time.deltaTime;
    }
}