using UnityEngine;
using AssemblyCSharp.Assets.Code.Core.Models.Interface.ModelEventsHandler.Entities;

namespace AssemblyCSharp.Assets.Code.Core.Models.Interface.ModelEventsHandler
{
    public interface IModelEventHandler
    {
        public void ActivatePlayfield(GameObject playfield);
        public void RemovePlayfield();
        public void DestroyPlayfield();
        public void ActivateModel(string zone);
        public void RaiseEventByEventName(ModelEvent eventNames, string zone);
        public void RaiseChangeVisibilityEvent(string zone, bool state);
        public void RaiseMonsterRemovalEvent(SkinnedMeshRenderer[] renderers);
        public void RaiseSummonSetCardEvent(string zone, string modelName, bool isMonster);
    }
}
