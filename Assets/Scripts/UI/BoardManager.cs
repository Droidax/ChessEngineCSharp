using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using Assets.Scripts.Core;
using Assets.Scripts.UI.Cursor;
using Unity.VisualScripting;
using UnityEngine;
using Object = System.Object;

namespace Assets.Scripts.UI
{
    public class BoardUi : MonoBehaviour
    {
        [SerializeField] private Tile _tilePrefab;
        [SerializeField] private Transform _cam;
        [SerializeField] private Piece _piecePrefab;
        [SerializeField] private Vector3 _scale;
        public MoveGenerator MoveGenerator;



        void Start()
        {
            Board.Instance.SetNewBoard();
            MoveGenerator moveGenerator = new MoveGenerator(Board.Instance);
            moveGenerator.GenerateLegalMoves();
            GenerateBoard();

        }
        void GenerateBoard()
        {
            for (var file = 0; file < 8; file++)
            {
                for (var rank = 0; rank < 8; rank++)
                {
                    var spawnedTile = Instantiate(_tilePrefab, new Vector3(file, rank), Quaternion.identity);
                    spawnedTile.name = Convert.ToString(Board.GetIndexFromPosition(file + 1, rank + 1));
                    spawnedTile.AddComponent<BoxCollider2D>();

                    var isLight = (file % 2 == 0 && rank % 2 != 0) || (file % 2 != 0 && rank % 2 == 0);
                    spawnedTile.Init(isLight);

                    if (Board.Instance.Square[Board.GetIndexFromPosition(file + 1, rank + 1)] == Pieces.Empty) continue;
                    var spawnedPiece = Instantiate(_piecePrefab, new Vector3(file, rank, -1), Quaternion.identity);
                    spawnedPiece.name = $"Piece" + Convert.ToString(Board.GetIndexFromPosition(file + 1, rank + 1));
                    spawnedPiece.ChangeSprite(Pieces.GetColor(Board.Instance.Square[Board.GetIndexFromPosition(file + 1, rank + 1)]), Pieces.GetPieceType(Board.Instance.Square[Board.GetIndexFromPosition(file + 1, rank + 1)]));
                    spawnedPiece.transform.localScale = _scale;
                    spawnedPiece.AddComponent<BoxCollider2D>();
                }
            }

            _cam.transform.position = new Vector3((float)4 - 0.5f, (float)4 - 0.5f, -10);

        }

        public static void DeleteAllPieces()
        {
            foreach (GameObject gameObject in FindObjectsOfType<GameObject>())
            {
                if (gameObject.name.Contains("Piece"))
                {
                    Destroy(gameObject);
                }
            }
        }

        public void SpawnAllPieces()
        {
            for (var file = 0; file < 8; file++)
            {
                for (var rank = 0; rank < 8; rank++)
                {
                    if (Board.Instance.Square[Board.GetIndexFromPosition(file + 1, rank + 1)] == Pieces.Empty) continue;
                    var spawnedPiece = Instantiate(_piecePrefab, new Vector3(file, rank, -1), Quaternion.identity);
                    spawnedPiece.name = $"Piece" + Convert.ToString(Board.GetIndexFromPosition(file + 1, rank + 1));
                    spawnedPiece.ChangeSprite(Pieces.GetColor(Board.Instance.Square[Board.GetIndexFromPosition(file + 1, rank + 1)]), Pieces.GetPieceType(Board.Instance.Square[Board.GetIndexFromPosition(file + 1, rank + 1)]));
                    spawnedPiece.transform.localScale = _scale;
                    spawnedPiece.AddComponent<BoxCollider2D>();
                }
            }
        }

    }
}