using Code.Features.SpeedDuel.UseCases;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Code.UI_Components.General
{
    public class ShowConnectionScene : MonoBehaviour
    {
        [SerializeField]
        private Button _button;

        private IEndOfDuelUseCase _endOfDuel;

        #region Constructors

        [Inject]
        public void Construct(
            IEndOfDuelUseCase endOfGame)
        {
            _endOfDuel = endOfGame;

            RegisterClickListeners();
        }

        #endregion

        private void RegisterClickListeners()
        {
            _button.onClick.AsObservable().Subscribe(_ => OnButtonPressed());
        }

        private void OnButtonPressed()
        {
            _endOfDuel.Execute();
        }
    }
}
