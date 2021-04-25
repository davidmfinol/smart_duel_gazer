using AssemblyCSharp.Assets.Code.Core.DataManager.Interface.Texture;
using AssemblyCSharp.Assets.Code.Core.DataManager.Interface.GameObject;
using AssemblyCSharp.Assets.Code.Core.DataManager.Interface.Connection;
using AssemblyCSharp.Assets.Code.Core.DataManager.Interface.ModelRecycler;

namespace AssemblyCSharp.Assets.Code.Core.DataManager.Interface
{
    public interface IDataManager : IConnectionDataManager, IGameObjectDataManager, ITextureDataManager, IModelRecycler
    {
    }
}
