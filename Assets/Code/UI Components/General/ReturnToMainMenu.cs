using Code.Core.Navigation;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Code.UI_Components.General
{
    public class ReturnToMainMenu : MonoBehaviour
    {
        [SerializeField]
        private Button _backButton;

        private INavigationService _navigationService;

        #region Constructors

        [Inject]
        public void Construct(
            INavigationService navigationService)
        {
            _navigationService = navigationService;

            RegisterClickListeners();
        }

        #endregion

        private void RegisterClickListeners()
        {
            _backButton.onClick.AsObservable().Subscribe(_ => OnBackButtonPressed());
        }

        private void OnBackButtonPressed()
        {
            _navigationService.ShowConnectionScene();
        }
    }
}
