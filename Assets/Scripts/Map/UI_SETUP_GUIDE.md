# Пошаговая настройка UI для системы строительства

## Введение

Этот гайд подробно объясняет, как создать UI панель для выбора зданий в Unity Editor.

## Шаг 1: Создание Canvas

1. **Правый клик в Hierarchy** → UI → Canvas
   - Автоматически создастся Canvas и EventSystem
   - Canvas Scaler автоматически настроен для UI

2. **Настройка Canvas (опционально)**:
   - Render Mode: Screen Space - Overlay (по умолчанию)
   - Для 3D игры можно использовать Screen Space - Camera

## Шаг 2: Создание Panel для UI зданий

1. **Правый клик на Canvas** в Hierarchy → UI → Panel
   - Переименуйте в "BuildingSelectionPanel"

2. **Позиционирование Panel**:
   - Выберите BuildingSelectionPanel в Hierarchy
   - В Inspector найдите Rect Transform
   - Установите Anchor Presets (квадратик слева вверху):
     - Для панели справа: Right-Middle
     - Для панели слева: Left-Middle
     - Для панели снизу: Bottom-Center
   
3. **Размер Panel**:
   - Width: 300 (ширина панели)
   - Height: 600 (высота панели)
   - Или установите Pos X, Pos Y, Width, Height вручную

4. **Добавление компонента BuildingSelectionUI**:
   - С выбранным BuildingSelectionPanel
   - В Inspector нажмите "Add Component"
   - Введите "BuildingSelectionUI"
   - Выберите скрипт из списка

## Шаг 3: Создание Title (Заголовок)

1. **Правый клик на BuildingSelectionPanel** → UI → Text - TextMeshPro
   - Если появится окно импорта TMP Essentials - нажмите "Import TMP Essentials"
   - Переименуйте в "Title"

2. **Настройка Title**:
   - **Rect Transform**:
     - Anchor Presets: Top-Center (или Top-Stretch для растяжения)
     - Pos X: 0
     - Pos Y: -30 (отступ от верха)
     - Width: 280
     - Height: 40
   
   - **TextMeshPro - Text**:
     - Text: "Строительство"
     - Font Size: 24
     - Alignment: Center + Middle
     - Color: White или любой другой

## Шаг 4: Создание Close Button (опционально)

1. **Правый клик на BuildingSelectionPanel** → UI → Button - TextMeshPro
   - Переименуйте в "CloseButton"

2. **Настройка CloseButton**:
   - **Rect Transform**:
     - Anchor Presets: Top-Right
     - Pos X: -40
     - Pos Y: -30
     - Width: 60
     - Height: 40
   
   - **Текст внутри кнопки**:
     - Откройте CloseButton в Hierarchy (кликните стрелку слева)
     - Выберите дочерний объект "Text (TMP)"
     - Измените Text на "X" или "Закрыть"

## Шаг 5: Создание ButtonsContainer (контейнер для кнопок)

1. **Правый клик на BuildingSelectionPanel** → UI → Empty (или просто GameObject)
   - Переименуйте в "ButtonsContainer"

2. **Настройка ButtonsContainer Rect Transform**:
   - **Anchor Presets**: Stretch-Stretch (растягивается по ширине и высоте родителя)
   - **Left**: 10 (отступ слева)
   - **Right**: 10 (отступ справа)
   - **Top**: 80 (отступ от верха, чтобы не накладывалось на Title)
   - **Bottom**: 10 (отступ снизу)

3. **Добавление Vertical Layout Group**:
   - С выбранным ButtonsContainer
   - В Inspector нажмите "Add Component"
   - Введите "Vertical Layout Group"
   - Выберите компонент

4. **Настройка Vertical Layout Group**:
   ```
   Padding:
   - Left: 10
   - Right: 10
   - Top: 10
   - Bottom: 10
   
   Spacing: 10 (расстояние между кнопками)
   
   Child Alignment: Upper Center
   
   Control Child Size:
   - ✓ Width (галочка)
   - Height (без галочки)
   
   Child Force Expand:
   - ✓ Width (галочка)
   - Height (без галочки)
   ```

5. **Добавление Content Size Fitter (опционально)**:
   - Если хотите автоматический размер контейнера
   - Add Component → Content Size Fitter
   - Vertical Fit: Preferred Size

## Шаг 6: Настройка компонента BuildingSelectionUI

1. **Выберите BuildingSelectionPanel** в Hierarchy

2. **В Inspector найдите компонент BuildingSelectionUI**

