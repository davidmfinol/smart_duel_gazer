using AssemblyCSharp.Assets.Code.Core.SmartDuelServer.Interface.Entities;

namespace AssemblyCSharp.Assets.Code.Core.SmartDuelServer.Interface
{
    public interface ISmartDuelServer
    {
        void Connect(ISmartDuelEventListener listener);
        void Dispose();
    }

    public interface ISmartDuelEventListener
    {
        void OnSmartDuelEventReceived(SmartDuelEvent smartDuelEvent);
    }
}