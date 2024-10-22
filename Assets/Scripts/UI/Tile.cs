using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.UI.Cursor;
using UnityEngine;

namespace Assets.Scripts
{
    public class Tile : MonoBehaviour
    {
        [SerializeField] private Color _darkColor, _lightColor, _selectedTile, _legalMove;
        [SerializeField] private SpriteRenderer _renderer;
        private Color _defaultColor;
        private bool _isLight;

        public void Init(bool isLight)
        {
            _isLight = isLight;
            _renderer.color = isLight ?  _lightColor : _darkColor;
            _defaultColor = _renderer.color;
        }

        public void ChangeColorDefault()
        {
            _renderer.color = _defaultColor;
        }

        public void ChangeColorSelect()
        {

            if (_isLight)
            {
                _renderer.color = _selectedTile * 1.1f;
            }
            else
            {
                _renderer.color = _selectedTile * 0.9f;
            }
        }

        public void ChangeColorLegal()
        {

            if (_isLight)
            {
                _renderer.color = _legalMove * 1.4f;
            }
            else
            {
                _renderer.color = _legalMove * 0.8f;
            }
        }





    }
}