3. **Настройте ссылки** (перетаскиванием из Hierarchy):
   
   **System References:**
   - **Building Controller**: Перетащите объект с компонентом BuildingController
     - Сначала найдите в Hierarchy объект BuildingController (он должен быть создан отдельно)
   
   - **Buttons Container**: Перетащите ButtonsContainer
     - Это объект, который мы создали на шаге 5
   
   - **Building Button Prefab**: Оставьте пустым (None)
     - Кнопки создадутся автоматически, или можете создать свой префаб позже

   **UI Elements:**
   - **Panel**: Перетащите сам BuildingSelectionPanel
     - Перетащите объект сам на себя или оставьте пустым
   
   - **Close Button**: Перетащите CloseButton (если создали)
     - Это кнопка с шага 4
   
   - **Title Text**: Перетащите Title
     - Это текст с шага 3

   **Settings:**
   - **Show All Buildings**: ✓ (галочка) - показывать все типы зданий
   - **Available Buildings**: Оставьте пустым если Show All Buildings включен

## Шаг 7: Создание BuildingController объекта

1. **Правый клик в Hierarchy** → Create Empty
   - Переименуйте в "BuildingSystem"

2. **Правый клик на BuildingSystem** → Create Empty
   - Переименуйте в "BuildingPreview"

3. **Добавление компонентов**:
   
   **На BuildingSystem:**
   - Add Component → BuildingController
   
   **На BuildingPreview:**
   - Add Component → BuildingPreview

4. **Настройка BuildingController**:
   - **Main Camera**: Автоматически найдется Camera.main (можно оставить пустым)
   - **Building Preview**: Перетащите дочерний объект BuildingPreview
   - **Pointer Position Action**: Откройте InputSystem_Actions → выберите Point
   - **Click Action**: Откройте InputSystem_Actions → выберите Click  
   - **Cancel Action**: Откройте InputSystem_Actions → выберите Cancel
   - **Terrain Layer**: Выберите слой где находится ваша карта (например, Default)
   - **Raycast Distance**: 1000 (по умолчанию)

## Итоговая структура в Hierarchy

```
Canvas
├─ BuildingSelectionPanel (BuildingSelectionUI)
│  ├─ Title (TextMeshProUGUI) - "Строительство"
│  ├─ CloseButton (Button)
│  │  └─ Text (TMP) - "X"
│  └─ ButtonsContainer (VerticalLayoutGroup)
│     └─ (Кнопки создадутся автоматически при запуске)
└─ EventSystem

BuildingSystem (BuildingController)
└─ BuildingPreview (BuildingPreview)

MapSystem (должен быть в сцене)
```

## Визуальная настройка (необязательно)

### Цвет панели

1. Выберите BuildingSelectionPanel
2. В компоненте Image измените Color
   - Например: темно-серый с прозрачностью (R:0.2, G:0.2, B:0.2, A:0.9)

### Кастомизация кнопок

Если хотите свой дизайн кнопок:

1. Создайте кнопку вручную (UI → Button - TextMeshPro)
2. Настройте её внешний вид
3. Перетащите в папку Assets для создания префаба
4. Назначьте этот префаб в BuildingSelectionUI → Building Button Prefab

## Тестирование

1. **Запустите игру** (Play)
2. Кнопки должны автоматически появиться в ButtonsContainer
3. При клике на кнопку должен активироваться предпросмотр здания

## Troubleshooting

### Кнопки не появляются
- Проверьте что BuildingController назначен в BuildingSelectionUI
- Проверьте что ButtonsContainer назначен
- Проверьте консоль на наличие ошибок

### Кнопки накладываются друг на друга
- Проверьте настройки Vertical Layout Group
- Убедитесь что Width в Control Child Size включен

### Панель не видна
- Проверьте что Canvas в режиме Screen Space - Overlay
- Проверьте Canvas Scaler
- Убедитесь что Panel имеет компонент Image с цветом (не прозрачный)

### Текст не отображается
- Убедитесь что импортировали TMP Essentials
- Проверьте размер и цвет текста
- Убедитесь что текст не выходит за границы RectTransform

## Горячие клавиши Unity Editor

- **Ctrl + D** - Дублировать объект
- **Ctrl + Shift + N** - Создать пустой GameObject
- **F** - Фокус на выбранном объекте
- **Ctrl + Z** - Отменить

## Дополнительно

### Анимация появления панели

Можно добавить Animator:
1. Add Component → Animator
2. Создайте Animation для появления/скрытия
3. Управляйте через `BuildingSelectionUI.Show()/Hide()`

### Звуки кнопок

1. На кнопках можно настроить звуки
2. Используйте события Button → OnClick
3. Добавьте AudioSource и проигрывайте звук

---

Если что-то непонятно - спрашивайте! Могу создать более детальные инструкции для конкретного шага.