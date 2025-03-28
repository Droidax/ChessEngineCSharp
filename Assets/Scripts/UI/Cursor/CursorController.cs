using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Core;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.XR;
using static Assets.Scripts.Core.Board;

namespace Assets.Scripts.UI.Cursor
{
    public class CursorController : MonoBehaviour
    {
        private Camera _mainCamera;
        private CursorControls _controls;
        public Texture2D cursor;
        public Texture2D cursorClicked;
        public GameObject targetSquare;
        private bool _isSelected;
        public int _index;
        public int _selected;
        public GameObject tile;
        public GameObject piece;

        void Awake()
        {
            ChangeCursor(cursor);
            UnityEngine.Cursor.lockState = CursorLockMode.None;
            _mainCamera = Camera.main;
            _controls = new CursorControls();
            _isSelected = false;
        }

        private void OnEnable()
        {
            _controls.Enable();
        }

        private void OnDisable()
        {
            _controls.Disable();
        }

        private void Start()
        {
            _controls.Mouse.Click.started += _ => StartedClick();
            _controls.Mouse.Click.performed += _ => EndedClick();
        }

        private void StartedClick()
        {
            ChangeCursor(cursorClicked);
        }

        private void EndedClick()
        {
            ChangeCursor(cursor);
            if (!GameManager.Instance.WaitingForMove)
                return;
            
            if (Instance.ColorToMove == Pieces.White)
            {
                if (Instance.WhitePlayer == GameManager.PlayerTypes.Human)
                {
                    DetectObject();
                }
            }

            if (Instance.ColorToMove == Pieces.Black)
            {
                if (Instance.BlackPlayer == GameManager.PlayerTypes.Human)
                {
                    DetectObject();
                }
            }
        }

        public void DetectObject()
        {
            Ray ray = _mainCamera.ScreenPointToRay(_controls.Mouse.position.ReadValue<Vector2>());
            RaycastHit2D[] hit2DAll = Physics2D.GetRayIntersectionAll(ray);
            
            if (hit2DAll == null || hit2DAll.Length == 0) return;

            _index = int.Parse(hit2DAll.Last().collider.name);

            if (Instance.Square[_index] == Pieces.Empty && _isSelected == false)
                return;

            if (Instance.Square[_index] != Pieces.Empty && _isSelected == false && GameManager.Instance.WaitingForMove)//select piece
            {

                if ( Pieces.IsColor(Instance.Square[_index], Instance.ColorToMove)) 
                {
                    tile = GameObject.Find(Convert.ToString(_index));
                    tile.GetComponent<Tile>().ChangeColorSelect();
                    _isSelected = true;
                    _selected = _index;
                    HighlightLegalSquare(_index);
                    return;
                }
            }

            if (Instance.Square[_index] != Pieces.Empty == _isSelected && _index == _selected) //deselect piece
            {
                ResetSquareColor();
                _isSelected = false;
                return;
            }

            if (_isSelected) //move piece
            {
                piece = GameObject.Find("Piece" + Convert.ToString(_selected));
                targetSquare = hit2DAll.Last().collider.gameObject;

                _isSelected = false;
                
                Instance.MovePiece(_selected, int.Parse(targetSquare.name), true);
            }
        }

        private void ChangeCursor(Texture2D cursorType)
        {
            Vector2 hotspot = new Vector2(cursorType.width / 2, cursorType.height / 2);
            UnityEngine.Cursor.SetCursor(cursorType, hotspot, CursorMode.Auto);
        }
    }
}