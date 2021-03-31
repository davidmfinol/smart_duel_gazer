using UnityEngine;
using AssemblyCSharp.Assets.Code.Core.Models.Interface.ModelEventsHandler.Entities;

public interface IModelEventHandler
{
    public void RaiseEvent(EventNames evenNames, string zone);
    public void RaiseEvent(EventNames eventNames, string zone, bool state);
    public void RaiseEvent(EventNames eventName, SkinnedMeshRenderer[] renderers);
}
