using Code.Core.DataManager.GameObjects.UseCases;
using Code.Core.Models.ModelEventsHandler.Entities;
using Code.Core.SmartDuelServer.Entities.EventData.CardEvents;
using Code.Features.SpeedDuel.Models;
using Code.Features.SpeedDuel.Models.Zones;
using UnityEngine;

namespace Code.Features.SpeedDuel.UseCases.MoveCard.ModelsAndEvents
{
    public interface IPlayCardImageUseCase
    {
        Zone Execute(SingleCardZone zone, PlayCard updatedCard, Transform playMatZone);
    }

    public class PlayCardImageUseCase : IPlayCardImageUseCase
    {
        private const string SetCardKey = "SetCard";

        private readonly IGetTransformedGameObjectUseCase _getTransformedGameObjectUseCase;
        private readonly IHandlePlayCardModelEventsUseCase _handlePlayCardModelEventsUseCase;

        public PlayCardImageUseCase(
            IGetTransformedGameObjectUseCase getTransformedGameObjectUseCase,
            IHandlePlayCardModelEventsUseCase handlePlayCardModelEventsUseCase)
        {
            _getTransformedGameObjectUseCase = getTransformedGameObjectUseCase;
            _handlePlayCardModelEventsUseCase = handlePlayCardModelEventsUseCase;
        }

        public Zone Execute(SingleCardZone zone, PlayCard updatedCard, Transform playMatZone)
        {
            var setCardModel = zone.SetCardModel
                ? zone.SetCardModel
                : _getTransformedGameObjectUseCase.Execute(SetCardKey, playMatZone.position, playMatZone.rotation);

            var modelEvent = GetModelEvent(zone, updatedCard);
            var isMonster = updatedCard.CardPosition.IsDefence();
            _handlePlayCardModelEventsUseCase.Execute(modelEvent, updatedCard, isMonster);

            return zone.CopyWith(updatedCard, setCardModel);
        }

        private static ModelEvent GetModelEvent(SingleCardZone zone, PlayCard updatedCard)
        {
            if (!zone.SetCardModel && updatedCard.CardPosition == CardPosition.FaceDown)
            {
                return default;
            }

            return updatedCard.CardPosition switch
            {
                CardPosition.FaceUp => ModelEvent.SpellTrapActivate,
                CardPosition.FaceDown => ModelEvent.ReturnToFaceDown,
                CardPosition.FaceUpDefence => ModelEvent.RevealSetMonster,
                CardPosition.FaceDownDefence => default,
                _ => default
            };
        }
    }
}