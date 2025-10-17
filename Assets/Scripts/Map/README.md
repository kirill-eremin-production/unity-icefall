# Система генерации карты Icefall

## Обзор

Полнофункциональная система генерации и управления картой 1000x1000 для игры Icefall.

### Основные компоненты

- **MapSystem** - главный координатор (Singleton MonoBehaviour)
- **MapData** - контейнер данных карты
- **MapGenerator** - генератор карты с Perlin Noise
- **BuildingPlacementValidator** - валидатор размещения зданий
- **BuildingPlacementManager** - менеджер размещения зданий
- **MapVisualizationController** - контроллер визуализации
- **ChunkVisualizer** - процедурная генерация mesh для чанков

### Характеристики

- Размер карты: 1000x1000 ячеек
- Система чанков: 20x20 чанков (по 50x50 ячеек каждый)
- Оптимизировано для WebGL
- Процедурная генерация mesh (один mesh на чанк)
- Поддержка различных типов терраина
- Система событий для реакции на изменения

## Быстрый старт

### 1. Интеграция в сцену

1. Откройте сцену [`SampleScene`](../../Scenes/SampleScene.unity)
2. Создайте пустой GameObject и назовите его "MapSystem"
3. Добавьте компонент [`MapSystem`](Core/MapSystem.cs)
4. Для управления камерой добавьте [`CameraController`](../Camera/CameraController.cs) на Main Camera
5. (Опционально) Создайте GameObject "MapController" и добавьте [`MapController`](MapController.cs)

### 2. Настройка MapSystem

В Inspector для MapSystem:

- **Generation Seed**: -1 (случайный) или любое число для фиксированной генерации
- **Generate On Start**: true (генерировать при старте)
- **Visualize On Start**: true (визуализировать при старте)
- **Render Distance**: 10 (количество чанков для рендеринга)

### 3. Настройка камеры

Для лучшего обзора карты настройте Main Camera:
- Position: (500, 100, 450)
- Rotation: (45, 0, 0)
- Field of View: 60

### 4. Запуск

Нажмите Play. Карта сгенерируется автоматически!

## Использование API

### Доступ к системе

```csharp
// Получение singleton instance
var mapSystem = MapSystem.Instance;

// Проверка инициализации
if (mapSystem.IsInitialized)
{
    // Работа с системой
}
```

### Размещение зданий

```csharp
// Проверить возможность размещения
if (MapSystem.Instance.CanPlaceBuilding(BuildingType.House, x, y))
{
    // Разместить здание
    BuildingData building = MapSystem.Instance.PlaceBuilding(BuildingType.House, x, y);
    Debug.Log($"Размещено: {building}");
}
```

### Удаление зданий

```csharp
// Удалить по ID
MapSystem.Instance.RemoveBuilding(buildingId);
```

### Работа с данными карты

```csharp
// Получить ячейку
GridCell cell = MapSystem.Instance.MapData.GetCell(x, y);

// Получить здание на позиции
BuildingData building = MapSystem.Instance.PlacementManager.GetBuildingAt(x, y);

// Получить все здания
foreach (var building in MapSystem.Instance.PlacementManager.GetAllBuildings())
{
    Debug.Log(building);
}
```

### События

```csharp
// Подписка на события
MapSystem.Instance.OnBuildingPlaced += (building) => 
{
    Debug.Log($"Здание размещено: {building}");
};

MapSystem.Instance.OnBuildingRemoved += (buildingId) => 
{
    Debug.Log($"Здание удалено: #{buildingId}");
};

MapSystem.Instance.OnMapGenerated += () => 
{
    Debug.Log("Карта сгенерирована!");
};
```

### Управление генерацией

```csharp
// Регенерировать карту
MapSystem.Instance.RegenerateMap();

// Очистить карту
MapSystem.Instance.ResetMap();

// Сгенерировать новую карту
MapSystem.Instance.GenerateMap();
```

## Управление камерой

Используйте [`CameraController`](../Camera/CameraController.cs) для управления:

**Клавиши управления:**
- `WASD` - движение камеры (SHIFT для ускорения)
- `Q/E` - вращение камеры
- `Mouse Drag (LMB)` - перетаскивание карты
- `Mouse Scroll` - плавный зум к курсору

