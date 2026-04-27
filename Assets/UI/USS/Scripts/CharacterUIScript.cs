using GameState.Core;
using System;
using UnityEngine;
using UnityEngine.UIElements;
//using UnityEngine.InputSystem;

/**
* This script is the contoller for the Character UI(s). It is basically just a way to change the #of hearts
* It just updates the visual elements, the game manager should be the script that ends the game once the player
* reaches 0 hp
*
* Most of the commented out debug logs were for error checking
*
* TODO: add a way to change the number of hearts by taking damage.
**/

public class NewMonoBehaviourScript : MonoBehaviour
{
    [SerializeField] UIDocument uiDoc;

    [Header("Heart Sprites")]
    public Sprite fullHearts;
    public Sprite twoHearts;
    public Sprite oneHeart;
    public Sprite noHearts;

    private VisualElement hearts;
    private VisualElement root;
    private readonly int _maxCoins = 100;
    private IntegerField coins;


    private void Start()
    {
        root = uiDoc.rootVisualElement;
        hearts = root.Q<VisualElement>("Hearts");

        /**This chunk of code is to fix a UI bug where the IntegerField is modified everytime we use 'wasd' on the keyboard
        * Essentially this is to prevent unwanted modification of the #of coins during gameplay
        * DO NOT REMOVE, PLEASE AND THANK YOU!
        **/
        coins = root.Q<IntegerField>("Coins");
        coins.focusable = false;

        //coins.value = UnityEngine.Random.Range(0, 101); //For play-testing
        RefreshHearts();

    }


    private void Update()
    {
        RefreshHearts();
        RefreshCoins();
    }

    private void RefreshHearts()
    {


        if (!GameStateManager.HasInstance || GameStateManager.Instance.Data == null) return;

        int health = GameStateManager.Instance.Data.PlayerHealth;

        switch (health)
        {
            case 3:
                hearts.style.backgroundImage = new StyleBackground(fullHearts);
                break;
            case 2:
                hearts.style.backgroundImage = new StyleBackground(twoHearts);
                break;
            case 1:
                hearts.style.backgroundImage = new StyleBackground(oneHeart);
                break;
            case 0:
                hearts.style.backgroundImage = new StyleBackground(noHearts);
                break;
        }
    }

    private void RefreshCoins() {
    
        if(!GameStateManager.HasInstance || GameStateManager.Instance.Data == null) return;

        coins.value = GameStateManager.Instance.Data.TotalCoins;
    }

    // TODO: attach this to game manager to track coins collected
    public void ModCoins()
    {
        if (coins.value > _maxCoins)
        {
          coins.value = 0;
        }
        else
        {
          coins.value++;
        }
    }

    /* //VisualElement bttn;
    // bool display;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    /*void Start()
    {
        //display = false;
        health = 3;
        root = uiDoc.rootVisualElement;

        var name = root.Q<VisualElement>("CharacterName");
        //bttn = root.Q<VisualElement>("Bttn");
        var button = root.Q<VisualElement>("TestButton");
        var coins = root.Q<IntegerField>("Coins");

        hearts = root.Q<VisualElement>("Hearts");
        hearts.style.backgroundImage = new StyleBackground(fullHearts);

        button.RegisterCallback<ClickEvent>(OnClick);

        coins.value = UnityEngine.Random.Range(0, 1001);
        //bttn.style.display = DisplayStyle.None;
    }

    private void OnClick(ClickEvent ev) 
    {

        //Debug.Log(health);
        switch(health)
        {
         case 3:
            //Debug.Log("Player Has 3 hearts");
            hearts.style.backgroundImage = new StyleBackground(fullHearts);
            break;
         case 2:
           // Debug.Log("Player Has 2 hearts");
            hearts.style.backgroundImage = new StyleBackground(twoHearts);
            break;
         case 1:
            //Debug.Log("Player Has 1 heart");
            hearts.style.backgroundImage = new StyleBackground(oneHeart);
            break;
         case 0:
            //Debug.Log("Player is dead");
            hearts.style.backgroundImage = new StyleBackground(noHearts);
            break;
        }

        if(health > 0)
        {
            health --;
        }

        //Debug.Log("Play Button Clicked");
        // display = !display;

        // if(!display)
        // {
        //     bttn.style.display = DisplayStyle.None;
        // }else{
        //     bttn.style.display = DisplayStyle.Flex;
        // }
        */

}

