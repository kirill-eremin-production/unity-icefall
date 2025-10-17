using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Icefall.Map.Core;
using System.Collections.Generic;

namespace Icefall.Map.UI
{
    /// <summary>
    /// UI панель для выбора типа здания для строительства
    /// </summary>
    public class BuildingSelectionUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private BuildingController buildingController;
        [SerializeField] private Transform buttonsContainer;
        [SerializeField] private GameObject buildingButtonPrefab;
        
        [Header("UI Elements")]
        [SerializeField] private GameObject panel;
        [SerializeField] private Button closeButton;
        [SerializeField] private TextMeshProUGUI titleText;

        [Header("Settings")]
        [SerializeField] private bool showAllBuildings = true;
        [SerializeField] private List<BuildingType> availableBuildings = new List<BuildingType>();

        private Dictionary<BuildingType, BuildingButton> buildingButtons = new Dictionary<BuildingType, BuildingButton>();
        private BuildingType? currentlySelected = null;

        private void Awake()
        {
            if (buildingController == null)
                buildingController = FindFirstObjectByType<BuildingController>();

            if (closeButton != null)
                closeButton.onClick.AddListener(Hide);

            // Создаем кнопки для зданий
            CreateBuildingButtons();
        }

        private void OnEnable()
        {
            if (buildingController != null)
            {
                buildingController.OnBuildingTypeSelected += OnBuildingTypeSelected;
                buildingController.OnPlacementCancelled += OnPlacementCancelled;
            }
        }

        private void OnDisable()
        {
            if (buildingController != null)
            {
                buildingController.OnBuildingTypeSelected -= OnBuildingTypeSelected;
                buildingController.OnPlacementCancelled -= OnPlacementCancelled;
            }
        }

        /// <summary>
        /// Создает кнопки для всех доступных зданий
        /// </summary>
        private void CreateBuildingButtons()
        {
            if (buttonsContainer == null)
            {
                Debug.LogError("BuildingSelectionUI: Buttons container is not assigned!");
                return;
            }

            // Определяем какие здания показывать
            List<BuildingType> buildingsToShow;
            if (showAllBuildings)
            {
                buildingsToShow = new List<BuildingType>(System.Enum.GetValues(typeof(BuildingType)) as BuildingType[]);
            }
            else
            {
                buildingsToShow = availableBuildings;
            }

            // Создаем кнопку для каждого типа здания
            foreach (var buildingType in buildingsToShow)
            {
                CreateBuildingButton(buildingType);
            }
        }

        /// <summary>
        /// Создает кнопку для конкретного типа здания
        /// </summary>
        private void CreateBuildingButton(BuildingType buildingType)
        {
            GameObject buttonObj;
            
            if (buildingButtonPrefab != null)
            {
                // Используем префаб если назначен
                buttonObj = Instantiate(buildingButtonPrefab, buttonsContainer);
            }
            else
            {
                // Создаем простую кнопку
                buttonObj = CreateSimpleButton();
                buttonObj.transform.SetParent(buttonsContainer, false);
            }

            buttonObj.name = $"Button_{buildingType}";

            // Настраиваем компонент BuildingButton
            var buildingButton = buttonObj.GetComponent<BuildingButton>();
            if (buildingButton == null)
                buildingButton = buttonObj.AddComponent<BuildingButton>();

            buildingButton.Initialize(buildingType, () => OnBuildingButtonClicked(buildingType));
            buildingButtons[buildingType] = buildingButton;
        }

