using Code.Features.SpeedDuel.Models.Zones;
using Code.Core.Logger;
using UnityEngine;
using Code.Features.SpeedDuel.EventHandlers;
using Code.Features.SpeedDuel.Models;
using Code.Core.SmartDuelServer.Entities.EventData.CardEvents;

namespace Code.Features.SpeedDuel.UseCases.CardBattle
{
    public interface IDirectAttackUseCase
    {
        void Execute(SingleCardZone playerZone, PlayerState targetState, GameObject speedDuelField);
    }

    public class DirectAttackUseCase : IDirectAttackUseCase
    {
        private const string Tag = "DirectAttackUseCase";

        private readonly IModelEventHandler _modelEventHandler;
        private readonly IAppLogger _logger;

        public DirectAttackUseCase(
            IModelEventHandler modelEventHandler,
            IAppLogger appLogger)
        {
            _modelEventHandler = modelEventHandler;
            _logger = appLogger;
        }

        public void Execute(SingleCardZone playerZone, PlayerState targetState, GameObject speedDuelField)
        {
            _logger.Log(Tag, $"Execute({playerZone}, {targetState}");

            if (playerZone.Card.CardPosition != CardPosition.FaceUp) return;

            var playMatZone = speedDuelField.transform.Find($"{targetState.PlayMatZonesPath}/Hand");

            if (playerZone.MonsterModel != null)
            {
                var model = playerZone.MonsterModel;
                _modelEventHandler.RaiseDirectAttack(model.GetInstanceID(), playMatZone);
            }
        }
    }
}