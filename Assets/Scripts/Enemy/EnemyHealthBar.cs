using UnityEngine;
using UnityEngine.UI;

public class EnemyHealthBar : MonoBehaviour
{
    [SerializeField] private Image fillImage;

    public void UpdateHealth(int current, int max)
    {
        if (fillImage == null) return;

        fillImage.fillAmount = (float)current / max;
    }
}