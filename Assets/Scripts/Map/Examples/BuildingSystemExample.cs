using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Icefall.Map;
using Icefall.Map.Core;

namespace Icefall.Map.Examples
{
    /// <summary>
    /// Пример использования системы строительства
    /// Демонстрирует основные возможности BuildingController и BuildingSelectionUI
    /// </summary>
    public class BuildingSystemExample : MonoBehaviour
    {
        [Header("System References")]
        [SerializeField] private BuildingController buildingController;
        [SerializeField] private BuildingSelectionUI buildingSelectionUI;

        [Header("UI References")]
        [SerializeField] private Button toggleUIButton;
        [SerializeField] private TextMeshProUGUI statusText;
        [SerializeField] private TextMeshProUGUI modeText;

        [Header("Quick Build Buttons (Optional)")]
        [SerializeField] private Button buildHouseButton;
        [SerializeField] private Button buildWarehouseButton;
        [SerializeField] private Button buildFarmButton;

        private void Start()
        {
            // Автопоиск компонентов если не назначены
            if (buildingController == null)
                buildingController = FindFirstObjectByType<BuildingController>();

            if (buildingSelectionUI == null)
                buildingSelectionUI = FindFirstObjectByType<BuildingSelectionUI>();

            // Подписываемся на события BuildingController
            if (buildingController != null)
            {
                buildingController.OnBuildingTypeSelected += OnBuildingTypeSelected;
                buildingController.OnBuildingPlaced += OnBuildingPlaced;
                buildingController.OnPlacementCancelled += OnPlacementCancelled;
            }

            // Настраиваем кнопки UI
            SetupUIButtons();

            // Инициализируем UI
            UpdateStatusText("Готов к работе");
            UpdateModeText("Нормальный режим");
        }

        private void OnDestroy()
        {
            // Отписываемся от событий
            if (buildingController != null)
            {
                buildingController.OnBuildingTypeSelected -= OnBuildingTypeSelected;
                buildingController.OnBuildingPlaced -= OnBuildingPlaced;
                buildingController.OnPlacementCancelled -= OnPlacementCancelled;
            }
        }

        private void Update()
        {
            // Горячие клавиши для быстрого доступа
            HandleHotkeys();
        }

        /// <summary>
        /// Настраивает кнопки UI
        /// </summary>
        private void SetupUIButtons()
        {
            if (toggleUIButton != null)
                toggleUIButton.onClick.AddListener(ToggleBuildingUI);

            if (buildHouseButton != null)
                buildHouseButton.onClick.AddListener(() => QuickBuild(BuildingType.House));

            if (buildWarehouseButton != null)
                buildWarehouseButton.onClick.AddListener(() => QuickBuild(BuildingType.Warehouse));

            if (buildFarmButton != null)
                buildFarmButton.onClick.AddListener(() => QuickBuild(BuildingType.Farm));
        }

        /// <summary>
        /// Обрабатывает горячие клавиши
        /// </summary>
        private void HandleHotkeys()
        {
            // B - открыть/закрыть UI выбора зданий
            if (Input.GetKeyDown(KeyCode.B))
            {
                ToggleBuildingUI();
            }

            // Цифры 1-9 для быстрого выбора типа здания
            if (Input.GetKeyDown(KeyCode.Alpha1))
                QuickBuild(BuildingType.House);
            else if (Input.GetKeyDown(KeyCode.Alpha2))
                QuickBuild(BuildingType.Warehouse);
            else if (Input.GetKeyDown(KeyCode.Alpha3))
                QuickBuild(BuildingType.PowerPlant);
            else if (Input.GetKeyDown(KeyCode.Alpha4))
                QuickBuild(BuildingType.Farm);
            else if (Input.GetKeyDown(KeyCode.Alpha5))
                QuickBuild(BuildingType.Workshop);
            else if (Input.GetKeyDown(KeyCode.Alpha6))
                QuickBuild(BuildingType.Hospital);
            else if (Input.GetKeyDown(KeyCode.Alpha7))
                QuickBuild(BuildingType.School);
            else if (Input.GetKeyDown(KeyCode.Alpha8))
                QuickBuild(BuildingType.Mine);
            else if (Input.GetKeyDown(KeyCode.Alpha9))
                QuickBuild(BuildingType.WoodCutter);
        }

