using System.Collections.Generic;
using UnityEngine;

namespace Icefall.Map.Core
{
    /// <summary>
    /// Главный контейнер данных карты
    /// Управляет сеткой ячеек, чанками и размещёнными зданиями
    /// </summary>
    public class MapData
    {
        // Константы размеров карты
        public const int MAP_WIDTH = 1000;
        public const int MAP_HEIGHT = 1000;
        public const int CHUNK_SIZE = 50;  // Размер чанка (50x50 ячеек)

        // Вычисляемые константы
        public static readonly int CHUNKS_X = MAP_WIDTH / CHUNK_SIZE;   // 20 чанков по X
        public static readonly int CHUNKS_Y = MAP_HEIGHT / CHUNK_SIZE;  // 20 чанков по Y
        public static readonly int TOTAL_CHUNKS = CHUNKS_X * CHUNKS_Y;  // 400 чанков всего

        // Основные данные
        private GridCell[,] grid;                           // Сетка 1000x1000 ячеек
        private MapChunk[,] chunks;                         // Сетка 20x20 чанков
        private Dictionary<int, BuildingData> buildings;    // Словарь зданий по ID
        private int nextBuildingId = 1;                     // Счётчик для ID зданий

        /// <summary>
        /// Конструктор - инициализирует пустую карту
        /// </summary>
        public MapData()
        {
            grid = new GridCell[MAP_WIDTH, MAP_HEIGHT];
            chunks = new MapChunk[CHUNKS_X, CHUNKS_Y];
            buildings = new Dictionary<int, BuildingData>();

            InitializeChunks();
        }

        /// <summary>
        /// Инициализирует чанки
        /// </summary>
        private void InitializeChunks()
        {
            for (int cx = 0; cx < CHUNKS_X; cx++)
            {
                for (int cy = 0; cy < CHUNKS_Y; cy++)
                {
                    chunks[cx, cy] = new MapChunk(cx, cy, CHUNK_SIZE);
                }
            }
        }

        #region Grid Access

        /// <summary>
        /// Возвращает ячейку по координатам
        /// </summary>
        public GridCell GetCell(int x, int y)
        {
            if (!IsValidPosition(x, y))
                return default;

            return grid[x, y];
        }

        /// <summary>
        /// Устанавливает ячейку по координатам
        /// </summary>
        public void SetCell(int x, int y, GridCell cell)
        {
            if (!IsValidPosition(x, y))
                return;

            grid[x, y] = cell;

            // Обновить чанк
            var chunk = GetChunkForPosition(x, y);
            if (chunk != null)
            {
                int localX = x % CHUNK_SIZE;
                int localY = y % CHUNK_SIZE;
                chunk.SetCell(localX, localY, cell);
                chunk.MarkForUpdate();
            }
        }

        /// <summary>
        /// Проверяет валидность позиции на карте
        /// </summary>
        public bool IsValidPosition(int x, int y)
        {
            return x >= 0 && x < MAP_WIDTH && y >= 0 && y < MAP_HEIGHT;
        }

        /// <summary>
        /// Проверяет, свободна ли ячейка для строительства
        /// </summary>
        public bool IsCellAvailable(int x, int y)
        {
            if (!IsValidPosition(x, y))
                return false;

            return grid[x, y].IsAvailable;
        }

        #endregion

        #region Chunk Access

        /// <summary>
        /// Возвращает чанк по его индексу в сетке чанков
        /// </summary>
        public MapChunk GetChunk(int chunkX, int chunkY)
        {
            if (chunkX < 0 || chunkX >= CHUNKS_X || chunkY < 0 || chunkY >= CHUNKS_Y)
                return null;

            return chunks[chunkX, chunkY];
        }

        /// <summary>
        /// Возвращает чанк для заданной позиции на карте
        /// </summary>
        public MapChunk GetChunkForPosition(int x, int y)
        {
            if (!IsValidPosition(x, y))
                return null;

            int chunkX = x / CHUNK_SIZE;
            int chunkY = y / CHUNK_SIZE;

            return GetChunk(chunkX, chunkY);
        }

        /// <summary>
        /// Возвращает все чанки
        /// </summary>
        public MapChunk[,] GetAllChunks()
        {
            return chunks;
        }

        #endregion

        #region Building Management

        /// <summary>
        /// Добавляет новое здание на карту
        /// </summary>
        public BuildingData AddBuilding(BuildingType type, int x, int y, int width, int height)
        {
            var building = new BuildingData(nextBuildingId++, type, x, y, width, height);
            buildings[building.BuildingId] = building;

            // Обновить ячейки
            UpdateCellsForBuilding(building, true);

            // Добавить в затронутые чанки
            AddBuildingToChunks(building);

            return building;
        }

