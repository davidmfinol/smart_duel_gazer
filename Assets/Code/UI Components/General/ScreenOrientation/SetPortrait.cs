using Code.Core.Screen;
using UnityEngine;
using Zenject;

public class SetPortrait : MonoBehaviour
{
    private IScreenService _screenService;

    [Inject]
    public void Construct(IScreenService screenService)
    {
        _screenService = screenService;

        _screenService.UsePortraitOrientation();
    }
}