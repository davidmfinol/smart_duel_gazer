using Code.Core.DataManager;
using Code.Core.DataManager.GameObjects.Entities;
using Code.Core.General.Helpers;
using Code.Core.Logger;
using Code.Core.Navigation;
using Code.Features.SpeedDuel.UseCases;
using Moq;
using NUnit.Framework;

namespace Editor.Tests.EditModeTests.Features.SpeedDuel.UseCases
{
    public class EndOfDuelUseCaseTests
    {
        private Mock<IDataManager> _dataManger;
        private Mock<INavigationService> _navigationService;
        private Mock<IAppLogger> _logger;

        private EndOfDuelUseCase _endOfDuel;

        [SetUp]
        public void Setup()
        {
            _dataManger = new Mock<IDataManager>();
            _navigationService = new Mock<INavigationService>();
            _logger = new Mock<IAppLogger>();

            _endOfDuel = new EndOfDuelUseCase(
                _dataManger.Object,
                _navigationService.Object,
                _logger.Object);
        }

        [Test]
        public void When_ExecuteCalled_Then_ConnectionScreenShown()
        {
            _endOfDuel.Execute();

            _navigationService.Verify(ns => ns.ShowConnectionScene(), Times.Once);
        }

        [Test]
        public void When_ExecuteCalled_Then_AllGameObjectRemoved()
        {
            var keys = EnumHelper.GetEnumValues<GameObjectKey>();

            _endOfDuel.Execute();

            foreach (var key in keys)
            {
                _dataManger.Verify(dm => dm.RemoveGameObject(key.GetStringValue()), Times.Once);
            }
        }

        [Test]
        public void When_ExecuteCalled_PlayfieldRemoved()
        {
            _endOfDuel.Execute();

            _dataManger.Verify(dm => dm.RemovePlayfield(), Times.Once);
        }
    }
}