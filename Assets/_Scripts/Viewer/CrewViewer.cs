using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FTL
{
    public class CrewViewer : MonoBehaviour
    {
        private static readonly int GLOW_EFFECT = Shader.PropertyToID("_Glow");

        public int CrewIndex;

        [SerializeField] private TMP_Text text;
        [SerializeField] private Image hpBar;
        [SerializeField] private SpriteRenderer crewSprite;
        [SerializeField] private SpriteRenderer crewTargetSprite;
        [field: SerializeField] public Button HealButton { get; private set; }
        [SerializeField] private CrewController crew;
        [SerializeField] private bool useGameContext;
        private MaterialPropertyBlock propertyBlock;
        private bool isSelected;
        private Vector3 targetPosition;

        private void Awake()
        {
            propertyBlock = new MaterialPropertyBlock();
        }

        private void Start()
        {
            if (crew == null)
            {
                useGameContext = true;
                HealButton.gameObject.SetActive(true);
                return;
            }

            useGameContext = false;
            crewTargetSprite.enabled = false;
            HealButton.gameObject.SetActive(false);
            SetSelectedState(false);
        }

        private void LateUpdate()
        {
            if (useGameContext)
            {
                UpdateGameContextViewer();
                return;
            }

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

        private void UpdateGameContextViewer()
        {
            if (GameManager.IsInitialized == false)
            {
                Debug.LogError($"{gameObject.name}: GameManager is not Initialized to use!");
                return;
            }

            GameManager gameManager = GameManager.Instance;
            if (gameManager.IsGameContextEmpty)
            {
                Debug.LogError($"{gameObject.name}: GameManager doesn't have gameContext");
                return;
            }

            text.SetText(gameManager.GetCrewName(CrewIndex));
            int hp = gameManager.GetCrewHp(CrewIndex);
            int maxHp = gameManager.GetCrewMaxHp(CrewIndex);

            hpBar.fillAmount = Mathf.InverseLerp(0f, maxHp, hp);
        }

        public void SetGameContextCrew(int crewIndex, TMP_Text text, Image hpBar)
        {
            useGameContext = true;
            this.CrewIndex = crewIndex;
            this.text = text;
            this.hpBar = hpBar;
        }

        public void SetText(TMP_Text text)
        {
            this.text = text;
        }

        public void SetHpBar(Image hpBar)
        {
            this.hpBar = hpBar;
        }

        public void SetCrewSprite (SpriteRenderer crewSprite)
        {
            this.crewSprite = crewSprite;
        }

        public void SetCrewTargetSpite(SpriteRenderer crewTargetSprite)
        {
            this.crewTargetSprite = crewTargetSprite;
        }

        public void SetCrewController(CrewController crew)
        {
            this.crew = crew;
        }
    }
}
