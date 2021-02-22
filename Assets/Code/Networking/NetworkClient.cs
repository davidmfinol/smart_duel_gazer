using UnityEngine;
using SocketIO;
using Newtonsoft.Json;
using System.Collections.Generic;
using AssemblyCSharp.Assets.Code.Core.DataManager.Interface.Connection.Entities;
using AssemblyCSharp.Assets.Code.Core.General.Extensions;
using System.Linq;

// Followed a tutorial on YouTube by Alex Hicks on using Socket.IO
// URL: https://www.youtube.com/watch?v=J0udhTJwR88&ab_channel=AlexHicks
namespace Project.Networking
{
    public class NetworkClient : SocketIOComponent
    {
        private static readonly int SummoningAnimatorId = Animator.StringToHash(SUMMONING_TRIGGER_NAME);

        private const string RESOURCES_MONSTERS_FOLDER_NAME = "Monsters";
        private const string SUMMONING_TRIGGER_NAME = "SummoningTrigger";

        private const string SUMMON_EVENT_NAME = "summonEvent";
        private const string REMOVE_CARD_EVENT = "removeCardEvent";

        private const string CONNECTION_URL = "ws://{0}:{1}/socket.io/?EIO=3&transport=websocket";
        private const string CONNECTION_INFO_KEY = "connectionInfo";

        [SerializeField]
        private GameObject _interaction;

        private GameObject[] _cardModels;

        private Dictionary<string, GameObject> _instantiatedModels;

        public override void Start()
        {
            base.Start();

            Init();
        }

        public override void Update()
        {
            base.Update();
        }

        private void Init()
        {
            _instantiatedModels = new Dictionary<string, GameObject>();

            ConnectToServer();
            SetScreenRotationToAuto();
            LoadCardModels();
            SetupEvents();
        }

        private void ConnectToServer()
        {
            if (!PlayerPrefs.HasKey(CONNECTION_INFO_KEY))
            {
                return;
            }

            var json = PlayerPrefs.GetString(CONNECTION_INFO_KEY);
            var connectionInfo = JsonConvert.DeserializeObject<ConnectionInfo>(json);

            url = string.Format(CONNECTION_URL, connectionInfo?.IpAddress, connectionInfo?.Port);

            Connect();
        }

        private void SetScreenRotationToAuto()
        {
            Screen.orientation = ScreenOrientation.AutoRotation;
        }

        private void LoadCardModels()
        {
            _cardModels = Resources.LoadAll<GameObject>(RESOURCES_MONSTERS_FOLDER_NAME);
        }

        private void SetupEvents()
        {
            On(SUMMON_EVENT_NAME, OnSummonEventReceived);
            On(REMOVE_CARD_EVENT, OnRemovecardEventReceived);
        }

        private void OnSummonEventReceived(SocketIOEvent e)
        {
            var yugiohCardId = e.data["yugiohCardId"].ToString().RemoveQuotes();
            var zoneName = e.data["zoneName"].ToString().RemoveQuotes();

            var arTapToPlaceObject = _interaction.GetComponent<ARTapToPlaceObject>();
            var speedDuelField = arTapToPlaceObject.PlacedObject;

            var zone = speedDuelField.transform.Find(zoneName);
            if (zone == null)
            {
                return;
            }

            var cardModel = _cardModels.SingleOrDefault(cm => cm.name == yugiohCardId);
            if (cardModel == null)
            {
                return;
            }

            var instantiatedModel = Instantiate(cardModel, zone.transform.position, zone.transform.rotation);

            var animator = instantiatedModel.GetComponentInChildren<Animator>();
            if (animator != null)
            {
                animator.SetTrigger(SummoningAnimatorId);
            }

            _instantiatedModels.Add(zoneName, instantiatedModel);
        }

        private void OnRemovecardEventReceived(SocketIOEvent e)
        {
            var zoneName = e.data["zoneName"].ToString().RemoveQuotes();

            var modelExists = _instantiatedModels.TryGetValue(zoneName, out var model);
            if (!modelExists)
            {
                return;
            }

            Destroy(model);
            _instantiatedModels.Remove(zoneName);
        }
    }
}
