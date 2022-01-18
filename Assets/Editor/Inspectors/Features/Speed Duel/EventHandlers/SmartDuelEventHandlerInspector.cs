using Code.Core.SmartDuelServer.Entities.EventData.CardEvents;
using Code.Features.SpeedDuel.EventHandlers;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(SmartDuelEventHandler))]
public class SmartDuelEventHandlerInspector : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        var _smartDuelEventHandler = (SmartDuelEventHandler)target;

        if (GUILayout.Button("Attack Event"))
        {
            //_smartDuelEventHandler.HandleAttackEvent(data);
        }
    }
}