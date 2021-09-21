using Code.Core.Navigation;
using Code.Features.SpeedDuel;
using Code.Features.SpeedDuel.EventHandlers;
using Code.Features.SpeedDuel.EventHandlers.Entities;
using Moq;
using NUnit.Framework;

namespace Tests.EditMode.Features.SpeedDuel
{
    public class SpeedDuelViewModelTests
    {
        //private SpeedDuelViewModel _viewModel;

        //private Mock<INavigationService> _navigationService;
        //private Mock<IPlayfieldEventHandler> _playfieldEventHandler;

        //[SetUp]
        //public void SetUp()
        //{
        //    _navigationService = new Mock<INavigationService>();
        //    _playfieldEventHandler = new Mock<IPlayfieldEventHandler>();

        //    _viewModel = new SpeedDuelViewModel(
        //        _navigationService.Object,
        //        _playfieldEventHandler.Object);
        //}

        //[Test]
        //public void When_RotateSliderIsAdjusted_Then_EventHandlerFiresRotateEvent()
        //{
        //    var expected = new PlayfieldEventArgs { FloatValue = 1f };

        //    _playfieldEventHandler.Setup(eh => eh.Action(PlayfieldEvent.Rotate, expected)).Raises(eh => eh.OnAction += null);
        //    _viewModel.RotatePlayfield(expected);

        //    _playfieldEventHandler.Verify(eh => eh.Action(PlayfieldEvent.Rotate, expected), Times.Once);
        //    _playfieldEventHandler.VerifyAll();
        //}

        //[Test]
        //public void When_ScaleSliderIsAdjusted_Then_EventHandlerFiresScaleEvent()
        //{
        //    var expected = new PlayfieldEventArgs { FloatValue = 1f };

        //    _playfieldEventHandler.Setup(eh => eh.Action(PlayfieldEvent.Scale, expected)).Raises(eh => eh.OnAction += null);
        //    _viewModel.ScalePlayfield(expected);

        //    _playfieldEventHandler.Verify(eh => eh.Action(PlayfieldEvent.Scale, expected), Times.Once);
        //    _playfieldEventHandler.VerifyAll();
        //}

        //[Test]
        //[TestCase(true)]
        //[TestCase(false)]
        //public void When_HideToggleIsAdjusted_Then_EventHandlerFiresHideEvent(bool value)
        //{
        //    var expected = new PlayfieldEventArgs { BoolValue = value };

        //    _playfieldEventHandler.Setup(eh => eh.Action(PlayfieldEvent.Hide, expected)).Raises(eh => eh.OnAction += null);
        //    _viewModel.HidePlayfield(expected);

        //    _playfieldEventHandler.Verify(eh => eh.Action(PlayfieldEvent.Hide, expected), Times.Once);
        //    _playfieldEventHandler.VerifyAll();
        //}

        //[Test]
        //[TestCase(true)]
        //[TestCase(false)]
        //public void When_FlipToggleIsAdjusted_Then_EventHandlerFiresFlipEvent(bool value)
        //{
        //    var expected = new PlayfieldEventArgs { BoolValue = value };

        //    _playfieldEventHandler.Setup(eh => eh.Action(PlayfieldEvent.Flip, expected)).Raises(eh => eh.OnAction += null);
        //    _viewModel.FlipPlayfield(expected);

        //    _playfieldEventHandler.Verify(eh => eh.Action(PlayfieldEvent.Flip, expected), Times.Once);
        //    _playfieldEventHandler.VerifyAll();
        //}

        //[Test]
        //public void When_BackButtonPressed_Then_ShowConnctionScene()
        //{
        //    _viewModel.OnBackButtonPressed();

        //    _navigationService.Verify(ns => ns.ShowConnectionScene(), Times.Once);
        //}
    }
}