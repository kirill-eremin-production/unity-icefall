using System;
using System.Collections.Generic;
using UnityEngine;
using Icefall.Map.Core;

namespace Icefall.Map.Placement
{
    /// <summary>
    /// Менеджер размещения зданий
    /// Управляет размещением и удалением зданий на карте
    /// </summary>
    public class BuildingPlacementManager
    {
        private MapData mapData;
        private BuildingPlacementValidator validator;

        // События
        public event Action<BuildingData> OnBuildingPlaced;
        public event Action<int> OnBuildingRemoved;
        public event Action<BuildingData> OnBuildingStateChanged;

        public BuildingPlacementManager(MapData mapData, BuildingPlacementValidator validator)
        {
            this.mapData = mapData;
            this.validator = validator;
        }

        /// <summary>
        /// Размещает здание на карте
        /// </summary>
        public BuildingData PlaceBuilding(BuildingType type, int x, int y)
        {
            int width = BuildingDefaults.GetDefaultWidth(type);
            int height = BuildingDefaults.GetDefaultHeight(type);

            return PlaceBuilding(type, x, y, width, height);
        }

        /// <summary>
        /// Размещает здание с кастомными размерами
        /// </summary>
        public BuildingData PlaceBuilding(BuildingType type, int x, int y, int width, int height)
        {
            // 1. Валидация позиции
            var validationResult = validator.ValidateBuildingPosition(x, y, width, height);
            if (!validationResult.IsValid)
            {
                Debug.LogWarning($"BuildingPlacementManager: Cannot place building - {validationResult.ErrorMessage}");
                return null;
            }

            // 2. Создание здания через MapData
            var building = mapData.AddBuilding(type, x, y, width, height);

            // 3. Логирование
            Debug.Log($"BuildingPlacementManager: Placed {building}");

            // 4. Триггер события
            OnBuildingPlaced?.Invoke(building);

            return building;
        }

        /// <summary>
        /// Удаляет здание с карты
        /// </summary>
        public bool RemoveBuilding(int buildingId)
        {
            var building = mapData.GetBuilding(buildingId);
            if (building == null)
            {
                Debug.LogWarning($"BuildingPlacementManager: Building #{buildingId} not found");
                return false;
            }

            // Удаляем через MapData
            bool removed = mapData.RemoveBuilding(buildingId);

            if (removed)
            {
                Debug.Log($"BuildingPlacementManager: Removed building #{buildingId}");
                OnBuildingRemoved?.Invoke(buildingId);
            }

            return removed;
        }

        /// <summary>
        /// Удаляет здание по позиции
        /// </summary>
        public bool RemoveBuildingAt(int x, int y)
        {
            var building = mapData.GetBuildingAt(x, y);
            if (building == null)
            {
                Debug.LogWarning($"BuildingPlacementManager: No building at ({x}, {y})");
                return false;
            }

            return RemoveBuilding(building.BuildingId);
        }

        /// <summary>
        /// Возвращает здание на заданной позиции
        /// </summary>
        public BuildingData GetBuildingAt(int x, int y)
        {
            return mapData.GetBuildingAt(x, y);
        }

        /// <summary>
        /// Возвращает все здания в заданной области
        /// </summary>
        public List<BuildingData> GetAllBuildingsInArea(int x, int y, int width, int height)
        {
            var buildingsInArea = new List<BuildingData>();
            var processedIds = new HashSet<int>();

            for (int bx = x; bx < x + width; bx++)
            {
                for (int by = y; by < y + height; by++)
                {
                    if (!mapData.IsValidPosition(bx, by))
                        continue;

                    var cell = mapData.GetCell(bx, by);
                    if (cell.HasBuilding && !processedIds.Contains(cell.BuildingId))
                    {
                        var building = mapData.GetBuilding(cell.BuildingId);
                        if (building != null)
                        {
                            buildingsInArea.Add(building);
                            processedIds.Add(cell.BuildingId);
                        }
                    }
                }
            }

            return buildingsInArea;
        }

        /// <summary>
        /// Возвращает все здания на карте
        /// </summary>
        public IEnumerable<BuildingData> GetAllBuildings()
        {
            return mapData.GetAllBuildings();
        }