        /// <summary>
        /// Переключает видимость UI выбора зданий
        /// </summary>
        public void ToggleBuildingUI()
        {
            if (buildingSelectionUI != null)
            {
                buildingSelectionUI.Toggle();
                Debug.Log("BuildingSystemExample: Toggled building selection UI");
            }
        }

        /// <summary>
        /// Быстрый выбор типа здания
        /// </summary>
        public void QuickBuild(BuildingType buildingType)
        {
            if (buildingController != null)
            {
                buildingController.SelectBuilding(buildingType);
                Debug.Log($"BuildingSystemExample: Quick build selected - {buildingType}");
            }
        }

        /// <summary>
        /// Обработчик выбора типа здания
        /// </summary>
        private void OnBuildingTypeSelected(BuildingType buildingType)
        {
            UpdateStatusText($"Выбран тип: {GetBuildingName(buildingType)}");
            UpdateModeText("Режим размещения");
            
            Debug.Log($"BuildingSystemExample: Building type selected - {buildingType}");
            
            // Можно добавить звуковой эффект
            // AudioManager.Instance.PlaySound("BuildingSelected");
        }

        /// <summary>
        /// Обработчик размещения здания
        /// </summary>
        private void OnBuildingPlaced()
        {
            UpdateStatusText("Здание размещено успешно!");
            
            // Получаем информацию о количестве зданий
            int buildingCount = MapSystem.Instance.PlacementManager.GetBuildingCount();
            Debug.Log($"BuildingSystemExample: Building placed. Total buildings: {buildingCount}");
            
            // Можно добавить эффекты
            // PlayBuildingPlacedEffect();
            // AudioManager.Instance.PlaySound("BuildingPlaced");
            
            // Показываем уведомление
            ShowNotification("Здание построено!", Color.green);
        }

        /// <summary>
        /// Обработчик отмены размещения
        /// </summary>
        private void OnPlacementCancelled()
        {
            UpdateStatusText("Размещение отменено");
            UpdateModeText("Нормальный режим");
            
            Debug.Log("BuildingSystemExample: Placement cancelled");
        }

        /// <summary>
        /// Обновляет текст статуса
        /// </summary>
        private void UpdateStatusText(string message)
        {
            if (statusText != null)
                statusText.text = message;
        }

        /// <summary>
        /// Обновляет текст режима
        /// </summary>
        private void UpdateModeText(string mode)
        {
            if (modeText != null)
                modeText.text = mode;
        }

        /// <summary>
        /// Показывает уведомление
        /// </summary>
        private void ShowNotification(string message, Color color)
        {
            Debug.Log($"Notification: {message}");
            // Здесь можно добавить визуальное уведомление
        }

        /// <summary>
        /// Возвращает русское название типа здания
        /// </summary>
        private string GetBuildingName(BuildingType type)
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

        /// <summary>
        /// Показывает информацию о системе (вызывается из UI кнопки)
        /// </summary>
        public void ShowSystemInfo()
        {
            if (!MapSystem.Instance.IsInitialized)
            {
                Debug.LogWarning("MapSystem not initialized!");
                return;
            }

            int totalBuildings = MapSystem.Instance.PlacementManager.GetBuildingCount();
            var stats = MapSystem.Instance.PlacementManager.GetBuildingStatistics();

            Debug.Log("=== Building System Info ===");
            Debug.Log($"Total Buildings: {totalBuildings}");
            Debug.Log($"Placement Mode: {(buildingController.IsInPlacementMode ? "Active" : "Inactive")}");
            
            if (buildingController.SelectedBuildingType.HasValue)
                Debug.Log($"Selected Type: {buildingController.SelectedBuildingType.Value}");

            Debug.Log("\nBuildings by type:");
            foreach (var kvp in stats)
            {
                Debug.Log($"  {GetBuildingName(kvp.Key)}: {kvp.Value}");
            }
            Debug.Log("===========================");
        }

        /// <summary>
        /// Очищает все здания (для тестирования)
        /// </summary>
        public void ClearAllBuildings()
        {
            if (!MapSystem.Instance.IsInitialized)
            {
                Debug.LogWarning("MapSystem not initialized!");
                return;
            }

            MapSystem.Instance.PlacementManager.ClearAllBuildings();
            UpdateStatusText("Все здания удалены");
            Debug.Log("BuildingSystemExample: All buildings cleared");
        }
    }
}