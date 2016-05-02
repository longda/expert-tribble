using UnityEngine;
using System.Collections;

public class MapGenerator : MonoBehaviour 
{
	public enum DrawMode { NoiseMap, ColorMap }
	public DrawMode drawMode;
	
	public int mapWidth;
	public int mapHeight;
	public float noiseScale;
	
	public int octaves;
	[Range(0, 1)]
	public float persistence;
	public float lacunarity;
	
	public int seed;
	public Vector2 offset;
	
	public bool autoGenerate;
	
	public TerrainType[] regions;
	
	public void GenerateMap()
	{
		var noiseMap = Noise.GenerateNoiseMap(mapWidth, mapHeight, seed, noiseScale, octaves, persistence, lacunarity, offset);
		var display = FindObjectOfType<MapDisplay>();
		var colorMap = new Color[mapWidth * mapHeight];
		
		for (var x = 0; x < mapWidth; x++)
		{
			for (var y = 0; y < mapHeight; y++)
			{
				var currentHeight = noiseMap[x, y];
				for (var i = 0; i < regions.Length; i++)
				{
					if (currentHeight <= regions[i].height)
					{
						colorMap[y * mapWidth + x] = regions[i].color;
						break;
					}
				}
			}
		}
		
		if (drawMode == DrawMode.NoiseMap)
		{
			display.DrawTexture(TextureGenerator.TextureFromHeightMap(noiseMap));	
		}
		else if (drawMode == DrawMode.ColorMap)
		{
			display.DrawTexture(TextureGenerator.TextureFromColorMap(colorMap, mapWidth, mapHeight));
		}
	}
	
	void OnValidate()
	{
		if (mapWidth < 1) mapWidth = 1;
		if (mapHeight < 1) mapHeight = 1;
		if (lacunarity < 1) lacunarity = 1;
		if (octaves < 0) octaves = 0;
	}
}

[System.Serializable]
public struct TerrainType
{
	public string name;
	public float height;
	public Color color;
}
