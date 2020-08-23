using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

// See references for useful algorithms:
// http://pcg.wikidot.com/category-pcg-algorithms
// https://www.hermetic.ch/compsci/cellular_automata_algorithms.htm

public class LevelController : MonoBehaviour
{
    public class TileData {
        public int state;
        public int sum;
        public bool isActive;
        public bool isNew;

        public TileData(int state) {
            this.state = state;
        }
    }

    [Header("Tilemap Refs")]
    public Tilemap tilemap;
    public Tile blankTile;
    public Player player;

    [Header("Q-state Life")]
    public int rows = 8;
    public int cols = 8;
    public int generations = 1;
    public bool autoTick = true;
    public float tickInterval = 0.5f;
    public int numStates = 2;
    public int defaultState = 1;
    public int upperMin = 10;
    public int upperMax = 11;
    public int lowerMin = 11;
    public int lowerMax = 11;

    private Dictionary<Vector3Int, TileData> tiles;
    private Vector3Int prevPlayerPos;
    private int currentGeneration;

    void Start() {
        tiles = new Dictionary<Vector3Int, TileData>();
        InitializeTiles();
        StartCoroutine(TickGenerations());
        prevPlayerPos = GetPlayerPos();
    }

    void Update() {
        Vector3Int playerPos = GetPlayerPos();
        if (playerPos != prevPlayerPos) {
            prevPlayerPos = playerPos;
            GenerateTiles();
        }
    }

    public void NextGeneration() {
        ApplyGenerations(1);
    }

    public void ResetTiles() {
        StopAllCoroutines();
        GenerateTiles();
        StartCoroutine(TickGenerations());
    }

    private void InitializeTiles() {
        // Initialize tiles
        for (int i = 0; i < rows; i++) {
            for (int j = 0; j < cols; j++) {
                Vector3Int tilePos = GetTilePos(i, j);
                tilemap.SetTile(tilePos, blankTile);
                tilemap.SetTileFlags(tilePos, TileFlags.None);

                tiles[tilePos] = new TileData(defaultState);
                tiles[tilePos].isNew = true;
                tiles[tilePos].isActive = true;
            }
        }

        UpdateTileColors();
    }

    private void GenerateTiles() {
        // Flag existing tiles as inactive
        tiles.ToList().ForEach(tile => {
            tiles[tile.Key].isActive = false;
            tiles[tile.Key].isNew = false;
        });

        // Initialize tiles
        for (int i = 0; i < rows; i++) {
            for (int j = 0; j < cols; j++) {
                Vector3Int tilePos = GetTilePos(i, j);
                tilemap.SetTile(tilePos, blankTile);
                tilemap.SetTileFlags(tilePos, TileFlags.None);

                if (!tiles.ContainsKey(tilePos)) {
                    int randState = Random.Range(0, numStates) + 1;
                    tiles[tilePos] = new TileData(randState);
                    tiles[tilePos].isNew = true;
                }
                tiles[tilePos].isActive = true;
            }
        }

        // Remove inactive tiles
        List<Vector3Int> unused = tiles
            .Where(tile => !tile.Value.isActive)
            .Select(tile => tile.Key)
            .ToList();
        unused.ForEach(tilePos => {
            tiles.Remove(tilePos);
            tilemap.SetTile(tilePos, null);
        });

        ApplyGenerations(generations);
    }

    private void ApplyGenerations(int generations) {
        for (int g = 0; g < generations; g++) {
            // Store the sum of neighbours for each tile
            tiles.ToList().ForEach(tile => {
                tiles[tile.Key].sum = GetSumOfNeighbours(tile.Key);
            });

            // Apply the rules to new tiles
            tiles.Where(tile => tile.Value.isNew).ToList().ForEach(tile => {
                int state = tiles[tile.Key].state;
                int sum = tile.Value.sum;
                if (state > numStates/2) {
                    if (sum >= upperMin && sum <= upperMax) {
                        tiles[tile.Key].state = Mathf.Clamp(state + 1, 1, numStates);
                    } else {
                        tiles[tile.Key].state = Mathf.Clamp(state - 1, 1, numStates);
                    }
                } else {
                    if (sum >= lowerMin && sum <= lowerMax) {
                        tiles[tile.Key].state = Mathf.Clamp(state + 1, 1, numStates);
                    } else {
                        tiles[tile.Key].state = Mathf.Clamp(state - 1, 1, numStates);
                    }
                }

                // if (g == generations - 1) {
                //     Debug.Log(tile.Key + ": " + state + " -> " + tiles[tile.Key].state + " (" + sum + ")");
                // }
            });
        }

        // Update tilemap with new tiles
        UpdateTileColors();
    }

    private void UpdateTileColors() {
        tiles.ToList().ForEach(tile => {
            Color color;
            int edgeSum = GetSumOfEdgeNeighbours(tile.Key);
            int cornerSum = GetSumOfCornerNeighbours(tile.Key);
            if (tile.Value.state == 1) {
                if (cornerSum == 4 && edgeSum >= 6) {
                    color = Color.black;
                    Debug.Log("Filled");
                } else {
                    color = Color.white;
                }
            } else {
                color = Color.black;
            }
            tilemap.SetColor(tile.Key, color);
        });
    }

    private int GetSumOfNeighbours(Vector3Int tilePos) {
        int sum = 0;
        sum += GetSumOfEdgeNeighbours(tilePos);
        sum += GetSumOfCornerNeighbours(tilePos);
        return sum;
    }

    private int GetSumOfEdgeNeighbours(Vector3Int tilePos) {
        int sum = 0;
        sum += GetStateIfExists(tilePos + Vector3Int.up);
        sum += GetStateIfExists(tilePos + Vector3Int.down);
        sum += GetStateIfExists(tilePos + Vector3Int.left);
        sum += GetStateIfExists(tilePos + Vector3Int.right);
        return sum;
    }

    private int GetSumOfCornerNeighbours(Vector3Int tilePos) {
        int sum = 0;
        sum += GetStateIfExists(tilePos + new Vector3Int(1, 1, 0));
        sum += GetStateIfExists(tilePos + new Vector3Int(1, -1, 0));
        sum += GetStateIfExists(tilePos + new Vector3Int(-1, 1, 0));
        sum += GetStateIfExists(tilePos + new Vector3Int(-1, -1, 0));
        return sum;
    }

    private int GetStateIfExists(Vector3Int tilePos) {
        return tiles.ContainsKey(tilePos) ? tiles[tilePos].state : 0;
    }

    private Vector3Int GetPlayerPos() {
        return tilemap.WorldToCell(player.transform.position);
    }

    private Vector3Int GetTilePos(int x, int y) {
        Vector3Int origin = GetPlayerPos() - new Vector3Int(cols / 2, rows / 2, 0);
        return origin + new Vector3Int(x, y, 0);
    }

    private IEnumerator TickGenerations() {
        while (true) {
            if (autoTick) {
                NextGeneration();
            }
            yield return new WaitForSeconds(tickInterval);
        }
    }
}
