using Code.Core.Config.Providers;
using Code.Core.Logger;
using Code.Features.SpeedDuel.EventHandlers;
using Code.Features.SpeedDuel.EventHandlers.Entities;
using UnityEngine;
using Zenject;

namespace Code.Features.SpeedDuel.PrefabManager.ModelComponentsManager.Entities
{
    public class ModelMovementManager : MonoBehaviour
    {
        private const string Tag = "ModelMovementManager";
        
        private IModelEventHandler _modelEventHandler;
        private ITimeProvider _timeProvider;
        private IAppLogger _logger;
        
        private Transform _modelTransform;
        private Vector3 _targetPosition;
        private Vector3 _startingPosition;

        private float _speed = 1.7F;
        private float _startTime;
        private float _journeyLength;
        private bool _isMoving = false;
        private bool _hasAttacked;

        #region Constructor

        [Inject]
        public void Construct(
            IModelEventHandler modelEventHandler,
            ITimeProvider timeProvider,
            IAppLogger appLogger)
        {
            _modelEventHandler = modelEventHandler;
            _timeProvider = timeProvider;
            _logger = appLogger;
        }

        #endregion

        #region LifeCycle

        // TODO: Make time for movement constant so that scaling doesn't change how long it takes
        private void Update()
        {
            if (!_isMoving) return;

            float distCovered = (_timeProvider.SceneRunTime - _startTime) * _speed;

            if (distCovered >= _journeyLength)
            {
                HandleEndOfJourney();
                return;
            }

            float fractionOfJourney = distCovered / _journeyLength;
            transform.position = Vector3.Lerp(_modelTransform.position, _targetPosition, fractionOfJourney);
        }

        #endregion
        
        public void Activate(Transform attackingMonster, Vector3 targetPosition)
        {
            _logger.Log(Tag, $"Activate(attackingMonster: {attackingMonster.name}, targetPosition: {targetPosition}");
            
            _modelTransform = attackingMonster;
            _targetPosition = targetPosition;

            _startingPosition = _modelTransform.position;
            _startTime = _timeProvider.SceneRunTime;
            _journeyLength = Vector3.Distance(_startingPosition, _targetPosition);

            _isMoving = true;
            _hasAttacked = false;
        }

        private void HandleEndOfJourney()
        {
            _logger.Log(Tag, "HandleEndOfJourney()");

            if (transform.position == _startingPosition)
            {
                _isMoving = false;
                return;
            }

            if (_hasAttacked) return;

            var eventArgs = new ModelActionAttackEvent { IsAttackingMonster = true };
            _modelEventHandler.Action(ModelEvent.DamageStep, transform.parent.gameObject.GetInstanceID(), eventArgs);
            _hasAttacked = true;
        }

        public void ReturnToZone()
        {
            _logger.Log(Tag, "ReturnToZone()");
            
            _modelTransform = transform;
            _targetPosition = _startingPosition;
            
            _startTime = _timeProvider.SceneRunTime;
            _journeyLength = Vector3.Distance(_modelTransform.position, _targetPosition);
        }
    }
}