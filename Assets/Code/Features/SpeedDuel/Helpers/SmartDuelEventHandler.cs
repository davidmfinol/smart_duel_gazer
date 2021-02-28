using Dpoch.SocketIO;
using System.Collections.Generic;
using System.Linq;
using AssemblyCSharp.Assets.Code.Core.General.Extensions;
using UnityEngine;

namespace AssemblyCSharp.Assets.Code.Features.SpeedDuel.Helpers
{
    public class SmartDuelEventHandler
    {
        private const string RESOURCES_MONSTERS_FOLDER_NAME = "Monsters";

        private GameObject[] _cardModels;
        private Dictionary<string, GameObject> _instantiatedModels;

        public SmartDuelEventHandler()
        {
            LoadCardModels();
        }

        private void LoadCardModels()
        {
            _cardModels = Resources.LoadAll<GameObject>(RESOURCES_MONSTERS_FOLDER_NAME);
        }

        public void OnSummonEventReceived(SocketIOEvent e)
        {
            var data = e.Data[0];
            var yugiohCardId = data["yugiohCardId"].ToString().RemoveQuotes();
            var zoneName = data["zoneName"].ToString().RemoveQuotes();

            //var arTapToPlaceObject = _interaction.GetComponent<ARTapToPlaceObject>();
            //var speedDuelField = arTapToPlaceObject.PlacedObject;

            //var zone = speedDuelField.transform.Find(zoneName);
            var zone = "lol";
            if (zone == null)
            {
                return;
            }

            var cardModel = _cardModels.SingleOrDefault(cm => cm.name == yugiohCardId);
            if (cardModel == null)
            {
                return;
            }

            //var instantiatedModel = Instantiate(cardModel, zone.transform.position, zone.transform.rotation);

            //var animator = instantiatedModel.GetComponentInChildren<Animator>();
            //if (animator != null)
            //{
            //    animator.SetTrigger(SummoningAnimatorId);
            //}

            //_instantiatedModels.Add(zoneName, instantiatedModel);
        }

        public void OnRemovecardEventReceived(SocketIOEvent e)
        {
            var data = e.Data[0];
            var zoneName = data["zoneName"].ToString().RemoveQuotes();

            //var modelExists = _instantiatedModels.TryGetValue(zoneName, out var model);
            //if (!modelExists)
            //{
            //    return;
            //}

            //Destroy(model);
            //_instantiatedModels.Remove(zoneName);
        }
    }
}
