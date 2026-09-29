using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelSpawner : MonoBehaviour
{
    public static LevelSpawner instance;

    public GameObject[] model;

    [HideInInspector]
    public GameObject[] modelPrefab = new GameObject[4];

    public GameObject WinPrefab;

    private GameObject temp1, temp2;

    public int level = 1;
    public int totalStacks = 0;

    [Header("Tower Height Settings")]
    public int baseStackCount = 50;     // Starting rings for Level 1
    public int extraStacksPerLevel = 5; // Extra rings added per level

    float i = 0;

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        level = PlayerPrefs.GetInt("Level", 1);

        ModelSelection();
        float random = Random.value;
        totalStacks = 0;

        int totalRingsToSpawn = baseStackCount + ((level - 1) * extraStacksPerLevel);
        float endY = -(totalRingsToSpawn * 0.5f);

        for (i = 0; i > endY; i -= 0.5f)
        {
            if (level <= 20)
                temp1 = Instantiate(modelPrefab[Random.Range(0, 2)]);

            if (level > 20 && level <= 50)
                temp1 = Instantiate(modelPrefab[Random.Range(1, 3)]);

            if (level > 50 && level <= 100)
                temp1 = Instantiate(modelPrefab[Random.Range(2, 4)]);

            if (level > 100)
                temp1 = Instantiate(modelPrefab[Random.Range(3, 4)]);

            temp1.transform.position = new Vector3(0, i - 0.01f, 0);
            temp1.transform.eulerAngles = new Vector3(0, i * 8, 0);

            if (Mathf.Abs(i) >= totalRingsToSpawn * 0.15f && Mathf.Abs(i) <= totalRingsToSpawn * 0.35f)
            {
                temp1.transform.eulerAngles = new Vector3(0, i * 8, 0);
                temp1.transform.eulerAngles += Vector3.up * 180;
            }
            else if (Mathf.Abs(i) >= totalRingsToSpawn * 0.45f)
            {
                temp1.transform.eulerAngles = new Vector3(0, i * 8, 0);

                if (random > .75f)
                    temp1.transform.eulerAngles += Vector3.up * 180;
            }

            temp1.transform.parent = Object.FindFirstObjectByType<Rotator>().transform;
            totalStacks++;
        }

        temp2 = Instantiate(WinPrefab);
        temp2.transform.position = new Vector3(0, i - 0.01f, 0);
    }

    void ModelSelection()
    {
        int currentPattern = (level - 1) % 5;

        switch (currentPattern)
        {
            case 0:
                for (int j = 0; j < 4; j++)
                    modelPrefab[j] = model[j];
                break;

            case 1:
                for (int j = 0; j < 4; j++)
                    modelPrefab[j] = model[j + 4];
                break;

            case 2:
                for (int j = 0; j < 4; j++)
                    modelPrefab[j] = model[j + 8];
                break;

            case 3:
                for (int j = 0; j < 4; j++)
                    modelPrefab[j] = model[j + 12];
                break;

            case 4:
                for (int j = 0; j < 4; j++)
                    modelPrefab[j] = model[j + 16];
                break;
        }
    }

    public void NextLevel()
    {
        int current = PlayerPrefs.GetInt("Level", 1);
        PlayerPrefs.SetInt("Level", current + 1);
        PlayerPrefs.Save();

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}