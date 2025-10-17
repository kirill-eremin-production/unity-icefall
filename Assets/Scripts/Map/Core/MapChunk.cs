using System.Collections.Generic;
using UnityEngine;

namespace Icefall.Map.Core
{
    /// <summary>
    /// Чанк карты для оптимизации визуализации и обработки
    /// Представляет квадратный участок карты фиксированного размера
    /// </summary>
    public class MapChunk
    {
        public int ChunkX { get; set; }                      // Позиция чанка в сетке чанков (не в ячейках!)
        public int ChunkY { get; set; }
        public int ChunkSize { get; private set; }           // Размер чанка (обычно 50 или 100)

        public GridCell[] Cells { get; private set; }        // Массив ячеек в чанке (размер = ChunkSize * ChunkSize)
        public List<int> BuildingIds { get; private set; }   // ID зданий, которые пересекаются с этим чанком

        // Визуализация
        public bool IsLoaded { get; set; }                   // Загружена ли визуализация чанка
        public GameObject ChunkObject { get; set; }          // Unity GameObject чанка
        public Mesh ChunkMesh { get; set; }                  // Процедурный mesh чанка

        /// <summary>
        /// Создаёт новый чанк
        /// </summary>
        public MapChunk(int chunkX, int chunkY, int chunkSize)
        {
            ChunkX = chunkX;
            ChunkY = chunkY;
            ChunkSize = chunkSize;
            Cells = new GridCell[chunkSize * chunkSize];
            BuildingIds = new List<int>();
            IsLoaded = false;
            ChunkObject = null;
            ChunkMesh = null;
        }

        /// <summary>
        /// Возвращает начальную позицию чанка в координатах карты
        /// </summary>
        public Vector2Int WorldPosition => new Vector2Int(ChunkX * ChunkSize, ChunkY * ChunkSize);

        /// <summary>
        /// Возвращает индекс ячейки в массиве Cells по локальным координатам
        /// </summary>
        public int GetCellIndex(int localX, int localY)
        {
            if (localX < 0 || localX >= ChunkSize || localY < 0 || localY >= ChunkSize)
                return -1;

            return localY * ChunkSize + localX;
        }

        /// <summary>
        /// Возвращает ячейку по локальным координатам в чанке
        /// </summary>
        public GridCell GetCell(int localX, int localY)
        {
            int index = GetCellIndex(localX, localY);
            if (index < 0 || index >= Cells.Length)
                return default;

            return Cells[index];
        }

        /// <summary>
        /// Устанавливает ячейку по локальным координатам в чанке
        /// </summary>
        public void SetCell(int localX, int localY, GridCell cell)
        {
            int index = GetCellIndex(localX, localY);
            if (index >= 0 && index < Cells.Length)
            {
                Cells[index] = cell;
            }
        }

        /// <summary>
        /// Проверяет, содержится ли здание в этом чанке
        /// </summary>
        public bool ContainsBuilding(int buildingId)
        {
            return BuildingIds.Contains(buildingId);
        }

        /// <summary>
        /// Добавляет здание в список зданий чанка
        /// </summary>
        public void AddBuilding(int buildingId)
        {
            if (!BuildingIds.Contains(buildingId))
            {
                BuildingIds.Add(buildingId);
            }
        }

        /// <summary>
        /// Удаляет здание из списка зданий чанка
        /// </summary>
        public void RemoveBuilding(int buildingId)
        {
            BuildingIds.Remove(buildingId);
        }

        /// <summary>
        /// Очищает список зданий
        /// </summary>
        public void ClearBuildings()
        {
            BuildingIds.Clear();
        }

        /// <summary>
        /// Проверяет, нуждается ли чанк в обновлении визуализации
        /// </summary>
        public bool NeedsVisualizationUpdate { get; set; } = true;

        /// <summary>
        /// Помечает чанк как требующий обновления визуализации
        /// </summary>
        public void MarkForUpdate()
        {
            NeedsVisualizationUpdate = true;
        }

        /// <summary>
        /// Очищает визуализацию чанка
        /// </summary>
        public void ClearVisualization()
        {
            if (ChunkObject != null)
            {
                Object.Destroy(ChunkObject);
                ChunkObject = null;
            }

            if (ChunkMesh != null)
            {
                Object.Destroy(ChunkMesh);
                ChunkMesh = null;
            }

            IsLoaded = false;
        }

        public override string ToString()
        {
            return $"MapChunk ({ChunkX}, {ChunkY}) - Size: {ChunkSize}x{ChunkSize}, Buildings: {BuildingIds.Count}, Loaded: {IsLoaded}";
        }
    }
}