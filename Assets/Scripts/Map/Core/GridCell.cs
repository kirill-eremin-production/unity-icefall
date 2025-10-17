using UnityEngine;

namespace Icefall.Map.Core
{
    /// <summary>
    /// Состояние ячейки карты
    /// </summary>
    public enum CellState : byte
    {
        Empty = 0,      // Пусто - доступно для строительства
        Building = 1,   // Занято зданием
        Blocked = 2,    // Заблокировано (препятствие)
        Water = 3       // Вода (опционально)
    }

    /// <summary>
    /// Тип терраина
    /// </summary>
    public enum TerrainType : byte
    {
        Snow = 0,       // Снег
        Rock = 1,       // Скала
        Ice = 2,        // Лёд
        Dirt = 3        // Грязь
    }

    /// <summary>
    /// Структура ячейки карты (квадрата сетки)
    /// Оптимизирована для минимального использования памяти
    /// </summary>
    public struct GridCell
    {
        public int X;                    // Координата X (4 байта)
        public int Y;                    // Координата Y (4 байта)
        public CellState State;          // Состояние ячейки (1 байт)
        public int BuildingId;           // ID здания, если занято (4 байта)
        public byte BuildingLocalX;      // Локальная позиция в здании 0-255 (1 байт)
        public byte BuildingLocalY;      // Локальная позиция в здании 0-255 (1 байт)
        public TerrainType Terrain;      // Тип терраина (1 байт)
        public float Height;             // Высота для вариативности (4 байта)

        // Общий размер: ~20 байт на ячейку
        // Для 1000x1000 карты = ~20 МБ (приемлемо)

        /// <summary>
        /// Создаёт новую пустую ячейку
        /// </summary>
        public GridCell(int x, int y)
        {
            X = x;
            Y = y;
            State = CellState.Empty;
            BuildingId = -1;
            BuildingLocalX = 0;
            BuildingLocalY = 0;
            Terrain = TerrainType.Snow;
            Height = 0f;
        }

        /// <summary>
        /// Проверяет, свободна ли ячейка для строительства
        /// </summary>
        public bool IsAvailable => State == CellState.Empty;

        /// <summary>
        /// Проверяет, занята ли ячейка зданием
        /// </summary>
        public bool HasBuilding => State == CellState.Building && BuildingId >= 0;

        /// <summary>
        /// Возвращает позицию в виде Vector2Int
        /// </summary>
        public Vector2Int Position => new Vector2Int(X, Y);

        /// <summary>
        /// Сбрасывает ячейку в пустое состояние
        /// </summary>
        public void Clear()
        {
            State = CellState.Empty;
            BuildingId = -1;
            BuildingLocalX = 0;
            BuildingLocalY = 0;
        }

        public override string ToString()
        {
            return $"GridCell({X}, {Y}) - State: {State}, Terrain: {Terrain}, Building: {BuildingId}";
        }
    }
}