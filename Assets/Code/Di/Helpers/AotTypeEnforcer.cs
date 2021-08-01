using Code.Core.SmartDuelServer.Entities.EventData.RoomEvents;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Utilities;
using UnityEngine;

namespace Code.Di.Helpers
{
    /// <summary>
    /// By default it's not possible to deserialize JSON collections when the app is compiled AoT.
    /// The app is compiled AoT for Android and iOS, but not the the Unity editor.
    /// To make sure collections can be deserialized as expected we use this helper class.
    ///
    /// More info: https://github.com/jilleJr/Newtonsoft.Json-for-Unity/wiki/Fix-AOT-using-AotHelper
    /// </summary>
    public class AotTypeEnforcer : MonoBehaviour
    {
        public void Awake()
        {
            AotHelper.EnsureList<int>();
            AotHelper.EnsureList<string>();
            AotHelper.EnsureList<Duelist>();
            
            AotHelper.EnsureType<StringEnumConverter>();
        }
    }
}