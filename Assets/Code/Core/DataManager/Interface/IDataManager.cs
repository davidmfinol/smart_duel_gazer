using AssemblyCSharp.Assets.Code.Core.DataManager.Interface.Texture;
using AssemblyCSharp.Assets.Code.Core.DataManager.Interface.GameObject;
using AssemblyCSharp.Assets.Code.Core.DataManager.Interface.Connection;
using Code.Core.DataManager.DuelRoom;

namespace AssemblyCSharp.Assets.Code.Core.DataManager.Interface
{
    public interface IDataManager : IConnectionDataManager, IGameObjectDataManager, ITextureDataManager, IDuelRoomDataManager
    {
    }
}
