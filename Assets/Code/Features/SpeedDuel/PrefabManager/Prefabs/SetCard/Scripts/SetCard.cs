using Zenject;
using UnityEngine;
using System.Collections.Generic;
using AssemblyCSharp.Assets.Code.Core.DataManager.Interface;
using AssemblyCSharp.Assets.Code.Core.Models.Impl.ModelEventsHandler;
using AssemblyCSharp.Assets.Code.UIComponents.Constants;
using System.Threading.Tasks;
using AssemblyCSharp.Assets.Code.Core.General.Extensions;

namespace AssemblyCSharp.Assets.Code.Features.SpeedDuel.PrefabManager.Prefabs.SetCard.Scripts
{
    [RequireComponent(typeof(Animator))]
    public class SetCard : MonoBehaviour
    {
        [SerializeField]
        private Renderer _image;
        [SerializeField]
        private List<Texture> _errorImages = new List<Texture>();

        private IDataManager _dataManager;
        private ModelEventHandler _modelEventHandler;

        private Animator _animator;
        private int _instanceID;
        private CurrentState _currentState;

        #region Constructors

        [Inject]
        public void Construct(IDataManager dataManager,
                              ModelEventHandler modelEventHandler)
        {
            _dataManager = dataManager;
            _modelEventHandler = modelEventHandler;
        }

        #endregion

        #region Lifecycle

        private void Awake()
        {
            _animator = GetComponent<Animator>();
            _instanceID = GetInstanceID();
        }

        private void OnEnable()
        {
            SubscribeToEvents();
        }

        private void OnDisable()
        {
            _currentState = CurrentState.FaceDown;

            UnsubscribeFromEvents();
        }

        #endregion

        #region Event Subcriptions

        private void SubscribeToEvents()
        {
            _modelEventHandler.OnSummonSetCard += OnSummonEvent;

            _modelEventHandler.OnSpellTrapActivate += OnSpellTrapActivate;
            _modelEventHandler.OnReturnToFaceDown += OnReturnToFaceDown;
            _modelEventHandler.OnSetCardRemove += OnSpellTrapRemove;
            _modelEventHandler.OnRevealSetMonster += SetMonsterEvent;
            _modelEventHandler.OnDestroySetMonster += DestroySetMonster;
            

            _modelEventHandler.OnActivatePlayfield += ActivatePlayfield;
            _modelEventHandler.OnPickupPlayfield += PickupPlayfield;
        }

        private void UnsubscribeFromEvents()
        {
            _modelEventHandler.OnSummonSetCard -= OnSummonEvent;

            _modelEventHandler.OnSpellTrapActivate -= OnSpellTrapActivate;
            _modelEventHandler.OnReturnToFaceDown -= OnReturnToFaceDown;
            _modelEventHandler.OnSetCardRemove -= OnSpellTrapRemove;
            _modelEventHandler.OnRevealSetMonster -= SetMonsterEvent;
            _modelEventHandler.OnDestroySetMonster -= DestroySetMonster;
            _modelEventHandler.OnChangeMonsterVisibility -= HideSetCardImage;

            _modelEventHandler.OnActivatePlayfield -= ActivatePlayfield;
            _modelEventHandler.OnPickupPlayfield -= PickupPlayfield;
        }

        #endregion

        private async void OnSummonEvent(int instanceID, string modelName, bool isMonster)
        {
            if (_instanceID != instanceID || transform.gameObject.activeSelf == false)
            {
                return;
            }
            
            if (isMonster)
            {
                SetMonster();
            }

            _modelEventHandler.OnSummonSetCard -= OnSummonEvent;
            
            await GetAndDisplayCardImage(modelName);
            _modelEventHandler.OnChangeMonsterVisibility += HideSetCardImage;
        }

        #region Playfield Events

        private void ActivatePlayfield(GameObject playfield)
        {
            switch (_currentState) 
            {
                case CurrentState.FaceDown:
                    _animator.SetTrigger(AnimatorParameters.FadeInSetCardTrigger);
                    break;
                case CurrentState.SpellActivated:
                    _animator.SetTrigger(AnimatorParameters.ActivateSpellOrTrapTrigger);
                    break;
                case CurrentState.SetMonsterRevealed:
                    _animator.SetTrigger(AnimatorParameters.RevealSetMonsterTrigger);
                    break;
            }
        }

        private void PickupPlayfield()
        {
            _animator.SetTrigger(AnimatorParameters.RemoveSetCardTrigger);
        }

        #endregion

        #region Spell/Trap Events

        private void OnSpellTrapActivate(int instanceID)
        {
            if (_instanceID != instanceID)
            {
                return;
            }

            _animator.SetTrigger(AnimatorParameters.ActivateSpellOrTrapTrigger);
            _currentState = CurrentState.SpellActivated;
        }

        private void OnReturnToFaceDown(int instanceID)
        {
            if (_instanceID != instanceID)
            {
                return;
            }

            _animator.SetTrigger(AnimatorParameters.ReturnSpellTrapToFaceDown);
            _currentState = CurrentState.FaceDown;
        }

        private void OnSpellTrapRemove(int instanceID)
        {
            if (_instanceID != instanceID)
            {
                return;
            }

            _animator.SetTrigger(AnimatorParameters.RemoveSetCardTrigger);
        }

        #endregion

        #region Set Monster Events

        private void SetMonster()
        {           
            transform.localRotation = Quaternion.Euler(0, 90, 0);
        }
        
        private void SetMonsterEvent(int instanceID)
        {
            if (_instanceID != instanceID)
            {
                return;
            }
            _animator.SetTrigger(AnimatorParameters.RevealSetMonsterTrigger);
            _currentState = CurrentState.SetMonsterRevealed;
        }

        private void HideSetCardImage(int instanceID, bool state)
        {
            if (_instanceID == instanceID && state == false)
            {
                _animator.SetTrigger(AnimatorParameters.HideSetMonsterImageTrigger);
                _currentState = CurrentState.FaceDown;
            }
        }

        private void DestroySetMonster(int instanceID)
        {
            if (_instanceID != instanceID)
            {
                return;
            }
            _animator.SetTrigger(AnimatorParameters.RevealSetMonsterTrigger);
        }

        #endregion

        #region Card Image

        private async Task GetAndDisplayCardImage(string cardId)
        {
            Debug.Log($"GetAndDisplayCardImage(cardId: {cardId})", this);

            var image = await _dataManager.GetCardImage(cardId.RemoveCloneSuffix());
            if (image == null)
            {
                SetRandomErrorImage();
                return;
            }

            _image.material.SetTexture("_MainTex", image);
        }

        private void SetRandomErrorImage()
        {
            var randomNum = Random.Range(0, _errorImages.Count);
            _image.material.SetTexture("_MainTex", _errorImages[randomNum]);
        }

        #endregion

        public class Factory : PlaceholderFactory<GameObject, SetCard>
        {
        }

        public enum CurrentState
        {
            FaceDown,
            SpellActivated,
            SetMonsterRevealed,
        }
    }
}
