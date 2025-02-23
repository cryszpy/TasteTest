using FishNet.Object;
using UnityEngine;

public class ApplianceCuttingBoard : ApplianceBase
{
    [Header("CUTTING BOARD")] // ----------------------------------------------------------------------------------

    public GameObject pivotPoint;

    [ServerRpc(RequireOwnership = false)]
    public override void ServerUseAppliance(PlayerApplianceRaycast playerRaycast) {

        if (playerRaycast.player.itemPickup.pickupInHand 
            && playerRaycast.player.itemPickup.pickupInHand.TryGetComponent<FoodBase>(out var food) 
            && !heldFood) {

            // Transformed ingredients, frameworks, and recipes cannot be further transformed
            if (food.type != FoodType.INGREDIENT) return;

            TransformedFood found = food.transformedVersions.Find(x => x.cookingStation == CookingStation.CUTTING_BOARD);

            // If the held ingredient can be transformed with this station—
            if (food.transformedVersions.Count > 0 && found != null) {

                // Assigns currently transforming ingredient
                heldFood = food;
                AssignHeldFood(this, food);

                // Reset player hand
                ResetHand(playerRaycast, this.gameObject);

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
    public override void ResetHand(PlayerApplianceRaycast playerRaycast, GameObject appliance) {

        // Set new parent and move food object
        if (appliance.TryGetComponent<ApplianceCuttingBoard>(out var board)) {
            playerRaycast.player.itemPickup.pickupInHand.transform.parent = board.pivotPoint.transform;
            playerRaycast.player.itemPickup.pickupInHand.transform.localPosition = Vector3.zero;
        } else {
            playerRaycast.player.itemPickup.pickupInHand.SetActive(false);
        }

        // Reset player pickups
        playerRaycast.player.itemPickup.selectedPickup = null;
        playerRaycast.player.itemPickup.pickupInHand = null;

        // Re-enables pickup radius
        if (!playerRaycast.player.pickupRadius.enabled) {
            playerRaycast.player.pickupRadius.enabled = true;
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