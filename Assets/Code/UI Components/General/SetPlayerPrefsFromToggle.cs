using UnityEngine;
using UnityEngine.UI;
using UniRx;
using Zenject;
using AssemblyCSharp.Assets.Code.Core.Storage.Impl.Providers.PlayerPrefs.Interface;

public class SetPlayerPrefsFromToggle : MonoBehaviour
{
    private readonly string Enabled = "Enabled";
    private readonly string Disabled = "Disabled";
    
    private IPlayerPrefsProvider _playerPrefsProvider;

    private Toggle _toggle;
    private string toggleKey;

    #region Constructors

    [Inject]
    public void Construct(IPlayerPrefsProvider playerPrefsProvider)
    {
        _playerPrefsProvider = playerPrefsProvider;
    }

    #endregion

    #region Lifecycle

    private void Awake()
    {
        _toggle = GetComponent<Toggle>();
        toggleKey = _toggle.name;
        
        if(!_playerPrefsProvider.HasKey(toggleKey))
        {
            _playerPrefsProvider.SetString(toggleKey, Enabled);
        }

        _toggle.isOn = ChangeStringToBool(_playerPrefsProvider.GetString(toggleKey));

        RegisterClickListeners();
    }

    #endregion

    private void RegisterClickListeners()
    {
        _toggle.OnValueChangedAsObservable().Subscribe(_ => SetPreferencesUsingToggleValue(_toggle.isOn));
    }

    private void SetPreferencesUsingToggleValue(bool isEnabled)
    {        
        if(toggleKey == null)
        {
            Debug.LogError($"No Key Has Been Set!", this);
            return;
        }

        var playerPreference = ChangeBoolToString(isEnabled);
        _playerPrefsProvider.SetString(toggleKey, playerPreference);
    }

    private string ChangeBoolToString(bool playerPreference)
    {
        if(!playerPreference)
        {
            return Disabled;
        }

        return Enabled;
    }

    private bool ChangeStringToBool(string playerPreference)
    {
        if(playerPreference == Disabled)
        {
            return false;
        }

        return true;
    }

}
