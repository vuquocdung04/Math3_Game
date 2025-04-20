using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
public class D_LevelDesign : MonoBehaviour
{
    public int row, col;
    public int tileAmount;
    [SerializeField] D_Tile tilePrefab;
    [Space(10)]
    public List<Color> lsTileColors = new List<Color>(5);
    public List<D_Tile> lsTiles = new();
    [Space(10)]
    [Header("Matrix")]
    [ShowInInspector]
    public int[,] matrix;

    private void Start()
    {
        this.GenerateMatrix();
    }

    [Button("Sinh Matrix",ButtonSizes.Large)]

    void GenerateMatrix()
    {
        matrix = new int[row,col];

        for (int i = 0; i < row; i++)
        {
            for(int j = 0; j < col; j++)
            {
                int rand = Random.Range(0, tileAmount);
                matrix[i, j] = rand;
                var tile = Instantiate(tilePrefab, new Vector2(i, j), Quaternion.identity);
                tile.posX = i;
                tile.posY = j;
                lsTiles.Add(tile);
                switch (rand)
                {
                    case 0:
                        tile.type = D_TipeType.type0;
                        tile.SpriteRenderer.color = lsTileColors[rand];
                        break;
                    case 1:
                        tile.type = D_TipeType.type1;
                        tile.SpriteRenderer.color = lsTileColors[rand];
                        break;
                    case 2:
                        tile.type = D_TipeType.type2;
                        tile.SpriteRenderer.color = lsTileColors[rand];
                        break;
                    case 3:
                        tile.type = D_TipeType.type3;
                        tile.SpriteRenderer.color = lsTileColors[rand];
                        break;
                    case 4:
                        tile.type = D_TipeType.type4;
                        tile.SpriteRenderer.color = lsTileColors[rand];
                        break;
                    case 5:
                        tile.type = D_TipeType.type5;
                        tile.SpriteRenderer.color = lsTileColors[rand];
                        break;
                }
            }
        }

        foreach (var tile in this.lsTiles) tile.SetTileNeighbor();

        this.CheckMatch();
    }

    [Button("Delete Matrix", ButtonSizes.Large)]
    void ClearMatrix()
    {
        for(int i = 0; i < row; i++)
        {
            for (int j = 0; j < col; j++)
            {
                matrix[i, j] = -1;
            }
        }

        foreach(var child in this.lsTiles)
        {
            DestroyImmediate(child.gameObject);
        }
        lsTiles.Clear();
    }

    void CheckMatch()
    {
        for (int i = 0; i < row ; i++)
        {
            for (int j = 0; j < col; j++)
            {
                if(j > 0 && j <= col - 2)
                {
                    if (matrix[i, j] == matrix[i, j + 1] && matrix[i, j] == matrix[i, j - 1])
                    {
                        matrix[i, j] = 10;

                    }
                }
                if (i > 0 && i <= row - 2)
                {
                    if (matrix[i, j] == matrix[i + 1, j] && matrix[i, j] == matrix[i - 1, j])
                    {
                        matrix[i, j] = 10;
                    }
                }
                
            }
        }

    }

    void Swap(int x1, int y1, int x2, int y2)
    {
        int temp = matrix[x1, y1];
        matrix[x1, y1] = matrix[x2, y2];
        matrix[x2, y2] = temp;
    }
}
