using UnityEngine;

namespace Icefall.Map.Core
{
    /// <summary>
    /// Тип здания
    /// </summary>
    public enum BuildingType
    {
        House,          // Жилой дом
        Warehouse,      // Склад
        PowerPlant,     // Электростанция
        Farm,           // Ферма
        Workshop,       // Мастерская
        Hospital,       // Больница
        School,         // Школа
        Mine,           // Шахта
        WoodCutter,     // Лесопилка
        HeatingPlant    // Отопительная станция
    }

    /// <summary>
    /// Состояние здания
    /// </summary>
    public enum BuildingState
    {
        Planned,            // Запланировано
        UnderConstruction,  // Строится
        Completed,          // Завершено
        Damaged,            // Повреждено
        Destroyed           // Разрушено
    }

    /// <summary>
    /// Данные о размещённом здании на карте
    /// </summary>
    public class BuildingData
    {
        public int BuildingId { get; set; }                  // Уникальный ID
        public BuildingType Type { get; set; }               // Тип здания
        public int PositionX { get; set; }                   // X позиция (левый-верхний угол)
        public int PositionY { get; set; }                   // Y позиция (левый-верхний угол)
        public int Width { get; set; }                       // Ширина в ячейках
        public int Height { get; set; }                      // Высота в ячейках
        public BuildingState State { get; set; }             // Состояние
        public float ConstructionProgress { get; set; }      // Прогресс строительства 0-100

        // Для будущего функционала
        public int OwnerId { get; set; }                     // ID владельца/фракции
        public float Health { get; set; }                    // Здоровье здания 0-100

        /// <summary>
        /// Создаёт новое здание
        /// </summary>
        public BuildingData(int id, BuildingType type, int x, int y, int width, int height)
        {
            BuildingId = id;
            Type = type;
            PositionX = x;
            PositionY = y;
            Width = width;
            Height = height;
            State = BuildingState.Planned;
            ConstructionProgress = 0f;
            OwnerId = -1;
            Health = 100f;
        }

        /// <summary>
        /// Возвращает позицию здания как Vector2Int
        /// </summary>
        public Vector2Int Position => new Vector2Int(PositionX, PositionY);

        /// <summary>
        /// Возвращает размер здания как Vector2Int
        /// </summary>
        public Vector2Int Size => new Vector2Int(Width, Height);

        /// <summary>
        /// Возвращает центр здания в координатах карты
        /// </summary>
        public Vector2 Center => new Vector2(PositionX + Width * 0.5f, PositionY + Height * 0.5f);

        /// <summary>
        /// Проверяет, находится ли точка внутри здания
        /// </summary>
        public bool ContainsPoint(int x, int y)
        {
            return x >= PositionX && x < PositionX + Width &&
                   y >= PositionY && y < PositionY + Height;
        }

        /// <summary>
        /// Проверяет, завершено ли строительство
        /// </summary>
        public bool IsCompleted => State == BuildingState.Completed;

        /// <summary>
        /// Проверяет, активно ли здание (не разрушено)
        /// </summary>
        public bool IsActive => State != BuildingState.Destroyed;

        public override string ToString()
        {
            return $"Building #{BuildingId} - {Type} at ({PositionX}, {PositionY}) {Width}x{Height}, State: {State}";
        }
    }

    /// <summary>
    /// Статические данные о размерах зданий
    /// </summary>
    public static class BuildingDefaults
    {
        /// <summary>
        /// Возвращает стандартную ширину здания по типу
        /// </summary>
        public static int GetDefaultWidth(BuildingType type)
        {
            return type switch
            {
                BuildingType.House => 2,
                BuildingType.Warehouse => 3,
                BuildingType.PowerPlant => 4,
                BuildingType.Farm => 5,
                BuildingType.Workshop => 3,
                BuildingType.Hospital => 4,
                BuildingType.School => 3,
                BuildingType.Mine => 3,
                BuildingType.WoodCutter => 2,
                BuildingType.HeatingPlant => 3,
                _ => 2
            };
        }

        /// <summary>
        /// Возвращает стандартную высоту здания по типу
        /// </summary>
        public static int GetDefaultHeight(BuildingType type)
        {
            return type switch
            {
                BuildingType.House => 2,
                BuildingType.Warehouse => 4,
                BuildingType.PowerPlant => 4,
                BuildingType.Farm => 4,
                BuildingType.Workshop => 3,
                BuildingType.Hospital => 3,
                BuildingType.School => 3,
                BuildingType.Mine => 3,
                BuildingType.WoodCutter => 2,
                BuildingType.HeatingPlant => 3,
                _ => 2
            };
        }

        /// <summary>
        /// Возвращает размер здания как Vector2Int
        /// </summary>
        public static Vector2Int GetDefaultSize(BuildingType type)
        {
            return new Vector2Int(GetDefaultWidth(type), GetDefaultHeight(type));
        }
    }
}