using Zenject;
using UnityEngine;
using AssemblyCSharp.Assets.Code.Core.General.Statics;
using AssemblyCSharp.Assets.Code.Core.Models.Impl.ModelEventsHandler;

public class SetCard : MonoBehaviour
{
    private ModelEventHandler _eventHandler;

    private IImageSetter _imageSetter;
    private Animator _animator;
    private string _zone;

    [Inject]
    public void Construct(ModelEventHandler modelEventHandler)
    {
        _eventHandler = modelEventHandler;
    }

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
        _eventHandler.OnRevealSetMonster += RevealSetMonster;
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

    public void RevealSetMonster(string zone)
    {
        if (_zone == zone)
        {
            _animator.SetTrigger(AnimatorParams.Reveal_Set_Monster_Trigger);
        }
    }

    public void DestroySetMonster(string zone)
    {
        if (_zone == zone)
        {
            _animator.SetTrigger(AnimatorParams.Reveal_Set_Monster_Trigger);
        }
    }
}
