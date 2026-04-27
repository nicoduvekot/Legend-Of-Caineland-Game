using GameState.SaveLoad;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SaveSlotUI : MonoBehaviour
{
    [SerializeField] private TMP_Text saveNameLabel;
    [SerializeField] private Button loadButton; // Used as "New Game" if slot is empty
    [SerializeField] private Button deleteButton;

    private string _saveName;
    private bool _isEmpty;

    // Updated Setup to handle empty slots
    public void Setup(GameDataDTO dto, string slotName, LoadMenuController controller)
    {
        _saveName = slotName;

        if (dto == null)
        {
            _isEmpty = true;
            saveNameLabel.text = "Empty Slot";
            deleteButton.gameObject.SetActive(false);
            loadButton.GetComponentInChildren<TMP_Text>().text = "New Game";

            loadButton.onClick.RemoveAllListeners();
            loadButton.onClick.AddListener(() => controller.CreateNewGameInSlot(slotName));
        }
        else
        {
            _isEmpty = false;
            saveNameLabel.text = dto.saveName;
            deleteButton.gameObject.SetActive(true);
            loadButton.GetComponentInChildren<TMP_Text>().text = "Load";

            loadButton.onClick.RemoveAllListeners();
            loadButton.onClick.AddListener(() => controller.LoadSave(_saveName));

            deleteButton.onClick.RemoveAllListeners();
            deleteButton.onClick.AddListener(() => controller.DeleteSave(_saveName));
        }
    }
}