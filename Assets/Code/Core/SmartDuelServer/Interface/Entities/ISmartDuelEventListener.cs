namespace AssemblyCSharp.Assets.Code.Core.SmartDuelServer.Interface.Entities
{
    public interface ISmartDuelEventListener
    {
        void OnSmartDuelEventReceived(SmartDuelEvent smartDuelEvent);
    }
}