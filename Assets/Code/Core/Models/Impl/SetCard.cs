using AssemblyCSharp.Assets.Code.Core.General.Statics;
using AssemblyCSharp.Assets.Code.Core.Models.Impl;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetCard : MonoBehaviour
{
    [SerializeField]
    private ModelEventHandler _eventHandler;

    private IImageSetter _imageSetter;
    private Animator _animator;
    private string _zone;

    #region Lifecycle

    private void Awake()
    {
        _imageSetter = GetComponent<IImageSetter>();
        _animator = GetComponent<Animator>();

        SubscribeToEvents();
    }

    private void OnEnable()
    {
        _eventHandler.OnSpellTrapActivate += OnSpellTrapActivate;
    }

    #endregion

    private void SubscribeToEvents()
    {
        _eventHandler.OnSummonSpellTrap += OnSummonSpellTrap;
        _eventHandler.OnSpellTrapActivate += OnSpellTrapActivate;
        _eventHandler.OnSpellTrapRemove += OnSpellTrapRemove;
        _eventHandler.OnDestroySetMonster += DestroySetMonster;
    }

    private void OnSummonSpellTrap(string zone, string modelName)
    {
        _zone = zone;
        _imageSetter.ChangeImageFromAPI(modelName);
        _eventHandler.OnSummonSpellTrap -= OnSummonSpellTrap;
    }

    private void OnSpellTrapActivate(string zone)
    { 
        if(_zone == zone)
        {
            _animator.SetTrigger(AnimatorParams.Activate_Spell_Or_Trap_Trigger);
        }
    }
    
    private void OnSpellTrapRemove(string zone)
    {
        if (_zone == zone)
        {
            _animator.SetTrigger(AnimatorParams.Remove_Spell_Or_Trap_Trigger);
        }
    }

    public void DestroySetMonster(string zone)
    {
        if (_zone == zone)
        {
            _animator.SetTrigger(AnimatorParams.Show_Set_Monster_Trigger);
        }
    }
}
