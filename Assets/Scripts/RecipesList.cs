using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
[CreateAssetMenu(menuName = "ScriptableObjects/RecipesList")]
public class RecipesList : ScriptableObject
{
    [Tooltip("List of all recipes in the game.")]
    public List<FoodBase> recipes = new();
}