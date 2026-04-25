using UnityEngine;
using UnityEngine.UI;

public class BossHealthBar : MonoBehaviour
{
    [SerializeField] private Image fillImage;

    public void UpdateHealth(int current, int max)
    {
        fillImage.fillAmount = (float)current / max;
    }
}