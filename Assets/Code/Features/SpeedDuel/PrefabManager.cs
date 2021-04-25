using Zenject;
using UnityEngine;
using AssemblyCSharp.Assets.Code.Core.Models.Impl.SetCard;
using AssemblyCSharp.Assets.Code.Core.DataManager.Interface;

namespace AssemblyCSharp.Assets.Code.Features.SpeedDuel
{
    /// <summary>
    /// Used for initialising prefabs that can be reused during a duel.
    /// e.g. set cards, destruction particles, monster models, ...
    /// </summary>
    public class PrefabManager : MonoBehaviour
    {
        private const string SetCardResourceName = "SetCard";
        private const string ParticlesKey = "Particles";
        private const string SetCardKey = "SetCards";

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
            InstantiatePrefabs(SetCardKey, 8);
            InstantiatePrefabs(ParticlesKey, 8);
        }

        #endregion

        private void InstantiatePrefabs(string key, int amount)
        {
            if (key == SetCardKey)
            {
                for (int i = 0; i < amount; i++)
                {
                    var obj = _setCardFactory.Create(_dataManager.GetCardModel(SetCardResourceName)).transform.gameObject;
                    obj.transform.SetParent(transform);
                    obj.SetActive(false);
                    _dataManager.SaveGameObject(key, obj);
                }
                return;
            }
            else if (key == ParticlesKey)
            {
                for (int i = 0; i < amount; i++)
                {
                    var obj = _particleFactory.Create(_particles).transform.gameObject;
                    obj.transform.SetParent(transform);
                    obj.SetActive(false);
                    _dataManager.SaveGameObject(key, obj);
                }
                return;
            }

            //Pre-Instantiate models from deck:
            //Recommend sending over deck information (ie. which/how many models) in order to have them ready
            //when the duel starts. This would be for multiplayer, it's not really needed currently
        }
    }
}
