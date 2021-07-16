using Code.Core.SmartDuelServer.Interface.Entities.EventData;

namespace Code.Core.SmartDuelServer.Interface.Entities
{
    public class SmartDuelEvent
    {
        public string Scope { get; }
        public string Action { get; }
        public SmartDuelEventData Data { get; }

        public SmartDuelEvent(string scope, string action, SmartDuelEventData data = null)
        {
            Scope = scope;
            Action = action;
            Data = data;
        }
    }
}