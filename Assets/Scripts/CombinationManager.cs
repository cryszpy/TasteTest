using System;
using System.Collections.Generic;
using FishNet;
using FishNet.Object;
using UnityEngine;

public class CombinationManager : NetworkBehaviour
{
    public RecipesList recipesList;

    public List<FoodCombo> heldComboList = new();

    [HideInInspector] public bool lockInUse = false;

    [ServerRpc(RequireOwnership = false)]
    public void ServerCombineIngredients(CombinationManager comboManager, FoodPickup ingredientOne, FoodPickup ingredientTwo, GameObject pickup1, GameObject pickup2, GameObject player1, GameObject player2) {
        Debug.Log("Called server combine ingredients!");

        // Gets the midpoint between the two combining ingredients
        Vector3 spawnPoint = pickup1.transform.position + ((pickup1.transform.position - pickup2.transform.position) / 2);

        // Finds the correct recipe to spawn
        FoodPair foundRecipe = comboManager.recipesList.recipes.Find(
            x => (x.pickup.baseDefinition.ingredients.ingredientOne.baseDefinition == ingredientOne.baseDefinition && x.pickup.baseDefinition.ingredients.ingredientTwo.baseDefinition == ingredientTwo.baseDefinition)
         || (x.pickup.baseDefinition.ingredients.ingredientOne.baseDefinition == ingredientTwo.baseDefinition && x.pickup.baseDefinition.ingredients.ingredientTwo.baseDefinition == ingredientOne.baseDefinition));

        InstanceFinder.ServerManager.Despawn(pickup1);
        InstanceFinder.ServerManager.Despawn(pickup2);

        // Spawns the recipe
        if (foundRecipe.foodObject != null) {

            ResetCombo(comboManager, ingredientOne, ingredientTwo, spawnPoint, player1, player2);

            GameObject spawnedRecipe = Instantiate(foundRecipe.foodObject, spawnPoint, Quaternion.identity);
            InstanceFinder.ServerManager.Spawn(spawnedRecipe);

            ServerUpdateNewRecipePosition(spawnedRecipe, spawnPoint);
        } else {
            Debug.LogWarning("Couldn't find recipe to spawn!");
        }
    }

    [ObserversRpc]
    public void ResetCombo(CombinationManager comboManager, FoodPickup ingredientOne, FoodPickup ingredientTwo, Vector3 spawnPoint, GameObject player1, GameObject player2) {
        Debug.Log("Combining ingredients");

        if (comboManager.heldComboList.Count > 0) {
            comboManager.heldComboList.RemoveAt(0); // Removes combo list item
        }

        // Reset player 1 held items
        if (player1.TryGetComponent<PlayerItemPickup>(out var script1)) {
            script1.pickupInHand = null;
            script1.selectedPickup = null;
        }

        if (player1.TryGetComponent<SphereCollider>(out var coll1)) {
            coll1.enabled = true;
        }

        // Reset player 2 held items
        if (player2.TryGetComponent<PlayerItemPickup>(out var script2)) {
            script2.pickupInHand = null;
            script2.selectedPickup = null;
        }

        if (player2.TryGetComponent<SphereCollider>(out var coll2)) {
            coll2.enabled = true;
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void ServerUpdateNewRecipePosition(GameObject recipeObject, Vector3 spawnPoint) {
        UpdateNewRecipePosition(recipeObject, spawnPoint);
    }

    [ObserversRpc]
    public void UpdateNewRecipePosition(GameObject recipeObject, Vector3 spawnPoint) {

        recipeObject.transform.position = new(recipeObject.transform.position.x, 1, recipeObject.transform.position.z);
        recipeObject.SetActive(true);

        Debug.Log("Spawned recipe: " + recipeObject.name);
    }
}
