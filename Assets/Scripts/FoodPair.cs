using FishNet.Object;
using UnityEngine;

[System.Serializable]
public struct FoodPair
{
    public FoodPickup pickup;

    public GameObject foodObject;

    public FoodPair(FoodPickup fp, GameObject fo) {
        pickup = fp;
        foodObject = fo;
    }
}
