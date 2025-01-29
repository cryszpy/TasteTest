using UnityEngine;

[System.Serializable]
public struct FlavorProfile {
    
    [Range(0, 3)]
    public int sweetValue;

    [Range(0, 3)]
    public int spicyValue;

    [Range(0, 3)]
    public int savoryValue;

    [Range(0, 3)]
    public int saltyValue;

    [Range(0, 3)]
    public int sourValue;
}

[System.Serializable]
[CreateAssetMenu(menuName = "ScriptableObjects/FoodBase")]
public class FoodBase : ScriptableObject
{
    public int foodId;

    public FlavorProfile flavorProfile;
    
    public string description;

    public float price;

    public FoodCombo ingredients;
}