## Управление картой

Используйте [`MapController`](MapController.cs) для программного управления картой:

```csharp
// Разместить здание
mapController.PlaceBuilding(BuildingType.House, x, y);

// Регенерировать карту
mapController.RegenerateMap();

// Получить количество зданий
int count = mapController.GetBuildingCount();
```

## Типы зданий

Доступные типы зданий (в [`BuildingData.cs`](Core/BuildingData.cs)):

- `House` - Жилой дом (2x2)
- `Warehouse` - Склад (3x4)
- `PowerPlant` - Электростанция (4x4)
- `Farm` - Ферма (5x4)
- `Workshop` - Мастерская (3x3)
- `Hospital` - Больница (4x3)
- `School` - Школа (3x3)
- `Mine` - Шахта (3x3)
- `WoodCutter` - Лесопилка (2x2)
- `HeatingPlant` - Отопительная станция (3x3)

## Типы терраина

Доступные типы терраина (в [`GridCell.cs`](Core/GridCell.cs)):

- `Snow` - Снег (белый с голубым оттенком)
- `Rock` - Скала (серый)
- `Ice` - Лёд (светло-голубой)
- `Dirt` - Грязь (коричневый)

## Оптимизация

### Производительность

- **Чанкирование**: Карта разделена на 400 чанков (20x20)
- **Процедурный mesh**: Каждый чанк = один mesh вместо 2500 объектов
- **Lazy loading**: Загрузка только видимых чанков (опция)
- **Vertex colors**: Цвет через вершины вместо текстур
- **Memory**: ~20 МБ для всей карты в памяти

### Настройка для WebGL

Для оптимальной работы в браузере:

1. Уменьшите `Render Distance` до 5-7 чанков
2. Включите `UpdateVisualizationBasedOnCamera()` в `MapSystem.Update()`
3. Используйте GPU Instancing для материалов

## Расширение системы

### Добавление нового типа здания

1. Добавьте новый тип в `BuildingType` enum
2. Укажите размеры в `BuildingDefaults.GetDefaultWidth/Height()`
3. Используйте как обычно: `PlaceBuilding(YourNewType, x, y)`

### Добавление нового типа терраина

1. Добавьте новый тип в `TerrainType` enum
2. Настройте цвет в `ChunkVisualizer.GetTerrainColor()`
3. Измените распределение в `MapGenerator.GetTerrainTypeFromNoise()`

## Структура файлов

```
Assets/Scripts/
├── Map/
│   ├── Core/
│   │   ├── GridCell.cs              - Структура ячейки
│   │   ├── BuildingData.cs          - Данные здания
│   │   ├── MapChunk.cs              - Структура чанка
│   │   ├── MapData.cs               - Контейнер данных
│   │   └── MapSystem.cs             - Главный координатор
│   ├── Generation/
│   │   └── MapGenerator.cs          - Генератор карты
│   ├── Placement/
│   │   ├── BuildingPlacementValidator.cs  - Валидатор
│   │   └── BuildingPlacementManager.cs    - Менеджер размещения
│   ├── Visualization/
│   │   ├── ChunkVisualizer.cs       - Визуализатор чанка
│   │   └── MapVisualizationController.cs  - Контроллер визуализации
│   ├── MapController.cs             - Контроллер карты
│   ├── INTEGRATION_GUIDE.md         - Руководство по интеграции
│   └── README.md                    - Этот файл
└── Camera/
    └── CameraController.cs          - Контроллер камеры
```

## Известные ограничения

1. Максимальный размер здания: 255x255 (ограничение byte)
2. Максимальное количество зданий: ~2 миллиарда (ограничение int)
3. Визуализация всех 400 чанков одновременно может быть медленной на слабых устройствах

## Следующие шаги

- [ ] Добавить систему дорог
- [ ] Реализовать сохранение/загрузку карты
- [ ] Добавить визуализацию зданий (3D модели)
- [ ] Реализовать систему зон
- [ ] Добавить динамическое изменение терраина

## Поддержка

Для вопросов и предложений обращайтесь к документации проекта в [`memory-bank/`](../../../memory-bank/).