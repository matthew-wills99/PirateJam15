using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class PlatformManager : MonoBehaviour
{
    public static PlatformManager Instance;
    //stores tile maps by colour
    private Dictionary<string, Tilemap> tilemapsByColour = new Dictionary<string, Tilemap>();

    private void Awake(){
        //checking if there is an instance, if there is destroy this one
        if(Instance == null){
            Instance = this;

        }else{
            Destroy(gameObject);
        }
    }
    //register tilemap with a specific colour
    public void RegisterTilemap(string colour, Tilemap tilemap){
        //check if colour is registerd
        if(!tilemapsByColour.ContainsKey(colour)){
            //add the colour to the dic with key being colour
            tilemapsByColour[colour] = tilemap;
        }
    }
    //showing platforms
    public void ShowPlatforms(string colour){
        foreach (var key in tilemapsByColour.Keys){
            //Show if the key matches the pickup colour
            //Show white platforms as default if no colour is selected
            bool show = key == colour || (colour != "white" && key == "white");
            tilemapsByColour[key].gameObject.SetActive(show);
        }
    }
}
