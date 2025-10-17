using System.Collections.Generic;
using UnityEngine;
using Icefall.Map.Core;

namespace Icefall.Map.Placement
{
    /// <summary>
    /// Результат валидации размещения здания
    /// </summary>
    public class ValidationResult
    {
        public bool IsValid { get; set; }
        public string ErrorMessage { get; set; }
        public List<Vector2Int> ConflictCells { get; set; }

        public ValidationResult(bool isValid, string errorMessage = "")
        {
            IsValid = isValid;
            ErrorMessage = errorMessage;
            ConflictCells = new List<Vector2Int>();
        }

        public static ValidationResult Success()
        {
            return new ValidationResult(true);
        }

        public static ValidationResult Failure(string message)
        {
            return new ValidationResult(false, message);
        }
    }

    /// <summary>
    /// Валидатор размещения зданий
    /// Проверяет возможность размещения здания на карте
    /// </summary>
    public class BuildingPlacementValidator
    {
        private MapData mapData;

        public BuildingPlacementValidator(MapData mapData)
        {
            this.mapData = mapData;
        }

        /// <summary>
        /// Проверяет, можно ли разместить здание на заданной позиции
        /// </summary>
        public bool CanPlaceBuilding(BuildingType type, int x, int y)
        {
            int width = BuildingDefaults.GetDefaultWidth(type);
            int height = BuildingDefaults.GetDefaultHeight(type);
            return CanPlaceBuilding(x, y, width, height);
        }

        /// <summary>
        /// Проверяет, можно ли разместить здание с заданными размерами
        /// </summary>
        public bool CanPlaceBuilding(int x, int y, int width, int height)
        {
            var result = ValidateBuildingPosition(x, y, width, height);
            return result.IsValid;
        }

        /// <summary>
        /// Полная валидация позиции здания с детальной информацией
        /// </summary>
        public ValidationResult ValidateBuildingPosition(int x, int y, int width, int height)
        {
            // 1. Проверка границ карты
            if (!IsWithinMapBounds(x, y, width, height))
            {
                return ValidationResult.Failure($"Здание выходит за границы карты. Позиция: ({x}, {y}), Размер: {width}x{height}");
            }

            // 2. Проверка доступности всех ячеек
            var unavailableCells = GetUnavailableCells(x, y, width, height);
            if (unavailableCells.Count > 0)
            {
                var result = ValidationResult.Failure($"Некоторые ячейки недоступны для строительства. Конфликтов: {unavailableCells.Count}");
                result.ConflictCells = unavailableCells;
                return result;
            }

            // 3. Проверка на водные препятствия (если требуется)
            if (HasWaterObstacles(x, y, width, height))
            {
                return ValidationResult.Failure("В зоне размещения есть водные препятствия");
            }

            // Все проверки пройдены
            return ValidationResult.Success();
        }

        /// <summary>
        /// Проверяет, находится ли здание в пределах карты
        /// </summary>
        private bool IsWithinMapBounds(int x, int y, int width, int height)
        {
            return x >= 0 && y >= 0 &&
                   x + width <= MapData.MAP_WIDTH &&
                   y + height <= MapData.MAP_HEIGHT;
        }

        /// <summary>
        /// Возвращает список недоступных ячеек в зоне здания
        /// </summary>
        private List<Vector2Int> GetUnavailableCells(int x, int y, int width, int height)
        {
            var unavailableCells = new List<Vector2Int>();

            for (int bx = x; bx < x + width; bx++)
            {
                for (int by = y; by < y + height; by++)
                {
                    if (!mapData.IsValidPosition(bx, by))
                    {
                        unavailableCells.Add(new Vector2Int(bx, by));
                        continue;
                    }

                    if (!mapData.IsCellAvailable(bx, by))
                    {
                        unavailableCells.Add(new Vector2Int(bx, by));
                    }
                }
            }

            return unavailableCells;
        }

        /// <summary>
        /// Проверяет наличие водных препятствий в зоне размещения
        /// </summary>
        private bool HasWaterObstacles(int x, int y, int width, int height)
        {
            for (int bx = x; bx < x + width; bx++)
            {
                for (int by = y; by < y + height; by++)
                {
                    if (!mapData.IsValidPosition(bx, by))
                        continue;

                    var cell = mapData.GetCell(bx, by);
                    if (cell.State == CellState.Water)
                        return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Возвращает все ячейки, которые займёт здание
        /// </summary>
        public List<GridCell> GetBuildingFootprint(int x, int y, int width, int height)
        {
            var cells = new List<GridCell>();

            for (int bx = x; bx < x + width; bx++)
            {
                for (int by = y; by < y + height; by++)
                {
                    if (mapData.IsValidPosition(bx, by))
                    {
                        cells.Add(mapData.GetCell(bx, by));
                    }
                }
            }

            return cells;
        }

        /// <summary>
        /// Проверяет конфликты с существующими зданиями
        /// </summary>
        public List<GridCell> CheckCollisions(int x, int y, int width, int height)
        {
            var collisions = new List<GridCell>();

            for (int bx = x; bx < x + width; bx++)
            {
                for (int by = y; by < y + height; by++)
                {
                    if (!mapData.IsValidPosition(bx, by))
                        continue;

                    var cell = mapData.GetCell(bx, by);
                    if (cell.HasBuilding)
                    {
                        collisions.Add(cell);
                    }
                }
            }

            return collisions;
        }

        /// <summary>
        /// Возвращает доступные позиции для размещения здания заданного типа
        /// ВНИМАНИЕ: Медленная операция для больших областей!
        /// Используйте только для небольших зон или в редакторе
        /// </summary>
        public List<Vector2Int> GetAvailablePlacementSpots(BuildingType type, int searchX, int searchY, int searchWidth, int searchHeight)
        {
            int buildingWidth = BuildingDefaults.GetDefaultWidth(type);
            int buildingHeight = BuildingDefaults.GetDefaultHeight(type);

            var availableSpots = new List<Vector2Int>();

            // Ограничиваем поиск заданной областью
            int endX = Mathf.Min(searchX + searchWidth, MapData.MAP_WIDTH - buildingWidth);
            int endY = Mathf.Min(searchY + searchHeight, MapData.MAP_HEIGHT - buildingHeight);

            for (int x = searchX; x < endX; x++)
            {
                for (int y = searchY; y < endY; y++)
                {
                    if (CanPlaceBuilding(x, y, buildingWidth, buildingHeight))
                    {
                        availableSpots.Add(new Vector2Int(x, y));
                    }
                }
            }

            return availableSpots;
        }

        /// <summary>
        /// Проверяет, можно ли разместить здание с учётом минимального расстояния до других зданий
        /// </summary>
        public bool CanPlaceBuildingWithSpacing(int x, int y, int width, int height, int minSpacing)
        {
            // Расширяем зону проверки на minSpacing во все стороны
            int expandedX = Mathf.Max(0, x - minSpacing);
            int expandedY = Mathf.Max(0, y - minSpacing);
            int expandedWidth = width + minSpacing * 2;
            int expandedHeight = height + minSpacing * 2;

            // Проверяем расширенную зону
            return CanPlaceBuilding(expandedX, expandedY, expandedWidth, expandedHeight);
        }

        /// <summary>
        /// Валидирует уже существующее здание
        /// </summary>
        public ValidationResult ValidateBuildingData(BuildingData building)
        {
            if (building == null)
                return ValidationResult.Failure("Данные здания отсутствуют");

            return ValidateBuildingPosition(
                building.PositionX,
                building.PositionY,
                building.Width,
                building.Height
            );
        }
    }
}