using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("Configuración del Spawn")]
    public GameObject enemyPrefab;       // Prefab del enemigo a spawnear
    public Transform[] spawnPoints;     // Puntos donde pueden aparecer los enemigos
    public float spawnInterval = 2f;    // Intervalo de tiempo entre spawns
    public int maxEnemies = 10;         // Máximo número de enemigos activos

    private int currentEnemyCount = 0;  // Número actual de enemigos activos

    void Start()
    {
        // Inicia el spawner
        StartCoroutine(SpawnEnemies());
    }

    IEnumerator SpawnEnemies()
    {
        while (true)
        {
            // Si no se supera el límite de enemigos
            if (currentEnemyCount < maxEnemies)
            {
                SpawnEnemy();
            }
            yield return new WaitForSeconds(spawnInterval); // Espera el intervalo antes de generar otro
        }
    }

    void SpawnEnemy()
    {
        if (spawnPoints.Length > 0)
        {
            // Selecciona un punto aleatorio
            Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];

            // Genera el enemigo
            Instantiate(enemyPrefab, spawnPoint.position, spawnPoint.rotation);
            currentEnemyCount++;
        }
    }

    public void EnemyDefeated()
    {
        // Llamar esta función cuando un enemigo es derrotado para reducir el conteo
        currentEnemyCount--;
    }
}