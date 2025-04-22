using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Sirenix.OdinInspector;
using System.Linq;
public class D_TileController : MonoBehaviour
{
    [SerializeField] D_Tile tile1, tile2;
    [SerializeField] D_Tile selectTile;

    [Space(10)]
    [SerializeField] List<D_Tile> horizontal = new();
    [SerializeField] List<D_Tile> vertical = new();

    [ShowInInspector] HashSet<D_Tile> hsMatch = new();
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
                if (!this.IsMatch())
                {
                    this.SwapTile(tile1,tile2);
                }
                else
                {
                    this.ClearMatch();
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


    void ClearMatch()
    {
        this.hsMatch.Clear();

        hsMatch.UnionWith(horizontal);
        hsMatch.UnionWith(vertical);


        var tilesByType = hsMatch.GroupBy(t => t.type);
        foreach (var group in tilesByType)
        {
            // Nếu có từ 3 tile cùng loại trở lên thì destroy
            if (group.Count() >= 3)
            {
                foreach (var tile in group)
                {
                    tile.transform.DOScale(0f, 0.5f)
                        .SetEase(Ease.OutQuad)
                        .OnComplete(() => Destroy(tile.gameObject));
                    Debug.LogError(tile.name);
                }
            }
        }
    }


    bool IsMatch()
    {
        this.horizontal.Clear();
        this.vertical.Clear();

        float sumHorizontal_1 = this.CheckHorizontal_Right(tile1) + this.CheckHorizontal_Left(tile1);
        float sumVertical_1 = this.CheckVertical_Up(tile1) + this.CheckVertical_Down(tile1);

        float sumHorizontal_2 = this.CheckHorizontal_Right(tile2) + this.CheckHorizontal_Left(tile2);
        float sumVertical_2 = this.CheckVertical_Up(tile2) + this.CheckVertical_Down(tile2);

        if (sumHorizontal_1 > 1) this.horizontal.Add(tile1);
        if (sumHorizontal_2 > 1) this.horizontal.Add(tile2);

        if (sumVertical_1 > 1) this.vertical.Add(tile1);
        if (sumVertical_2 > 1) this.vertical.Add(tile2);

        return sumHorizontal_1 > 1 || sumHorizontal_2 > 1 || sumVertical_1 > 1 || sumVertical_2 > 1;
    }


    int CheckHorizontal_Left(D_Tile tile)
    {
        if(tile.tileNeighborLeft == null) return 0;

        if(tile.type == tile.tileNeighborLeft.type)
        {
            this.horizontal.Add(tile.tileNeighborLeft);
            return this.CheckHorizontal_Left(tile.tileNeighborLeft) + 1;
        }
        return 0;
    }

    int CheckHorizontal_Right(D_Tile tile)
    {
        if (tile.tileNeighborRight == null) return 0;

        if (tile.type == tile.tileNeighborRight.type)
        {
            this.horizontal.Add(tile.tileNeighborRight);
            return this.CheckHorizontal_Right(tile.tileNeighborRight) + 1;
        }
        return 0;
    }

    int CheckVertical_Up(D_Tile tile)
    {
        if (tile.tileNeighborUp == null) return 0;

        if (tile.type == tile.tileNeighborUp.type)
        {
            this.vertical.Add(tile.tileNeighborUp);
            return this.CheckVertical_Up(tile.tileNeighborUp) + 1;
        }
        return 0;
    }
    int CheckVertical_Down(D_Tile tile)
    {
        if (tile.tileNeighborDown == null) return 0;

        if (tile.type == tile.tileNeighborDown.type)
        {
            this.vertical.Add(tile.tileNeighborDown);
            return this.CheckVertical_Down(tile.tileNeighborDown) + 1;
        }
        return 0;
    }


    /// ODIN
    /// 
    [Button("Sinh Matrix", ButtonSizes.Large)]
    void S()
    {
        GamePlayController.Instance.levelDesign.GenerateMatrix();
    }

    [Button("Delete Matrix", ButtonSizes.Large)]
    void D()
    {
        GamePlayController.Instance.levelDesign.ClearMatrix();
    }
}
