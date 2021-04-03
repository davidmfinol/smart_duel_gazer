namespace AssemblyCSharp.Assets.Code.Core.Models.Interface.ModelComponentsManager
{
    public interface IModelComponentsManager
    {
        public void ScaleModel();
        public void SummonMonster(string zone);
        public void SetMonsterVisibility(string zone, bool state);
        public void DestroyMonster(string zone, bool state);
    }
}
