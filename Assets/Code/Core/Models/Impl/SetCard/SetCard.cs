using Zenject;
using UnityEngine;
using System.Collections.Generic;
using AssemblyCSharp.Assets.Code.Core.DataManager.Interface;
using AssemblyCSharp.Assets.Code.Core.Models.Impl.ModelEventsHandler;
using AssemblyCSharp.Assets.Code.UIComponents.Constants;
using System.Threading.Tasks;

namespace AssemblyCSharp.Assets.Code.Core.Models.Impl.SetCard
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
        private string _zone;

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
        }

        private void OnEnable()
        {
            SubscribeToEvents();
        }

        private void OnDisable()
        {
            _zone = null;
            _animator.SetTrigger(AnimatorParameters.HideSetMonsterImageTrigger);

            UnsubscribeFromEvents();
        }

        #endregion

        #region Event Subcriptions

        private void SubscribeToEvents()
        {
            _modelEventHandler.OnSummonSetCard += OnSummonSetCard;
            _modelEventHandler.OnSpellTrapActivate += OnSpellTrapActivate;
            _modelEventHandler.OnSetCardRemove += OnSpellTrapRemove;
            _modelEventHandler.OnRevealSetMonster += RevealSetMonster;
            _modelEventHandler.OnDestroySetMonster += DestroySetMonster;
            _modelEventHandler.OnChangeMonsterVisibility += HideSetCardImage;
        }

        private void UnsubscribeFromEvents()
        {
            _modelEventHandler.OnSpellTrapActivate -= OnSpellTrapActivate;
            _modelEventHandler.OnSetCardRemove -= OnSpellTrapRemove;
            _modelEventHandler.OnRevealSetMonster -= RevealSetMonster;
            _modelEventHandler.OnDestroySetMonster -= DestroySetMonster;
            _modelEventHandler.OnChangeMonsterVisibility -= HideSetCardImage;
        }

        #endregion

        private async void OnSummonSetCard(string zone, string modelName, bool isMonster)
        {
            if (transform.gameObject.activeSelf == false)
            {
                return;
            }

            _zone = zone;
            await GetAndDisplayCardImage(modelName);
            _modelEventHandler.OnSummonSetCard -= OnSummonSetCard;

            if (isMonster)
            {
                SetMonster();
            }
        }

        #region Spell/Trap Events

        private void OnSpellTrapActivate(string zone)
        {
            if (_zone != zone)
            {
                return;
            }

            _animator.SetTrigger(AnimatorParameters.ActivateSpellOrTrapTrigger);
        }

        private void OnSpellTrapRemove(string zone)
        {
            if (_zone != zone)
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
        
        private void RevealSetMonster(string zone)
        {
            if (_zone != zone)
            {
                return;
            }

            _animator.SetTrigger(AnimatorParameters.RevealSetMonsterTrigger);
        }

        private void HideSetCardImage(string zone, bool state)
        {
            if (_zone == zone && state == false)
            {
                _animator.SetTrigger(AnimatorParameters.HideSetMonsterImageTrigger);
            }
        }

        private void DestroySetMonster(string zone)
        {
            if (_zone != zone)
            {
                return;
            }

            _animator.SetTrigger(AnimatorParameters.RevealSetMonsterTrigger);
        }

        #endregion

        #region Card Image

        private async Task GetAndDisplayCardImage(string cardId)
        {
            Debug.Log($"GetAndDisplayCardImage(cardId: {cardId})");

            // TODO: Sometimes this cardId has a trailing (Clone). Figure out why that is.
            var image = await _dataManager.GetCardImage(cardId.Split('(')[0]);
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
    }
}
