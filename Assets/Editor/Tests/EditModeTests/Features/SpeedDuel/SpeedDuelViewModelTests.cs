using System.Collections.Generic;
using Code.Core.DataManager;
using Code.Core.Logger;
using Code.Features.SpeedDuel;
using Code.Features.SpeedDuel.EventHandlers;
using Code.Features.SpeedDuel.EventHandlers.Entities;
using Code.Features.SpeedDuel.Models;
using Code.Features.SpeedDuel.UseCases;
using Moq;
using NUnit.Framework;
using UniRx;
using UnityEngine;

namespace Editor.Tests.EditModeTests.Features.SpeedDuel
{
    public class SpeedDuelViewModelTests
    {
        private SpeedDuelViewModel _viewModel;

        private Mock<IPlayfieldEventHandler> _playfieldEventHandler;
        private Mock<IDataManager> _dataManager;
        private Mock<IEndOfDuelUseCase> _endOfDuelUseCase;
        private Mock<IAppLogger> _logger;

        private const float PlayfieldScaleValue = 5f;
        private const float PlayfieldRotationValue = 180f;

        private readonly Vector3 _playfieldScale = new Vector3(PlayfieldScaleValue, 0, 0);
        private readonly Quaternion _playfieldRotation = new Quaternion(0, PlayfieldRotationValue, 0, 0);

        private GameObject _playfieldPrefab;
        private GameObject _playfield;

        [SetUp]
        public void SetUp()
        {
            _playfieldEventHandler = new Mock<IPlayfieldEventHandler>();
            _dataManager = new Mock<IDataManager>();
            _endOfDuelUseCase = new Mock<IEndOfDuelUseCase>();
            _logger = new Mock<IAppLogger>();

            _playfieldPrefab = new GameObject();
            _playfield = Object.Instantiate(_playfieldPrefab);
            _playfield.transform.localScale = _playfieldScale;
            _playfield.transform.localRotation = _playfieldRotation;

            _dataManager.Setup(dm => dm.GetPlayfield()).Returns(_playfield);

            _viewModel = new SpeedDuelViewModel(
                _playfieldEventHandler.Object,
                _dataManager.Object,
                _endOfDuelUseCase.Object,
                _logger.Object);
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_playfieldPrefab);
            Object.DestroyImmediate(_playfield);
        }

        [Test]
        public void When_PlayfieldActivated_Then_PlayfieldFetched()
        {
            _viewModel.Init();
            _playfieldEventHandler.Raise(eh => eh.OnActivatePlayfield += null);
            _playfieldEventHandler.Object.ActivatePlayfield();

            _dataManager.Verify(dm => dm.GetPlayfield(), Times.Once);
        }

        [Test]
        public void Given_PlayfieldDoesNotExist_When_PlayfieldActivated_Then_PlayfieldNotActivated()
        {
            _dataManager.Setup(dm => dm.GetPlayfield()).Returns((GameObject)null);

            var activated = false;
            _viewModel.ActivatePlayfieldUIElements.Subscribe(_ => activated = true);

            _viewModel.Init();
            _playfieldEventHandler.Raise(eh => eh.OnActivatePlayfield += null);
            _playfieldEventHandler.Object.ActivatePlayfield();

            Assert.IsFalse(activated);
        }

        [Test]
        public void Given_PlayfieldExists_When_PlayfieldActivated_Then_PlayfieldValuesEmitted()
        {
            var expected = PlayfieldScaleValue;

            var onNext = new List<float>();
            _viewModel.ActivatePlayfieldUIElements.Subscribe(value => onNext.Add(value));

            _viewModel.Init();
            _playfieldEventHandler.Raise(eh => eh.OnActivatePlayfield += null);
            _playfieldEventHandler.Object.ActivatePlayfield();
            
            Assert.AreEqual(new List<float> { expected }, onNext);
        }

        [Test]
        public void When_PlayfieldTransparencyUpdated_Then_TransparencyActionTriggered()
        {
            const float value = 3f;
            var expectedArgs = new PlayfieldEventValue<float>(value);

            _viewModel.UpdatePlayfieldTransparency(value);

            _playfieldEventHandler.Verify(eh => eh.Action(PlayfieldEvent.Transparency, expectedArgs), Times.Once);
        }

        [Test]
        public void When_PlayfieldScaleUpdated_Then_ScaleActionTriggered()
        {
            const float value = 3f;
            var expectedArgs = new PlayfieldEventValue<float>(value);

            _viewModel.UpdatePlayfieldScale(value);

            _playfieldEventHandler.Verify(eh => eh.Action(PlayfieldEvent.Scale, expectedArgs), Times.Once);
        }

        [Test]
        public void When_PlayfieldRemoved_Then_RemovePlayfieldEmitsFalse()
        {
            var onNext = new List<bool>();
            _viewModel.RemovePlayfield.Subscribe(value => onNext.Add(value));

            _viewModel.OnRemovePlayfield();

            Assert.AreEqual(new List<bool> { false }, onNext);
        }

        [Test]
        public void When_PlayfieldRemoved_Then_RemovePlayfieldEventIsFired()
        {
            _viewModel.OnRemovePlayfield();

            _playfieldEventHandler.Verify(eh => eh.RemovePlayfield(), Times.Once);
        }

        [Test]
        public void When_SettingsMenuToggled_Then_SettingsMenuVisibilityEmitsValue()
        {
            var onNext = new List<bool>();
            _viewModel.ShowSettingsMenu.Subscribe(value => onNext.Add(value));
            
            _viewModel.OnToggleSettingsMenu(true);

            Assert.AreEqual(new List<bool> { false, true }, onNext);
        }

        [Test]
        public void When_SettingsMenuToggled_Then_SetSettingsMenuStateCalled()
        {
            _viewModel.OnToggleSettingsMenu(true);

            _playfieldEventHandler.Verify(eh => eh.SetSettingsMenuState(true), Times.Once);
        }

        [Test]
        public void When_BackButtonPressed_Then_DuelIsEnded()
        {
            _viewModel.OnBackButtonPressed();

            _endOfDuelUseCase.Verify(uc => uc.Execute(), Times.Once);
        }
    }
}