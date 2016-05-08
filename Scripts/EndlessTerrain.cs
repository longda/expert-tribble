using UnityEngine;
using System.Collections.Generic;

public class EndlessTerrain : MonoBehaviour 
{
	public const float MAX_VIEW_DIST = 450;
	public Transform viewer;
	public Material mapMaterial;
	
	public static Vector2 viewerPosition;
	static MapGenerator mapGenerator;
	
	int chunkSize;
	int chunksVisibleInDist;
	Dictionary<Vector2, TerrainChunk> terrainChunkDictionary = new Dictionary<Vector2, TerrainChunk>();
	List<TerrainChunk> terrainChunksVisibleLastUpdate = new List<TerrainChunk>();

	void Start() 
	{
		mapGenerator = FindObjectOfType<MapGenerator>();
		chunkSize = MapGenerator.MAP_CHUNK_SIZE - 1;
		chunksVisibleInDist = Mathf.RoundToInt(MAX_VIEW_DIST / chunkSize);
	}
	
	void Update()
	{
		viewerPosition = new Vector2(viewer.position.x, viewer.position.z);
		UpdateVisibleChunks();
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
					terrainChunkDictionary.Add(viewedChunkCoord, new TerrainChunk(viewedChunkCoord, chunkSize, transform, mapMaterial));
				}
			}
		}
	}
	
	public class TerrainChunk
	{
		GameObject meshObject;
		Vector2 position;
		Bounds bounds;
		MapData mapData;
		
		MeshRenderer meshRenderer;
		MeshFilter meshFilter;
		
		public TerrainChunk(Vector2 coord, int size, Transform parent, Material material)
		{
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
			
			mapGenerator.RequestMapData(OnMapDataReceived);
		}
		
		void OnMapDataReceived(MapData mapData)
		{
			mapGenerator.RequestMeshData(mapData, OnMeshDataReceived);
		}
		
		void OnMeshDataReceived(MeshData meshData)
		{
			meshFilter.mesh = meshData.CreateMesh();
		}
		
		public void UpdateTerrainChunk()
		{
			var viewerDistFromNearestEdge = Mathf.Sqrt(bounds.SqrDistance(viewerPosition));
			var visible = viewerDistFromNearestEdge <= MAX_VIEW_DIST;
			SetVisible(visible);
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
}