        /// <summary>
        /// Удаляет здание с карты
        /// </summary>
        public bool RemoveBuilding(int buildingId)
        {
            if (!buildings.TryGetValue(buildingId, out var building))
                return false;

            // Очистить ячейки
            UpdateCellsForBuilding(building, false);

            // Удалить из чанков
            RemoveBuildingFromChunks(building);

            // Удалить из словаря
            buildings.Remove(buildingId);

            return true;
        }

        /// <summary>
        /// Возвращает здание по ID
        /// </summary>
        public BuildingData GetBuilding(int buildingId)
        {
            buildings.TryGetValue(buildingId, out var building);
            return building;
        }

        /// <summary>
        /// Возвращает здание на заданной позиции
        /// </summary>
        public BuildingData GetBuildingAt(int x, int y)
        {
            if (!IsValidPosition(x, y))
                return null;

            var cell = grid[x, y];
            if (!cell.HasBuilding)
                return null;

            return GetBuilding(cell.BuildingId);
        }

        /// <summary>
        /// Возвращает все здания
        /// </summary>
        public IEnumerable<BuildingData> GetAllBuildings()
        {
            return buildings.Values;
        }

        /// <summary>
        /// Возвращает количество зданий
        /// </summary>
        public int BuildingCount => buildings.Count;

        /// <summary>
        /// Обновляет ячейки для здания (занимает или освобождает)
        /// </summary>
        private void UpdateCellsForBuilding(BuildingData building, bool occupy)
        {
            for (int x = building.PositionX; x < building.PositionX + building.Width; x++)
            {
                for (int y = building.PositionY; y < building.PositionY + building.Height; y++)
                {
                    if (!IsValidPosition(x, y))
                        continue;

                    var cell = grid[x, y];

                    if (occupy)
                    {
                        cell.State = CellState.Building;
                        cell.BuildingId = building.BuildingId;
                        cell.BuildingLocalX = (byte)(x - building.PositionX);
                        cell.BuildingLocalY = (byte)(y - building.PositionY);
                    }
                    else
                    {
                        cell.Clear();
                    }

                    SetCell(x, y, cell);
                }
            }
        }

        /// <summary>
        /// Добавляет здание во все затронутые чанки
        /// </summary>
        private void AddBuildingToChunks(BuildingData building)
        {
            int startChunkX = building.PositionX / CHUNK_SIZE;
            int startChunkY = building.PositionY / CHUNK_SIZE;
            int endChunkX = (building.PositionX + building.Width - 1) / CHUNK_SIZE;
            int endChunkY = (building.PositionY + building.Height - 1) / CHUNK_SIZE;

            for (int cx = startChunkX; cx <= endChunkX; cx++)
            {
                for (int cy = startChunkY; cy <= endChunkY; cy++)
                {
                    var chunk = GetChunk(cx, cy);
                    chunk?.AddBuilding(building.BuildingId);
                }
            }
        }

        /// <summary>
        /// Удаляет здание из всех чанков
        /// </summary>
        private void RemoveBuildingFromChunks(BuildingData building)
        {
            int startChunkX = building.PositionX / CHUNK_SIZE;
            int startChunkY = building.PositionY / CHUNK_SIZE;
            int endChunkX = (building.PositionX + building.Width - 1) / CHUNK_SIZE;
            int endChunkY = (building.PositionY + building.Height - 1) / CHUNK_SIZE;

            for (int cx = startChunkX; cx <= endChunkX; cx++)
            {
                for (int cy = startChunkY; cy <= endChunkY; cy++)
                {
                    var chunk = GetChunk(cx, cy);
                    chunk?.RemoveBuilding(building.BuildingId);
                }
            }
        }

        #endregion

        #region Utility

        /// <summary>
        /// Очищает всю карту
        /// </summary>
        public void Clear()
        {
            // Очистить здания
            buildings.Clear();
            nextBuildingId = 1;

            // Очистить сетку
            for (int x = 0; x < MAP_WIDTH; x++)
            {
                for (int y = 0; y < MAP_HEIGHT; y++)
                {
                    grid[x, y] = new GridCell(x, y);
                }
            }

            // Очистить чанки
            for (int cx = 0; cx < CHUNKS_X; cx++)
            {
                for (int cy = 0; cy < CHUNKS_Y; cy++)
                {
                    chunks[cx, cy].ClearBuildings();
                    chunks[cx, cy].MarkForUpdate();
                }
            }
        }

        /// <summary>
        /// Возвращает статистику карты
        /// </summary>
        public string GetStats()
        {
            return $"Map: {MAP_WIDTH}x{MAP_HEIGHT}, Chunks: {CHUNKS_X}x{CHUNKS_Y} ({TOTAL_CHUNKS} total), Buildings: {BuildingCount}";
        }

        #endregion
    }
}