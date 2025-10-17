# Руководство по интеграции системы карты

## Пошаговая интеграция в Unity сцену

### Шаг 1: Подготовка сцены

1. Откройте сцену [`SampleScene`](../../Scenes/SampleScene.unity) в Unity
2. Удалите или отключите стандартные объекты, если они мешают

### Шаг 2: Добавление MapSystem

1. В Hierarchy нажмите правой кнопкой → Create Empty
2. Назовите объект **"MapSystem"**
3. В Inspector нажмите **Add Component**
4. Найдите и добавьте скрипт **Map System**

### Шаг 3: Настройка MapSystem

В Inspector для MapSystem настройте параметры:

```
Map Generation Settings:
├─ Generation Seed: -1           (случайная генерация каждый раз)
├─ Generate On Start: ✓          (генерировать при запуске)
└─ Visualize On Start: ✓         (показывать визуализацию)

Visualization Settings:
└─ Render Distance: 10            (чанков вокруг камеры)
```

### Шаг 4: Настройка камеры

Настройте Main Camera для лучшего обзора:

1. Выберите Main Camera в Hierarchy
2. В Inspector установите Transform:
   - Position: `(500, 100, 450)`
   - Rotation: `(45, 0, 0)`
   - Scale: `(1, 1, 1)`
3. Camera component:
   - Field of View: `60`
   - Clipping Planes Near: `0.3`
   - Clipping Planes Far: `2000`

### Шаг 5: Добавление управления

**Управление камерой:**

1. Выберите **Main Camera** в Hierarchy
2. В Inspector нажмите **Add Component**
3. Найдите и добавьте **Camera Controller** (из [`CameraController.cs`](../Camera/CameraController.cs))
4. В Inspector настройте параметры:
   - **Camera Move Speed**: `50` (скорость движения камеры)
   - **Camera Rotate Speed**: `100` (скорость вращения камеры)
   - **Camera Zoom Speed**: `50` (скорость зума камеры)
   - **Use Bounds**: ✓ (ограничивать движение камеры)

**Управление картой (опционально):**

1. Создайте новый пустой GameObject и назовите его "MapController"
2. В Inspector нажмите **Add Component**
3. Найдите и добавьте **Map Controller** (из [`MapController.cs`](MapController.cs))
4. В Inspector настройте параметры:
   - **Auto Initialize**: ✓ (автоматическая инициализация)
   - **Initialization Delay**: `0.5` (задержка перед инициализацией)

> **Примечание:** [`CameraController`](../Camera/CameraController.cs) использует новый **Input System Package**. Убедитесь, что пакет установлен через Package Manager (Window → Package Manager → Input System).

### Шаг 6: Настройка освещения

Для лучшей визуализации:

1. Выберите Directional Light
2. Установите Rotation: `(50, -30, 0)`
3. Intensity: `1.5`

### Шаг 7: Запуск

1. Сохраните сцену (Ctrl+S)
2. Нажмите Play
3. Подождите несколько секунд - карта генерируется
4. Используйте WASD для навигации камеры

## Проверка работоспособности

### После запуска вы должны увидеть:

✓ В Console лог "MapSystem: Initialization complete"
✓ В Console лог "MapGenerator: Map generation completed"
✓ В Scene View видна 3D карта с вариативным терраином
✓ Разные цвета терраина (белый снег, серые скалы, голубой лёд, коричневая грязь)
✓ Карта готова к использованию

### Если что-то не работает:

1. **Проверьте Console на ошибки компиляции**
   - Все скрипты должны быть без ошибок
   
2. **MapSystem не инициализируется**
   - Проверьте, что компонент MapSystem добавлен на GameObject
   - Проверьте, что Generate On Start включен
   
3. **Карта не видна**
   - Проверьте позицию камеры
   - Проверьте, что Visualize On Start включен
   - Откройте Scene View и найдите MapRoot объект

4. **Камера не двигается при нажатии WASD**
   - Проверьте, что [`CameraController`](../Camera/CameraController.cs) добавлен на **Main Camera**
   - Убедитесь, что компонент включен (галочка в Inspector)
   - Проверьте, что Camera Move Speed > 0
   - **Убедитесь, что Input System Package установлен** (Package Manager → Input System)
   - В Console не должно быть ошибок типа `InvalidOperationException` про Input
   
5. **Низкая производительность**
   - Уменьшите Render Distance до 5
   - Отключите размещение тестовых зданий (Place Test Buildings = false)
   - Проверьте Profiler (Window → Analysis → Profiler)

