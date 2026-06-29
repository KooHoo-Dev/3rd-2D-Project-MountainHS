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
        private SpriteRenderer crewSprite;
        private SpriteRenderer crewTargetSprite;
        private MaterialPropertyBlock propertyBlock;
        private TMP_Text text;
        private Image hpBar; 
        private bool isSelected;
        private Vector3 targetPosition;

        private void Awake()
        {
            crew = GetComponent<CrewController>();
            crewSprite = transform.GetChild(0).GetComponent<SpriteRenderer>();
            crewTargetSprite = transform.GetChild(1).GetComponent<SpriteRenderer>();
            propertyBlock = new MaterialPropertyBlock();
            isSelected = false;
            SetSelectedState(false);
        }

        private void Start()
        {
            crewTargetSprite.enabled = false;
        }

        private void LateUpdate()
        {
            SetSelectedState(crew.IsSelected);
            UpdateViewer();
            UpdateTargetRoom();
        }

        private void UpdateTargetRoom()
        {
            Room targetRoom = crew.TargetRoom;
            if (targetRoom == null)
            {
                crewTargetSprite.enabled = false;
                return;
            }

            if (targetRoom != null && crewTargetSprite.enabled == false)
            {
                targetPosition = targetRoom.GetCrewPosition(crew, CrewController.Offset);
                crewTargetSprite.transform.position = targetPosition;
                crewTargetSprite.enabled = true;
            }
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
            crewSprite.GetPropertyBlock(propertyBlock);

            if (selected)
            {
                propertyBlock.SetFloat(GLOW_EFFECT, 100f);
            }
            else
            {
                propertyBlock.SetFloat(GLOW_EFFECT, 0f);
            }

            crewSprite.SetPropertyBlock(propertyBlock);
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
