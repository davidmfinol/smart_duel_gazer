using UnityEngine;
using AssemblyCSharp.Assets.Code.Core.Models.Interface.ModelEventsHandler.Entities;

namespace AssemblyCSharp.Assets.Code.Core.Models.Interface.ModelEventsHandler
{
    public interface IModelEventHandler
    {
        public void ActivatePlayfield(GameObject playfield);
        public void PickupPlayfield();
        public void DestroyPlayfield();
        public void ActivateModel(int instanceID);
        public void RaiseEventByEventName(ModelEvent eventNames, int instanceID);
        public void RaiseChangeVisibilityEvent(int instanceID, bool state);
        public void RaiseMonsterRemovalEvent(SkinnedMeshRenderer[] renderers);
        public void RaiseSummonSetCardEvent(int instanceID, string modelName, bool isMonster);
    }
}
