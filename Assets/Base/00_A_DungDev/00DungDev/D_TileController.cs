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

        foreach (var tile in GamePlayController.Instance.levelDesign.lsTiles)
        {
            if (tile.posX == posX && tile.posY == posY) return tile;
        }

        return null;
    }


    void ClearMatch()
    {
        this.hsMatch.Clear();

        hsMatch.UnionWith(horizontal);
        hsMatch.UnionWith(vertical);

        var tilesByType = hsMatch.GroupBy(t => t.type);
        List<D_Tile> tilesToRemove = new List<D_Tile>();

        foreach (var group in tilesByType)
        {
            // Nếu có 3 ô trở lên cùng loại, hủy chúng
            if (group.Count() >= 3)
            {
                foreach (var tile in group)
                {
                    tilesToRemove.Add(tile);
                }
            }
        }

        if (tilesToRemove.Count > 0)
        {
            StartCoroutine(RemoveTilesAndHandleFalling(tilesToRemove));
        }
    }

    IEnumerator RemoveTilesAndHandleFalling(List<D_Tile> tilesToRemove)
    {
        // Đầu tiên, xóa tham chiếu láng giềng cho tất cả các ô sẽ bị xóa
        foreach (var tile in tilesToRemove)
        {
            tile.ClearNeighborRef();

            // Xóa khỏi danh sách thiết kế cấp độ
            GamePlayController.Instance.levelDesign.lsTiles.Remove(tile);

            // Tạo hoạt ảnh hủy ô
            tile.transform.DOScale(0f, 0.3f)
                .SetEase(Ease.OutQuad)
                .OnComplete(() => Destroy(tile.gameObject));
        }

        // Đợi hoạt ảnh xóa hoàn thành
        yield return new WaitForSeconds(0.3f);

        // Xử lý các ô rơi xuống
        HandleTileFalling();
    }

    void HandleTileFalling()
    {
        bool needToCheck = false;
        Dictionary<int, int> columnsNeedingNewTiles = new Dictionary<int, int>(); // <cột, số ô cần tạo>

        // Bắt đầu từ hàng dưới cùng và kiểm tra từng cột
        for (int x = 0; x < GamePlayController.Instance.levelDesign.row; x++)
        {
            // Đếm số ô trống trong cột này
            int emptySlots = 0;

            // Kiểm tra từng vị trí trong cột từ dưới lên trên
            for (int y = 0; y < GamePlayController.Instance.levelDesign.col; y++)
            {
                // Lấy ô hiện tại tại vị trí này
                D_Tile currentTile = GetTile(x, y);

                if (currentTile == null)
                {
                    // Nếu không có ô, tăng bộ đếm ô trống
                    emptySlots++;
                }
                else if (emptySlots > 0)
                {
                    // Nếu chúng ta có ô trống bên dưới và tìm thấy một ô, di chuyển nó xuống

                    // Tính toán vị trí mới
                    int newY = y - emptySlots;
                    Vector3 newPosition = new Vector3(x, newY, 0);

                    // Tạo hoạt ảnh ô rơi
                    currentTile.transform.DOMove(newPosition, 0.3f).SetEase(Ease.OutBounce);

                    // Cập nhật chỉ số ô
                    currentTile.UpdateIndex(new Vector2(x, newY));

                    // Chúng ta đã di chuyển một ô, vì vậy cần kiểm tra các ô khớp sau khi tất cả các ô đã rơi
                    needToCheck = true;
                }
            }

            // Ghi lại các cột cần tạo ô mới
            if (emptySlots > 0)
            {
                columnsNeedingNewTiles[x] = emptySlots;
                needToCheck = true;
            }
        }

        // Nếu có ô trống, tạo ô mới sau khi các ô hiện có đã rơi
        if (columnsNeedingNewTiles.Count > 0)
        {
            StartCoroutine(SpawnNewTilesAfterFalling(columnsNeedingNewTiles, needToCheck));
        }
        else if (needToCheck)
        {
            // Nếu không cần tạo ô mới nhưng có ô đã di chuyển, kiểm tra các ô khớp mới
            StartCoroutine(CheckMatchesAfterFalling());
        }
    }

    IEnumerator SpawnNewTilesAfterFalling(Dictionary<int, int> columnsNeedingNewTiles, bool needToCheck)
    {
        // Đợi cho các ô hiện có hoàn thành việc rơi
        yield return new WaitForSeconds(0.3f);

        // Thiết lập các ô láng giềng cho tất cả các ô hiện có
        foreach (var tile in GamePlayController.Instance.levelDesign.lsTiles)
        {
            tile.SetTileNeighbor();
        }

        // Kiểm tra xem còn nước đi tiềm năng không
        bool hasPotentialMoves = HasPotentialMoves();

        // Tạo các ô mới ở trên cùng của mỗi cột cần ô mới
        foreach (var columnData in columnsNeedingNewTiles)
        {
            int x = columnData.Key;
            int emptySlots = columnData.Value;

            if (!hasPotentialMoves)
            {
                // Nếu không còn nước đi tiềm năng, sử dụng logic thông minh
                SpawnSmartTilesInColumn(x, emptySlots);
            }
            else
            {
                // Nếu còn nước đi tiềm năng, sử dụng logic thông thường
                SpawnNewTilesInColumn(x, emptySlots);
            }
        }

        // Đợi cho các ô mới hoàn thành việc rơi
        yield return new WaitForSeconds(0.5f);

        // Thiết lập các ô láng giềng cho tất cả các ô
        foreach (var tile in GamePlayController.Instance.levelDesign.lsTiles)
        {
            tile.SetTileNeighbor();
        }

        // Kiểm tra các ô khớp mới
        if (needToCheck)
        {
            bool hasMatches = CheckForMatches();

            if (hasMatches)
            {
                ClearMatch();
            }
        }
    }
    void SpawnNewTilesInColumn(int x, int emptySlots)
    {
        var levelDesign = GamePlayController.Instance.levelDesign;

        // Tạo các ô mới ở trên cùng của cột
        for (int i = 0; i < emptySlots; i++)
        {
            int y = levelDesign.col - i - 1;

            // Tạo loại ô ngẫu nhiên
            int rand = Random.Range(0, levelDesign.tileAmount);

            // Khởi tạo ô mới phía trên lưới
            Vector3 spawnPosition = new Vector3(x, levelDesign.col + i, 0);
            D_Tile newTile = Instantiate(levelDesign.tilePrefab, spawnPosition, Quaternion.identity);

            // Thiết lập thuộc tính ô
            newTile.posX = x;
            newTile.posY = y;
            levelDesign.lsTiles.Add(newTile);

            // Thiết lập loại và sprite của ô
            newTile.type = (D_TipeType)rand;
            newTile.SpriteRenderer.sprite = levelDesign.lsTileSprite[rand];
            newTile.name = "Tile" + newTile.type.ToString();

            // Tạo hoạt ảnh ô rơi từ trên xuống
            Vector3 targetPosition = new Vector3(x, y, 0);
            float delay = 0.05f * i; // Độ trễ nhỏ để tạo hiệu ứng xếp tầng
            newTile.transform.DOMove(targetPosition, 0.3f).SetEase(Ease.OutBounce).SetDelay(delay);
        }
    }
    IEnumerator CheckMatchesAfterFalling()
    {
        // Đợi cho các ô hoàn thành việc rơi
        yield return new WaitForSeconds(0.5f);

        // Thiết lập các ô láng giềng cho tất cả các ô
        foreach (var tile in GamePlayController.Instance.levelDesign.lsTiles)
        {
            tile.SetTileNeighbor();
        }

        // Kiểm tra các ô khớp mới
        bool hasMatches = CheckForMatches();

        if (hasMatches)
        {
            ClearMatch();
        }
        // Sau này chúng ta sẽ thêm xử lý tạo ô mới ở đây
    }

    // Kiểm tra xem còn nước đi tiềm năng không
    bool HasPotentialMoves()
    {
        var levelDesign = GamePlayController.Instance.levelDesign;
        List<D_Tile> tiles = levelDesign.lsTiles;

        // Kiểm tra từng ô
        foreach (var tile in tiles)
        {
            // Kiểm tra hoán đổi với ô bên phải
            if (tile.tileNeighborRight != null)
            {
                // Hoán đổi tạm thời
                D_TipeType tempType = tile.type;
                tile.type = tile.tileNeighborRight.type;
                tile.tileNeighborRight.type = tempType;

                // Kiểm tra xem có tạo match-3 không
                bool hasMatch = CheckPotentialMatch(tile) || CheckPotentialMatch(tile.tileNeighborRight);

                // Hoàn trả lại loại ban đầu
                tile.tileNeighborRight.type = tile.type;
                tile.type = tempType;

                if (hasMatch)
                    return true;
            }

            // Kiểm tra hoán đổi với ô bên trên
            if (tile.tileNeighborUp != null)
            {
                // Hoán đổi tạm thời
                D_TipeType tempType = tile.type;
                tile.type = tile.tileNeighborUp.type;
                tile.tileNeighborUp.type = tempType;

                // Kiểm tra xem có tạo match-3 không
                bool hasMatch = CheckPotentialMatch(tile) || CheckPotentialMatch(tile.tileNeighborUp);

                // Hoàn trả lại loại ban đầu
                tile.tileNeighborUp.type = tile.type;
                tile.type = tempType;

                if (hasMatch)
                    return true;
            }
        }

        return false;
    }

    // Kiểm tra xem một ô có tạo thành match-3 không
    bool CheckPotentialMatch(D_Tile tile)
    {
        // Đếm số ô cùng loại theo chiều ngang (trái + phải)
        int horizontalCount = 1; // Bao gồm ô hiện tại

        // Kiểm tra sang phải
        D_Tile rightTile = tile.tileNeighborRight;
        while (rightTile != null && rightTile.type == tile.type)
        {
            horizontalCount++;
            rightTile = rightTile.tileNeighborRight;
        }

        // Kiểm tra sang trái
        D_Tile leftTile = tile.tileNeighborLeft;
        while (leftTile != null && leftTile.type == tile.type)
        {
            horizontalCount++;
            leftTile = leftTile.tileNeighborLeft;
        }

        // Đếm số ô cùng loại theo chiều dọc (trên + dưới)
        int verticalCount = 1; // Bao gồm ô hiện tại

        // Kiểm tra lên trên
        D_Tile upTile = tile.tileNeighborUp;
        while (upTile != null && upTile.type == tile.type)
        {
            verticalCount++;
            upTile = upTile.tileNeighborUp;
        }

        // Kiểm tra xuống dưới
        D_Tile downTile = tile.tileNeighborDown;
        while (downTile != null && downTile.type == tile.type)
        {
            verticalCount++;
            downTile = downTile.tileNeighborDown;
        }

        // Nếu có ít nhất 3 ô cùng loại theo chiều ngang hoặc dọc
        return horizontalCount >= 3 || verticalCount >= 3;
    }

    // Tạo ô mới thông minh để đảm bảo có ít nhất một nước đi tiềm năng
    void SpawnSmartTilesInColumn(int x, int emptySlots)
    {
        var levelDesign = GamePlayController.Instance.levelDesign;
        List<D_Tile> newTiles = new List<D_Tile>();

        // Tạo các ô mới ở trên cùng của cột
        for (int i = 0; i < emptySlots; i++)
        {
            int y = levelDesign.col - i - 1;

            // Khởi tạo ô mới phía trên lưới
            Vector3 spawnPosition = new Vector3(x, levelDesign.col + i, 0);
            D_Tile newTile = Instantiate(levelDesign.tilePrefab, spawnPosition, Quaternion.identity);

            // Thiết lập thuộc tính ô (ngoại trừ loại)
            newTile.posX = x;
            newTile.posY = y;
            levelDesign.lsTiles.Add(newTile);
            newTiles.Add(newTile);

            // Tạm thời đặt loại ngẫu nhiên, sẽ được cập nhật sau
            int rand = Random.Range(0, levelDesign.tileAmount);
            newTile.type = (D_TipeType)rand;
            newTile.SpriteRenderer.sprite = levelDesign.lsTileSprite[rand];
            newTile.name = "Tile" + newTile.type.ToString();
        }

        // Thiết lập các ô láng giềng cho các ô mới
        foreach (var tile in newTiles)
        {
            tile.SetTileNeighbor();
        }

        // Thử tạo nước đi tiềm năng với các ô mới
        if (!TryCreatePotentialMove(newTiles))
        {
            // Nếu không thể tạo nước đi tiềm năng, đặt loại ngẫu nhiên
            foreach (var tile in newTiles)
            {
                int rand = Random.Range(0, levelDesign.tileAmount);
                tile.type = (D_TipeType)rand;
                tile.SpriteRenderer.sprite = levelDesign.lsTileSprite[rand];
                tile.name = "Tile" + tile.type.ToString();
            }
        }

        // Tạo hoạt ảnh cho các ô mới rơi xuống
        for (int i = 0; i < newTiles.Count; i++)
        {
            D_Tile tile = newTiles[i];
            Vector3 targetPosition = new Vector3(x, tile.posY, 0);
            float delay = 0.05f * i; // Độ trễ nhỏ để tạo hiệu ứng xếp tầng
            tile.transform.DOMove(targetPosition, 0.3f).SetEase(Ease.OutBounce).SetDelay(delay);
        }
    }

    // Thử tạo nước đi tiềm năng với các ô mới
    bool TryCreatePotentialMove(List<D_Tile> newTiles)
    {
        // Danh sách các loại có thể
        List<D_TipeType> possibleTypes = new List<D_TipeType>();
        for (int i = 0; i < GamePlayController.Instance.levelDesign.tileAmount; i++)
        {
            possibleTypes.Add((D_TipeType)i);
        }

        // Thử tối đa 10 lần để tạo nước đi tiềm năng
        for (int attempt = 0; attempt < 10; attempt++)
        {
            // Thử đặt loại ngẫu nhiên cho các ô mới
            foreach (var tile in newTiles)
            {
                int randIndex = Random.Range(0, possibleTypes.Count);
                tile.type = possibleTypes[randIndex];
                tile.SpriteRenderer.sprite = GamePlayController.Instance.levelDesign.lsTileSprite[(int)tile.type];
                tile.name = "Tile" + tile.type.ToString();
            }

            // Kiểm tra xem có nước đi tiềm năng không
            if (HasPotentialMoves())
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    bool CheckForMatches()
    {
        this.hsMatch.Clear();
        this.horizontal.Clear();
        this.vertical.Clear();

        bool foundMatches = false;

        // Kiểm tra ô khớp cho từng ô
        foreach (var tile in GamePlayController.Instance.levelDesign.lsTiles)
        {
            float sumHorizontal = this.CheckHorizontal_Right(tile) + this.CheckHorizontal_Left(tile);
            float sumVertical = this.CheckVertical_Up(tile) + this.CheckVertical_Down(tile);

            if (sumHorizontal > 1)
            {
                this.horizontal.Add(tile);
                foundMatches = true;
            }

            if (sumVertical > 1)
            {
                this.vertical.Add(tile);
                foundMatches = true;
            }
        }

        return foundMatches;
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
