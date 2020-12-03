
using UnityEngine;

public class TerrainChunk {
        
    public event System.Action<TerrainChunk, bool> onVisibiltyChange;
    public Vector2 coord;

    GameObject meshObject;
    Vector2 sampleCenter;
    Bounds bounds;

    MeshRenderer meshRenderer;
    MeshFilter meshFilter;
    MeshCollider meshCollider;

    LODInfo[] detailLevels;
    LODMesh[] lodMeshes;
    int colliderLODIndex;

    HeightMap heightMap;
    bool mapdataRecived;
    int previousLODIndex = -1;
    bool hasSetCollider;

    HeightMapSettings heightMapSettings;
    MeshSettings meshSettings;
    float maxViewDist;
    const float colliderGenDistThreshold = 5;

    Transform viewer;

    public TerrainChunk(Vector2 coord, HeightMapSettings heightMapSettings, MeshSettings meshSettings, LODInfo[] detailLevels, int colliderLODIndex, Transform parent, Material material, Transform viewer) {
        this.coord = coord;
        this.detailLevels = detailLevels;
        this.colliderLODIndex = colliderLODIndex;
        this.heightMapSettings = heightMapSettings;
        this.meshSettings = meshSettings;
        this.viewer = viewer;
        
        sampleCenter = coord * meshSettings.meshWorldSize / meshSettings.meshScale;
        Vector2 position = coord * meshSettings.meshWorldSize;
        bounds = new Bounds(position, Vector2.one * meshSettings.meshWorldSize);

        meshObject = new GameObject("TerrainChunk");
        meshRenderer = meshObject.AddComponent<MeshRenderer>();
        meshFilter = meshObject.AddComponent<MeshFilter>();
        meshCollider = meshObject.AddComponent<MeshCollider>();
        meshRenderer.material = material;

        meshObject.transform.position = new Vector3(position.x, 0, position.y);
        meshObject.transform.parent = parent;

        lodMeshes = new LODMesh[detailLevels.Length];
        for (int i = 0; i < detailLevels.Length; i++) {
            lodMeshes[i] = new LODMesh(detailLevels[i].lod, UpdateTerrainChunk);
            lodMeshes[i].updateCallback += UpdateTerrainChunk;
            if(i == colliderLODIndex) {
                lodMeshes[i].updateCallback += UpdateCollisionMesh;
            }
        }

        maxViewDist = detailLevels[detailLevels.Length-1].visibleDstThreshold;
    }

    public void load() {
        ThreadedDataRequester.RequestData(() => HeightMapGenerator.GenerateHeightMap(meshSettings.mapChunkSize, meshSettings.mapChunkSize, heightMapSettings, sampleCenter), OnMapDataReceived);
    }

    void OnMapDataReceived(object heightMapObject) {
        this.heightMap = (HeightMap)heightMapObject;
        mapdataRecived = true;

        UpdateTerrainChunk ();
    }

    Vector2 viewerPosition {
        get {
            return new Vector2(viewer.position.x, viewer.position.z);
        }
    }

    public void UpdateTerrainChunk() {
        if (mapdataRecived) {
            float viewerDistFromNearestEdge = Mathf.Sqrt(bounds.SqrDistance (viewerPosition));

            bool wasVisible = IsVisible();
            bool visible = viewerDistFromNearestEdge <= maxViewDist;

            if (visible) {
                int lodIndex = 0;

                for (int i = 0; i < detailLevels.Length - 1; i++){
                    if (viewerDistFromNearestEdge > detailLevels [i].visibleDstThreshold) {
                        lodIndex = i + 1;
                    } else {
                        break;
                    }
                }

                if(lodIndex != previousLODIndex) {
                    LODMesh lodMesh = lodMeshes [lodIndex];
                    if (lodMesh.hasMesh) {
                        meshFilter.mesh = lodMesh.mesh;
                        meshCollider.sharedMesh = lodMesh.mesh;
                        previousLODIndex = lodIndex;
                    } else if (!lodMesh.hasRequestedMesh) {
                        lodMesh.RequestMesh(heightMap, meshSettings);
                    }
                }

                

                
            }
            if (wasVisible != visible) {
                SetVisible(visible);
                if(onVisibiltyChange != null) {
                    onVisibiltyChange (this, visible);
                }
            }
            
        }
    }

    public void UpdateCollisionMesh() {
        if(!hasSetCollider) {
            float sqrdistFromViewToEdge = bounds.SqrDistance (viewerPosition);

            if (sqrdistFromViewToEdge < detailLevels[colliderLODIndex].sqrVisibleDistThreshold) {
                if(!lodMeshes[colliderLODIndex].hasRequestedMesh) {
                    lodMeshes[colliderLODIndex].RequestMesh(heightMap, meshSettings);
                }
            }

            if (sqrdistFromViewToEdge < colliderGenDistThreshold * colliderGenDistThreshold) {
                if (lodMeshes[colliderLODIndex].hasMesh){
                    meshCollider.sharedMesh = lodMeshes[colliderLODIndex].mesh;
                    hasSetCollider = true;
                }
            }
        }
    }

    public void SetVisible(bool visible) {
        meshObject.SetActive (visible);
    }

    public bool IsVisible() {
        return meshObject.activeSelf;
    }
}

class LODMesh {
    public Mesh mesh;
    public bool hasRequestedMesh;
    public bool hasMesh;
    int lod;
    public event System.Action updateCallback;

    public LODMesh(int lod, System.Action updateCallback) {
        this.lod = lod;
        this.updateCallback = updateCallback;
    }

    void OnMeshDataReceived(object meshDataObject) {
        mesh = ((MeshData)meshDataObject).CreateMesh ();
        hasMesh = true;

        updateCallback ();
    }

    public void RequestMesh(HeightMap mapData, MeshSettings meshSettings) {
        hasRequestedMesh = true;
        ThreadedDataRequester.RequestData(() => MeshGenerator.GenerateTerrainMesh(mapData.values, meshSettings, lod), OnMeshDataReceived);
    }
}