using Code.Core.DataManager;
using Code.UI_Components.Constants;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Code.UI_Components.General
{
    public class SetUserSettingsFromToggle : MonoBehaviour
    {
        [SerializeField]
        private SettingsItems _settingsItem;

        private IDataManager _dataManager;

        private Toggle _toggle;
        private string _toggleKey;

        #region Constructors

        [Inject]
        public void Construct(IDataManager dataManager)
        {
            _dataManager = dataManager;
        }

        #endregion

        #region Lifecycle

        private void Awake()
        {
            _toggle = GetComponent<Toggle>();
            _toggleKey = _settingsItem.ToString();

            _toggle.isOn = _dataManager.IsToggleSettingEnabled(_toggleKey);

            RegisterClickListeners();
        }

        #endregion

        private void RegisterClickListeners()
        {
            _toggle.OnValueChangedAsObservable().Subscribe(SetPreferencesUsingToggleValue);
        }

        private void SetPreferencesUsingToggleValue(bool isEnabled)
        {
            if (_toggleKey == null) return;

            _dataManager.SetToggleSetting(_toggleKey, isEnabled);
        }
    }
}
