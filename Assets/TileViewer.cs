using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public class TileViewer : MonoBehaviour
{
    public Tilemap tilemap;
    public Text outputText;

    void Update() {
        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3Int tilePos = tilemap.WorldToCell(mousePos);
        TileData tileData = LevelController.instance.GetTileData(tilePos);
        if (tileData != null) {
            WriteTileData(tilePos, tileData);
        }
    }

    private void WriteTileData(Vector3Int pos, TileData data) {
        string output = "Position: " + pos + "\n";
        if (data != null) {
            output += 
                "State: " + data.state + "\n" +
                "Sum: " + data.sum + "\n" +
                "Type: " + data.tileType.ToString();
        } else {
            output += 
                "No Tile Found";
        }
        outputText.text = output;
    }
}
