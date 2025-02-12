using FishNet.Object;
using FishNet.Connection;
using UnityEngine;
using System.Collections.Generic;

public enum FoodType {
    INGREDIENT, TRANSFORMED, RECIPE, FRAMEWORK
}

[System.Serializable]
public struct FlavorProfile {
    
    [Range(0, 6)]
    public int sweetValue;

    [Range(0, 6)]
    public int spicyValue;

    [Range(0, 6)]
    public int savoryValue;

    [Range(0, 6)]
    public int saltyValue;

    [Range(0, 6)]
    public int sourValue;
}

public class FoodBase : NetworkBehaviour
{

    [Header("SCRIPT REFERENCES")]

    public GameObject spawnObject;

    [Header("STATS")]

    public FoodType type;
    
    public int foodId;

    public bool isRaw;

    public FlavorProfile flavorProfile;
    
    public string description;

    public float price;

    public List<FoodCombo> ingredients;
}