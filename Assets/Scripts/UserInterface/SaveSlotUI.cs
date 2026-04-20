using GameState.SaveLoad;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SaveSlotUI : MonoBehaviour
{
    [SerializeField] private TMP_Text saveNameLabel;
    [SerializeField] private Button loadButton;
    [SerializeField] private Button deleteButton;

    //[SerializeField] private TMP_Text levelLabel;
    //[SerializeField] private TMP_Text coinsLabel;

    private string _saveName;

    public void Setup(GameDataDTO dto, LoadMenuController controller)
    {
        _saveName = dto.saveName;

        saveNameLabel.text = dto.saveName;

        //levelLabel.text = dto.currentLevel;
        //coinsLabel.text = dto.totalCoins.ToString();

        loadButton.onClick.RemoveAllListeners();
        loadButton.onClick.AddListener(() => controller.LoadSave(_saveName));

        deleteButton.onClick.RemoveAllListeners();
        deleteButton.onClick.AddListener(() => controller.DeleteSave(_saveName));
    }
    }