using Code.Core.Screen;

public class SetPortrait
{
    private IScreenService _screenService;

    public SetPortrait(IScreenService screenService)
    {
        _screenService = screenService;

        _screenService.UsePortraitOrientation();
    }
}
