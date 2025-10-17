using System;
using UnityEngine;
using Icefall.Map.Generation;
using Icefall.Map.Placement;
using Icefall.Map.Visualization;

namespace Icefall.Map.Core
{
    /// <summary>
    /// Главная система карты - Singleton MonoBehaviour
    /// Координирует все подсистемы: данные, генерацию, размещение и визуализацию
    /// </summary>
    public class MapSystem : MonoBehaviour
    {
        private static MapSystem instance;

        // Публичный доступ к системе
        public static MapSystem Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindFirstObjectByType<MapSystem>();
                    if (instance == null)
                    {
                        var go = new GameObject("MapSystem");
                        instance = go.AddComponent<MapSystem>();
                    }
                }
                return instance;
            }
        }

        // Подсистемы
        public MapData MapData { get; private set; }
        public MapGenerator Generator { get; private set; }
        public BuildingPlacementValidator Validator { get; private set; }
        public BuildingPlacementManager PlacementManager { get; private set; }
        public MapVisualizationController VisualizationController { get; private set; }

        // Настройки
        [Header("Map Generation Settings")]
        [SerializeField] private int generationSeed = -1;  // -1 = случайный seed
        [SerializeField] private bool generateOnStart = true;
        [SerializeField] private bool visualizeOnStart = true;

        [Header("Visualization Settings")]
        [SerializeField] private int renderDistance = 10;  // Дистанция рендеринга в чанках

        [Header("Debug")]
        [SerializeField] private bool verboseLogs = false;

        private void LogV(string message)
        {
            if (verboseLogs)
                Debug.Log(message);
        }

        // События
        public event Action<BuildingData> OnBuildingPlaced;
        public event Action<int> OnBuildingRemoved;
        public event Action OnMapGenerated;
        public event Action OnMapReset;

        // Состояние
        public bool IsInitialized { get; private set; }
        public bool IsMapGenerated { get; private set; }

        private void Awake()
        {
            // Проверка на синглтон
            if (instance != null && instance != this)
            {
                Debug.LogWarning("MapSystem: Another instance already exists. Destroying this one.");
                Destroy(gameObject);
                return;
            }

            instance = this;
            DontDestroyOnLoad(gameObject);

            LogV("MapSystem: Awake - Singleton initialized");
        }

        private void Start()
        {
            if (generateOnStart)
            {
                Initialize();
            }
        }

        /// <summary>
        /// Инициализирует систему карты
        /// </summary>
        public void Initialize()
        {
            if (IsInitialized)
            {
                Debug.LogWarning("MapSystem: Already initialized");
                return;
            }

            LogV("MapSystem: Initializing map system...");

            // 1. Создаём данные карты
            MapData = new MapData();
            LogV($"MapSystem: MapData created - {MapData.GetStats()}");

            // 2. Создаём генератор
            Generator = new MapGenerator(MapData, generationSeed);
            LogV("MapSystem: MapGenerator created");

            // 3. Создаём валидатор
            Validator = new BuildingPlacementValidator(MapData);
            LogV("MapSystem: BuildingPlacementValidator created");

            // 4. Создаём менеджер размещения
            PlacementManager = new BuildingPlacementManager(MapData, Validator);
            
            // Подписываемся на события менеджера размещения
            PlacementManager.OnBuildingPlaced += OnBuildingPlacedHandler;
            PlacementManager.OnBuildingRemoved += OnBuildingRemovedHandler;
            LogV("MapSystem: BuildingPlacementManager created");

            // 5. Создаём контроллер визуализации
            VisualizationController = new MapVisualizationController(MapData);
            VisualizationController.SetRenderDistance(renderDistance);
            LogV("MapSystem: MapVisualizationController created");

            IsInitialized = true;
            LogV("MapSystem: Initialization complete");

            // Генерируем карту, если требуется
            if (generateOnStart)
            {
                GenerateMap();
            }
        }

        /// <summary>
        /// Генерирует новую карту
        /// </summary>
        public void GenerateMap()
        {
            if (!IsInitialized)
            {
                Debug.LogError("MapSystem: Cannot generate map - system not initialized");
                return;
            }

            LogV("MapSystem: Generating new map...");

            // Генерируем карту
            Generator.GenerateNewMap();
            IsMapGenerated = true;

            // Визуализируем, если требуется
            if (visualizeOnStart)
            {
                VisualizationController.InitializeVisualization();
            }

            // Триггерим событие
            OnMapGenerated?.Invoke();

            LogV("MapSystem: Map generation complete");
            LogMapStats();
        }

        /// <summary>
        /// Сбрасывает карту
        /// </summary>
        public void ResetMap()
        {
            if (!IsInitialized)
            {
                Debug.LogError("MapSystem: Cannot reset map - system not initialized");
                return;
            }

            LogV("MapSystem: Resetting map...");

            // Очищаем визуализацию
            VisualizationController.ClearVisualization();

            // Очищаем данные
            MapData.Clear();
            PlacementManager.ClearAllBuildings();

            IsMapGenerated = false;

            // Триггерим событие
            OnMapReset?.Invoke();

            LogV("MapSystem: Map reset complete");
        }

        /// <summary>
        /// Регенерирует карту
        /// </summary>
        public void RegenerateMap()
        {
            ResetMap();
            GenerateMap();
        }

        #region Building Management Quick Access

        /// <summary>
        /// Размещает здание на карте
        /// </summary>
        public BuildingData PlaceBuilding(BuildingType type, int x, int y)
        {
            if (!IsInitialized || PlacementManager == null)
            {
                Debug.LogError("MapSystem: Cannot place building - system not initialized");
                return null;
            }

            return PlacementManager.PlaceBuilding(type, x, y);
        }

        /// <summary>
        /// Удаляет здание с карты
        /// </summary>
        public bool RemoveBuilding(int buildingId)
        {
            if (!IsInitialized || PlacementManager == null)
            {
                Debug.LogError("MapSystem: Cannot remove building - system not initialized");
                return false;
            }

            return PlacementManager.RemoveBuilding(buildingId);
        }

        /// <summary>
        /// Проверяет, можно ли разместить здание
        /// </summary>
        public bool CanPlaceBuilding(BuildingType type, int x, int y)
        {
            if (!IsInitialized || Validator == null)
                return false;

            return Validator.CanPlaceBuilding(type, x, y);
        }

        #endregion

        #region Event Handlers

        private void OnBuildingPlacedHandler(BuildingData building)
        {
            LogV($"MapSystem: Building placed - {building}");
            
            // Обновляем визуализацию затронутых чанков
            UpdateChunksForBuilding(building);

            // Пробрасываем событие
            OnBuildingPlaced?.Invoke(building);
        }

        private void OnBuildingRemovedHandler(int buildingId)
        {
            LogV($"MapSystem: Building removed - #{buildingId}");

            // Обновляем все dirty чанки
            VisualizationController?.UpdateDirtyChunks();

            // Пробрасываем событие
            OnBuildingRemoved?.Invoke(buildingId);
        }

        private void UpdateChunksForBuilding(BuildingData building)
        {
            if (VisualizationController == null)
                return;

            // Вычисляем затронутые чанки
            int startChunkX = building.PositionX / MapData.CHUNK_SIZE;
            int startChunkY = building.PositionY / MapData.CHUNK_SIZE;
            int endChunkX = (building.PositionX + building.Width - 1) / MapData.CHUNK_SIZE;
            int endChunkY = (building.PositionY + building.Height - 1) / MapData.CHUNK_SIZE;

            // Обновляем все затронутые чанки
            for (int cx = startChunkX; cx <= endChunkX; cx++)
            {
                for (int cy = startChunkY; cy <= endChunkY; cy++)
                {
                    VisualizationController.UpdateChunkVisuals(cx, cy);
                }
            }
        }

        #endregion

        #region Debug and Utility

        /// <summary>
        /// Логирует статистику карты
        /// </summary>
        public void LogMapStats()
        {
            if (!IsInitialized || !verboseLogs)
                return;

            LogV("=== Map Statistics ===");
            LogV(MapData.GetStats());
            LogV(VisualizationController.GetVisualizationStats());
            
            var buildingStats = PlacementManager.GetBuildingStatistics();
            if (buildingStats.Count > 0)
            {
                LogV("Building types:");
                foreach (var kvp in buildingStats)
                {
                    LogV($"  {kvp.Key}: {kvp.Value}");
                }
            }
            LogV("====================");
        }

        /// <summary>
        /// Создаёт тестовое здание (для отладки)
        /// </summary>
        public void CreateTestBuilding(int x, int y, BuildingType type)
        {
            PlaceBuilding(type, x, y);
        }

        #endregion

        #region Update Loop

        private void Update()
        {
            if (!IsInitialized || !IsMapGenerated)
                return;

            // Обновляем визуализацию на основе позиции камеры (опционально)
            // UpdateVisualizationBasedOnCamera();
        }

        private void UpdateVisualizationBasedOnCamera()
        {
            if (Camera.main != null)
            {
                VisualizationController.UpdateVisualizationBasedOnCamera(Camera.main.transform.position);
            }
        }

        #endregion

        private void OnDestroy()
        {
            // Отписываемся от событий
            if (PlacementManager != null)
            {
                PlacementManager.OnBuildingPlaced -= OnBuildingPlacedHandler;
                PlacementManager.OnBuildingRemoved -= OnBuildingRemovedHandler;
            }

            // Очищаем визуализацию
            VisualizationController?.ClearVisualization();

            if (instance == this)
            {
                instance = null;
            }
        }
    }
}