using UnityEngine;

[System.Serializable]
public class FoodCombo
{
    public FoodBase ingredient1;
    public GameObject pickup1;

    public FoodBase ingredient2;
    public GameObject pickup2;
    
    public FoodCombo (FoodBase item1, FoodBase item2, GameObject obj1, GameObject obj2) {
        ingredient1 = item1;
        ingredient2 = item2;
        pickup1 = obj1;
        pickup2 = obj2;
    }
}