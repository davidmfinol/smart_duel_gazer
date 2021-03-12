namespace AssemblyCSharp.Assets.Code.Core.SmartDuelServer.Interface.Entities
{
    public interface ISmartDuelEventListener
    {
        void onSmartDuelEventReceived(SmartDuelEvent smartDuelEvent);
    }
}