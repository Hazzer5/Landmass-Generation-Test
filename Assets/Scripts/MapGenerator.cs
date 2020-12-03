using System;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    public enum DrawMode {NoiseMap, Mesh, FalloffMap};
    public DrawMode drawmode;

    public MeshSettings meshSettings;
    public HeightMapSettings heightMapSettings;
    public TexturaData texturaData;

    public Material terrainMaterial;

    [Range(0,MeshSettings.numSuportedLODs-1)]
    public int editorPreviewLOD;

    public bool autoUpdate;

    float[,] falloffMap;

    

    void OnValuesUpdated(){
        if (!Application.isPlaying) {
            DrawMapInEditor();
        }
    }

    void Start() {
        texturaData.ApplyToMaterial(terrainMaterial);
        texturaData.UpdateMeshHeight(terrainMaterial, heightMapSettings.minHeight, heightMapSettings.maxHeight);
    }
    
    void OnTextureValuesUpdated() {
        texturaData.ApplyToMaterial(terrainMaterial);
    }

    

    public void DrawMapInEditor() {
        texturaData.UpdateMeshHeight(terrainMaterial, heightMapSettings.minHeight, heightMapSettings.maxHeight);
        HeightMap mapdata = HeightMapGenerator.GenerateHeightMap(meshSettings.mapChunkSize, meshSettings.mapChunkSize, heightMapSettings, Vector2.zero);

        MapDisplay display = FindObjectOfType<MapDisplay>();
        if (drawmode == DrawMode.NoiseMap){
            display.DrawTexture(TextureGenerator.TextureFromHeightMap(mapdata.values));
        } else if (drawmode == DrawMode.Mesh)
        {
            display.DrawMesh(MeshGenerator.GenerateTerrainMesh(mapdata.values, meshSettings, editorPreviewLOD));
        } else if (drawmode == DrawMode.FalloffMap) {
            display.DrawTexture(TextureGenerator.TextureFromHeightMap(FalloffGenerator.GenerateFalloffMap(meshSettings.mapChunkSize)));
        }
    }

    private void OnValidate() {

        if (meshSettings != null) {
            meshSettings.OnValuesUpdated -= OnValuesUpdated;
            meshSettings.OnValuesUpdated += OnValuesUpdated;
        }
        if (heightMapSettings != null) {
            heightMapSettings.OnValuesUpdated -= OnValuesUpdated;
            heightMapSettings.OnValuesUpdated += OnValuesUpdated;
        }
        if (texturaData !=null) {
            texturaData.OnValuesUpdated -= OnTextureValuesUpdated;
            texturaData.OnValuesUpdated += OnTextureValuesUpdated;
        }
    }
}


