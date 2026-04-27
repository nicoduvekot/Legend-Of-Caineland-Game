using UnityEngine;
/**
 * This interface defines the contract for any enemy in the game.
 * It includes a method for taking damage, which can be implemented
 * by any class that represents an enemy character. Idea brought up by Nicooo
 */
public interface IEnemy 
{
    void TakeDamage(int amount);
}
