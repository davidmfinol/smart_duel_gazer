using Code.Core.Models.ModelEventsHandler;
using Code.Core.Models.ModelEventsHandler.Entities;
using Code.Features.SpeedDuel.Models;

namespace Code.Features.SpeedDuel.UseCases.MoveCard.ModelsAndEvents
{
    public interface IHandlePlayCardModelEventsUseCase
    {
        void Execute(ModelEvent modelEvent, PlayCard updatedCard, int instanceID, bool isMonster);
    }

    public class HandlePlayCardModelEventsUseCase : IHandlePlayCardModelEventsUseCase
    {
        private readonly ModelEventHandler _modelEventHandler;

        public HandlePlayCardModelEventsUseCase(
            ModelEventHandler modelEventHandler)
        {
            _modelEventHandler = modelEventHandler;
        }
        
        public void Execute(ModelEvent modelEvent, PlayCard updatedCard, int instanceID, bool isMonster)
        {
            var cardId = updatedCard.Id.ToString();
            var zone = updatedCard.ZoneType.ToString();

            _modelEventHandler.RaiseSummonSetCardEvent(instanceID, cardId, isMonster);
            _modelEventHandler.RaiseEventByEventName(modelEvent, instanceID);
        }
    }
}