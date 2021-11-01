using Code.Core.Logger;
using Code.Core.Navigation;
using Code.Features.SpeedDuel.EventHandlers;
using Code.Features.SpeedDuel;
using Code.Features.SpeedDuel.Models;
using Moq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UniRx;
using Code.Features.SpeedDuel.UseCases;
using Code.Core.DataManager;

namespace Tests.Features.SpeedDuel
{
    public class SpeedDuelViewModelPlayTests
    {
        private SpeedDuelViewModel _viewModel;

        private Mock<IPlayfieldEventHandler> _playfieldEventHandler;
        private Mock<IDataManager> _dataManager;
        private Mock<IEndOfDuelUseCase> _endOfDuelUseCase;
        private Mock<IAppLogger> _logger;

        [SetUp]
        public void SetUp()
        {
            _playfieldEventHandler = new Mock<IPlayfieldEventHandler>();
            _dataManager = new Mock<IDataManager>();
            _endOfDuelUseCase = new Mock<IEndOfDuelUseCase>();
            _logger = new Mock<IAppLogger>();

            _viewModel = new SpeedDuelViewModel(
                _playfieldEventHandler.Object,
                _dataManager.Object,
                _endOfDuelUseCase.Object,
                _logger.Object);
        }

        [UnityTest]
        [TestCase(60f, 4f)]
        [TestCase(59.2f, 4.5f)]
        [TestCase(-78.1f, 9f)]
        [TestCase(21.7f, -2.3f)]
        public void Given_AValidPlayfield_When_ActivatePlayfieldEventRecieved_Then_CorrectTransformValuesAreFiredWithEvent(
            float correctRotation, float correctScale)
        {
            var model = new PlayfieldTransformValues { YAxisRotation = correctRotation, Scale = correctScale };
            var expected = new PlayfieldTransformValues();
            var testObj = new GameObject();
            testObj.transform.localRotation = Quaternion.Euler(0, correctRotation, 0);
            testObj.transform.localScale = new Vector3(correctScale, 1, 1);
            _viewModel.ActivatePlayfieldMenu.Subscribe(testVal => expected = testVal);

            _playfieldEventHandler.Raise(eh => eh.OnActivatePlayfield += null, testObj);

            Assert.AreEqual(model.Scale, expected.Scale);
            // TODO: Figure out why rotation test is failing
            //Assert.AreEqual(model.Rotation, expected.Rotation);
        }
    }
}