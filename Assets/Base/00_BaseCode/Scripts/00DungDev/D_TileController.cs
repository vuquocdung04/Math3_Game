using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
public class D_TileController : MonoBehaviour
{
    [SerializeField] D_Tile tilePrefab;

    [SerializeField] D_Tile tile1, tile2;
    [SerializeField] D_Tile selectTile;

    public bool isTest;
    public List<D_Tile> lsTiles = new();

    private void Update()
    {
        this.SelectTile1();
        this.SelectTile2();
    }

    void SelectTile1()
    {
        if (!Input.GetMouseButtonDown(0)) return;

        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector3.zero);
        if (hit.collider != null)
        {
            selectTile = hit.collider.GetComponent<D_Tile>();
            if (tile1 == null)
            {
                tile1 = selectTile; 
                return;
            }
        }
    }

    void SelectTile2()
    {
        if (!Input.GetMouseButton(0)) return;

        if (tile1 == null) return;
        if (tile2 != null) return;

        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector3.zero);

        if (hit.collider == null) return;

        selectTile = hit.collider.GetComponent<D_Tile>();

        if (tile1 != null && tile1.GetInstanceID() == selectTile.GetInstanceID()) return;

        Debug.LogError(Mathf.Abs(selectTile.posX - tile1.posX) + Mathf.Abs(selectTile.posY - tile1.posY));

        if((Mathf.Abs(selectTile.posX - tile1.posX) + Mathf.Abs(selectTile.posY - tile1.posY)) == 1)
        {
            tile2 = selectTile;
        }

        if (tile2 == null)
        {
            tile1 = null;
            return;
        }
        this.SwapTile(tile1, tile2, () =>
            {
                if (!CheckHorizontal() || !CheckVertical())
                {
                    this.SwapTile(tile1,tile2);
                }
                else
                {
                    foreach (var tile in GamePlayController.Instance.levelDesign.lsTiles) tile.SetTileNeighbor();
                }
                tile1 = null;
                tile2 = null;
                selectTile = null;
            });

    }

    void SwapTile(D_Tile tile1Param, D_Tile tile2Param, System.Action callback = null)
    {
        Vector3 pos1 = tile1Param.transform.position;
        Vector2 index1 = new Vector2(tile1.posX, tile1.posY);

        Vector3 pos2= tile2Param.transform.position;
        Vector2 index2 = new Vector2(tile2.posX, tile2.posY);

        tile1Param.transform.DOMove(pos2, 0.2f).Play();

        tile2Param.transform.DOMove(pos1, 0.2f).OnComplete(() => {
            tile1Param.UpdateIndex(index2);
            tile2Param.UpdateIndex(index1);

            UpdateReleventNeighbors(tile1Param, tile2Param);

            callback?.Invoke();
        }).Play();
    }



    public D_Tile GetTile(int posX, int posY)
    {
        if (posX < 0 || posY < 0) return null;
        if (posX >= GamePlayController.Instance.levelDesign.row || posY >= GamePlayController.Instance.levelDesign.col) return null;

        foreach (var tile in GamePlayController.Instance.levelDesign.lsTiles) if (tile.posX == posX && tile.posY == posY) return tile;

        return null;
    }

    void UpdateReleventNeighbors(D_Tile tile1, D_Tile tile2)
    {
        tile1.SetTileNeighbor();
        tile2.SetTileNeighbor();

        UpdateNeighborIfExists(tile1.tileRight);
        UpdateNeighborIfExists(tile1.tileLeft);
        UpdateNeighborIfExists(tile1.tileUp);
        UpdateNeighborIfExists(tile1.tileDown);

        UpdateNeighborIfExists(tile2.tileRight);
        UpdateNeighborIfExists(tile2.tileLeft);
        UpdateNeighborIfExists(tile2.tileUp);
        UpdateNeighborIfExists(tile2.tileDown);
    }

    void UpdateNeighborIfExists(D_Tile tile)
    {
        if(tile != null) tile.SetTileNeighbor();
    }

    //check tile
    bool CheckHorizontal() //x
    {
        if (isTest) return false;

        if (tile1.tileRight.type == tile1.type && tile1.tileLeft.type == tile1.type)
        {
            Destroy(tile1.gameObject);
            Destroy(tile1.tileRight.gameObject);
            Destroy(tile1.tileLeft.gameObject);
            Debug.LogError("Horizontai - between");
            return true;
        }
        if (tile1.tileLeft.type == tile1.tileLeft.tileLeft.type && tile1.type == tile1.tileLeft.type)
        {
            Destroy(tile1.gameObject);
            Destroy(tile1.tileLeft.gameObject);
            Destroy(tile1.tileLeft.tileLeft.gameObject);
            Debug.LogError("Horizontai - Left");

            return true;
        }

        if (tile1.type == tile1.tileRight.tileRight.type && tile1.type == tile1.tileRight.type)
        {
            Destroy(tile1.gameObject);
            Destroy(tile1.tileRight.gameObject);
            Destroy(tile1.tileRight.tileRight.gameObject);

            Debug.LogError("Horizontai - Right");

            return true;
        }
        return false;
    }

    bool CheckVertical() //y
    {
        if (isTest) return false;

        if (tile1.type == tile1.tileUp.type && tile1.type == tile1.tileDown.type)
        {
            Destroy(tile1.gameObject);
            Destroy(tile1.tileDown.gameObject);
            Destroy(tile1.tileUp.gameObject);
            Debug.LogError("Vertical - between");

            return true;
        }
        if(tile1.type == tile1.tileDown.tileDown.type && tile1.type == tile1.tileDown.type)
        {
            Destroy(tile1.gameObject);
            Destroy(tile1.tileDown.gameObject);
            Destroy(tile1.tileDown.tileDown.gameObject);
            Debug.LogError("Vertical - down");

            return true;
        }

        if (tile1.type == tile1.tileUp.tileUp.type && tile1.type == tile1.tileUp.type)
        {
            Destroy(tile1.gameObject);
            Destroy(tile1.tileUp.gameObject);
            Destroy(tile1.tileUp.tileUp.gameObject);
            Debug.LogError("Vertical - Up");

            return true;
        }
        return false;

    }
}
