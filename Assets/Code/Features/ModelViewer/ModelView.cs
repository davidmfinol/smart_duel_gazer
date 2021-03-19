using System.Collections;
using System.Collections.Generic;
using AssemblyCSharp.Assets.Code.Core.DataManager.Interface;
using AssemblyCSharp.Assets.Code.Core.Screen.Interface;
using AssemblyCSharp.Assets.Code.Core.SmartDuelServer.Interface;
using AssemblyCSharp.Assets.Code.Core.SmartDuelServer.Interface.Entities;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using Zenject;

namespace AssemblyCSharp.Assets.Code.Features.ModelViewer
{
    public class ModelView : MonoBehaviour, ISmartDuelEventListener
    {
        private static readonly int SummoningAnimatorId = Animator.StringToHash("SummoningTrigger");

        [SerializeField]
        private GameObject _objectToPlace;
        [SerializeField]
        private GameObject _particles;

        private ISmartDuelServer _smartDuelServer;
        private IDataManager _dataManager;

        #region Properties

        [SerializeField]
        private GameObject SpeedDuelField;

        private Dictionary<string, GameObject> InstantiatedModels { get; } = new Dictionary<string, GameObject>();

        #endregion

        #region Constructors

        [Inject]
        public void Construct(
            ISmartDuelServer smartDuelServer,
            IDataManager dataManager,
            IScreenService screenService)
        {
            _smartDuelServer = smartDuelServer;
            _dataManager = dataManager;

            screenService.UseLandscapeOrientation();
            ConnectToServer();
        }

        #endregion

        #region Lifecycle

        private void OnDestroy()
        {
            _smartDuelServer?.Dispose();
        }

        #endregion

        #region Smart duel events

        private void ConnectToServer()
        {
            _smartDuelServer.Connect(this);
        }

        public void onSmartDuelEventReceived(SmartDuelEvent smartDuelEvent)
        {
            if (smartDuelEvent is SummonEvent summonEvent)
            {
                OnSummonEventReceived(summonEvent);
            }
            else if (smartDuelEvent is RemoveCardEvent removeCardEvent)
            {
                OnRemovecardEventReceived(removeCardEvent);
            }
        }

        private void OnSummonEventReceived(SummonEvent summonEvent)
        {
            var zone = SpeedDuelField.transform.Find("Playmat/" + summonEvent.ZoneName);
            if (zone == null)
            {
                return;
            }

            var cardModel = _dataManager.GetCardModel(summonEvent.CardId);
            if (cardModel == null)
            {
                return;
            }

            var instantiatedModel = Instantiate(cardModel, zone.transform.position, zone.transform.rotation, SpeedDuelField.transform);

            var animator = instantiatedModel.GetComponentInChildren<Animator>();
            if (animator != null)
            {
                animator.SetTrigger(SummoningAnimatorId);
            }

            InstantiatedModels.Add(summonEvent.ZoneName, instantiatedModel);
        }

        private void OnRemovecardEventReceived(RemoveCardEvent removeCardEvent)
        {
            var modelExists = InstantiatedModels.TryGetValue(removeCardEvent.ZoneName, out var model);
            if (!modelExists)
            {
                return;
            }

            var characterMesh = model.GetComponentsInChildren<SkinnedMeshRenderer>();
            foreach (SkinnedMeshRenderer item in characterMesh)
            {
                item.enabled = false;
            }

            var meshToDestroy = _particles.GetComponent<ISetMeshCharacter>();
            meshToDestroy.GetCharacterMesh(characterMesh[0]);
            var destructionParticles = Instantiate(_particles, SpeedDuelField.transform);

            StartCoroutine(DestroyMonster(model, destructionParticles));
            InstantiatedModels.Remove(removeCardEvent.ZoneName);
        }

        private IEnumerator DestroyMonster(GameObject model, GameObject particles)
        {
            yield return new WaitForSeconds(10);
            Destroy(model);
            Destroy(particles);
        }

        #endregion
    }
}