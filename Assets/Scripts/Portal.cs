using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Portal : MonoBehaviour
{
    /* For this to work make sure the Player has the tag "Player" in the insepector (To change tags its at the top of the inspector)
        It will take in the current level its on and get the scene for the next level
        Once the player hits the portal it will load the next level. If you beat the last level its going to give an error.
    */
    private void OnTriggerEnter2D(Collider2D collision){
        if(collision.CompareTag("Player")){
            int currentLvl = SceneManager.GetActiveScene().buildIndex;
            int nextLvl = currentLvl + 1;

            if (nextLvl < SceneManager.sceneCountInBuildSettings){
                SceneManager.LoadScene(nextLvl);
            }else{
                Debug.Log("THIS IS THE LAST LEVEL!");
            }
        }
    }
}
