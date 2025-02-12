using System;
using System.Collections.Generic;
using FishNet;
using FishNet.Object;
using UnityEngine;

public class CombinationManager : NetworkBehaviour
{

    public GameObject recipeFramework;

    public RecipesList recipesList;

    public List<FoodCombo> heldComboList = new();

    [ServerRpc(RequireOwnership = false)]
    public void ServerCombineIngredients(CombinationManager comboManager, FoodBase ingredientOne, FoodBase ingredientTwo, GameObject pickup1, GameObject pickup2, GameObject player1, GameObject player2) {
        Debug.Log("Called server combine ingredients!");

        // Gets the midpoint between the two combining ingredients
        Vector3 spawnPoint = pickup1.transform.position + ((pickup1.transform.position - pickup2.transform.position) / 2);

        // Finds the correct recipe to spawn
        List<FoodBase> foundRecipes = comboManager.recipesList.recipes.FindAll(x => x.ingredients.Contains(x.ingredients.Find(y => y.ingredient1.foodId == ingredientOne.foodId && y.ingredient2.foodId == ingredientTwo.foodId)) 
            || x.ingredients.Contains(x.ingredients.Find(y => y.ingredient1.foodId == ingredientTwo.foodId && y.ingredient2.foodId == ingredientOne.foodId)));
        
        InstanceFinder.ServerManager.Despawn(pickup1);
        InstanceFinder.ServerManager.Despawn(pickup2);

        // Spawns found recipes
        if (foundRecipes.Count > 0) {

            ResetCombo(comboManager, player1, player2);

            // For every recipe made with these ingredients—
            foreach (FoodBase recipe in foundRecipes) {

                // Spawn the recipe object if already discovered
                if (OrderManager.DiscoveredRecipes.Contains(recipe)) {

                    // Spawn the recipe on the server
                    GameObject discoveredRecipe = Instantiate(recipe.spawnObject, spawnPoint, Quaternion.identity);
                    InstanceFinder.ServerManager.Spawn(discoveredRecipe);

                    // Update its position
                    ServerUpdateNewRecipePosition(discoveredRecipe, spawnPoint);
                }
                // If the recipe has NOT been discovered, spawn a framework
                else {

                    // Spawn the framework on the server
                    GameObject framework = Instantiate(comboManager.recipeFramework, spawnPoint, Quaternion.identity);
                    InstanceFinder.ServerManager.Spawn(framework);

                    // Assign recipes to the framework
                    ServerAssignRecipes(framework, recipe);

                    // Update its position
                    ServerUpdateNewRecipePosition(framework, spawnPoint);
                }
            }

        } else {
            Debug.LogWarning("Couldn't find any recipes to spawn!");
        }
    }

    [ObserversRpc]
    public void ResetCombo(CombinationManager comboManager, GameObject player1, GameObject player2) {
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
        UpdateNewRecipePosition(recipeObject);
    }

    [ObserversRpc]
    public void UpdateNewRecipePosition(GameObject recipeObject) {

        recipeObject.transform.position = new(recipeObject.transform.position.x, 1, recipeObject.transform.position.z);
        recipeObject.SetActive(true);

        Debug.Log("Spawned recipe: " + recipeObject.name);
    }

    [ServerRpc(RequireOwnership = false)]
    public void ServerAssignRecipes(GameObject framework, FoodBase recipeToAdd) {
        AssignRecipes(framework, recipeToAdd);
    }

    [ObserversRpc]
    public void AssignRecipes(GameObject framework, FoodBase recipeToAdd) {

        if (framework.TryGetComponent<RecipeFramework>(out var script)) {
                        
            if (!script.undiscoveredRecipes.Contains(recipeToAdd)) {
                script.undiscoveredRecipes.Add(recipeToAdd);
            }
        } else {
            Debug.LogError("Could not find RecipeFramework on: " + framework.name + " object!");
        }
    }
}