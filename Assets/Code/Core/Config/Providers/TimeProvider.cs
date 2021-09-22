namespace Code.Core.Config.Providers
{
    public interface ITimeProvider
    {
        float SceneRunTime { get; }
    }

    public class TimeProvider : ITimeProvider
    {
        public float SceneRunTime { get => UnityEngine.Time.time; }
    }
}