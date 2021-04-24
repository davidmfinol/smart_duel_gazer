using System.Linq;
using AssemblyCSharp.Assets.Code.Core.DataManager.Interface.CardModel;
using UnityEngine;

namespace AssemblyCSharp.Assets.Code.Core.DataManager.Impl.CardModel
{
    public class CardModelDataManager : ICardModelDataManager
    {
        private const string MonsterResourcesFolderPath = "Monsters";

        private GameObject[] _cardModels;

        public GameObject GetCardModel(string cardId)
        {
            if (_cardModels == null || _cardModels.Length == 0)
            {
                LoadCardModels();
            }

            return _cardModels.SingleOrDefault(model => model.name == cardId);
        }

        private void LoadCardModels()
        {
            _cardModels = Resources.LoadAll<GameObject>(MonsterResourcesFolderPath);
        }
    }
}