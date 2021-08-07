using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Code.UI_Components.General
{
    public class ShowSettingsMenu : MonoBehaviour
    {
        [SerializeField]
        private GameObject _settingsMenu;

        private Button _settingsButton;

        private void Awake()
        {
            _settingsButton = GetComponent<Button>();
            RegisterClickListeners();
        }

        private void RegisterClickListeners()
        {
            _settingsButton.OnClickAsObservable().Subscribe(_ => OnSettingsButtonPressed());
        }

        private void OnSettingsButtonPressed()
        {
            if (_settingsMenu.activeSelf)
            {
                _settingsMenu.SetActive(false);
                return;
            }

            _settingsMenu.SetActive(true);
        }
    }
}
