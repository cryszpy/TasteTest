using System.Collections;
using System.Collections.Generic;
using FishNet;
using FishNet.Object;
using UnityEngine;

public class Counter : NetworkBehaviour
{
    public ParticleSystem particles;

    public GameObject spawnPoint;

    public int slotNumber;

    public GameObject heldItem;

    [ServerRpc(RequireOwnership = false)]
    public void ServerResetHand(PlayerOrderRadius orderRadius) {
        ResetHand(orderRadius);
    }

    [ObserversRpc]
    public void ResetHand(PlayerOrderRadius orderRadius) {

        // Remove submitted item from player hand
        orderRadius.player.itemPickup.pickupInHand = null;
        orderRadius.player.itemPickup.selectedPickup = null;

        // Re-enables player pickup collider
        if (orderRadius.player.TryGetComponent<SphereCollider>(out var coll)) {
            coll.enabled = true;
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void ServerUnlockRecipe(PlayerOrderRadius orderRadius, Counter counter) {
        StartCoroutine(DiscoverAnimation(counter));
    }

    private IEnumerator DiscoverAnimation(Counter counter) {
        Debug.Log("ANIMAITON");

        // Play particle animation OBSERVERS

        yield return new WaitForSeconds(0.5f);

        // Stop particle animation OBSERVERS

        yield return new WaitForSeconds(1.5f);

        // Delete item and spawn newly discovered recipes

        List<GameObject> spawnedRecipes = new();

        if (counter.heldItem.TryGetComponent<RecipeFramework>(out var script)) {

            foreach (var recipe in script.undiscoveredRecipes) {
                
                GameObject spawnedRecipe = Instantiate(recipe.spawnObject, counter.spawnPoint.transform);
                InstanceFinder.ServerManager.Spawn(spawnedRecipe);

                ServerUpdatePosition(spawnedRecipe);

                spawnedRecipes.Add(spawnedRecipe);
            }

            if (base.IsHostInitialized) {

                // Delete recipe framework
                InstanceFinder.ServerManager.Despawn(counter.heldItem);
            }

            // Wait
            yield return new WaitForSeconds(1.5f);

            // Delete spawned discovered recipes

            if (base.IsHostInitialized) {
                foreach (var obj in spawnedRecipes) {
                    InstanceFinder.ServerManager.Despawn(obj);
                }
            }
            
            spawnedRecipes.Clear();

        } else {
            Debug.LogError("Could not get RecipeFramework component on: " + counter.heldItem.name + "!");
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void ServerUpdatePosition(GameObject obj) {
        UpdatePosition(obj);
    }

    [ObserversRpc]
    private void UpdatePosition(GameObject obj) {
        obj.transform.localPosition = Vector3.zero;
    }
}