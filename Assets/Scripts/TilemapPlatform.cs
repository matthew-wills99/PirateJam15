using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
public class TilemapPlatform : MonoBehaviour
{
    public string colour;
    //registering tilemaps with specific colour
    private void Start(){
        PlatformManager.Instance.RegisterTilemap(colour, GetComponent<Tilemap>());
        //White platforms is default
        gameObject.SetActive(colour == "white");
    }
}
