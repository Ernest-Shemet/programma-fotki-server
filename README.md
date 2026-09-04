# Photo Uploader — .NET MAUI + Python

A small client-server project for transferring photos from an iOS device to a computer over HTTP.

The mobile client is built with **.NET MAUI / C#**. It can take a photo or select one from the gallery and upload it to a lightweight **Python HTTP server** running on a Mac or another computer in the same network.

## Tech stack

- C# / .NET MAUI
- Python 3
- HTTP multipart upload
- iOS camera and photo-library APIs

## Project structure

```text
client/             .NET MAUI UI and client-side upload logic
server/receiver.py  lightweight Python upload server
```

## Run the server

```bash
cd server
python3 receiver.py --port 5000 --output ~/Desktop/received_photos
```

Test it without the mobile app:

```bash
curl -X POST -F "file=@/path/to/photo.jpg" http://<computer-ip>:5000/upload
```

## Run the iOS client

Install the MAUI workload and create a project:

```bash
dotnet workload install maui
dotnet new maui -n PhotoUploader
cd PhotoUploader
```

Use the files from `client/` in the generated project and configure the required iOS camera/photo-library permissions in `Info.plist`.

Run on an iOS device:

```bash
dotnet build -t:Run -f net8.0-ios
```

## How it works

1. Enter the upload endpoint, for example `http://<computer-ip>:5000/upload`.
2. Take a photo or choose one from the gallery.
3. Preview the selected image.
4. Upload it to the server.
5. The Python server stores the received file in the configured directory.

## Notes

The phone and computer should be reachable over the same network, and the selected server port must be accessible through the local firewall.

---

Student project by Ernest Shemet.
