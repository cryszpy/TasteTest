using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
[CreateAssetMenu(menuName = "ScriptableObjects/FoodList")]
public class FoodList : ScriptableObject
{
    [Tooltip("List of all base ingredients in the game.")]
    public List<FoodBase> ingredients = new();

    [Tooltip("List of all transformed ingredients in the game.")]
    public List<FoodBase> transformed = new();

    [Tooltip("List of all recipes in the game.")]
    public List<FoodBase> recipes = new();
}