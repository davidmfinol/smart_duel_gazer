using UnityEngine;
using Newtonsoft.Json;
using System.Collections.Generic;
using AssemblyCSharp.Assets.Code.Core.DataManager.Interface.Connection.Entities;
using AssemblyCSharp.Assets.Code.Core.General.Extensions;
using System.Linq;
using Dpoch.SocketIO;

// Followed a tutorial on YouTube by Alex Hicks on using Socket.IO
// URL: https://www.youtube.com/watch?v=J0udhTJwR88&ab_channel=AlexHicks
namespace Project.Networking
{
    public class NetworkClient : MonoBehaviour
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

        private SocketIO _socket;

        public void Start()
        {
            Init();
        }

        private void Init()
        {
            _instantiatedModels = new Dictionary<string, GameObject>();

            ConnectToServer();
            SetScreenRotationToAuto();
            LoadCardModels();
        }

        private void ConnectToServer()
        {
            if (!PlayerPrefs.HasKey(CONNECTION_INFO_KEY))
            {
                return;
            }

            var json = PlayerPrefs.GetString(CONNECTION_INFO_KEY);
            var connectionInfo = JsonConvert.DeserializeObject<ConnectionInfo>(json);

            var url = $"ws://{connectionInfo?.IpAddress}:{connectionInfo?.Port}/socket.io/?EIO=4&transport=websocket";
            var socket = new SocketIO(url);

            socket.OnOpen += () => Debug.Log("Socket open!");
            socket.OnConnectFailed += () => Debug.Log("Socket failed to connect!");
            socket.OnClose += () => Debug.Log("Socket closed!");
            socket.OnError += (err) => Debug.Log("Socket Error: " + err);
            socket.On(SUMMON_EVENT_NAME, OnSummonEventReceived);
            socket.On(REMOVE_CARD_EVENT, OnRemovecardEventReceived);

            socket.Connect();
        }

        private void SetScreenRotationToAuto()
        {
            Screen.orientation = ScreenOrientation.AutoRotation;
        }

        private void LoadCardModels()
        {
            _cardModels = Resources.LoadAll<GameObject>(RESOURCES_MONSTERS_FOLDER_NAME);
        }

        private void OnSummonEventReceived(SocketIOEvent e)
        {
            var data = e.Data[0];
            var yugiohCardId = data["yugiohCardId"].ToString().RemoveQuotes();
            var zoneName = data["zoneName"].ToString().RemoveQuotes();

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
            var data = e.Data[0];
            var zoneName = data["zoneName"].ToString().RemoveQuotes();

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
