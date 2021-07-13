using AssemblyCSharp.Assets.Code.Core.SmartDuelServer.Interface.Entities.EventData;

namespace AssemblyCSharp.Assets.Code.Core.SmartDuelServer.Interface.Entities
{
    public class SmartDuelEvent
    {
        public string Scope { get; private set; }
        public string Action { get; private set; }
        public SmartDuelEventData Data { get; private set; }

        public SmartDuelEvent(string scope, string action, SmartDuelEventData data)
        {
            Scope = scope;
            Action = action;
            Data = data;
        }
    }
}