        /// <summary>
        /// Создает простую кнопку (если префаб не назначен)
        /// </summary>
        private GameObject CreateSimpleButton()
        {
            GameObject buttonObj = new GameObject("BuildingButton");
            
            // Добавляем Image
            var image = buttonObj.AddComponent<Image>();
            image.color = new Color(0.2f, 0.2f, 0.2f, 0.9f);
            
            // Добавляем Button
            var button = buttonObj.AddComponent<Button>();
            
            // Добавляем текст
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(buttonObj.transform, false);
            var text = textObj.AddComponent<TextMeshProUGUI>();
            text.alignment = TextAlignmentOptions.Center;
            text.fontSize = 14;
            text.color = Color.white;
            
            // Настраиваем RectTransform для текста
            var textRect = text.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;
            
            // Настраиваем RectTransform для кнопки
            var buttonRect = buttonObj.GetComponent<RectTransform>();
            buttonRect.sizeDelta = new Vector2(150, 40);
            
            return buttonObj;
        }

        /// <summary>
        /// Обработчик клика по кнопке здания
        /// </summary>
        private void OnBuildingButtonClicked(BuildingType buildingType)
        {
            if (buildingController == null)
                return;

            buildingController.SelectBuilding(buildingType);
            Debug.Log($"BuildingSelectionUI: Selected {buildingType}");
        }

        /// <summary>
        /// Обработчик выбора типа здания в контроллере
        /// </summary>
        private void OnBuildingTypeSelected(BuildingType buildingType)
        {
            currentlySelected = buildingType;
            UpdateButtonStates();
        }

        /// <summary>
        /// Обработчик отмены размещения
        /// </summary>
        private void OnPlacementCancelled()
        {
            currentlySelected = null;
            UpdateButtonStates();
        }

        /// <summary>
        /// Обновляет состояние всех кнопок
        /// </summary>
        private void UpdateButtonStates()
        {
            foreach (var kvp in buildingButtons)
            {
                bool isSelected = currentlySelected.HasValue && kvp.Key == currentlySelected.Value;
                kvp.Value.SetSelected(isSelected);
            }
        }

        /// <summary>
        /// Показывает панель выбора зданий
        /// </summary>
        public void Show()
        {
            if (panel != null)
                panel.SetActive(true);
        }

        /// <summary>
        /// Скрывает панель выбора зданий
        /// </summary>
        public void Hide()
        {
            if (panel != null)
                panel.SetActive(false);
        }

        /// <summary>
        /// Переключает видимость панели
        /// </summary>
        public void Toggle()
        {
            if (panel != null)
                panel.SetActive(!panel.activeSelf);
        }
    }

    /// <summary>
    /// Компонент для отдельной кнопки здания
    /// </summary>
    public class BuildingButton : MonoBehaviour
    {
        private BuildingType buildingType;
        private Button button;
        private TextMeshProUGUI text;
        private Image image;
        private System.Action onClick;

        private Color normalColor = new Color(0.2f, 0.2f, 0.2f, 0.9f);
        private Color selectedColor = new Color(0.3f, 0.6f, 0.3f, 0.9f);

        public void Initialize(BuildingType type, System.Action onClickAction)
        {
            buildingType = type;
            onClick = onClickAction;

            // Получаем компоненты
            button = GetComponent<Button>();
            if (button == null)
                button = gameObject.AddComponent<Button>();

            image = GetComponent<Image>();
            text = GetComponentInChildren<TextMeshProUGUI>();

            // Настраиваем текст
            if (text != null)
            {
                text.text = GetBuildingDisplayName(buildingType);
            }

            // Подписываемся на клик
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => onClick?.Invoke());
        }

        public void SetSelected(bool selected)
        {
            if (image != null)
            {
                image.color = selected ? selectedColor : normalColor;
            }
        }

        private string GetBuildingDisplayName(BuildingType type)
        {
            return type switch
            {
                BuildingType.House => "Дом",
                BuildingType.Warehouse => "Склад",
                BuildingType.PowerPlant => "Электростанция",
                BuildingType.Farm => "Ферма",
                BuildingType.Workshop => "Мастерская",
                BuildingType.Hospital => "Больница",
                BuildingType.School => "Школа",
                BuildingType.Mine => "Шахта",
                BuildingType.WoodCutter => "Лесопилка",
                BuildingType.HeatingPlant => "Теплостанция",
                _ => type.ToString()
            };
        }
    }
}