        /// <summary>
        /// Обновляет состояние здания
        /// </summary>
        public bool UpdateBuildingState(int buildingId, BuildingState newState)
        {
            var building = mapData.GetBuilding(buildingId);
            if (building == null)
                return false;

            var oldState = building.State;
            building.State = newState;

            Debug.Log($"BuildingPlacementManager: Building #{buildingId} state changed from {oldState} to {newState}");
            OnBuildingStateChanged?.Invoke(building);

            return true;
        }

        /// <summary>
        /// Обновляет прогресс строительства
        /// </summary>
        public bool UpdateConstructionProgress(int buildingId, float progress)
        {
            var building = mapData.GetBuilding(buildingId);
            if (building == null)
                return false;

            progress = Mathf.Clamp(progress, 0f, 100f);
            building.ConstructionProgress = progress;

            // Автоматически завершаем строительство при достижении 100%
            if (progress >= 100f && building.State == BuildingState.UnderConstruction)
            {
                UpdateBuildingState(buildingId, BuildingState.Completed);
            }

            return true;
        }

        /// <summary>
        /// Начинает строительство здания
        /// </summary>
        public bool StartConstruction(int buildingId)
        {
            var building = mapData.GetBuilding(buildingId);
            if (building == null)
                return false;

            if (building.State != BuildingState.Planned)
            {
                Debug.LogWarning($"BuildingPlacementManager: Cannot start construction - building is not in Planned state");
                return false;
            }

            building.State = BuildingState.UnderConstruction;
            building.ConstructionProgress = 0f;

            Debug.Log($"BuildingPlacementManager: Started construction of building #{buildingId}");
            OnBuildingStateChanged?.Invoke(building);

            return true;
        }

        /// <summary>
        /// Завершает строительство здания
        /// </summary>
        public bool CompleteConstruction(int buildingId)
        {
            var building = mapData.GetBuilding(buildingId);
            if (building == null)
                return false;

            building.State = BuildingState.Completed;
            building.ConstructionProgress = 100f;

            Debug.Log($"BuildingPlacementManager: Completed construction of building #{buildingId}");
            OnBuildingStateChanged?.Invoke(building);

            return true;
        }

        /// <summary>
        /// Проверяет, можно ли разместить здание
        /// </summary>
        public bool CanPlaceBuilding(BuildingType type, int x, int y)
        {
            return validator.CanPlaceBuilding(type, x, y);
        }

        /// <summary>
        /// Возвращает результат валидации размещения
        /// </summary>
        public ValidationResult ValidatePlacement(BuildingType type, int x, int y)
        {
            int width = BuildingDefaults.GetDefaultWidth(type);
            int height = BuildingDefaults.GetDefaultHeight(type);
            return validator.ValidateBuildingPosition(x, y, width, height);
        }

        /// <summary>
        /// Возвращает количество зданий на карте
        /// </summary>
        public int GetBuildingCount()
        {
            return mapData.BuildingCount;
        }

        /// <summary>
        /// Возвращает количество зданий определённого типа
        /// </summary>
        public int GetBuildingCountByType(BuildingType type)
        {
            int count = 0;
            foreach (var building in mapData.GetAllBuildings())
            {
                if (building.Type == type)
                    count++;
            }
            return count;
        }

        /// <summary>
        /// Возвращает статистику зданий
        /// </summary>
        public Dictionary<BuildingType, int> GetBuildingStatistics()
        {
            var stats = new Dictionary<BuildingType, int>();

            foreach (var building in mapData.GetAllBuildings())
            {
                if (!stats.ContainsKey(building.Type))
                    stats[building.Type] = 0;

                stats[building.Type]++;
            }

            return stats;
        }

        /// <summary>
        /// Удаляет все здания с карты
        /// </summary>
        public void ClearAllBuildings()
        {
            var buildingIds = new List<int>();
            foreach (var building in mapData.GetAllBuildings())
            {
                buildingIds.Add(building.BuildingId);
            }

            foreach (var id in buildingIds)
            {
                RemoveBuilding(id);
            }

            Debug.Log("BuildingPlacementManager: All buildings cleared");
        }
    }
}