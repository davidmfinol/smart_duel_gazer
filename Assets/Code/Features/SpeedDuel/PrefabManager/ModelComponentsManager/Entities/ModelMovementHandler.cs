using Code.Core.Config.Providers;
using Code.Core.Logger;
using Code.Features.SpeedDuel.EventHandlers;
using Code.Features.SpeedDuel.EventHandlers.Entities;
using System.Threading.Tasks;
using UnityEngine;
using Zenject;

namespace Code.Features.SpeedDuel.PrefabManager.ModelComponentsManager.Entities
{
    public class ModelMovementHandler : MonoBehaviour
    {
        private const string Tag = "ModelMovementHandler";
        
        private IModelEventHandler _modelEventHandler;
        private IDelayProvider _delayProvider;
        private ITimeProvider _timeProvider;
        private IAppLogger _logger;
        
        private Transform _modelTransform;
        private Vector3 _targetPosition;
        private Vector3 _startingPosition;

        private float _speed = 1.7F;
        private float _startTime;
        private float _journeyLength;
        private bool _isMoving = false;
        private bool _hasAttacked = false;
        private int _waitTimeForEnemyAnimations = 600;

        #region Constructor

        [Inject]
        public void Construct(
            IModelEventHandler modelEventHandler,
            IDelayProvider delayProvider,
            ITimeProvider timeProvider,
            IAppLogger appLogger)
        {
            _modelEventHandler = modelEventHandler;
            _delayProvider = delayProvider;
            _timeProvider = timeProvider;
            _logger = appLogger;
        }

        #endregion

        #region LifeCycle

        private async void Update()
        {
            if (!_isMoving) return;

            float distCovered = (_timeProvider.SceneRunTime - _startTime) * _speed;

            if (distCovered >= _journeyLength)
            {
                await HandleEndOfJourney();
                return;
            }

            float fractionOfJourney = distCovered / _journeyLength;
            transform.position = Vector3.Lerp(_modelTransform.position, _targetPosition, fractionOfJourney);
        }

        #endregion

        public void Activate(Transform attackingMonster, Vector3 targetPosition)
        {
            _logger.Log(Tag, $"{attackingMonster.name} is attacking position {targetPosition}");
            
            _modelTransform = attackingMonster;
            _targetPosition = targetPosition;

            _startingPosition = _modelTransform.position;

            _startTime = _timeProvider.SceneRunTime;
            _journeyLength = Vector3.Distance(_modelTransform.position, _targetPosition);

            _isMoving = true;
            _hasAttacked = false;
        }

        private async Task HandleEndOfJourney()
        {
            if (transform.position == _startingPosition)
            {
                _logger.Log(Tag, $"{transform.parent.name} has completed it's movement");
                
                _isMoving = false;                
                return;
            }

            if (!_hasAttacked)
            {
                _modelEventHandler.Action(ModelEvent.Attack, transform.parent.gameObject.GetInstanceID());
                _hasAttacked = true;
            }            

            await _delayProvider.Wait(_waitTimeForEnemyAnimations);
            ReturnToOriginalPosition();
        }

        private void ReturnToOriginalPosition()
        {
            _logger.Log(Tag, $"{transform.parent.name} is returning to it's original position at {_startingPosition}");
            
            _modelTransform = transform;
            _targetPosition = _startingPosition;
            
            _startTime = _timeProvider.SceneRunTime;
            _journeyLength = Vector3.Distance(_modelTransform.position, _targetPosition);
        }
    }
}