## Управление в режиме Play (если добавлен CameraController)

| Клавиша/Действие | Описание |
|------------------|----------|
| **W/A/S/D** | Движение камеры относительно направления взгляда |
| **SHIFT + WASD** | Ускоренное движение камеры (2.5x быстрее) |
| **Q/E** | Вращение камеры вокруг вертикальной оси |
| **ЛКМ (зажать и двигать)** | Перетаскивание карты (pan) |
| **Mouse Scroll** | Плавный зум к позиции курсора на карте |

### Особенности управления:

- **Интуитивное движение**: Камера двигается относительно своего направления взгляда. После поворота через Q/E, WASD автоматически работают в новом направлении.
- **Умный зум**: При прокрутке колесика мыши камера плавно приближается к точке под курсором, а не просто к центру экрана.
- **Комфортная навигация**: Сочетайте перетаскивание мышью, WASD для точного позиционирования и зум для детального обзора.

## Следующие шаги

После успешной интеграции вы можете:

1. **Создать собственный UI** для управления картой
2. **Добавить систему камеры** с более продвинутыми функциями
3. **Реализовать сохранение/загрузку** карты
4. **Добавить 3D модели** для зданий
5. **Расширить типы** терраина и зданий

## Производительность

### Ожидаемые метрики:

- **Время генерации карты**: 100-500 мс
- **Memory usage**: 30-50 МБ
- **FPS**: 60+ при просмотре карты
- **Draw calls**: 400-500 (один на чанк + overhead)

### Оптимизация для WebGL:

1. В MapSystem уменьшите Render Distance до 5-7
2. В Build Settings выберите WebGL platform
3. В Player Settings → WebGL:
   - Memory Size: 1024 MB (минимум)
   - Enable Exceptions: None
   - Compression Format: Gzip
4. В Quality Settings:
   - Используйте Low или Medium preset

## API для разработчиков

### Базовое использование:

```csharp
using Icefall.Map.Core;

public class YourScript : MonoBehaviour
{
    void Start()
    {
        // Подождать инициализации
        StartCoroutine(WaitForMapSystem());
    }
    
    IEnumerator WaitForMapSystem()
    {
        // Ждём пока система инициализируется
        while (!MapSystem.Instance.IsInitialized)
        {
            yield return null;
        }
        
        // Теперь можно работать с картой
        PlaceBuilding();
    }
    
    void PlaceBuilding()
    {
        int x = 500;
        int y = 500;
        
        if (MapSystem.Instance.CanPlaceBuilding(BuildingType.House, x, y))
        {
            var building = MapSystem.Instance.PlaceBuilding(BuildingType.House, x, y);
            Debug.Log($"Здание размещено: {building}");
        }
    }
}
```

### Подписка на события:

```csharp
void OnEnable()
{
    MapSystem.Instance.OnBuildingPlaced += HandleBuildingPlaced;
    MapSystem.Instance.OnMapGenerated += HandleMapGenerated;
}

void OnDisable()
{
    if (MapSystem.Instance != null)
    {
        MapSystem.Instance.OnBuildingPlaced -= HandleBuildingPlaced;
        MapSystem.Instance.OnMapGenerated -= HandleMapGenerated;
    }
}

void HandleBuildingPlaced(BuildingData building)
{
    Debug.Log($"Новое здание: {building.Type} на ({building.PositionX}, {building.PositionY})");
}

void HandleMapGenerated()
{
    Debug.Log("Карта готова к использованию!");
}
```

## Поддержка и дополнительная информация

- Основная документация: [`README.md`](README.md)
- Детальный план системы: [`memory-bank/map-generation-plan.md`](../../../memory-bank/map-generation-plan.md)
- Обзор проекта: [`memory-bank/project-overview.md`](../../../memory-bank/project-overview.md)

## Чеклист интеграции

- [ ] MapSystem добавлен в сцену
- [ ] MapSystem настроен (Generate On Start ✓, Visualize On Start ✓)
- [ ] Камера позиционирована правильно
- [ ] **CameraController добавлен на Main Camera** (для управления WASD)
- [ ] **MapController добавлен в сцену** (опционально, для управления картой)
- [ ] Сцена сохранена
- [ ] Проект компилируется без ошибок
- [ ] Play Mode работает корректно
- [ ] Карта видна в Scene/Game View
- [ ] Console показывает логи успешной инициализации
- [ ] **Камера двигается при нажатии WASD**
- [ ] Производительность приемлемая (60+ FPS)