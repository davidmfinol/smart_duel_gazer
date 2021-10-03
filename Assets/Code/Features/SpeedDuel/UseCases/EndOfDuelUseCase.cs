using Code.Core.DataManager;
using Code.Core.DataManager.GameObjects.Entities;
using Code.Core.Logger;
using Code.Core.Navigation;

namespace Code.Features.SpeedDuel.UseCases
{
    public interface IEndOfDuelUseCase
    {
        void Execute();
    }

    public class EndOfDuelUseCase : IEndOfDuelUseCase
    {
        private const string Tag = "EndOfDuelUseCase";

        private readonly IDataManager _dataManager;
        private readonly INavigationService _navigationService;
        private readonly IAppLogger _logger;

        #region Constructor

        public EndOfDuelUseCase(
            IDataManager dataManager,
            INavigationService navigationService,
            IAppLogger appLogger)
        {
            _dataManager = dataManager;
            _navigationService = navigationService;
            _logger = appLogger;
        }

        #endregion

        //Handle Async functions that haven't completed yet
        public void Execute()
        {
            _logger.Log(Tag, "Execute()");

            _dataManager.RemoveGameObject(GameObjectKeys.ParticlesKey);
            _dataManager.RemoveGameObject(GameObjectKeys.SetCardKey);
            _dataManager.RemoveGameObject(GameObjectKeys.PlayfieldKey);
            _dataManager.RemoveStoredPlayfield();

            _navigationService.ShowConnectionScene();
        }
    }
}