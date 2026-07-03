using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;
using Utilities;

namespace FTL
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class CursorController : SingletonBase<CursorController>
    {
        [SerializeField] private Sprite cursorSprite;
        [SerializeField] private List<Sprite> targetSprites;
        private SpriteRenderer spriteRenderer;

        protected override void OnInitialize()
        {
            base.OnInitialize();
            spriteRenderer = GetComponent<SpriteRenderer>();
            spriteRenderer.sprite = cursorSprite;
            spriteRenderer.enabled = true;
        }

        private void Start()
        {
            cursorSprite = GetComponent<SpriteRenderer>().sprite;
            Cursor.visible = false;
        }

        private void Update()
        {
            SetMousePos();
        }

        private void SetMousePos()
        {
            Vector2 mousePos = SimpleInput.GetMousePosition();
            mousePos = Camera.main.ScreenToWorldPoint(mousePos);
            gameObject.transform.position = mousePos;
        }

        public void SetCursorAim(int index)
        {
            spriteRenderer.sprite = targetSprites[index];
        }

        public void ResetMouseSprite()
        {
            spriteRenderer.sprite = cursorSprite;
        }
    }
}
