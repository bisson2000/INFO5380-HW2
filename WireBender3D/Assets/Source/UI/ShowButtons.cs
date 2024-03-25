using UnityEngine;
using UnityEngine.UI;

public class ShowButtons : MonoBehaviour
{
    public Button buttonToClick;
    public Button buttonToShow1; // Change this to Button to add click listeners
    public Button buttonToShow2; // Change this to Button to add click listeners

    private void Start()
    {
        // Initially make the buttons invisible
        buttonToShow1.gameObject.SetActive(false);
        buttonToShow2.gameObject.SetActive(false);

        // Add a click listener to the main button to show the others
        buttonToClick.onClick.AddListener(() => {
            buttonToShow1.gameObject.SetActive(true);
            buttonToShow2.gameObject.SetActive(true);

            // Schedule the hiding of the buttons
            CancelInvoke(nameof(HideButtons)); // Cancel any previous scheduled hides
            Invoke(nameof(HideButtons), 3f); // Hide buttons after 3 seconds
        });

        // Add click listeners to the buttons that are shown to reset the hide timer when they are clicked
        buttonToShow1.onClick.AddListener(ButtonClicked);
        buttonToShow2.onClick.AddListener(ButtonClicked);
    }

    private void HideButtons()
    {
        buttonToShow1.gameObject.SetActive(false);
        buttonToShow2.gameObject.SetActive(false);
    }

    private void ButtonClicked()
    {
        // If any button is clicked, reset the timer to hide them
        CancelInvoke(nameof(HideButtons)); // Cancel the scheduled hide
        Invoke(nameof(HideButtons), 3f); // Schedule a new hide
    }
}
