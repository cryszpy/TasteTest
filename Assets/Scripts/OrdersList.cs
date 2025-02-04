using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
[CreateAssetMenu(menuName = "ScriptableObjects/OrdersList")]
public class OrdersList : ScriptableObject
{

    [Tooltip("List of all unique Flavors in the game.")]
    public List<Flavor> flavors = new();

    [Tooltip("List of all unique Flavor Amounts in the game.")]
    public List<FlavorAmount> flavorAmounts = new();

    [Tooltip("List of all possible critic titles.")]
    public List<string> criticTitles = new();

    [Tooltip("List of all possible critic surnames.")]
    public List<string> criticSurnames = new();

    [Tooltip("List of all possible customer names.")]
    public List<string> customerNames = new();
}