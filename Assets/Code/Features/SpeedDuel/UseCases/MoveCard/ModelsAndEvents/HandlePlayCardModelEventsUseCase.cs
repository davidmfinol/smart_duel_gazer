using AssemblyCSharp.Assets.Code.Core.Models.Impl.ModelEventsHandler;
using AssemblyCSharp.Assets.Code.Core.Models.Interface.ModelEventsHandler.Entities;
using Code.Features.SpeedDuel.Models;

namespace Code.Features.SpeedDuel.UseCases.MoveCard.ModelsAndEvents
{
    public interface IHandlePlayCardModelEventsUseCase
    {
        void Execute(ModelEvent modelEvent, PlayCard updatedCard, bool isMonster);
    }

    public class HandlePlayCardModelEventsUseCase : IHandlePlayCardModelEventsUseCase
    {
        private readonly ModelEventHandler _modelEventHandler;

        public HandlePlayCardModelEventsUseCase(
            ModelEventHandler modelEventHandler)
        {
            _modelEventHandler = modelEventHandler;
        }
        
        public void Execute(ModelEvent modelEvent, PlayCard updatedCard, bool isMonster)
        {
            var cardId = updatedCard.Id.ToString();
            var zone = updatedCard.ZoneType.ToString();

            _modelEventHandler.RaiseSummonSetCardEvent(zone, cardId, isMonster);
            _modelEventHandler.RaiseEventByEventName(modelEvent, zone);
        }
    }
}