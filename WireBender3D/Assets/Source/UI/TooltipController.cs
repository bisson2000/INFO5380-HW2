using UnityEngine;
using System.Collections;

public class TooltipController : MonoBehaviour
{
    public GameObject[] tooltips; // Assign all your tooltips to this array in the Inspector
    public float tooltipTime = 5f; // Time for tooltips to remain visible

    // Start is called before the first frame update
    void Start()
    {
        // Initially disable all tooltips when the game starts
        foreach (GameObject tooltip in tooltips)
        {
            tooltip.SetActive(false);
        }
    }

    public void ShowTooltips()
    {
        StopAllCoroutines(); // Stops all coroutines to prevent overlap if the button is pressed again
        // Activate all tooltips
        foreach (GameObject tooltip in tooltips)
        {
            tooltip.SetActive(true);
        }
        StartCoroutine(HideTooltipsAfterDelay());
    }

    private IEnumerator HideTooltipsAfterDelay()
    {
        // Wait for the specified tooltip time
        yield return new WaitForSeconds(tooltipTime);
        // Then, deactivate all tooltips
        foreach (GameObject tooltip in tooltips)
        {
            tooltip.SetActive(false);
        }
    }
}
