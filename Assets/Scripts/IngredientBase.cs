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
[CreateAssetMenu(menuName = "ScriptableObjects/IngredientBase")]
public class IngredientBase : ScriptableObject
{
    public int ingredientId;

    public FlavorProfile flavorProfile;
    
    public string description;

    public float price;
}
