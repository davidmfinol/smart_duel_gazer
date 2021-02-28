using AssemblyCSharp.Assets.Code.Core.DataManager.Interface.CardModel;
using AssemblyCSharp.Assets.Code.Core.DataManager.Interface.Connection;

namespace AssemblyCSharp.Assets.Code.Core.DataManager.Interface
{
    public interface IDataManager : IConnectionDataManager, ICardModelDataManager
    {
    }
}
