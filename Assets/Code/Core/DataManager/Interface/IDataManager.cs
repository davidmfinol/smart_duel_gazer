using AssemblyCSharp.Assets.Code.Core.DataManager.Interface.CardImage;
using AssemblyCSharp.Assets.Code.Core.DataManager.Interface.CardModel;
using AssemblyCSharp.Assets.Code.Core.DataManager.Interface.Connection;
using AssemblyCSharp.Assets.Code.Core.DataManager.Interface.ModelRecycler;

namespace AssemblyCSharp.Assets.Code.Core.DataManager.Interface
{
    public interface IDataManager : IConnectionDataManager, ICardModelDataManager, ICardImageDataManager, IModelRecycler
    {
    }
}
