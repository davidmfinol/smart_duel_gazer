using Code.Features.SpeedDuel.Models.Zones;
using Code.Core.Logger;
using Code.Features.SpeedDuel.EventHandlers;
using Code.Features.SpeedDuel.Models;
using Code.Core.SmartDuelServer.Entities.EventData.CardEvents;
using Code.Features.SpeedDuel.EventHandlers.Entities;
using Code.Core.DataManager;

namespace Code.Features.SpeedDuel.UseCases.CardBattle
{
    public interface IDirectAttackUseCase
    {
        void Execute(SingleCardZone playerZone, PlayerState targetState);
    }

    public class DirectAttackUseCase : IDirectAttackUseCase
    {
        private const string Tag = "DirectAttackUseCase";

        private readonly IModelEventHandler _modelEventHandler;
        private readonly IDataManager _dataManager;
        private readonly IAppLogger _logger;

        #region Constructor

        public DirectAttackUseCase(
            IModelEventHandler modelEventHandler,
            IDataManager dataManager,
            IAppLogger appLogger)
        {
            _modelEventHandler = modelEventHandler;
            _dataManager = dataManager;
            _logger = appLogger;
        }

        #endregion

        public void Execute(SingleCardZone playerZone, PlayerState targetState)
        {
            _logger.Log(Tag, $"Execute({playerZone}, {targetState}");

            // Check if Attacking Monster is in Defence position
            if (playerZone.Card.CardPosition != CardPosition.FaceUp) return;

            var speedDuelField = _dataManager.GetPlayfield();
            var playMatZone = speedDuelField.transform.Find($"{targetState.PlayMatZonesPath}/Hand");

            if (playerZone.MonsterModel != null)
            {
                var model = playerZone.MonsterModel;
                var eventArgs = new ModelActionAttackEvent
                {
                    IsAttackingMonster = true,
                    PlayfieldTargetTransform = playMatZone,
                    AttackTargetGameObject = playMatZone.gameObject
                };
                _modelEventHandler.Action(ModelEvent.AttackDeclaration, model.GetInstanceID(), eventArgs);
            }
        }
    }
}