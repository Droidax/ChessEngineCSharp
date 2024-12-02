using System;
using Assets.Scripts.Core;
using Unity.VisualScripting;
using UnityEngine;

namespace Assets.Scripts.UI
{
    public class Piece : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer _spriteRenderer;
        [SerializeField] private Sprite[] _spriteArrayBlack;
        [SerializeField] private Sprite[] _spriteArrayWhite;
        private Rigidbody2D rb2d;

        private void OnEnable()
        {
            Actions.OnPieceMove += UpdatePosition;
            Actions.OnDestroyPiece += DestroySelf;
        }


        private void OnDisable()
        {
            Actions.OnPieceMove -= UpdatePosition;
            Actions.OnDestroyPiece -= DestroySelf;

        }
        void Start()
        {
            rb2d = GetComponent<Rigidbody2D>();
        }

        private void DestroySelf(int index)
        {
            if (Convert.ToString(index) == name.Remove(0, 5))
            {
                Destroy(gameObject);
            }
        }

        private void UpdatePosition(int startIndex, int targetIndex)
        {
            if (name.Remove(0, 5) != Convert.ToString(startIndex))
                return;

            rb2d.transform.position = GameObject.Find(targetIndex.ToString()).transform.position;
            name = "Piece" + Convert.ToString(targetIndex);
        }
        
        
        public void ChangeSprite(int color, int type)
        {
            _spriteRenderer.sprite = color == Pieces.White ? _spriteArrayWhite[type - 1] : _spriteArrayBlack[type - 1];
        }
    }
}