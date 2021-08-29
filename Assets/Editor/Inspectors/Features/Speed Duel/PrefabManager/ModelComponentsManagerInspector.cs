using UnityEngine;
using UnityEditor;
using Code.Features.SpeedDuel.PrefabManager.ModelComponentsManager;

[CustomEditor(typeof(ModelComponentsManager))]
public class ModelComponentsManagerInspector : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        ModelComponentsManager modelComponentsManager = (ModelComponentsManager)target;

        if (GUILayout.Button("Summon Object"))
        {
            modelComponentsManager.CallSummonMonster();
        }

        if (GUILayout.Button("Remove Object"))
        {
            modelComponentsManager.CallRemoveMonster();
        }
    }
}