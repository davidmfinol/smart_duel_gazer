using Zenject;
using UnityEngine;
using System.Collections.Generic;
using AssemblyCSharp.Assets.Code.Core.General.Statics;
using AssemblyCSharp.Assets.Code.Core.DataManager.Interface;
using AssemblyCSharp.Assets.Code.Core.Models.Impl.ModelEventsHandler;

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
            _animator.SetTrigger(AnimatorParams.Hide_Set_Monster_Image_Trigger);

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

        private void OnSummonSetCard(string zone, string modelName, bool isMonster)
        {
            if (transform.gameObject.activeSelf == false)
            {
                return;
            }

            _zone = zone;
            GetAndDisplayCardImage(modelName);
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

            _animator.SetTrigger(AnimatorParams.Activate_Spell_Or_Trap_Trigger);
        }

        private void OnSpellTrapRemove(string zone)
        {
            if (_zone != zone)
            {
                return;
            }

            _animator.SetTrigger(AnimatorParams.Remove_Set_Card_Trigger);
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

            _animator.SetTrigger(AnimatorParams.Reveal_Set_Monster_Trigger);
        }

        private void HideSetCardImage(string zone, bool state)
        {
            if (_zone == zone && state == false)
            {
                _animator.SetTrigger(AnimatorParams.Hide_Set_Monster_Image_Trigger);
            }

        }

        private void DestroySetMonster(string zone)
        {
            if (_zone != zone)
            {
                return;
            }

            _animator.SetTrigger(AnimatorParams.Reveal_Set_Monster_Trigger);
        }

        #endregion

        #region Card Image

        private void GetAndDisplayCardImage(string cardID)
        {
            if (_dataManager.DoesCachedImageExist(cardID))
            {
                var image = _dataManager.GetCachedImage(cardID);
                if (image != null)
                {
                    _image.material.SetTexture("_MainTex", image);
                    return;
                }
            }
            SetRandomErrorImage();
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
