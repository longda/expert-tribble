﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EndlessTerrain : MonoBehaviour 
{
	public const float MAX_VIEW_DIST = 450;
	public Transform viewer;
	public static Vector2 viewerPosition;
	
	int chunkSize;
	int chunksVisibleInDist;
	Dictionary<Vector2, TerrainChunk> terrainChunkDictionary = new Dictionary<Vector2, TerrainChunk>();
	List<TerrainChunk> terrainChunksVisibleLastUpdate = new List<TerrainChunk>();

	void Start() 
	{
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
					terrainChunkDictionary.Add(viewedChunkCoord, new TerrainChunk(viewedChunkCoord, chunkSize, transform));
				}
			}
		}
	}
	
	public class TerrainChunk
	{
		GameObject meshObject;
		Vector2 position;
		Bounds bounds;
		
		public TerrainChunk(Vector2 coord, int size, Transform parent)
		{
			position = coord * size;
			bounds = new Bounds(position, Vector2.one * size);
			var positionV3 = new Vector3(position.x, 0, position.y);
			
			meshObject = GameObject.CreatePrimitive(PrimitiveType.Plane);
			meshObject.transform.position = positionV3;
			meshObject.transform.localScale = Vector3.one * size / 10f;
			meshObject.transform.parent = parent;
			SetVisible(false);
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
