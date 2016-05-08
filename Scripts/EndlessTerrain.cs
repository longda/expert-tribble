using UnityEngine;
using System.Collections.Generic;

public class EndlessTerrain : MonoBehaviour 
{
	const float VIEWER_MOVE_THRESHOLD = 25f;
	const float SQR_VIEWER_MOVE_THRESHOLD = VIEWER_MOVE_THRESHOLD * VIEWER_MOVE_THRESHOLD;
	
	public LODInfo[] detailLevels;
	public static float maxViewDist = 450;
	public Transform viewer;
	public Material mapMaterial;
	
	public static Vector2 viewerPosition;
	Vector2 viewerPositionOld;
	static MapGenerator mapGenerator;
	
	int chunkSize;
	int chunksVisibleInDist;
	Dictionary<Vector2, TerrainChunk> terrainChunkDictionary = new Dictionary<Vector2, TerrainChunk>();
	List<TerrainChunk> terrainChunksVisibleLastUpdate = new List<TerrainChunk>();

	void Start() 
	{
		mapGenerator = FindObjectOfType<MapGenerator>();
		maxViewDist = detailLevels[detailLevels.Length - 1].visibleDistThreshold;
		chunkSize = MapGenerator.MAP_CHUNK_SIZE - 1;
		chunksVisibleInDist = Mathf.RoundToInt(maxViewDist / chunkSize);
		
		UpdateVisibleChunks();
	}
	
	void Update()
	{
		viewerPosition = new Vector2(viewer.position.x, viewer.position.z);
		
		if ((viewerPositionOld - viewerPosition).sqrMagnitude > SQR_VIEWER_MOVE_THRESHOLD)
		{
			viewerPositionOld = viewerPosition;
			UpdateVisibleChunks();
		}
		
	}

	void UpdateVisibleChunks()
	{
		foreach (var chunk in terrainChunksVisibleLastUpdate)
		{
			chunk.SetVisible(false);
		}
		terrainChunksVisibleLastUpdate.Clear();
		
		var currentChunkCoordX = Mathf.RoundToInt(viewerPosition.x / chunkSize);
		var currentChunkCoordY = Mathf.RoundToInt(viewerPosition.y / chunkSize);
		
		for (var xOffset = -chunksVisibleInDist; xOffset <= chunksVisibleInDist; xOffset++)
		{
			for (var yOffset = -chunksVisibleInDist; yOffset <= chunksVisibleInDist; yOffset++)
			{
				var viewedChunkCoord = new Vector2(currentChunkCoordX + xOffset, currentChunkCoordY + yOffset);
				
				if (terrainChunkDictionary.ContainsKey(viewedChunkCoord))
				{
					terrainChunkDictionary[viewedChunkCoord].UpdateTerrainChunk();
					if (terrainChunkDictionary[viewedChunkCoord].IsVisible())
					{
						terrainChunksVisibleLastUpdate.Add(terrainChunkDictionary[viewedChunkCoord]);
					}
				}
				else
				{
					terrainChunkDictionary.Add(viewedChunkCoord, new TerrainChunk(viewedChunkCoord, chunkSize, detailLevels, transform, mapMaterial));
				}
			}
		}
	}
	
	public class TerrainChunk
	{
		GameObject meshObject;
		Vector2 position;
		Bounds bounds;
		
		MeshRenderer meshRenderer;
		MeshFilter meshFilter;
		
		LODInfo[] detailLevels;
		LODMesh[] lodMeshes;
		
		MapData mapData;
		bool mapDataReceived;
		int previousLODIndex = -1;
		
		public TerrainChunk(Vector2 coord, int size, LODInfo[] detailLevels, Transform parent, Material material)
		{
			this.detailLevels = detailLevels;
			position = coord * size;
			bounds = new Bounds(position, Vector2.one * size);
			var positionV3 = new Vector3(position.x, 0, position.y);
			
			meshObject = new GameObject("Terrain Chunk");
			meshRenderer = meshObject.AddComponent<MeshRenderer>();
			meshRenderer.material = material;
			meshFilter = meshObject.AddComponent<MeshFilter>();
			
			meshObject.transform.position = positionV3;
			meshObject.transform.parent = parent;
			SetVisible(false);
			
			lodMeshes = new LODMesh[detailLevels.Length];
			for (var i = 0; i < detailLevels.Length; i++)
			{
				lodMeshes[i] = new LODMesh(detailLevels[i].lod, UpdateTerrainChunk);
			}
			
			mapGenerator.RequestMapData(position, OnMapDataReceived);
		}
		
		void OnMapDataReceived(MapData mapData)
		{
			this.mapData = mapData;
			mapDataReceived = true;
			
			var texture = TextureGenerator.TextureFromColorMap(mapData.colorMap, MapGenerator.MAP_CHUNK_SIZE, MapGenerator.MAP_CHUNK_SIZE);
			meshRenderer.material.mainTexture = texture;
			
			UpdateTerrainChunk();
		}
		
		public void UpdateTerrainChunk()
		{
			if (mapDataReceived)
			{
				var viewerDistFromNearestEdge = Mathf.Sqrt(bounds.SqrDistance(viewerPosition));
				var visible = viewerDistFromNearestEdge <= maxViewDist;
				
				if (visible)
				{
					var lodIndex = 0;
					
					for (var i = 0; i < detailLevels.Length - 1; i++)
					{
						if (viewerDistFromNearestEdge > detailLevels[i].visibleDistThreshold)
						{
							lodIndex = i + 1;
						}
						else
						{
							break;
						}
					}
					
					if (lodIndex != previousLODIndex)
					{
						LODMesh lodMesh = lodMeshes[lodIndex];
						if (lodMesh.hasMesh)
						{
							previousLODIndex = lodIndex;
							meshFilter.mesh = lodMesh.mesh;
						}
						else if (!lodMesh.hasRequestedMesh)
						{
							lodMesh.RequestMesh(mapData);
						}
					}
				}
				
				SetVisible(visible);
			}
		}
		
		public void SetVisible(bool visible)
		{
			meshObject.SetActive(visible);
		}
		
		public bool IsVisible()
		{
			return meshObject.activeSelf;
		}
	}
	
	class LODMesh
	{
		public Mesh mesh;
		public bool hasRequestedMesh;
		public bool hasMesh;
		int lod;
		System.Action updateCallback;
		
		public LODMesh(int lod, System.Action updateCallback)
		{
			this.lod = lod;
			this.updateCallback = updateCallback;
		}
		
		void OnMeshDataReceived(MeshData meshData)
		{
			mesh = meshData.CreateMesh();
			hasMesh = true;
			
			updateCallback();
		}
		
		public void RequestMesh(MapData mapData)
		{
			hasRequestedMesh = true;
			mapGenerator.RequestMeshData(mapData, lod, OnMeshDataReceived);
		}
	}
	
	[System.Serializable]
	public struct LODInfo
	{
		public int lod;
		public float visibleDistThreshold;
	}
}
