using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PulseEnergyUI : MonoBehaviour
{
    [SerializeField] private PulseEnergy pulseEnergy;
    [SerializeField] private Image fillImage;
    [SerializeField] private TMP_Text readyText;

    private void Update()
    {
        if (pulseEnergy == null)
            return;

        if (fillImage != null)
        {
            fillImage.fillAmount = pulseEnergy.CurrentEnergy;
        }

        if (readyText != null)
        {
            readyText.gameObject.SetActive(pulseEnergy.IsReady);
        }
    }
}