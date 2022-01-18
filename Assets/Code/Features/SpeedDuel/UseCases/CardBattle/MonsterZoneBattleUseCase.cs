using Code.Core.DataManager;
using Code.Core.Logger;
using Code.Core.SmartDuelServer.Entities.EventData.CardEvents;
using Code.Features.SpeedDuel.EventHandlers;
using Code.Features.SpeedDuel.EventHandlers.Entities;
using Code.Features.SpeedDuel.Models.Zones;

namespace Code.Features.SpeedDuel.UseCases.CardBattle
{
    public interface IMonsterZoneBattleUseCase
    {
        void Execute(SingleCardZone playerZone, SingleCardZone targetzone, string zonePath);
    }

    public class MonsterZoneBattleUseCase : IMonsterZoneBattleUseCase
    {
        private const string Tag = "MonsterZoneBattleUseCase";

        private readonly IModelEventHandler _modelEventHandler;
        private readonly ISetCardEventHandler _setCardEventHandler;
        private readonly IDataManager _dataManager;
        private readonly IAppLogger _logger;

        #region Constructor

        public MonsterZoneBattleUseCase(
            IModelEventHandler modelEventHandler,
            ISetCardEventHandler setCardEventHandler,
            IDataManager dataManager,
            IAppLogger appLogger)
        {
            _modelEventHandler = modelEventHandler;
            _setCardEventHandler = setCardEventHandler;
            _dataManager = dataManager;
            _logger = appLogger;
        }

        #endregion

        public void Execute(SingleCardZone playerZone, SingleCardZone targetZone, string zonePath)
        {
            _logger.Log(Tag, $"Execute(playerZone: {playerZone}, targetZone: {targetZone}, zonePath: {zonePath}");

            ExecuteAttackEvent(playerZone, targetZone, zonePath, true);
            ExecuteAttackEvent(targetZone, targetZone, zonePath, false);
        }

        private void ExecuteAttackEvent(SingleCardZone playerZone, SingleCardZone targetZone, string zonePath,
            bool isAttackingMonster)
        {
            _logger.Log(Tag, $"ExecuteAttackEvent(playerZone: {playerZone.Card.YugiohCard.Id}, targetZone: {targetZone}, " +
                             $"zonePath: {zonePath}, isAttackingMonster: {isAttackingMonster})");

            // Check if Card is Attacking while in Defence position
            if (isAttackingMonster && playerZone.Card.CardPosition != CardPosition.FaceUp) return;

            var cardId = GetCardModelInstanceId(playerZone);
            var setCardId = GetSetCardInstanceId(playerZone);
            if (cardId.HasValue)
            {
                var speedDuelField = _dataManager.GetPlayfield();
                var targetTransformPath = $"{zonePath}/{targetZone.ZoneType}/{targetZone.ZoneType}AttackZone";
                var targetTransform = speedDuelField.transform.Find(targetTransformPath);

                var eventArgs = new ModelActionAttackEvent
                {
                    IsAttackingMonster = isAttackingMonster,
                    PlayfieldTargetTransform = targetTransform,
                    AttackTargetGameObject = targetZone.MonsterModel
                };
                _modelEventHandler.Action(ModelEvent.AttackDeclaration, cardId.Value, eventArgs);
            }

            if (setCardId.HasValue && !isAttackingMonster)
            {
                _setCardEventHandler.Action(SetCardEvent.Hurt, setCardId.Value);
            }
        }

        private int? GetCardModelInstanceId(SingleCardZone zone)
        {
            if (zone.MonsterModel != null)
            {
                return zone.MonsterModel.GetInstanceID();
            }

            _logger.Log(Tag, $"{zone} does not have a valid monster for battle");
            return null;
        }

        private int? GetSetCardInstanceId(SingleCardZone zone)
        {
            if (zone.SetCardModel != null)
            {
                return zone.SetCardModel.GetInstanceID();
            }

            _logger.Log(Tag, $"{zone} does not have a set card");
            return null;
        }
    }
}