using Code.Core.SmartDuelServer.Entities.EventData;

namespace Code.Core.SmartDuelServer.Entities
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