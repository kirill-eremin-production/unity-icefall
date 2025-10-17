# Структура проекта Icefall

## Основные директории

### [`Assets/`](Assets/)
Главная папка с ресурсами проекта.

#### Сцены
- [`Assets/Scenes/SampleScene.unity`](Assets/Scenes/SampleScene.unity) - демонстрационная сцена

#### Настройки
- [`Assets/Settings/`](Assets/Settings/) - конфигурация URP
  - Профили рендеринга для ПК и мобильных устройств
  - Настройки пост-обработки
  - Глобальные настройки URP

#### Скрипты
- [`Assets/TutorialInfo/Scripts/Readme.cs`](Assets/TutorialInfo/Scripts/Readme.cs) - ScriptableObject для readme
- [`Assets/TutorialInfo/Scripts/Editor/ReadmeEditor.cs`](Assets/TutorialInfo/Scripts/Editor/ReadmeEditor.cs) - редактор для readme

#### Файлы ввода
- [`Assets/InputSystem_Actions.inputactions`](Assets/InputSystem_Actions.inputactions) - конфигурация Input System

#### Readme
- [`Assets/Readme.asset`](Assets/Readme.asset) - информация о URP шаблоне

### [`ProjectSettings/`](ProjectSettings/)
Системные настройки проекта Unity.

Ключевые файлы:
- [`ProjectSettings/ProjectSettings.asset`](ProjectSettings/ProjectSettings.asset) - основные настройки проекта
- [`ProjectSettings/ProjectVersion.txt`](ProjectSettings/ProjectVersion.txt) - версия Unity
- [`ProjectSettings/InputManager.asset`](ProjectSettings/InputManager.asset) - настройки старой системы ввода
- [`ProjectSettings/QualitySettings.asset`](ProjectSettings/QualitySettings.asset) - настройки качества
- [`ProjectSettings/TagManager.asset`](ProjectSettings/TagManager.asset) - теги и слои
- [`ProjectSettings/URPProjectSettings.asset`](ProjectSettings/URPProjectSettings.asset) - настройки URP

### [`Packages/`](Packages/)
Управление пакетами проекта.

- [`Packages/manifest.json`](Packages/manifest.json) - список зависимостей пакетов
- [`Packages/packages-lock.json`](Packages/packages-lock.json) - закрепленные версии пакетов

### [`Library/`](Library/)
Кэш и временные файлы Unity (генерируется автоматически, не включается в систему контроля версий).

### [`Temp/`](Temp/)
Временные файлы компиляции и сборки (генерируется автоматически).

### [`Logs/`](Logs/)
Логи компиляции и импорта ресурсов.

### Solution файлы
- [`icefall.sln`](icefall.sln) - Visual Studio solution
- [`Assembly-CSharp.csproj`](Assembly-CSharp.csproj) - основная сборка C#
- [`Assembly-CSharp-Editor.csproj`](Assembly-CSharp-Editor.csproj) - сборка для редактора

## Структура кода

На данный момент в проекте минимальное количество кода:
- **Классы**: `Readme`, `ReadmeEditor`
- **Пространства имен**: Используются стандартные Unity пространства имен
- **Сборки**: Assembly-CSharp (runtime), Assembly-CSharp-Editor (editor)

## Gitignore рекомендации

При работе с системой контроля версий рекомендуется исключить:
- `/Library/`
- `/Temp/`
- `/Logs/`
- `/UserSettings/`
- `*.csproj`
- `*.sln`
- `*.suo`
- `*.user`