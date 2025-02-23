using FishNet.Object;
using FishNet.Connection;
using UnityEngine;
using System.Collections.Generic;

public enum CookingStation {
    NONE, CUTTING_BOARD, STOVETOP_OVEN
}

public enum FoodType {
    INGREDIENT, TRANSFORMED, RECIPE, FRAMEWORK
}

[System.Serializable]
public class FlavorProfile {

    public Flavor flavor;

    [Range(0, 6)]
    public int value;
}

[System.Serializable]
public class TransformedFood {

    public FoodBase transformedIngredient;

    public CookingStation cookingStation;
}

public class FoodBase : NetworkBehaviour
{

    [Header("SCRIPT REFERENCES")]

    public GameObject spawnObject;

    public List<TransformedFood> transformedVersions = new();

    [Header("STATS")]

    public FoodType type;
    
    public int foodId;

    public bool isRaw;

    public List<FlavorProfile> flavorProfile;
    
    public string description;

    public float price;

    public List<FoodCombo> ingredients;
}