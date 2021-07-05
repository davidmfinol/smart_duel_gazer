using AssemblyCSharp.Assets.Code.Core.Models.Impl.ModelEventsHandler;
using AssemblyCSharp.Assets.Code.Core.Models.Interface.ModelEventsHandler.Entities;
using AssemblyCSharp.Assets.Code.Core.Storage.Impl.Providers.PlayerPrefs.Interface;
using UnityEngine;
using Zenject;

public class TapToAttack : MonoBehaviour
{
    private IPlayerPrefsProvider _playerPrefsProvider;
    private ModelEventHandler _modelEventHandler;
    
    private Camera _camera;

    #region Constructors

    [Inject]
    public void Construct(IPlayerPrefsProvider playerPrefsProvider,
                          ModelEventHandler modelEventHandler)
    {
        _playerPrefsProvider = playerPrefsProvider;
        _modelEventHandler = modelEventHandler;
    }

    #endregion

    #region Lifecycle

    private void Awake()
    {
        _camera = Camera.main;
    }

    private void Update()
    {        
        var tapToAttackPlayerSetting = _playerPrefsProvider.GetString("TapToAttack");
        if(tapToAttackPlayerSetting == null)
        {
            Debug.LogWarning($"This setting doesn't exist. Check that the names are correct");
            return;
        }

        if(tapToAttackPlayerSetting == "Disabled")
        {
            return;
        }

        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            SendRaycast(Input.GetTouch(0));
        }

        #region EditorCode
#if UNITY_EDITOR

        if (Input.GetMouseButtonUp(0))
        {
            var ray = _camera.ScreenPointToRay(Input.mousePosition);
            var hitSomething = Physics.Raycast(ray, out RaycastHit hit);

            if (!hitSomething)
            {
                return;
            }

            var objectID = hit.transform.GetInstanceID().ToString();
            _modelEventHandler.RaiseEventByEventName(ModelEvent.Attack, objectID);
        }

#endif
        #endregion

    }

    #endregion

    private void SendRaycast(Touch touch)
    {
        var ray = _camera.ScreenPointToRay(touch.position);
        var hitSomething = Physics.Raycast(ray, out RaycastHit hit);

        if (!hitSomething)
        {
            return;
        }

        var objectID = hit.transform.GetInstanceID().ToString();
        _modelEventHandler.RaiseEventByEventName(ModelEvent.Attack, objectID);
    }
}
