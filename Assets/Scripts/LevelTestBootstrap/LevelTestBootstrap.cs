using UnityEngine;
using UnityEngine.SceneManagement;
using GameState.Core;
using GameState.SaveLoad;
using CoinManagement;

/** This script is used to bootstrap the level for testing purposes. 
 * It will create a new game if there is no existing game data, 
 * and set the current level and checkpoint to the beginning of the level.
 * 
 */
public class LevelTestBootstrap : MonoBehaviour
{
    [SerializeField] private bool createTestGameOnStart = true;

    private void Start()
    {
        if (!createTestGameOnStart)
            return;

        if (GameStateManager.Instance.Data == null)
        {
            SaveLoadSystem.Instance.NewGame();
        }

        string sceneName = SceneManager.GetActiveScene().name;
        int maxCoins = FindObjectsByType<Coin>(FindObjectsSortMode.None).Length;

        GameStateManager.Instance.SetCurrentLevel(sceneName);
        GameStateManager.Instance.BeginLevelFresh(sceneName, maxCoins);
        GameStateManager.Instance.SetCheckpoint(0);
    }
}