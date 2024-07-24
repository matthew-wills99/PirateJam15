using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pickup : MonoBehaviour
{
    public string colour;
    /*
        Same thing as portal with the tags
    */
    private void OnTriggerEnter2D(Collider2D collision){
        if(collision.CompareTag("Player")){
            PlatformManager.Instance.ShowPlatforms(colour);
            Destroy(gameObject);
        }
    }
}
