using System;
using Assets.Scripts.Core;
using UnityEngine;

namespace Assets.Scripts.UI
{
    public class Piece : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer _spriteRenderer;
        [SerializeField] private Sprite[] _spriteArrayBlack;
        [SerializeField] private Sprite[] _spriteArrayWhite;
        public void ChangeSprite(int color, int type)
        {
            _spriteRenderer.sprite = color == Pieces.White ? _spriteArrayWhite[type - 1] : _spriteArrayBlack[type - 1];
        }
    }
}