using NRKernal;

namespace Code.Wrappers.WrapperNreal
{
    public interface INrealSessionManagerWrapper
    {
        public void EnablePlaneDetection();
        public void DisablePlaneDetection();
    }

    public class NrealSessionManagerWrapper : INrealSessionManagerWrapper
    {
        public void EnablePlaneDetection()
        {
            var config = NRSessionManager.Instance.NRSessionBehaviour.SessionConfig;
            config.PlaneFindingMode = TrackablePlaneFindingMode.HORIZONTAL;
            NRSessionManager.Instance.SetConfiguration(config);
        }

        public void DisablePlaneDetection()
        {
            var config = NRSessionManager.Instance.NRSessionBehaviour.SessionConfig;
            config.PlaneFindingMode = TrackablePlaneFindingMode.DISABLE;
            NRSessionManager.Instance.SetConfiguration(config);
        }
    }
}