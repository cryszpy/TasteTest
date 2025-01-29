using UnityEngine;

[System.Serializable]
public class FoodCombo
{
    public FoodPickup ingredientOne;

    public FoodPickup ingredientTwo;

    public GameObject pickup1;
    public GameObject pickup2;

    public FoodCombo (FoodPickup item1, FoodPickup item2, GameObject obj1, GameObject obj2) {
        ingredientOne = item1;
        ingredientTwo = item2;
        pickup1 = obj1;
        pickup2 = obj2;
    }
}
