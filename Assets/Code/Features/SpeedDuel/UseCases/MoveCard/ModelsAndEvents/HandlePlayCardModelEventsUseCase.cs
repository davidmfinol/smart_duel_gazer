using Code.Features.SpeedDuel.EventHandlers;
using Code.Features.SpeedDuel.EventHandlers.Entities;
using Code.Features.SpeedDuel.Models;

namespace Code.Features.SpeedDuel.UseCases.MoveCard.ModelsAndEvents
{
    public interface IHandlePlayCardModelEventsUseCase
    {
        void Execute(SetCardEvent modelEvent, PlayCard updatedCard, int instanceID, bool isMonster);
    }

    public class HandlePlayCardModelEventsUseCase : IHandlePlayCardModelEventsUseCase
    {
        private readonly SetCardEventHandler _setCardEventHandler;

        #region Constructor

        public HandlePlayCardModelEventsUseCase(
            SetCardEventHandler setCardEventHandler)
        {
            _setCardEventHandler = setCardEventHandler;
        }

        #endregion

        public void Execute(SetCardEvent setCardEvent, PlayCard updatedCard, int instanceID, bool isMonster)
        {
            var cardId = updatedCard.Id.ToString();

            _setCardEventHandler.Summon(instanceID, cardId, isMonster);
            _setCardEventHandler.Action(setCardEvent, instanceID);
        }
    }
}