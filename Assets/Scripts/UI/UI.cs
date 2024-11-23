using Assets.Scripts.UI;
using System.Collections;
using System.Collections.Generic;
using Assets.Scripts;
using UnityEngine;
using Assets.Scripts.Core;
using System;
using Unity.VisualScripting;

public class UI : Singleton<UI>
{
    [SerializeField] private Tile _tilePrefab;
    private Transform _cam;
    [SerializeField] private Piece _piecePrefab;
    [SerializeField] private Vector3 _scale;

    protected override void Awake()
    {
        GameObject camera = GameObject.Find("Main Camera");
        _cam = camera.transform;
        base.Awake();
    }

    public void SpawnBoard()
    {
        for (int file = 0; file < 8; file++)
        {
            for (int rank = 0; rank < 8; rank++)
            {
                var spawnedTile = Instantiate(_tilePrefab, new Vector3(file, rank), Quaternion.identity);
                spawnedTile.name = Convert.ToString(Board.GetIndexFromPosition(file + 1, rank + 1));

                var isLight = (file % 2 == 0 && rank % 2 != 0) || (file % 2 != 0 && rank % 2 == 0);
                spawnedTile.Init(isLight);
            }
        }
        _cam.transform.position = new Vector3((float)4 - 0.5f, (float)4 - 0.5f, -10);
    }

    public void SpawnPieces()
    {
        for (int file = 0; file < 8; file++)
        {
            for (int rank = 0; rank < 8; rank++)
            {
                if (Board.Instance.Square[Board.GetIndexFromPosition(file + 1, rank + 1)] == Pieces.Empty) continue;
                var spawnedPiece = Instantiate(_piecePrefab, new Vector3(file, rank, -1), Quaternion.identity);
                spawnedPiece.name = $"Piece" + Convert.ToString(Board.GetIndexFromPosition(file + 1, rank + 1));
                spawnedPiece.ChangeSprite(Pieces.GetColor(Board.Instance.Square[Board.GetIndexFromPosition(file + 1, rank + 1)]), Pieces.GetPieceType(Board.Instance.Square[Board.GetIndexFromPosition(file + 1, rank + 1)]));
                spawnedPiece.transform.localScale = _scale;
            }
        }
        _cam.transform.position = new Vector3((float)4 - 0.5f, (float)4 - 0.5f, -10);
    }
}
