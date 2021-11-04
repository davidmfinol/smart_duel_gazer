using Code.Core.Logger;
using Code.Core.SmartDuelServer.Entities.EventData.CardEvents;
using Code.Features.SpeedDuel.EventHandlers;
using Code.Features.SpeedDuel.EventHandlers.Entities;
using Code.Features.SpeedDuel.Models.Zones;

namespace Code.Features.SpeedDuel.UseCases.CardBattle
{
    public interface IMonsterZoneBattleUseCase
    {
        void Execute(SingleCardZone playerZone, SingleCardZone targetzone, string path, UnityEngine.GameObject speedDuelField);
    }

    public class MonsterZoneBattleUseCase : IMonsterZoneBattleUseCase
    {
        private const string Tag = "MonsterZoneBattleUseCase";

        private readonly IModelEventHandler _modelEventHandler;
        private readonly ISetCardEventHandler _setCardEventHandler;
        private readonly IAppLogger _logger;

        public MonsterZoneBattleUseCase(
            IModelEventHandler modelEventHandler,
            ISetCardEventHandler setCardEventHandler,
            IAppLogger appLogger)
        {
            _modelEventHandler = modelEventHandler;
            _setCardEventHandler = setCardEventHandler;
            _logger = appLogger;
        }

        public void Execute(SingleCardZone playerZone, SingleCardZone targetZone, string path, UnityEngine.GameObject speedDuelField)
        {
            ExecuteAttackEvent(playerZone, targetZone, path, true, speedDuelField);
            ExecuteAttackEvent(targetZone, targetZone, path, false, speedDuelField);
        }

        private void ExecuteAttackEvent(SingleCardZone playerZone, SingleCardZone targetZone, string path, bool isAttackingMonster, 
            UnityEngine.GameObject speedDuelField)
        {
            _logger.Log(Tag, $"Execute({playerZone.Card.Id}, isAttackingMonster: {isAttackingMonster})");

            if (isAttackingMonster && playerZone.Card.CardPosition != CardPosition.FaceUp) return;

            var cardId = GetCardModelInstanceId(playerZone);
            var setCardId = GetSetCardInstanceId(playerZone);
            if (cardId.HasValue)
            {
                var targetTransformPath = $"{path}/{targetZone.ZoneType}/{targetZone.ZoneType}AttackZone";
                var targetTransform = speedDuelField.transform.Find(targetTransformPath);
                
                var eventArgs = new ModelActionAttackEvent 
                { 
                    IsAttackingMonster = isAttackingMonster, 
                    PlayfieldTargetTransform = targetTransform,
                    TargetMonster = targetZone.MonsterModel
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