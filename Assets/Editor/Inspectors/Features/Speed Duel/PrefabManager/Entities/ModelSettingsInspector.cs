using Code.Core.DataManager.GameObjects.Entities;
using Code.Features.SpeedDuel.PrefabManager.ModelComponentsManager.Entities;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Zenject;

[CustomEditor(typeof(ModelSettings))]
public class ModelSettingsInspector : Editor
{
    SerializedProperty projectileType;
    private string[] _projectiles = { GameObjectKeys.BulletProjectileKey, GameObjectKeys.FireProjectileKey, GameObjectKeys.MagicalProjectileKey };
    private int _index = 0;

    public void OnEnable()
    {
        projectileType = serializedObject.FindProperty("ProjectileKey");
        _index = Array.IndexOf(_projectiles, projectileType.stringValue);
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        serializedObject.Update();
        ModelSettings modelSettings = (ModelSettings)target;

        _index = EditorGUILayout.Popup("ProjectileKey", _index, _projectiles);
        if (_index < 0)
        {
            _index = 0;
        }

        projectileType.stringValue = _projectiles[_index];
        serializedObject.ApplyModifiedProperties();
    }
}
