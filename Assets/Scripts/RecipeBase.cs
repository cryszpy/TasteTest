using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
[CreateAssetMenu(menuName = "ScriptableObjects/RecipeBase")]
public class RecipeBase : ScriptableObject
{

    public int recipeId;
    
    public List<IngredientBase> ingredients;
}
