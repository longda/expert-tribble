using UnityEngine;
using System.Collections;

public class MapGenerator : MonoBehaviour 
{
	public int mapWidth;
	public int mapHeight;
	public float noiseScale;
	public bool autoGenerate;
	
	public void GenerateMap()
	{
		var noiseMap = Noise.GenerateNoiseMap(mapWidth, mapHeight, noiseScale);
		var display = FindObjectOfType<MapDisplay>();
		
		display.DrawNoiseMap(noiseMap);
	}
}
