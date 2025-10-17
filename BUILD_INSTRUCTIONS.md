# 🎮 Инструкция по сборке WebGL

## Способ 1: Через Unity Editor (рекомендуется)

### Шаги для сборки:

1. **Откройте Build Settings**
   - Меню: `File → Build Settings` (или `Ctrl+Shift+B`)

2. **Выберите WebGL платформу**
   - В списке Platform выберите `WebGL`
   - Если платформа не активна, нажмите кнопку `Switch Platform` (может занять несколько минут)

3. **Проверьте сцены**
   - Убедитесь, что `Assets/Scenes/SampleScene.unity` добавлена в Scenes In Build
   - Если нет - нажмите `Add Open Scenes`

4. **Настройте Player Settings (опционально)**
   - Нажмите `Player Settings...`
   - В разделе `Resolution and Presentation`:
     - WebGL Template: `Default`
   - В разделе `Publishing Settings`:
     - Compression Format: `Brotli` (для лучшего сжатия)
     - Memory Size: `256 MB`

5. **Запустите сборку**
   - Нажмите кнопку `Build`
   - Выберите папку `Build/WebGL` (создайте если нет)
   - Дождитесь завершения сборки (5-15 минут в зависимости от компьютера)

6. **Запустите локальный сервер**
   - После сборки перейдите в папку `Build\WebGL`
   - Запустите `start-server.bat` из корня проекта находясь в папке WebGL
   - Или скопируйте оба файла (`start-server.bat` и `webgl-server.py`) в папку `Build\WebGL`
   - Или используйте Unity: `File → Build And Run` (запустит сразу в браузере)

---

## 📊 Размер сборки

После сборки ожидайте размер:
- **Development Build**: ~20-30 MB (несжатый)
- **Release Build с Brotli**: ~5-10 MB (сжатый)

---

## 🎯 Следующие шаги

После успешной сборки и запуска:
1. Протестируйте базовую функциональность
2. Проверьте производительность в браузере
3. Оптимизируйте при необходимости