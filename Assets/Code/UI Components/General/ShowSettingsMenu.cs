using UnityEngine;
using UnityEngine.UI;
using UniRx;

public class ShowSettingsMenu : MonoBehaviour
{
    [SerializeField]
    private Button _settingsButton;

    [SerializeField]
    private GameObject _settingsMenu;

    private void Awake()
    {
        RegisterClickListeners();
    }

    private void RegisterClickListeners()
    {
        _settingsButton.OnClickAsObservable().Subscribe(_ => OnSettingsButtonPressed());
    }

    private void OnSettingsButtonPressed()
    {
        if(_settingsMenu.activeSelf)
        {
            _settingsMenu.SetActive(false);
            return;
        }
        
        _settingsMenu.SetActive(true);
    }
}
