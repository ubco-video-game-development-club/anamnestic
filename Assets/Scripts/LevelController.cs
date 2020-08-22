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
        public bool used;

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
    public int upperMin = 10;
    public int upperMax = 11;
    public int lowerMin = 11;
    public int lowerMax = 11;

    private Dictionary<Vector3Int, TileData> tiles;
    private Vector3Int prevPlayerPos;
    private int currentGeneration;

    void Start() {
        tiles = new Dictionary<Vector3Int, TileData>();
        GenerateTiles();
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

    private void GenerateTiles() {
        // Flag tiles as unused
        tiles.ToList().ForEach(tile => tiles[tile.Key].used = false);

        // Initialize tiles
        for (int i = 0; i < rows; i++) {
            for (int j = 0; j < cols; j++) {
                Vector3Int tilePos = GetTilePos(i, j);
                tilemap.SetTile(tilePos, blankTile);
                tilemap.SetTileFlags(tilePos, TileFlags.None);

                if (!tiles.ContainsKey(tilePos)) {
                    int randState = Random.Range(0, numStates) + 1;
                    tiles[tilePos] = new TileData(randState);
                }
                tiles[tilePos].used = true;
            }
        }

        // Remove unused tiles
        List<Vector3Int> unused = tiles
            .Where(tile => !tile.Value.used)
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

            // Apply the rules
            tiles.ToList().ForEach(tile => {
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
            });
        }

        // Update tilemap
        tiles.ToList().ForEach(tile => {
            Color tileColor = Color.Lerp(Color.black, Color.white, (float)tiles[tile.Key].state / numStates);
            tilemap.SetColor(tile.Key, tileColor);
        });
    }

    private int GetSumOfNeighbours(Vector3Int tilePos) {
        // Determine boundaries
        int xMin = Mathf.Max(tilePos.x - 1, 0);
        int xMax = Mathf.Min(tilePos.x + 1, cols - 1);
        int yMin = Mathf.Max(tilePos.y - 1, 0);
        int yMax = Mathf.Min(tilePos.y + 1, rows - 1);
        
        // Calculate sum
        int sum = 0;
        for (int i = xMin; i <= xMax; i++) {
            for (int j = yMin; j <= yMax; j++) {
                if (i == tilePos.x && j == tilePos.y) {
                    continue;
                }
                sum += tiles[tilePos].state;
            }
        }
        return sum;
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
