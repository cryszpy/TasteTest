using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct FoodPair {

    public FoodPickup pickup;

    public GameObject foodObject;
}

[System.Serializable]
[CreateAssetMenu(menuName = "ScriptableObjects/RecipesList")]
public class RecipesList : ScriptableObject
{
    [Tooltip("List of all recipes in the game.")]
    public List<FoodPair> recipes = new();
}