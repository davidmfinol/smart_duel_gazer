using Code.Core.DataManager;
using Code.Core.DataManager.GameObjects.Entities;
using Code.Core.General.Helpers;
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

        // TODO: is this comment below still necessary?
        // Handle Async functions that haven't completed yet
        public void Execute()
        {
            _logger.Log(Tag, "Execute()");

            var gameObjectKeys = EnumHelper.GetEnumValues<GameObjectKey>();
            foreach (var key in gameObjectKeys)
            {
                _dataManager.RemoveGameObject(key.GetStringValue());
            }
            
            _dataManager.RemovePlayfield();

            _navigationService.ShowConnectionScene();
        }
    }
}