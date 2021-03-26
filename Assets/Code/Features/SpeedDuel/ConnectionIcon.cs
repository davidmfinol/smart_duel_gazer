using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Zenject;
using AssemblyCSharp.Assets.Code.Core.SmartDuelServer.Impl;
using AssemblyCSharp.Assets.Code.Core.DataManager.Interface;
using Dpoch.SocketIO;
using AssemblyCSharp.Assets.Code.Features.SpeedDuel;

public class ConnectionIcon : MonoBehaviour
{
    public Image _connectIcon;
    public Image _disconnectionIcon;

    private SpeedDuelView _server;
    private IDataManager _dataManager;
    private SocketIO socket;

    [Inject]
    public void Construct(IDataManager dataManager)
    {
        _dataManager = dataManager;
    }

    private void Update()
    {
        if(_server)
        {
            Connected();
            return;
        }

        Disconnected();
    }

    private void Connected()
    {
        _connectIcon.enabled = true;
    }

    private void Disconnected()
    {
        _disconnectionIcon.enabled = true;
    }
}
