using System.Collections.Generic;
using System.Threading.Tasks;
using Code.Core.DataManager;
using Code.Core.General.Extensions;
using Code.Core.Logger;
using Code.Features.SpeedDuel.EventHandlers;
using Code.UI_Components.Constants;
using UnityEngine;
using Zenject;

namespace Code.Features.SpeedDuel.PrefabManager.Prefabs.SetCard.Scripts
{
    [RequireComponent(typeof(Animator))]
    public class SetCard : MonoBehaviour
    {
        private const string Tag = "SetCard";

        private static readonly int MainTex = Shader.PropertyToID("_MainTex");

        [SerializeField] private Renderer _image;
        [SerializeField] private List<Texture> _errorImages = new List<Texture>();

        private IDataManager _dataManager;
        private IAppLogger _logger;
        private SetCardEventHandler _setCardEventHandler;
        private PlayfieldEventHandler _playfieldEventHandler;

        private Animator _animator;
        private int _instanceID;
        private CurrentState _currentState;

        #region Constructors

        [Inject]
        public void Construct(
            IDataManager dataManager,            
            IAppLogger logger,
            SetCardEventHandler modelEventHandler,
            PlayfieldEventHandler playfieldEventHandler)
        {
            _dataManager = dataManager;
            _logger = logger;
            _setCardEventHandler = modelEventHandler;
            _playfieldEventHandler = playfieldEventHandler;
        }

        #endregion

        #region Lifecycle

        private void Awake()
        {
            _animator = GetComponent<Animator>();
            _instanceID = gameObject.GetInstanceID();
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
            _setCardEventHandler.OnSummonSetCard += OnSummonEvent;

            _setCardEventHandler.OnSpellTrapActivate += OnSpellTrapActivate;
            _setCardEventHandler.OnReturnToFaceDown += OnReturnToFaceDown;
            _setCardEventHandler.OnSetCardRemove += OnSpellTrapRemove;
            _setCardEventHandler.OnShowSetCard += SetMonsterEvent;
            _setCardEventHandler.OnDestroySetMonster += DestroySetMonster;
            _setCardEventHandler.OnHideSetMonster += HideSetCardImage;

            _playfieldEventHandler.OnActivatePlayfield += ActivatePlayfield;
            _playfieldEventHandler.OnPickupPlayfield += PickupPlayfield;
        }

        private void UnsubscribeFromEvents()
        {
            _setCardEventHandler.OnSummonSetCard -= OnSummonEvent;

            _setCardEventHandler.OnSpellTrapActivate -= OnSpellTrapActivate;
            _setCardEventHandler.OnReturnToFaceDown -= OnReturnToFaceDown;
            _setCardEventHandler.OnSetCardRemove -= OnSpellTrapRemove;
            _setCardEventHandler.OnShowSetCard -= SetMonsterEvent;
            _setCardEventHandler.OnDestroySetMonster -= DestroySetMonster;
            _setCardEventHandler.OnHideSetMonster -= HideSetCardImage;

            _playfieldEventHandler.OnActivatePlayfield -= ActivatePlayfield;
            _playfieldEventHandler.OnPickupPlayfield -= PickupPlayfield;
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

            _setCardEventHandler.OnSummonSetCard -= OnSummonEvent;
            _animator.SetBool(AnimatorParameters.AllowDestroyBool, false);

            await GetAndDisplayCardImage(modelName);
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

        private void HideSetCardImage(int instanceID)
        {
            if (_instanceID != instanceID)  return;

            _animator.SetTrigger(AnimatorParameters.HideSetMonsterImageTrigger);
            _currentState = CurrentState.FaceDown;
        }

        private void DestroySetMonster(int instanceID)
        {
            if (_instanceID != instanceID)
            {
                return;
            }
            _animator.SetBool(AnimatorParameters.AllowDestroyBool, true);
        }

        #endregion

        #region Card Image

        private async Task GetAndDisplayCardImage(string cardId)
        {
            _logger.Log(Tag, $"GetAndDisplayCardImage(cardId: {cardId})");

            var image = await _dataManager.GetCardImage(cardId.RemoveCloneSuffix());
            if (image == null)
            {
                SetRandomErrorImage();
                return;
            }

            _image.material.SetTexture(MainTex, image);
        }

        private void SetRandomErrorImage()
        {
            var randomNum = Random.Range(0, _errorImages.Count);
            _image.material.SetTexture(MainTex, _errorImages[randomNum]);
        }

        #endregion

        public class Factory : PlaceholderFactory<GameObject, SetCard>
        {
        }

        private enum CurrentState
        {
            FaceDown,
            SpellActivated,
            SetMonsterRevealed,
        }
    }
}