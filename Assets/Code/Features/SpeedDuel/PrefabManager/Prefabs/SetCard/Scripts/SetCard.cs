using System.Collections.Generic;
using System.Threading.Tasks;
using Code.Core.DataManager;
using Code.Core.General.Extensions;
using Code.Core.Logger;
using Code.Features.SpeedDuel.EventHandlers;
using Code.Features.SpeedDuel.EventHandlers.Entities;
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
        private ISetCardEventHandler _setCardEventHandler;
        private IPlayfieldEventHandler _playfieldEventHandler;
        private IAppLogger _logger;

        private Animator _animator;
        private CurrentState _currentState;

        #region Constructors

        [Inject]
        public void Construct(
            IDataManager dataManager,
            ISetCardEventHandler modelEventHandler,
            IPlayfieldEventHandler playfieldEventHandler,
            IAppLogger logger)
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
            SubscribeToEvents();
        }

        private void OnDisable()
        {
            _currentState = CurrentState.FaceDown;
            transform.localRotation = Quaternion.Euler(0, 0, 0);
            _animator.SetBool(AnimatorParameters.ShowSetCardImageBool, false);
        }

        private void OnDestroy()
        {
            _setCardEventHandler.OnSummonSetCard -= OnSummon;
            _setCardEventHandler.OnAction -= OnAction;
            _setCardEventHandler.OnSetCardRemove -= OnRemove;

            _playfieldEventHandler.OnActivatePlayfield -= ActivatePlayfield;
            _playfieldEventHandler.OnRemovePlayfield -= RemovePlayfield;
        }

        #endregion

        #region Event Subcriptions

        private void SubscribeToEvents()
        {
            _setCardEventHandler.OnSummonSetCard += OnSummon;
            _setCardEventHandler.OnAction += OnAction;
            _setCardEventHandler.OnSetCardRemove += OnRemove;

            _playfieldEventHandler.OnActivatePlayfield += ActivatePlayfield;
            _playfieldEventHandler.OnRemovePlayfield += RemovePlayfield;
        }

        #endregion

        #region Events

        private async void OnSummon(int instanceID, string modelName, bool isMonster)
        {
            if (!gameObject.ShouldModelListenToEvent(instanceID)) return;

            if (isMonster)
            {
                SetMonster();
            }

            await GetAndDisplayCardImage(modelName);
        }

        private void OnAction(SetCardEvent eventName, int instanceID)
        {
            if (!gameObject.ShouldModelListenToEvent(instanceID)) return;

            switch (eventName)
            {
                case SetCardEvent.SpellTrapActivate:
                    SpellTrapActivate();
                    break;
                case SetCardEvent.ReturnToFaceDown:
                    ReturnToFaceDown();
                    break;
                case SetCardEvent.RevealSetCardImage:
                    RevealSetCardImage();
                    break;
                case SetCardEvent.HideSetCardImage:
                    HideSetCardImage();
                    break;
                case SetCardEvent.Hurt:
                    PlayHurtAnimation();
                    break;
            }
        }

        private void OnRemove(int instanceID)
        {
            if (!gameObject.ShouldModelListenToEvent(instanceID)) return;

            _animator.SetTrigger(AnimatorParameters.RemoveSetCardTrigger);
        }

        #endregion

        #region Functions

        #region Playfield Functions

        private void ActivatePlayfield()
        {
            if (!gameObject.activeSelf) return;

            switch (_currentState)
            {
                case CurrentState.FaceDown:
                    _animator.SetTrigger(AnimatorParameters.FadeInSetCardTrigger);
                    break;
                case CurrentState.SpellActivated:
                    _animator.SetTrigger(AnimatorParameters.ActivateSpellOrTrapTrigger);
                    break;
                case CurrentState.SetMonsterRevealed:
                    _animator.SetBool(AnimatorParameters.ShowSetCardImageBool, true);
                    break;
            }
        }

        private void RemovePlayfield()
        {
            _animator.SetTrigger(AnimatorParameters.RemoveSetCardTrigger);
        }

        #endregion

        #region Spell/Trap Functions

        private void SpellTrapActivate()
        {
            _animator.SetTrigger(AnimatorParameters.ActivateSpellOrTrapTrigger);
            _currentState = CurrentState.SpellActivated;
        }

        private void ReturnToFaceDown()
        {
            _animator.SetTrigger(AnimatorParameters.ReturnSpellTrapToFaceDown);
            _currentState = CurrentState.FaceDown;
        }

        #endregion

        #region Set Monster Functions

        private void SetMonster()
        {
            transform.localRotation = Quaternion.Euler(0, 90, 0);
        }

        private void RevealSetCardImage()
        {
            _animator.SetBool(AnimatorParameters.ShowSetCardImageBool, true);
            _animator.SetTrigger(AnimatorParameters.RevealSetMonsterTrigger);
            _currentState = CurrentState.SetMonsterRevealed;
        }

        private void HideSetCardImage()
        {
            _animator.SetBool(AnimatorParameters.ShowSetCardImageBool, false);
            _animator.SetTrigger(AnimatorParameters.HideSetCardTrigger);
            _currentState = CurrentState.FaceDown;
        }

        private void PlayHurtAnimation()
        {
            _animator.SetTrigger(AnimatorParameters.TakeDamageTrigger);
        }

        #endregion

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