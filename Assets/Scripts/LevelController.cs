using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

// See references for useful algorithms:
// http://pcg.wikidot.com/category-pcg-algorithms
// https://www.hermetic.ch/compsci/cellular_automata_algorithms.htm

public class LevelController : MonoBehaviour
{
    public Tilemap tilemap;
    public Tile blankTile;
    public int rows = 20;
    public int cols = 20;
    public int generations = 1;
    public int q = 2;
    public int k1 = 10;
    public int k2 = 11;
    public int k3 = 11;
    public int k4 = 11;

    private int[,] tiles;

    void Start() {
        GenerateTiles();
    }

    public void NextGeneration() {
        ApplyGenerations(1);
    }

    private void GenerateTiles() {
        // 1 = dead, 2 = alive
        tiles = new int[rows, cols];

        // Initialize the tiles with state 1 or 2
        for (int i = 0; i < rows; i++) {
            for (int j = 0; j < cols; j++) {
                Vector3Int tilePos = new Vector3Int(i, j, 0);
                tilemap.SetTile(tilePos, blankTile);
                tilemap.SetTileFlags(tilePos, TileFlags.None);
                
                tiles[i, j] = Random.Range(0, q) + 1;
            }
        }

        // Apply generations
        ApplyGenerations(generations);
    }

    private void ApplyGenerations(int generations) {
        int[,] sums = new int[rows, cols];

        for (int g = 0; g < generations; g++) {
            // Store the sum of neighbours for each tile
            for (int i = 0; i < rows; i++) {
                for (int j = 0; j < cols; j++) {
                    sums[i, j] = GetSumOfNeighbours(i, j);
                }
            }

            // Apply the rules
            for (int i = 0; i < rows; i++) {
                for (int j = 0; j < cols; j++) {
                    float sum = sums[i, j];
                    if (tiles[i, j] > q/2) {
                        if (sum >= k1 && sum <= k2) {
                            tiles[i, j] = Mathf.Clamp(tiles[i, j] + 1, 1, q);
                        } else {
                            tiles[i, j] = Mathf.Clamp(tiles[i, j] - 1, 1, q);
                        }
                    } else {
                        if (sum >= k3 && sum <= k4) {
                            tiles[i, j] = Mathf.Clamp(tiles[i, j] + 1, 1, q);
                        } else {
                            tiles[i, j] = Mathf.Clamp(tiles[i, j] - 1, 1, q);
                        }
                    }
                }
            }
        }

        // Update tilemap
        for (int i = 0; i < rows; i++) {
            for (int j = 0; j < cols; j++) {
                Vector3Int tilePos = new Vector3Int(i, j, 0);
                Color tileColor = Color.Lerp(Color.black, Color.white, (float)tiles[i, j] / q);
                tilemap.SetColor(tilePos, tileColor);
            }
        }
    }

    private int GetSumOfNeighbours(int x, int y) {
        int sum = 0;
        int xMin = Mathf.Max(x - 1, 0);
        int xMax = Mathf.Min(x + 1, rows - 1);
        int yMin = Mathf.Max(y - 1, 0);
        int yMax = Mathf.Min(y + 1, cols - 1);
        for (int i = xMin; i <= xMax; i++) {
            for (int j = yMin; j <= yMax; j++) {
                if (i == 0 && j == 0) {
                    continue;
                }
                sum += tiles[i, j];
            }
        }
        return sum;
    }
}
