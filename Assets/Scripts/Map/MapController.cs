using UnityEngine;
using Icefall.Map.Core;

namespace Icefall.Map
{
    /// <summary>
    /// Основной контроллер системы карты
    /// Управляет инициализацией и базовым взаимодействием с картой
    /// </summary>
    public class MapController : MonoBehaviour
    {
        [Header("Initialization")]
        [SerializeField] private bool autoInitialize = true;
        [SerializeField] private float initializationDelay = 0.5f;

        private void Start()
        {
            if (autoInitialize)
            {
                Invoke(nameof(Initialize), initializationDelay);
            }
        }

        /// <summary>
        /// Инициализирует систему карты
        /// </summary>
        public void Initialize()
        {
            if (!MapSystem.Instance.IsInitialized)
            {
                Debug.LogError("MapController: MapSystem not initialized!");
                return;
            }

            Debug.Log("MapController: Map system initialized successfully");
        }

        /// <summary>
        /// Размещает здание на карте
        /// </summary>
        /// <param name="type">Тип здания</param>
        /// <param name="x">Координата X</param>
        /// <param name="y">Координата Y</param>
        /// <returns>Размещённое здание или null если размещение невозможно</returns>
        public BuildingData PlaceBuilding(BuildingType type, int x, int y)
        {
            if (!MapSystem.Instance.IsInitialized)
            {
                Debug.LogError("MapController: Cannot place building - MapSystem not initialized!");
                return null;
            }

            if (MapSystem.Instance.CanPlaceBuilding(type, x, y))
            {
                var building = MapSystem.Instance.PlaceBuilding(type, x, y);
                if (building != null)
                {
                    Debug.Log($"MapController: Building placed at ({x}, {y}): {building}");
                }
                return building;
            }
            else
            {
                Debug.LogWarning($"MapController: Cannot place building at ({x}, {y})");
                return null;
            }
        }

        /// <summary>
        /// Регенерирует карту
        /// </summary>
        public void RegenerateMap()
        {
            if (!MapSystem.Instance.IsInitialized)
            {
                Debug.LogError("MapController: Cannot regenerate - MapSystem not initialized!");
                return;
            }

            Debug.Log("MapController: Regenerating map...");
            MapSystem.Instance.RegenerateMap();
        }

        /// <summary>
        /// Получает количество размещённых зданий
        /// </summary>
        public int GetBuildingCount()
        {
            if (!MapSystem.Instance.IsInitialized)
            {
                return 0;
            }

            return MapSystem.Instance.PlacementManager.GetBuildingCount();
        }

        /// <summary>
        /// Проверяет возможность размещения здания
        /// </summary>
        public bool CanPlaceBuilding(BuildingType type, int x, int y)
        {
            if (!MapSystem.Instance.IsInitialized)
            {
                return false;
            }

            return MapSystem.Instance.CanPlaceBuilding(type, x, y);
        }
    }
}