using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainGenerator : MonoBehaviour{

    

    const float viewerMoveForChunkUpdate = 25f;
    const float sqrViewerMoveForChunkUpdate = viewerMoveForChunkUpdate * viewerMoveForChunkUpdate;


    public int colliderLODIndex;
    public LODInfo[] detailLevels;

    public MeshSettings meshSettings;
    public HeightMapSettings heightMapSettings;
    public TexturaData texturaSettings;

    public Transform viewer;
    public Material mapMaterial;

    Vector2 viewerPosition;
    Vector2 viewerPositionOld;
    float meshWorldSize;
    int chunksVisibleInVewDist;

    Dictionary<Vector2, TerrainChunk> terrainChunkDictonary = new Dictionary<Vector2, TerrainChunk>();
    List<TerrainChunk> visibleTerrainChunk = new List<TerrainChunk>();

    void Start() {
        float maxViewDist = detailLevels[detailLevels.Length - 1].visibleDstThreshold;
        meshWorldSize = meshSettings.meshWorldSize;
        chunksVisibleInVewDist = Mathf.RoundToInt(maxViewDist / meshWorldSize);

        UpdateVisibleChunks();
    }

    private void Update() {
        viewerPosition = new Vector2(viewer.position.x, viewer.position.z);

        if(viewerPosition != viewerPositionOld) {
            foreach (TerrainChunk chunk in visibleTerrainChunk) {
                chunk.UpdateCollisionMesh();
            }
        }
        if ((viewerPositionOld - viewerPosition).sqrMagnitude > sqrViewerMoveForChunkUpdate) {
            viewerPositionOld = viewerPosition;
            UpdateVisibleChunks();
        }
    }

    void UpdateVisibleChunks() {
        HashSet<Vector2> alreadUpdatedCoord = new HashSet<Vector2>();

        for (int i = visibleTerrainChunk.Count - 1; i >= 0; i--) {
            alreadUpdatedCoord.Add(visibleTerrainChunk[i].coord);
            visibleTerrainChunk[i].UpdateTerrainChunk();
        }

        int currentChunkCoordX = Mathf.RoundToInt(viewerPosition.x / meshWorldSize);
        int currentChunkCoordY = Mathf.RoundToInt(viewerPosition.y / meshWorldSize);

        for (int yOffset = -chunksVisibleInVewDist; yOffset <= chunksVisibleInVewDist; yOffset++) {
            for(int xOffset = -chunksVisibleInVewDist; xOffset <= chunksVisibleInVewDist; xOffset++) {
                Vector2 viewedChunckCoord = new Vector2(currentChunkCoordX + xOffset, currentChunkCoordY + yOffset);

                if(!alreadUpdatedCoord.Contains(viewedChunckCoord)) {
                    if(terrainChunkDictonary.ContainsKey (viewedChunckCoord)){
                        terrainChunkDictonary [viewedChunckCoord].UpdateTerrainChunk ();
                        if (terrainChunkDictonary [viewedChunckCoord].IsVisible()) {
                            visibleTerrainChunk.Add(terrainChunkDictonary [viewedChunckCoord]);
                        }
                    } else {
                        TerrainChunk newChunk = new TerrainChunk(viewedChunckCoord, heightMapSettings, meshSettings, detailLevels, colliderLODIndex, transform, mapMaterial, viewer);
                        terrainChunkDictonary.Add(viewedChunckCoord, newChunk);
                        newChunk.onVisibiltyChange += OnTerrainChunkVissibiltyChange;

                        newChunk.load();
                    }
                }
            }
        }
    }
    void OnTerrainChunkVissibiltyChange(TerrainChunk chunk, bool isVisible) {
        if(isVisible) {
            visibleTerrainChunk.Add(chunk);
        } else {
            visibleTerrainChunk.Remove(chunk);
        }
    }
    
}
        

[System.Serializable]
public struct LODInfo {
    [Range(0, MeshSettings.numSuportedLODs - 1)]
    public int lod;
    public float visibleDstThreshold;

    public float sqrVisibleDistThreshold {
        get {
            return visibleDstThreshold * visibleDstThreshold;
        }
    }
}
