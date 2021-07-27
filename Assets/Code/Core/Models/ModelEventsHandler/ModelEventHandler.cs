using System;
using Code.Core.Models.ModelEventsHandler.Entities;
using UnityEngine;

namespace Code.Core.Models.ModelEventsHandler
{
    public interface IModelEventHandler
    {
        public void ActivatePlayfield(GameObject playfield);
        public void PickupPlayfield();
        public void DestroyPlayfield();
        public void ActivateModel(string zone);
        public void RaiseEventByEventName(ModelEvent eventNames, string zone);
        public void RaiseChangeVisibilityEvent(string zone, bool state);
        public void RaiseMonsterRemovalEvent(SkinnedMeshRenderer[] renderers);
        public void RaiseSummonSetCardEvent(string zone, string modelName, bool isMonster);
    }
    
    public class ModelEventHandler : IModelEventHandler
    {
        //Create playfield event handler
        private event Action<GameObject> _OnActivatePlayfield;
        private event Action _OnPickUpPlayfield;
        private event Action _OnDestroyPlayfield;
        
        private event Action<int> _OnActivateModel;
        private event Action<int> _OnSummonMonster;
        private event Action<int> _OnDestroyMonster;
        private event Action<int> _OnAttack;

        private event Action<int> _OnRevealSetMonster;
        private event Action<int> _OnDestroySetMonster;

        private event Action<int> _OnSpellTrapActivate;
        private event Action<int> _OnReturnToFaceDown;
        private event Action<int> _OnSetCardRemove;

        private event Action<int, bool> _OnChangeMonsterVisibility;
        private event Action<int, string, bool> _OnSummonSetCard;
        private event Action<SkinnedMeshRenderer[]> _OnMonsterRemoval;

        #region Event Accessors

        public event Action<GameObject> OnActivatePlayfield
        {
            add => _OnActivatePlayfield += value;
            remove => _OnActivatePlayfield -= value;
        }
        public event Action OnPickupPlayfield
        {
            add => _OnPickUpPlayfield += value;
            remove => _OnPickUpPlayfield -= value;
        }
        public event Action OnDestroyPlayfield
        {
            add => _OnDestroyPlayfield += value;
            remove => _OnDestroyPlayfield -= value;
        }
        public event Action<int> OnActivateModel
        {
            add => _OnActivateModel += value;
            remove => _OnActivateModel -= value;
        }
        public event Action<int> OnSummonMonster 
        { 
            add => _OnSummonMonster += value;
            remove => _OnSummonMonster -= value;
        }
        public event Action<int> OnDestroyMonster 
        { 
            add => _OnDestroyMonster += value;
            remove => _OnDestroyMonster -= value;
        }
        public event Action<int> OnAttack
        {
            add => _OnAttack += value;
            remove => _OnAttack -= value;
        }
        public event Action<int> OnRevealSetMonster
        {
            add => _OnRevealSetMonster += value;
            remove => _OnRevealSetMonster -= value;
        }
        public event Action<int> OnDestroySetMonster
        {
            add => _OnDestroySetMonster += value;
            remove => _OnDestroySetMonster -= value;
        }
        public event Action<int> OnSpellTrapActivate
        {
            add => _OnSpellTrapActivate += value;
            remove => _OnSpellTrapActivate -= value;
        }
        public event Action<int> OnReturnToFaceDown
        {
            add => _OnReturnToFaceDown += value;
            remove => _OnReturnToFaceDown -= value;
        }
        public event Action<int> OnSetCardRemove
        {
            add => _OnSetCardRemove += value;
            remove => _OnSetCardRemove -= value;
        }
        public event Action<int, bool> OnChangeMonsterVisibility 
        {
            add => _OnChangeMonsterVisibility += value;
            remove => _OnChangeMonsterVisibility -= value;
        }
        public event Action<int, string, bool> OnSummonSetCard
        {
            add => _OnSummonSetCard += value;
            remove => _OnSummonSetCard -= value;
        }
        public event Action<SkinnedMeshRenderer[]> OnMonsterRemoval 
        { 
            add => _OnMonsterRemoval += value;
            remove => _OnMonsterRemoval -= value;
        }

        #endregion

        #region Playfield Events

        public void ActivatePlayfield(GameObject playfield)
        {
            _OnActivatePlayfield?.Invoke(playfield);
        }

        public void PickupPlayfield()
        {
            _OnPickUpPlayfield?.Invoke();
        }

        public void DestroyPlayfield()
        {
            _OnDestroyPlayfield?.Invoke();
        }

        #endregion

        #region Model Events

        public void ActivateModel(int instanceID)
        {
            _OnActivateModel?.Invoke(instanceID);
        }
        
        public void RaiseEventByEventName(ModelEvent eventName, int instanceID)
        {
            switch (eventName)
            {
                case ModelEvent.SummonMonster:
                    _OnSummonMonster?.Invoke(instanceID);
                    break;
                case ModelEvent.DestroyMonster:
                    _OnDestroyMonster?.Invoke(instanceID);
                    break;
                case ModelEvent.RevealSetMonster:
                    _OnRevealSetMonster?.Invoke(instanceID);
                    break;
                case ModelEvent.DestroySetMonster:
                    _OnDestroySetMonster?.Invoke(instanceID);
                    break;
                case ModelEvent.SpellTrapActivate:
                    _OnSpellTrapActivate?.Invoke(instanceID);
                    break;
                case ModelEvent.SetCardRemove:
                    _OnSetCardRemove?.Invoke(instanceID);
                    break;
                case ModelEvent.ReturnToFaceDown:
                    _OnReturnToFaceDown?.Invoke(instanceID);
                    break;
                case ModelEvent.Attack:
                    _OnAttack?.Invoke(instanceID);
                    break;
            }
        }

        public void RaiseSummonSetCardEvent(int instanceID, string modelName, bool isMonster)
        {
            _OnSummonSetCard?.Invoke(instanceID, modelName, isMonster);
        }
        
        public void RaiseChangeVisibilityEvent(int instanceID, bool state)
        {
            _OnChangeMonsterVisibility?.Invoke(instanceID, state);
        }

        public void RaiseMonsterRemovalEvent(SkinnedMeshRenderer[] renderers)
        {
            _OnMonsterRemoval?.Invoke(renderers);
        }

        #endregion
    }
}
