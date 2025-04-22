using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum D_TipeType
{
    type0,type1, type2, type3, type4, type5, type6,
}

public class D_Tile : MonoBehaviour
{
    public D_TipeType type;

    public D_Tile tileNeighborRight;
    public D_Tile tileNeighborLeft;
    public D_Tile tileNeighborUp;
    public D_Tile tileNeighborDown;

    [SerializeField] SpriteRenderer spriteRenderer;
    public SpriteRenderer SpriteRenderer => spriteRenderer;

    public int posX,posY;
    public void UpdateIndex(Vector2 index)
    {
        posX = (int)index.x;
        posY = (int)index.y;
        this.SetTileNeighbor();
    }
    public void SetTileNeighbor()
    {
        var tileCtrl = GamePlayController.Instance.tileController;
        this.tileNeighborRight = tileCtrl.GetTile(posX+1,posY);
        this.tileNeighborLeft = tileCtrl.GetTile(posX-1,posY);
        this.tileNeighborUp = tileCtrl.GetTile(posX,posY+1);
        this.tileNeighborDown = tileCtrl.GetTile(posX,posY-1);
    }


}
