using UnityEngine;

namespace FTL
{
    public class CrewViewer : MonoBehaviour
    {
        private static readonly int GLOW_EFFECT = Shader.PropertyToID("_Glow");

        private CrewController crewController;
        private SpriteRenderer spriteRenderer;
        private MaterialPropertyBlock propertyBlock;
        private bool isSelected;

        private void Awake()
        {
            crewController = GetComponent<CrewController>();
            spriteRenderer = GetComponent<SpriteRenderer>();
            propertyBlock = new MaterialPropertyBlock();
            isSelected = false;
            SetSelectedState(false);
        }

        private void Update()
        {
            if (isSelected == crewController.IsSelected)
            {
                return;
            }

            SetSelectedState(crewController.IsSelected);
        }

        private void SetSelectedState(bool selected)
        {
            isSelected = selected;
            spriteRenderer.GetPropertyBlock(propertyBlock);

            if (selected)
            {
                propertyBlock.SetFloat(GLOW_EFFECT, 100f);
            }
            else
            {
                propertyBlock.SetFloat(GLOW_EFFECT, 0f);
            }

            spriteRenderer.SetPropertyBlock(propertyBlock);
        }
    }
}
