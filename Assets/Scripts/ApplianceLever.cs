using System.Linq;
using FishNet;
using FishNet.Object;
using UnityEngine;

public class ApplianceLever : ApplianceBase
{
    [SerializeField] private ApplianceFridge fridge;

    [ServerRpc(RequireOwnership = false)]
    public override void ServerUseAppliance(PlayerApplianceRaycast playerRaycast) {

        // Spawn queued food
        ServerSpawnCartFood(fridge, this);
    }

    public virtual void ServerSpawnCartFood(ApplianceFridge _fridge, ApplianceBase appliance) {

        // If there is food in the cart to spawn—
        if (_fridge.cartQueue.Count > 0) {

            GameObject foodToSpawn = _fridge.cartQueue.First();

            Vector3 rotation = new(0, 0, -5);

            GameObject spawnedFood = Instantiate(foodToSpawn, appliance.spawnPoint.transform.position, Quaternion.identity);
            InstanceFinder.ServerManager.Spawn(spawnedFood);

            UpdateRotation(spawnedFood, rotation);

            _fridge.ServerRemoveFromCart(_fridge);
        } else {
            Debug.LogWarning("No ingredients in cart to spawn!");
        }

        ToggleLever();
    }

    [ObserversRpc]
    public virtual void UpdateRotation(GameObject thing, Vector3 target) {

        thing.transform.localEulerAngles = new(target.x, target.y, target.z);
    }

    [ServerRpc(RequireOwnership = false)]
    public override void ServerRetrieveItem(PlayerApplianceRaycast playerRaycast) {

        StopAllCoroutines();

        ToggleLever();
    }

    [ObserversRpc]
    public virtual void ToggleLever() {

        animator.SetTrigger("Pulled");
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