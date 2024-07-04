using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelController : MonoBehaviour
{
    public List<Transform> SpawnPoints = new List<Transform>();
    public int spawnIndex = 1;
    public GameObject EnemyPrefab;
    public Transform PlayerPos;
}
