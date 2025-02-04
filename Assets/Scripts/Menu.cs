using UnityEngine;

public class Menu : MonoBehaviour
{

    public GameObject mainMenu;

    public GameObject joinMenu;

    public GameObject joinCodeDisplay;

    public GameObject loadingScreen;

    public GameObject startButton;

    // Called when the join butten is pressed from OnClick() event
    public void JoinButtonPressed() {

        // Shows the join menu UI
        joinMenu.SetActive(true);
    }

    // Called from OnClick() event when BACK button in join menu is pressed
    public void CloseJoinMenu() {

        // Hides the join menu
        joinMenu.SetActive(false);
    }

    public void EnableLoadingScreen() {

        Debug.Log("Loading...");

        mainMenu.SetActive(false);

        joinMenu.SetActive(false);

        loadingScreen.SetActive(true);
    }

    public void DisableLoadingScreen() {
        Debug.Log("Done loading!");

        loadingScreen.SetActive(false);
    }
}
