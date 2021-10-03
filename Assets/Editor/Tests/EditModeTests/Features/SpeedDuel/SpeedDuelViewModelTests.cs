using Code.Core.Logger;
using Code.Core.Navigation;
using Code.Features.SpeedDuel;
using Code.Features.SpeedDuel.EventHandlers;
using Code.Features.SpeedDuel.EventHandlers.Entities;
using Code.Features.SpeedDuel.UseCases;
using Moq;
using NUnit.Framework;
using UniRx;

namespace Tests.Features.SpeedDuel
{
    public class SpeedDuelViewModelTests
    {
        private SpeedDuelViewModel _viewModel;

        private Mock<IPlayfieldEventHandler> _playfieldEventHandler;
        private Mock<IEndOfDuelUseCase> _endOfDuelUseCase;
        private Mock<IAppLogger> _logger;

        [SetUp]
        public void SetUp()
        {
            _playfieldEventHandler = new Mock<IPlayfieldEventHandler>();
            _endOfDuelUseCase = new Mock<IEndOfDuelUseCase>();
            _logger = new Mock<IAppLogger>();

            _viewModel = new SpeedDuelViewModel(
                _playfieldEventHandler.Object,
                _endOfDuelUseCase.Object,
                _logger.Object);
        }

        [Test]
        public void Given_AnInvalidPlayfield_When_ActivatePlayfieldEventRecieved_Then_NoEventIsFired()
        {
            var expected = false;
            _viewModel.ActivatePlayfieldMenu.Subscribe(_ => expected = true);

            _playfieldEventHandler.Object.ActivatePlayfield(null);

            Assert.IsFalse(expected);
        }

        [Test]
        [TestCase(3f)]
        [TestCase(3.2f)]
        [TestCase(-2.1f)]
        [TestCase(10.7f)]
        public void Given_AFloatValue_When_RotateSliderIsAdjusted_Then_EventHandlerFiresRotateEvent(float value)
        {
            var model = new PlayfieldEventValue<float> { Value = value };

            _viewModel.RotatePlayfield(model);

            _playfieldEventHandler.Verify(eh => eh.Action(PlayfieldEvent.Rotate, model), Times.Once);
            _playfieldEventHandler.VerifyAll();
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void Given_ANonFloatValue_When_RotateSliderIsAdjusted_Then_NoEventIsFired(bool value)
        {
            var model = new PlayfieldEventValue<bool> { Value = value };

            _viewModel.RotatePlayfield(model);

            _playfieldEventHandler.Verify(eh => eh.Action(PlayfieldEvent.Rotate, model), Times.Never);
            _playfieldEventHandler.VerifyAll();
        }

        [Test]
        [TestCase(3f)]
        [TestCase(3.2f)]
        [TestCase(-2.1f)]
        [TestCase(10.7f)]
        public void Given_AFloatValue_When_ScaleSliderIsAdjusted_Then_EventHandlerFiresScaleEvent(float value)
{
            var model = new PlayfieldEventValue<float> { Value = value };

            _viewModel.ScalePlayfield(model);

            _playfieldEventHandler.Verify(eh => eh.Action(PlayfieldEvent.Scale, model), Times.Once);
            _playfieldEventHandler.VerifyAll();
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void Given_ANonFloatValue_When_ScaleSliderIsAdjusted_Then_NoEventIsFired(bool value)
        {
            var model = new PlayfieldEventValue<bool> { Value = value };

            _viewModel.ScalePlayfield(model);

            _playfieldEventHandler.Verify(eh => eh.Action(PlayfieldEvent.Scale, model), Times.Never);
            _playfieldEventHandler.VerifyAll();
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void Give_ABoolValue_When_HideToggleIsAdjusted_Then_EventHandlerFiresHideEvent(bool value)
        {
            var model = new PlayfieldEventValue<bool> { Value = value };

            _viewModel.HidePlayfield(model);

            _playfieldEventHandler.Verify(eh => eh.Action(PlayfieldEvent.Hide, model), Times.Once);
            _playfieldEventHandler.VerifyAll();
        }

        [Test]
        [TestCase(3f)]
        [TestCase(3.2f)]
        [TestCase(-2.1f)]
        [TestCase(10.7f)]
        public void Given_AFloatValue_When_HideToggleIsAdjusted_Then_NoEventIsFired(float value)
        {
            var model = new PlayfieldEventValue<float> { Value = value };

            _viewModel.HidePlayfield(model);

            _playfieldEventHandler.Verify(eh => eh.Action(PlayfieldEvent.Hide, model), Times.Never);
            _playfieldEventHandler.VerifyAll();
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void Give_ABoolValue_When_FlipToggleIsAdjusted_Then_EventHandlerFiresFlipEvent(bool value)
        {
            var model = new PlayfieldEventValue<bool> { Value = value };

            _viewModel.FlipPlayfield(model);

            _playfieldEventHandler.Verify(eh => eh.Action(PlayfieldEvent.Flip, model), Times.Once);
            _playfieldEventHandler.VerifyAll();
        }

        [Test]
        [TestCase(3f)]
        [TestCase(3.2f)]
        [TestCase(-2.1f)]
        [TestCase(10.7f)]
        public void Given_AFloatValue_When_FlipToggleIsAdjusted_Then_NoEventIsFired(float value)
        {
            var model = new PlayfieldEventValue<float> { Value = value };

            _viewModel.FlipPlayfield(model);

            _playfieldEventHandler.Verify(eh => eh.Action(PlayfieldEvent.Flip, model), Times.Never);
            _playfieldEventHandler.VerifyAll();
        }

        [Test]
        public void When_PlayfieldMenuTogglePressed_Then_MenuIsToggled()
        {
            bool expected = false;
            _viewModel.TogglePlayfieldMenu.Subscribe(value => expected = value);

            _viewModel.OnTogglePlayfieldMenu(true);

            Assert.IsTrue(expected);
        }

        [Test]
        public void When_RemovePlayfieldButtonPressed_Then_RemovePlayfieldEventIsFired()
        {
            bool expected = false;
            _viewModel.RemovePlayfield.Subscribe(_ => expected = true);

            _viewModel.OnRemovePlayfield();

            _playfieldEventHandler.Verify(eh => eh.RemovePlayfield(), Times.Once);
            Assert.IsTrue(expected);
        }

        [Test]
        public void When_BackButtonPressed_Then_EndOfDuelExeuted()
        {
            _viewModel.OnBackButtonPressed();

            _endOfDuelUseCase.Verify(uc => uc.Execute(), Times.Once);
        }
    }
}