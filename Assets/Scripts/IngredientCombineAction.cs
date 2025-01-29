using System;
using System.Linq;
using FishNet.Object;
using UnityEngine;

public class IngredientCombineAction : NetworkBehaviour
{

    private CombinationManager comboManager;

    public FoodPickup pickup;

    private void Start() {
        comboManager = GameObject.FindGameObjectWithTag("CombinationManager").GetComponent<CombinationManager>();
    }

    private void OnTriggerEnter(Collider collider) {

        if (collider.CompareTag("Pickup")) {

            if (collider.TryGetComponent<FoodPickup>(out var script)) {

                if (script.heldBy && pickup.heldBy) {
                    Debug.Log("Triggered combination");

                    // Run combination of ingredients
                    ServerTriggerCombo(comboManager, pickup, script, pickup.gameObject, collider.gameObject, pickup.heldBy.gameObject, script.heldBy.gameObject);
                }
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void ServerTriggerCombo(CombinationManager comboManager, FoodPickup ingredientOne, FoodPickup ingredientTwo, GameObject pickup1, GameObject pickup2, GameObject player1, GameObject player2) {
        TriggerCombo(comboManager, ingredientOne, ingredientTwo, pickup1, pickup2, player1, player2);
    }

    public void TriggerCombo(CombinationManager comboManager, FoodPickup ingredientOne, FoodPickup ingredientTwo, GameObject pickup1, GameObject pickup2, GameObject player1, GameObject player2) {

        FoodCombo existingPair = comboManager.heldComboList.Find(x => (x.ingredientOne == ingredientOne && x.ingredientTwo == ingredientTwo)
        || (x.ingredientOne == ingredientTwo && x.ingredientTwo == ingredientOne));

        if (existingPair == null) {
            Debug.Log("Didn't find pair!");
            FoodCombo newPair = new(ingredientOne, ingredientTwo, pickup1, pickup2);
            comboManager.heldComboList.Add(newPair);
            comboManager.ServerCombineIngredients(comboManager, ingredientOne, ingredientTwo, pickup1, pickup2, player1, player2);
        }
    }
}
