using UnityEngine;

public class Menu : MonoBehaviour
{

    public GameObject mainMenu;

    public GameObject joinMenu;

    public GameObject joinCodeDisplay;

    public GameObject loadingScreen;

    public GameObject startButton;

    public GameObject pauseScreen;

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

    public void EnableStartMenu() {

        mainMenu.SetActive(true);

        joinMenu.SetActive(true);

        loadingScreen.SetActive(false);
    }

    // Toggles pause menu
    public void TogglePauseMenu() {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // UNPAUSE
        if (pauseScreen.activeInHierarchy) {

            // Hides pause menu
            pauseScreen.SetActive(false);
        } 
        // PAUSE
        else {

            // Shows pause menu
            pauseScreen.SetActive(true);
        }
    }

    public void QuitButton() {
        Debug.Log("Quit application!");
        Application.Quit();
    }
}
