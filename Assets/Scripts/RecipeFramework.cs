using System.Collections.Generic;
using FishNet.Object;
using UnityEngine;

public class RecipeFramework : NetworkBehaviour
{

    public FoodType type;
    
    public List<FoodBase> undiscoveredRecipes = new();
}