# programma-fotki-server

Набор для простой схемы: на iOS-приложении (MAUI, C#) делаем/выбираем фото и отправляем его по HTTP на ваш Mac. На Mac — минимальный Python-сервер, который принимает `POST /upload` и кладёт файл в выбранную папку.

## Состав
- `client/` — файлы экрана для .NET MAUI (iOS). Подставляются в созданный шаблон.
- `server/receiver.py` — приёмник на Python без внешних библиотек.

## Быстрый старт: сервер на Mac
```bash
cd server
python3 receiver.py --port 5000 --output ~/Desktop/received_photos
```
Папка создастся автоматически. Проверка без приложения:
```bash
curl -X POST -F "file=@/path/to/pic.jpg" http://<ip-mac>:5000/upload
```

## Быстрый старт: клиент iOS (MAUI)
1) Установите MAUI workload (понадобится интернет, Xcode):  
   `dotnet workload install maui`
2) Создайте шаблон:  
   ```bash
   dotnet new maui -n PhotoUploader
   cd PhotoUploader
   ```
3) Замените `MainPage.xaml` и `MainPage.xaml.cs` на файлы из папки `client` этого репо.
4) В `Platforms/iOS/Info.plist` добавьте ключи:
   ```xml
   <key>NSCameraUsageDescription</key>
   <string>Нужно, чтобы делать снимки и отправлять их на компьютер.</string>
   <key>NSPhotoLibraryUsageDescription</key>
   <string>Нужно, чтобы выбрать фото и отправить его на компьютер.</string>
   <key>NSPhotoLibraryAddUsageDescription</key>
   <string>Нужно, чтобы сохранять снимки в медиатеку.</string>
   ```
5) Запуск на устройстве:  
   `dotnet build -t:Run -f net8.0-ios`

## Использование приложения
- Введите адрес сервера, например `http://<ip-mac>:5000/upload`.
- Нажмите «Сделать фото» или «Выбрать из галереи» — появится предпросмотр.
- Нажмите «Отправить фото» — файл уйдёт на сервер, ответ покажется текстом.

## Замечания
- Телефон и Mac должны быть в одной сети, порт 5000 не должен блокироваться.
- Разрешения на камеру/фотографии запросит система при первом использовании.
