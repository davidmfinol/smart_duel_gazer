using Code.Features.SpeedDuel.PrefabManager.Prefabs.SetCard.Scripts;
using UnityEngine;

namespace Code.Core.General.Extensions
{
    public static class GameObjectExtensions
    {
        public static bool IsClone(this GameObject value)
        {
            return value.name.ToLowerInvariant().Contains("clone");
        }

        // TODO: give this function a better name
        public static bool IsIntendedGameObject(this GameObject value, int objectInstanceID, int targetInstanceID)
        {
            return objectInstanceID == targetInstanceID && value.activeSelf;
        }

        /// <summary>
        /// This function moves a model on top of a floating Set Card so that the model is not clipping through the
        /// Set Card model. Instead this moves the model on top of the set card.
        /// </summary>
        /// <param name="model">The model that needs to be moved</param>
        /// <param name="setCard">The card that the model should be placed on top of</param>
        public static void PlaceOnTopOfSetCard(this GameObject model, GameObject setCard)
        {
            var cardImage = setCard.GetComponentInChildren<CardFrontImage>();
            model.transform.position = cardImage.ImagePosition;
        }
    }
}
