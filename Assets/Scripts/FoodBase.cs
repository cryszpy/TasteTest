using UnityEngine;

public enum FoodType {
    INGREDIENT, TRANSFORMED, RECIPE
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

[System.Serializable]
[CreateAssetMenu(menuName = "ScriptableObjects/FoodBase")]
public class FoodBase : ScriptableObject
{
    public FoodType type;
    
    public int foodId;

    public FlavorProfile flavorProfile;
    
    public string description;

    public float price;

    public FoodCombo ingredients;
}