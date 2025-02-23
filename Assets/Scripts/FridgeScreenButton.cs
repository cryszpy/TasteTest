using FishNet.Object;
using UnityEngine;

public class FridgeScreenButton : MonoBehaviour
{
    private ApplianceFridge fridge;

    public int ingredientId;

    public void Awake()
    {
        fridge = GameObject.FindGameObjectWithTag("Fridge").GetComponentInChildren<ApplianceFridge>();
    }

    public void LookUp() {

        GameObject target = fridge.recipesList.ingredients.Find(x => x.foodId == ingredientId).spawnObject;

        if (target != null) {
            fridge.ServerAddToCart(fridge, target);
        } else {
            Debug.LogError("Could not find ingredient: " + ingredientId + " to add to cart!");
        }
    }
}