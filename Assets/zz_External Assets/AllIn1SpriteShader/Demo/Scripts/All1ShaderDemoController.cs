using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem.UI;
#endif

namespace AllIn1SpriteShader
{
    public class All1ShaderDemoController : MonoBehaviour
    {
        [SerializeField] private DemoCircleExpositor[] expositors;
        [SerializeField] private Text expositorsTitle, expositorsTitleOutline;
        public float expositorDistance;

        [SerializeField] private GameObject background;
        [SerializeField] private float colorLerpSpeed;
        private Material backgroundMat;
        private Color[] currentColors;

        private int currExpositor;

        private EventSystem eventSystem;

        // Custom sort axis fields
        private Camera mainCamera;
        private Vector3 originalSortAxis;
        private TransparencySortMode originalSortMode;
        private bool sortingWasModified;
        private Color[] targetColors;

        private void Start()
        {
            EventSystem eventSystemInScene = FindAnyObjectByType<EventSystem>();
            if (eventSystemInScene != null)
            {
                Destroy(eventSystemInScene.gameObject);
            }

            GameObject goEventSystem = new("Event System");
            eventSystem = goEventSystem.AddComponent<EventSystem>();

#if ENABLE_INPUT_SYSTEM
            goEventSystem.AddComponent<InputSystemUIInputModule>();
#elif ENABLE_LEGACY_INPUT_MANAGER
			goEventSystem.AddComponent<StandaloneInputModule>();
#endif

            // Setup custom sort axis
            SetupCustomSortAxis();

            currExpositor = 0;
            SetExpositorText();

            for (int i = 0; i < expositors.Length; i++)
            {
                expositors[i].transform.position = new Vector3(0, expositorDistance * i, 0);
            }

            backgroundMat = background.GetComponent<Image>().material;
            targetColors = new Color[4];
            targetColors[0] = backgroundMat.GetColor("_GradTopLeftCol");
            targetColors[1] = backgroundMat.GetColor("_GradTopRightCol");
            targetColors[2] = backgroundMat.GetColor("_GradBotLeftCol");
            targetColors[3] = backgroundMat.GetColor("_GradBotRightCol");
            currentColors = targetColors.Clone() as Color[];
        }

        private void Update()
        {
            GetInput();

            currentColors[0] = Color.Lerp(currentColors[0], targetColors[(0 + currExpositor) % targetColors.Length],
                colorLerpSpeed * Time.deltaTime);
            currentColors[1] = Color.Lerp(currentColors[1], targetColors[(1 + currExpositor) % targetColors.Length],
                colorLerpSpeed * Time.deltaTime);
            currentColors[2] = Color.Lerp(currentColors[2], targetColors[(2 + currExpositor) % targetColors.Length],
                colorLerpSpeed * Time.deltaTime);
            currentColors[3] = Color.Lerp(currentColors[3], targetColors[(3 + currExpositor) % targetColors.Length],
                colorLerpSpeed * Time.deltaTime);
            backgroundMat.SetColor("_GradTopLeftCol", currentColors[0]);
            backgroundMat.SetColor("_GradTopRightCol", currentColors[1]);
            backgroundMat.SetColor("_GradBotLeftCol", currentColors[2]);
            backgroundMat.SetColor("_GradBotRightCol", currentColors[3]);
        }

        private void OnDestroy()
        {
            // Restore original camera settings
            if (sortingWasModified && mainCamera != null)
            {
                mainCamera.transparencySortMode = originalSortMode;
                mainCamera.transparencySortAxis = originalSortAxis;
            }
        }

        private void GetInput()
        {
            if (AllIn1InputSystem.GetKeyDown(KeyCode.LeftArrow) || AllIn1InputSystem.GetKeyDown(KeyCode.A))
            {
                expositors[currExpositor].ChangeTarget(-1);
            }
            else if (AllIn1InputSystem.GetKeyDown(KeyCode.RightArrow) || AllIn1InputSystem.GetKeyDown(KeyCode.D))
            {
                expositors[currExpositor].ChangeTarget(1);
            }
            else if (AllIn1InputSystem.GetKeyDown(KeyCode.UpArrow) || AllIn1InputSystem.GetKeyDown(KeyCode.W))
            {
                ChangeExpositor(-1);
            }
            else if (AllIn1InputSystem.GetKeyDown(KeyCode.DownArrow) || AllIn1InputSystem.GetKeyDown(KeyCode.S))
            {
                ChangeExpositor(1);
            }
        }

        private void ChangeExpositor(int offset)
        {
            currExpositor += offset;
            if (currExpositor > expositors.Length - 1)
            {
                currExpositor = 0;
            }
            else if (currExpositor < 0)
            {
                currExpositor = expositors.Length - 1;
            }

            SetExpositorText();
        }

        private void SetExpositorText()
        {
            expositorsTitle.text = expositors[currExpositor].name;
            expositorsTitleOutline.text = expositors[currExpositor].name;
        }

        public int GetCurrExpositor() { return currExpositor; }

        private void SetupCustomSortAxis()
        {
            mainCamera = Camera.main;
            if (mainCamera == null)
            {
                Debug.LogWarning(
                    "All1ShaderDemoController: Main camera not found. Custom sort axis will not be applied.");
                return;
            }

            // Save original settings
            originalSortMode = mainCamera.transparencySortMode;
            originalSortAxis = mainCamera.transparencySortAxis;

            // Detect render pipeline
            string pipelineType = GetRenderPipelineType();

            try
            {
                // Apply custom sort axis for all pipelines
                // This is a Camera property that works across all render pipelines
                mainCamera.transparencySortMode = TransparencySortMode.CustomAxis;
                mainCamera.transparencySortAxis = new Vector3(0, 0, 1);
                sortingWasModified = true;

                //Debug.Log($"All1ShaderDemoController: Custom sort axis applied successfully ({mainCamera.transparencySortAxis}) for {pipelineType}");
            }
            catch (Exception e)
            {
                Debug.LogWarning(
                    $"All1ShaderDemoController: Failed to apply custom sort axis for {pipelineType}. Error: {e.Message}");
                sortingWasModified = false;
            }
        }

        private string GetRenderPipelineType()
        {
            RenderPipelineAsset currentRP = GraphicsSettings.currentRenderPipeline;

            if (currentRP == null)
            {
                return "Built-in Render Pipeline";
            }

            string rpTypeName = currentRP.GetType().Name;

            if (rpTypeName.Contains("Universal") || rpTypeName.Contains("URP"))
            {
                return "Universal Render Pipeline (URP)";
            }

            if (rpTypeName.Contains("HD") || rpTypeName.Contains("HighDefinition"))
            {
                return "High Definition Render Pipeline (HDRP)";
            }

            return $"Custom SRP ({rpTypeName})";
        }
    }
}
