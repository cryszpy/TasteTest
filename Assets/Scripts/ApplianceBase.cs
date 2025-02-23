using System.Collections;
using FishNet;
using FishNet.Object;
using UnityEngine;

public class ApplianceBase : NetworkBehaviour
{
    
    [Header("SCRIPT REFERENCES")] // ----------------------------------------------------------------------------------------

    public FoodBase heldFood;

    public GameObject spawnPoint;

    public Animator animator;

    [Header("STATS")] // -----------------------------------------------------------------------------------------

    public float transformTime;

    [ServerRpc(RequireOwnership = false)]
    public virtual void ServerUseAppliance(PlayerApplianceRaycast playerRaycast) {

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

    [ServerRpc(RequireOwnership = false)]
    public virtual void ServerSpawnTransformedFood(PlayerApplianceRaycast playerRaycast, ApplianceBase appliance, TransformedFood foodToSpawn) {

        if (foodToSpawn != null) {
            GameObject spawnedFood = Instantiate(foodToSpawn.transformedIngredient.spawnObject, appliance.spawnPoint.transform.position, Quaternion.identity);
            InstanceFinder.ServerManager.Spawn(spawnedFood);

            // Despawn old ingredient
            ServerRemoveIngredient(playerRaycast, appliance.heldFood.gameObject);

            UseAppliance(playerRaycast, appliance.gameObject);
        } else {
            Debug.LogError("Could not find respective transformed version of ingredient!");
        }
    }

    public virtual IEnumerator WaitForTransform(PlayerApplianceRaycast playerRaycast, ApplianceBase appliance, TransformedFood food) {

        if (appliance.heldFood == null) yield break;

        // Starts animation
        ServerSetAnimation(appliance, true);

        // Wait for the specified time
        yield return new WaitForSeconds(appliance.transformTime);

        // Stops animation
        ServerSetAnimation(appliance, false);

        // Spawn transformed ingredient
        ServerSpawnTransformedFood(playerRaycast, this, food);
    }

    [ServerRpc(RequireOwnership = false)]
    public virtual void ServerSetAnimation(ApplianceBase appliance, bool value) {
        SetAnimation(appliance, value);
    }

    [ObserversRpc]
    public virtual void SetAnimation(ApplianceBase appliance, bool value) {
        appliance.animator.SetBool("InUse", value);
    }

    [ObserversRpc]
    public virtual void UseAppliance(PlayerApplianceRaycast playerRaycast, GameObject applianceObject = default) {

        if (applianceObject.TryGetComponent<ApplianceBase>(out var appliance)) {

            // Reset appliance
            appliance.heldFood = null;

        } else {
            Debug.LogError("Could not find ApplianceBase component on: " + applianceObject.name + "!");
        }
    }

    [ObserversRpc]
    public virtual void AssignHeldFood(ApplianceBase appliance, FoodBase target) {
        appliance.heldFood = target;
    }

    [ServerRpc(RequireOwnership = false)]
    public virtual void ServerRemoveIngredient(PlayerApplianceRaycast playerRaycast, GameObject target) {

        // Despawn object
        InstanceFinder.ServerManager.Despawn(target);
    }

    [ObserversRpc]
    public virtual void ResetHand(PlayerApplianceRaycast playerRaycast, GameObject appliance = default) {

        playerRaycast.player.itemPickup.pickupInHand.SetActive(false);

        // Reset player pickups
        playerRaycast.player.itemPickup.selectedPickup = null;
        playerRaycast.player.itemPickup.pickupInHand = null;

        // Re-enables pickup radius
        if (!playerRaycast.player.pickupRadius.enabled) {
            playerRaycast.player.pickupRadius.enabled = true;
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public virtual void ServerRetrieveItem(PlayerApplianceRaycast playerRaycast) {

        StopAllCoroutines();

        RetrieveItem(playerRaycast, this.gameObject);
    }

    [ObserversRpc]
    public virtual void RetrieveItem(PlayerApplianceRaycast playerRaycast, GameObject applianceObject = default) {

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
    public virtual void ServerLeftAppliance(PlayerApplianceRaycast playerRaycast) {
        LeftAppliance(playerRaycast);
    }

    [ObserversRpc]
    public virtual void LeftAppliance(PlayerApplianceRaycast playerRaycast) {
        return;
    }
}