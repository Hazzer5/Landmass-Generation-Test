using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class MeshSettings : UpdateableData
{
    public const int numSuportedLODs = 5;
    public const int numSupportedChunkSizes = 7;
    public const int numSupportedFlatShadedChunkSizes = 3;
    public static readonly int[] supportedChunkSizes = {48,72,96,120,168,216,240};

    [Range(0, numSupportedChunkSizes - 1)]
    public int chunkSizeIndex;
    [Range(0, numSupportedFlatShadedChunkSizes - 1)]
    public int flatShadedChunkSizeIndex;

    public float meshScale = 2.5f;
    public bool useFlatShading;

    //number of vertices per line + 2 invisible vertices used to calculate normals
    public int mapChunkSize {
        get {
            return supportedChunkSizes[(useFlatShading) ? flatShadedChunkSizeIndex : chunkSizeIndex] + 1;
        }
    }
    

    public float meshWorldSize {
        get {
            return (mapChunkSize - 3) * meshScale;
        }
    }
}
