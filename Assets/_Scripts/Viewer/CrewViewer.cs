using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FTL
{
    public class CrewViewer : MonoBehaviour
    {
        private static readonly int GLOW_EFFECT = Shader.PropertyToID("_Glow");

        private CrewController crew;
        private SpriteRenderer crewSpriteRenderer;
        private MaterialPropertyBlock propertyBlock;
        private TMP_Text text;
        private Image hpBar; 
        private bool isSelected;

        private void Awake()
        {
            crew = GetComponent<CrewController>();
            crewSpriteRenderer = GetComponent<SpriteRenderer>();
            propertyBlock = new MaterialPropertyBlock();
            isSelected = false;
            SetSelectedState(false);
        }

        private void LateUpdate()
        {
            SetSelectedState(crew.IsSelected);
            UpdateViewer();
        }

        private void UpdateViewer()
        {
            text.SetText(crew.Name);
            float fillAmount = Mathf.InverseLerp(0f, crew.MaxHp, crew.Hp);
            hpBar.fillAmount = fillAmount;
        }

        private void SetSelectedState(bool selected)
        {
            if (isSelected == crew.IsSelected)
            {
                return;
            }

            isSelected = selected;
            crewSpriteRenderer.GetPropertyBlock(propertyBlock);

            if (selected)
            {
                propertyBlock.SetFloat(GLOW_EFFECT, 100f);
            }
            else
            {
                propertyBlock.SetFloat(GLOW_EFFECT, 0f);
            }

            crewSpriteRenderer.SetPropertyBlock(propertyBlock);
        }

        public void SetText(TMP_Text text)
        {
            this.text = text;
        }

        public void SetHpBar(Image hpBar)
        {
            this.hpBar = hpBar;
        }
    }
}
