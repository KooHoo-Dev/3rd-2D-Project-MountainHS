using UnityEngine;
using Utilities;

namespace FTL
{
    public class CursorController : MonoBehaviour
    {
        [SerializeField] private Sprite cursorSprite;

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
    }
}
