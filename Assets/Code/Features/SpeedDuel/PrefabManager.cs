using Zenject;
using UnityEngine;
using AssemblyCSharp.Assets.Code.Core.Models.Impl.SetCard;
using AssemblyCSharp.Assets.Code.Core.DataManager.Interface;

namespace AssemblyCSharp.Assets.Code.Features.SpeedDuel
{
    public class PrefabManager : MonoBehaviour
    {
        private static readonly string SET_CARD = "SetCard";

        private const string _keyParticles = "Particles";
        private const string _keySetCard = "SetCards";

        [SerializeField]
        private GameObject _particles;

        private IDataManager _dataManager;
        private SetCard.Factory _setCardFactory;
        private DestructionParticles.Factory _particleFactory;

        #region Constructor

        [Inject]
        public void Construct(IDataManager dataManager,
                              SetCard.Factory setCardFactory,
                              DestructionParticles.Factory particlesFactory)
        {
            _dataManager = dataManager;
            _setCardFactory = setCardFactory;
            _particleFactory = particlesFactory;
        }

        #endregion

        #region LifeCycle

        private void Awake()
        {
            InstantiatePrefabs(_keySetCard, 8);
            InstantiatePrefabs(_keyParticles, 8);
        }

        #endregion

        private void InstantiatePrefabs(string key, int amount)
        {
            if (key == _keySetCard)
            {
                for (int i = 0; i < amount; i++)
                {
                    var obj = _setCardFactory.Create(_dataManager.GetCardModel(SET_CARD)).transform.gameObject;
                    obj.transform.SetParent(transform);
                    _dataManager.AddGameObjectToQueue(key, obj);
                }
                return;
            }
            else if (key == _keyParticles)
            {
                for (int i = 0; i < amount; i++)
                {
                    var obj = _particleFactory.Create(_particles).transform.gameObject;
                    obj.transform.SetParent(transform);
                    _dataManager.AddGameObjectToQueue(key, obj);
                }
                return;
            }

            //Pre-Instantiate models from deck:
            //Recommend sending over deck information (ie. which/how many models) in order to have them ready
            //when the duel starts. This would be for multiplayer, it's not really needed currently
        }
    }
}
