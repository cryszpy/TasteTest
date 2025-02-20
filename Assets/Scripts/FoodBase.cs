using FishNet.Object;
using FishNet.Connection;
using UnityEngine;
using System.Collections.Generic;

public enum FoodType {
    INGREDIENT, TRANSFORMED, RECIPE, FRAMEWORK
}

[System.Serializable]
public class FlavorProfile {

    public Flavor flavor;

    [Range(0, 6)]
    public int value;
}

public class FoodBase : NetworkBehaviour
{

    [Header("SCRIPT REFERENCES")]

    public GameObject spawnObject;

    [Header("STATS")]

    public FoodType type;
    
    public int foodId;

    public bool isRaw;

    public List<FlavorProfile> flavorProfile;
    
    public string description;

    public float price;

    public List<FoodCombo> ingredients;
}