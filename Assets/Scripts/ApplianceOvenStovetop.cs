using FishNet.Object;
using UnityEngine;

public class ApplianceOvenStovetop : ApplianceBase
{
    [ServerRpc(RequireOwnership = false)]
    public override void ServerUseAppliance(PlayerApplianceRaycast playerRaycast) {

        if (playerRaycast.player.itemPickup.pickupInHand 
            && playerRaycast.player.itemPickup.pickupInHand.TryGetComponent<FoodBase>(out var food) 
            && !heldFood) {

            // Transformed ingredients, frameworks, and recipes cannot be further transformed
            if (food.type != FoodType.INGREDIENT) return;

            TransformedFood found = food.transformedVersions.Find(x => x.cookingStation == CookingStation.STOVETOP_OVEN);

            // If the held ingredient can be transformed with this station—
            if (food.transformedVersions.Count > 0 && found != null) {

                // Assigns currently transforming ingredient
                heldFood = food;
                AssignHeldFood(this, food);

                // Reset player hand
                ResetHand(playerRaycast);

                // Start transforming
                StartCoroutine(WaitForTransform(playerRaycast, this, found));

            } else {
                Debug.LogWarning("Held item is not transformable at this station!");
            }

        } else {
            Debug.LogWarning("Held item is not transformable or this cooking station is already in use!");
            return;
        }
    }

    [ObserversRpc]
    public override void UseAppliance(PlayerApplianceRaycast playerRaycast, GameObject applianceObject = default) {

        if (applianceObject.TryGetComponent<ApplianceBase>(out var appliance)) {

            // Reset appliance
            appliance.heldFood = null;

        } else {
            Debug.LogError("Could not find ApplianceBase component on: " + applianceObject.name + "!");
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public override void ServerRetrieveItem(PlayerApplianceRaycast playerRaycast) {

        StopAllCoroutines();

        RetrieveItem(playerRaycast, this.gameObject);
    }

    [ObserversRpc]
    public override void RetrieveItem(PlayerApplianceRaycast playerRaycast, GameObject applianceObject = default) {

        if (applianceObject && applianceObject.TryGetComponent<ApplianceBase>(out var appliance)) {

            playerRaycast.player.itemPickup.ServerPickupItem(appliance.heldFood.gameObject, playerRaycast.player.itemPickup.itemPoint, 
                playerRaycast.player.itemPickup, playerRaycast.player.gameObject);

            // Reset appliance
            appliance.heldFood = null;
            appliance.ServerSetAnimation(appliance, false);
            
        } else {
            Debug.LogError("Could not find ApplianceOvenStovetop component on this appliance!");
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public override void ServerLeftAppliance(PlayerApplianceRaycast playerRaycast) {
        LeftAppliance(playerRaycast);
    }

    [ObserversRpc]
    public override void LeftAppliance(PlayerApplianceRaycast playerRaycast) {
        return;
    }
}