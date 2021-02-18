using UnityEngine;
using SocketIO;
using Project.Extensions;
using System.Linq;
using Newtonsoft.Json;

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

        private const string CONNECTION_URL = "ws://{0}:{1}/socket.io/?EIO=3&transport=websocket";
        private const string CONNECTION_INFO_KEY = "connectionInfo";

        [SerializeField]
        private GameObject _interaction;

        private GameObject[] _cardModels;

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
            On(SUMMON_EVENT_NAME, (E) =>
            {
                string yugiohCardId = E.data["yugiohCardId"].ToString().RemoveQuotes();
                string zoneName = E.data["zoneName"].ToString().RemoveQuotes();

                var arTapToPlaceObject = _interaction.GetComponent<ARTapToPlaceObject>();
                var speedDuelField = arTapToPlaceObject.PlacedObject;
                var zone = speedDuelField.transform.Find(zoneName);

                var cardModel = _cardModels.SingleOrDefault(cm => cm.name == yugiohCardId);

                if (cardModel != null && zone != null)
                {
                    var instantiatedModel = Instantiate(cardModel, zone.transform.position, zone.transform.rotation);

                    var animator = instantiatedModel.GetComponentInChildren<Animator>();
                    if (animator != null)
                    {
                        animator.SetTrigger(SummoningAnimatorId);
                    }
                }

                Debug.Log($"Card played with ID: {yugiohCardId}");
            });
        }
    }
}
