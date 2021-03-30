using AssemblyCSharp.Assets.Code.Core.Models.Interface.Entities;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IModelComponentsManager
{
    public void SummonMonster(string zone);
    public void SetMonsterVisibility(string zone, bool state);
    public void DestroyMonster(string zone, bool state);
}
