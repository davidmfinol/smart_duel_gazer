using Code.Core.DataManager.GameObjects.Entities;
using Code.Core.DataManager.GameObjects.UseCases;
using Code.Core.SmartDuelServer.Entities.EventData.CardEvents;
using Code.Features.SpeedDuel.EventHandlers.Entities;
using Code.Features.SpeedDuel.Models;
using Code.Features.SpeedDuel.Models.Zones;
using UnityEngine;

namespace Code.Features.SpeedDuel.UseCases.MoveCard.ModelsAndEvents
{
    public interface IPlayCardImageUseCase
    {
        Zone Execute(SingleCardZone zone, PlayCard card, Transform playMatZone);
    }

    public class PlayCardImageUseCase : IPlayCardImageUseCase
    {
        private readonly IGetTransformedGameObjectUseCase _getTransformedGameObjectUseCase;
        private readonly IHandlePlayCardModelEventsUseCase _handlePlayCardModelEventsUseCase;

        #region Constructor

        public PlayCardImageUseCase(
            IGetTransformedGameObjectUseCase getTransformedGameObjectUseCase,
            IHandlePlayCardModelEventsUseCase handlePlayCardModelEventsUseCase)
        {
            _getTransformedGameObjectUseCase = getTransformedGameObjectUseCase;
            _handlePlayCardModelEventsUseCase = handlePlayCardModelEventsUseCase;
        }

        #endregion

        public Zone Execute(SingleCardZone zone, PlayCard card, Transform playMatZone)
        {
            var setCardModel = zone.SetCardModel
                ? zone.SetCardModel
                : _getTransformedGameObjectUseCase.Execute(GameObjectKey.SetCard.GetStringValue(), playMatZone.position,
                    playMatZone.rotation);

            var modelEvent = GetModelEvent(zone, card);
            var isMonster = card.CardPosition.IsDefence();
            _handlePlayCardModelEventsUseCase.Execute(modelEvent, card, setCardModel.GetInstanceID(), isMonster);

            return zone.CopyWith(card, setCardModel);
        }

        private static SetCardEvent GetModelEvent(SingleCardZone zone, PlayCard updatedCard)
        {
            if (!zone.SetCardModel && updatedCard.CardPosition == CardPosition.FaceDown)
            {
                return default;
            }

            return updatedCard.CardPosition switch
            {
                CardPosition.FaceUp => SetCardEvent.SpellTrapActivate,
                CardPosition.FaceDown => SetCardEvent.ReturnToFaceDown,
                CardPosition.FaceUpDefence => SetCardEvent.RevealSetCardImage,
                CardPosition.FaceDownDefence => SetCardEvent.HideSetCardImage,
                _ => default
            };
        }
    }
}