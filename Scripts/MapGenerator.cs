using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class MapGenerator : MonoBehaviour 
{
	public enum DrawMode { NoiseMap, ColorMap, Mesh }
	public DrawMode drawMode;
	
	public const int MAP_CHUNK_SIZE = 241;
	[Range(0, 6)]
	public int levelOfDetail;
	public float noiseScale;
	public float meshHeightMultiplier;
	public AnimationCurve meshHeightCurve;
	
	public int octaves;
	[Range(0, 1)]
	public float persistence;
	public float lacunarity;
	
	public int seed;
	public Vector2 offset;
	
	public bool autoGenerate;
	
	public TerrainType[] regions;
	
	Queue<MapThreadInfo<MapData>> mapDataThreadInfoQueue = new Queue<MapThreadInfo<MapData>>();
	Queue<MapThreadInfo<MeshData>> meshDataThreadInfoQueue = new Queue<MapThreadInfo<MeshData>>();
	
	public void DrawMapInEditor()
	{
		var mapData = GenerateMapData();
		var display = FindObjectOfType<MapDisplay>();
		
		if (drawMode == DrawMode.NoiseMap)
		{
			display.DrawTexture(TextureGenerator.TextureFromHeightMap(mapData.heightMap));	
		}
		else if (drawMode == DrawMode.ColorMap)
		{
			display.DrawTexture(TextureGenerator.TextureFromColorMap(mapData.colorMap, MAP_CHUNK_SIZE, MAP_CHUNK_SIZE));
		}
		else if (drawMode == DrawMode.Mesh)
		{
			display.DrawMesh(MeshGenerator.GenerateTerrainMesh(mapData.heightMap, meshHeightMultiplier, meshHeightCurve, levelOfDetail), TextureGenerator.TextureFromColorMap(mapData.colorMap, MAP_CHUNK_SIZE, MAP_CHUNK_SIZE));
		}
	}
	
	public void RequestMapData(Action<MapData> callback)
	{
		ThreadStart threadStart = delegate
		{
			MapDataThread(callback);
		};
		
		new Thread(threadStart).Start();
	}
	
	void MapDataThread(Action<MapData> callback)
	{
		var mapData = GenerateMapData();
		lock (mapDataThreadInfoQueue)
		{
			mapDataThreadInfoQueue.Enqueue(new MapThreadInfo<MapData>(callback, mapData));
		}
	}
	
	public void RequestMeshData(MapData mapData, Action<MeshData> callback)
	{
		ThreadStart threadStart = delegate
		{
			MeshDataThread(mapData, callback);
		};
		
		new Thread(threadStart).Start();
	}
	
	void MeshDataThread(MapData mapData, Action<MeshData> callback)
	{
		var meshData = MeshGenerator.GenerateTerrainMesh(mapData.heightMap, meshHeightMultiplier, meshHeightCurve, levelOfDetail);
		lock (meshDataThreadInfoQueue)
		{
			meshDataThreadInfoQueue.Enqueue(new MapThreadInfo<MeshData>(callback, meshData));
		}
	}
	
	void Update()
	{
		if (mapDataThreadInfoQueue.Count > 0)
		{
			for (var i = 0; i < mapDataThreadInfoQueue.Count; i++)
			{
				var threadInfo = mapDataThreadInfoQueue.Dequeue();
				threadInfo.callback(threadInfo.parameter);
			}
		}
		
		if (meshDataThreadInfoQueue.Count > 0)
		{
			for (var i = 0; i < meshDataThreadInfoQueue.Count; i++)
			{
				var threadInfo = meshDataThreadInfoQueue.Dequeue();
				threadInfo.callback(threadInfo.parameter);
			}
		}
	}
	
	MapData GenerateMapData()
	{
		var noiseMap = Noise.GenerateNoiseMap(MAP_CHUNK_SIZE, MAP_CHUNK_SIZE, seed, noiseScale, octaves, persistence, lacunarity, offset);
		
		var colorMap = new Color[MAP_CHUNK_SIZE * MAP_CHUNK_SIZE];
		
		for (var x = 0; x < MAP_CHUNK_SIZE; x++)
		{
			for (var y = 0; y < MAP_CHUNK_SIZE; y++)
			{
				var currentHeight = noiseMap[x, y];
				for (var i = 0; i < regions.Length; i++)
				{
					if (currentHeight <= regions[i].height)
					{
						colorMap[y * MAP_CHUNK_SIZE + x] = regions[i].color;
						break;
					}
				}
			}
		}
		
		return new MapData(noiseMap, colorMap);
	}
	
	void OnValidate()
	{
		if (lacunarity < 1) lacunarity = 1;
		if (octaves < 0) octaves = 0;
	}
	
	struct MapThreadInfo<T>
	{
		public readonly Action<T> callback;
		public readonly T parameter;
		
		public MapThreadInfo(Action<T> callback, T parameter)
		{
			this.callback = callback;
			this.parameter = parameter;
		}
	}
}

[System.Serializable]
public struct TerrainType
{
	public string name;
	public float height;
	public Color color;
}

public struct MapData
{
	public readonly float[,] heightMap;
	public readonly Color[] colorMap;
	
	public MapData(float[,] heightMap, Color[] colorMap)
	{
		this.heightMap = heightMap;
		this.colorMap = colorMap;
	}
}
