using UnityEngine;
using UnityEngine.Tilemaps;

public class Enemy : MonoBehaviour
{
    public float runSpeed = 2f;
    public int scoreVal;
    public Tilemap groundTilemap;

    private int moveDirection = -1;

    void Update()
    {
        transform.position += new Vector3(moveDirection * runSpeed * Time.deltaTime, 0, 0);

        if (!IsGroundAhead() || IsObstacleAhead())
        {
            TurnAround();
        }
    }

    private void TurnAround()
    {
        moveDirection *= -1;
        transform.localScale = new Vector3(-moveDirection, 1, 1);
    }

    private bool IsGroundAhead()
    {
        float offset = 0.4f;
        float downDistance = 1.0f;
        Vector3 checkPos = transform.position + new Vector3(moveDirection * offset, 0, 0);
        Vector3Int cellPosition = groundTilemap.WorldToCell(checkPos + Vector3.down * downDistance);
        return groundTilemap.GetTile(cellPosition) != null;
    }

    private bool IsObstacleAhead()
    {
        float offset = 0.4f;
        Vector3 checkPos = transform.position + new Vector3(moveDirection * offset, 0, 0);
        Vector3Int cellPosition = groundTilemap.WorldToCell(checkPos);
        return groundTilemap.GetTile(cellPosition) != null;
    }

    public int Score => scoreVal;
}