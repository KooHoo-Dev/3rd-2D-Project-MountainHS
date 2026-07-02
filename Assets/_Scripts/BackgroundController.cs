using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace FTL
{
    public class BackgroundController:MonoBehaviour
    {
        [SerializeField] private List<Sprite> sprites;
        [SerializeField] private Image background;

        public void SetRandomBackground()
        {
            int randIndex = Random.Range(0, sprites.Count);
            background.sprite = sprites[randIndex];
        }
    }
}
