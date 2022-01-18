using System.Collections.Generic;
using System;
using UnityEditor;
using UnityEngine;

namespace CustomAttributes
{
    public class ProjectileListAsDropdownMenuAttribute : PropertyAttribute
    {
        public Type PropertyType;
        public string PropertyName;
        public string InspectorName;

        public ProjectileListAsDropdownMenuAttribute(Type type, string propertyName, string inspectorName)
        {
            PropertyType = type;
            PropertyName = propertyName;
            InspectorName = inspectorName;
        }
    }

#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(ProjectileListAsDropdownMenuAttribute))]
    public class ListToPropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            ProjectileListAsDropdownMenuAttribute att = attribute as ProjectileListAsDropdownMenuAttribute;
            List<string> stringList;

            if (att.PropertyType.GetField(att.PropertyName) == null) return;

            stringList = att.PropertyType.GetField(att.PropertyName).GetValue(att.PropertyType) as List<string>;

            if (stringList == null && stringList.Count == 0) return;

            int selectedIndex = stringList.IndexOf(property.stringValue);
            if(selectedIndex < 0)
            {
                selectedIndex = 0;
            }

            selectedIndex = EditorGUI.Popup(position, att.InspectorName, selectedIndex, stringList.ToArray());
            property.stringValue = stringList[selectedIndex];
        }
    }
#endif
}