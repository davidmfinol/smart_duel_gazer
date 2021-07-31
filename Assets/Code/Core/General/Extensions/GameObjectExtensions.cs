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

        
        /// <summary>
        /// Returns the instance ID of the GameObject that holds the 'ModelCompenentsManager' Script. This Instance
        /// ID is used to activate the model's animator.
        /// </summary>
        /// <param name="model"></param>
        /// <returns>int InstanceID</returns>
        public static int GetInstanceIDForModel(this GameObject model)
        {
            return model.transform.GetChild(0).GetInstanceID();
        }

        /// <summary>
        /// Returns the Instance ID of the GameObject that holds the 'SetCard' script. This Instance ID is used to
        /// activate the setCard's animator.
        /// </summary>
        /// <param name="setCard"></param>
        /// <returns>int InstanceID</returns>
        public static int GetInstanceIDForSetCard(this GameObject setCard)
        {
            return setCard.GetInstanceID();
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
