using System;
using System.Linq;
using FishNet.Object;
using UnityEngine;

public class IngredientCombineAction : NetworkBehaviour
{

    private CombinationManager comboManager;

    public FoodBase foodBase;

    public Pickup pickup;

    private void Start() {
        comboManager = GameObject.FindGameObjectWithTag("CombinationManager").GetComponent<CombinationManager>();
    }

    private void OnTriggerEnter(Collider collider) {

        if (collider.CompareTag("Pickup")) {

            if (collider.TryGetComponent<FoodBase>(out var foodScript) && collider.TryGetComponent<Pickup>(out var pickupScript)) {

                if (pickupScript.heldBy && pickup.heldBy) {
                    Debug.Log("Triggered combination");

                    // Run combination of ingredients
                    ServerTriggerCombo(comboManager, foodBase, foodScript, pickup.gameObject, collider.gameObject, pickup.heldBy.gameObject, pickupScript.heldBy.gameObject);
                }
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void ServerTriggerCombo(CombinationManager comboManager, FoodBase ingredientOne, FoodBase ingredientTwo, GameObject pickup1, GameObject pickup2, GameObject player1, GameObject player2) {
        TriggerCombo(comboManager, ingredientOne, ingredientTwo, pickup1, pickup2, player1, player2);
    }

    public void TriggerCombo(CombinationManager comboManager, FoodBase ingredientOne, FoodBase ingredientTwo, GameObject pickup1, GameObject pickup2, GameObject player1, GameObject player2) {

        FoodCombo existingPair = comboManager.heldComboList.Find(x => (x.ingredient1 == ingredientOne && x.ingredient2 == ingredientTwo)
        || (x.ingredient1 == ingredientTwo && x.ingredient2 == ingredientOne));

        if (existingPair == null) {
            Debug.Log("Didn't find pair!");
            FoodCombo newPair = new(ingredientOne, ingredientTwo, pickup1, pickup2);
            comboManager.heldComboList.Add(newPair);
            comboManager.ServerCombineIngredients(comboManager, ingredientOne, ingredientTwo, pickup1, pickup2, player1, player2);
        }
    }
}