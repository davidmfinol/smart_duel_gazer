using UnityEngine;

namespace Code.Features.SpeedDuel.PrefabManager.Prefabs.SetCard.Scripts
{
    public class CardFrontImage : MonoBehaviour
    {
        public Vector3 ImagePosition { get => GetCurrentPosition(); }

        private Vector3 GetCurrentPosition()
        {
            return transform.position;
        }
    }
}
