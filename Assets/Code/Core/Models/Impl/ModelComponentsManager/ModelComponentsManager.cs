using Zenject;
using UnityEngine;
using AssemblyCSharp.Assets.Code.Core.General;
using AssemblyCSharp.Assets.Code.Core.Models.Impl.ModelEventsHandler;
using AssemblyCSharp.Assets.Code.Core.Models.Impl.ModelComponentsManager.Entities;
using AssemblyCSharp.Assets.Code.Core.Models.Interface.ModelComponentsManager;
using AssemblyCSharp.Assets.Code.Core.Models.Interface.ModelEventsHandler.Entities;

namespace AssemblyCSharp.Assets.Code.Core.Models.Impl.ModelComponentsManager
{
    [RequireComponent(typeof(Animator)), RequireComponent(typeof(ModelSettings))]
    public class ModelComponentsManager : MonoBehaviour, IModelComponentsManager
    {
        private ModelEventHandler _eventHandler;

        private Animator _animator;
        private SkinnedMeshRenderer[] _renderers;
        private ModelSettings _settings;
        private string _zone;

        [Inject]
        public void Construct(ModelEventHandler modelEventHandler)
        {
            _eventHandler = modelEventHandler;
        }

        #region Lifecycle

        private void Awake()
        {
            _animator = GetComponent<Animator>();
            _renderers = GetComponentsInChildren<SkinnedMeshRenderer>();
            _settings = GetComponent<ModelSettings>();            
        }

        private void OnEnable()
        {
            SubscribeToEvents();
        }

        private void OnDisable()
        {
            _zone = null;
            _eventHandler.OnChangeMonsterVisibility -= SetMonsterVisibility;
        }

        #endregion

        private void SubscribeToEvents()
        {
            _eventHandler.OnSummonMonster += SummonMonster;
            _eventHandler.OnChangeMonsterVisibility += SetMonsterVisibility;
            _eventHandler.OnDestroyMonster += DestroyMonster;
        }

        public void ScaleModel()
        {
            transform.parent.transform.localScale = _settings.ModelScale;
        }

        public void SummonMonster(string zone)
        {
            _zone = zone;

            ScaleModel();
            _renderers.SetRendererVisibility(true);
            _animator.SetTrigger(AnimatorIDSetter.Animator_Summoning_Trigger);

            _eventHandler.OnSummonMonster -= SummonMonster;
        }

        public void SetMonsterVisibility(string zone, bool state)
        {
            if (zone == _zone)
            {
                _renderers.SetRendererVisibility(state);
            }
        }

        public void DestroyMonster(string zone)
        {
            if (zone == _zone)
            {
                if (_animator.HasState(0, AnimatorIDSetter.Death_Trigger))
                {
                    _animator.SetTrigger(AnimatorIDSetter.Death_Trigger);
                    return;
                }
                ActivateParticlesAndRemoveModel();
            }
        }

        public void ActivateParticlesAndRemoveModel()
        {
            _eventHandler.RaiseEvent(EventNames.OnMonsterDestruction, _renderers);
            _renderers.SetRendererVisibility(false);
            _eventHandler.OnDestroyMonster -= DestroyMonster;
        }

        public class Factory : PlaceholderFactory<GameObject, ModelComponentsManager>
        {
        }
    }

    public static class ModelComponentUtilities
    {
        public static void SetRendererVisibility(this SkinnedMeshRenderer[] renderers, bool visibility)
        {
            foreach (SkinnedMeshRenderer item in renderers)
            {
                item.enabled = visibility;
            }
        }
    }
}
