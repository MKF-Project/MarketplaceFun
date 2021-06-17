using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneBuilder : MonoBehaviour
{
    public GameObject UnbreakableWallPrefab;
    
    public GameObject BreakableWallPrefab;

    public GameObject FloorPrefab;

    public Transform SceneManager;

    private void Awake()
    {
        Build(18, 12, 85);
    }

    public void Build(int width, int height, int percentage)
    {
        //validate if width or height are even then transform then in odd
        if (isEven(width))
        {
            width = width + 1;
        }
        if (isEven(height))
        {
            height = height + 1;
        }
        

        List<Vector3> listFreeSpaces = new List<Vector3>();
        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < height; z++)
            {
                //instantiate floor
                Vector3 position = new Vector3(x*10+5, 0, z*10+5);
                
                Instantiate(FloorPrefab, position, Quaternion.identity, SceneManager);
                
                position.y = 5.5f;
                
                //instantiate unbreakable walls
                if (isEven(x) && isEven(z))
                {
                    Instantiate(UnbreakableWallPrefab, position, Quaternion.identity, SceneManager);
                    continue;
                }

                //instantiate edges
                if (x == 0 || x == width - 1 || z == 0 || z == height - 1)
                {
                    Instantiate(UnbreakableWallPrefab, position, Quaternion.identity, SceneManager);
                    continue;
                }
                
                //verify if is not player spawn places
                if(x == 1)
                {
                    if (z == 1 || z == 2 || z == height - 2 || z == height - 3)
                    {
                        continue;
                    }
                }
                if(x == 2)
                {
                    if (z == 1 || z == height - 2 )
                    {
                        continue;
                    }
                }
                if(x == width - 2)
                {
                    if (z == 1 || z == 2 || z == height - 2 || z == height - 3)
                    {
                        continue;
                    }
                }
                if(x == width - 3)
                {
                    if (z == 1 || z == height - 2 )
                    {
                        continue;
                    }
                }
                
                listFreeSpaces.Add(position);
            }
        }


        int numberOfBreakableWalls = listFreeSpaces.Count * percentage / 100;
        Debug.Log(listFreeSpaces.Count);
        Debug.Log(numberOfBreakableWalls);
        while (numberOfBreakableWalls > 0)
        {
            int random = Random.Range(0, listFreeSpaces.Count - 1);
            Instantiate(BreakableWallPrefab, listFreeSpaces[random], Quaternion.identity, SceneManager);
            listFreeSpaces.RemoveAt(random);
            numberOfBreakableWalls--;
        }
        
    }

   

    public bool isEven(int value)
    {
        return value % 2 == 0;
    }



}
