using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;
using System.Net.Http.Headers;

namespace PhotoUploader;

public partial class MainPage : ContentPage
{
    private FileResult? _selectedPhoto;
    private string _serverUrl = "http://<ip-mac>:5000/upload";
    private readonly HttpClient _httpClient = new();

    public MainPage()
    {
        InitializeComponent();
        ServerEntry.Text = _serverUrl;
    }

    private async void OnCapturePhoto(object sender, EventArgs e)
    {
        await EnsurePermissions();
        try
        {
            var photo = await MediaPicker.CapturePhotoAsync(new MediaPickerOptions
            {
                Title = $"photo_{DateTime.Now:yyyyMMdd_HHmmss}.jpg"
            });
            await SetSelectedPhoto(photo);
            StatusLabel.Text = "Снимок сделан и сохранён в галерее.";
            ErrorLabel.Text = string.Empty;
        }
        catch (Exception ex)
        {
            ErrorLabel.Text = $"Ошибка камеры: {ex.Message}";
        }
    }

    private async void OnPickPhoto(object sender, EventArgs e)
    {
        await EnsurePermissions();
        try
        {
            var photo = await MediaPicker.PickPhotoAsync(new MediaPickerOptions
            {
                Title = "Выберите фото"
            });
            await SetSelectedPhoto(photo);
            StatusLabel.Text = "Фото выбрано.";
            ErrorLabel.Text = string.Empty;
        }
        catch (Exception ex)
        {
            ErrorLabel.Text = $"Не удалось выбрать фото: {ex.Message}";
        }
    }

    private async void OnSendPhoto(object sender, EventArgs e)
    {
        if (_selectedPhoto == null)
        {
            ErrorLabel.Text = "Сначала выберите фото.";
            return;
        }

        _serverUrl = ServerEntry.Text?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(_serverUrl))
        {
            ErrorLabel.Text = "Укажите адрес сервера.";
            return;
        }

        try
        {
            using var stream = await _selectedPhoto.OpenReadAsync();
            using var content = new MultipartFormDataContent();
            var streamContent = new StreamContent(stream);
            streamContent.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");
            content.Add(streamContent, "file", Path.GetFileName(_selectedPhoto.FullPath));

            var response = await _httpClient.PostAsync(_serverUrl, content);
            var body = await response.Content.ReadAsStringAsync();
            response.EnsureSuccessStatusCode();

            StatusLabel.Text = $"Отправлено: {body}";
            ErrorLabel.Text = string.Empty;
        }
        catch (Exception ex)
        {
            ErrorLabel.Text = $"Ошибка отправки: {ex.Message}";
        }
    }

    private async Task SetSelectedPhoto(FileResult? photo)
    {
        if (photo == null)
        {
            _selectedPhoto = null;
            SendButton.IsEnabled = false;
            Preview.Source = null;
            FileInfoLabel.Text = string.Empty;
            return;
        }

        _selectedPhoto = photo;
        SendButton.IsEnabled = true;

        try
        {
            // Use the file path for preview to avoid duplicating streams.
            if (!string.IsNullOrEmpty(photo.FullPath))
            {
                Preview.Source = ImageSource.FromFile(photo.FullPath);
            }
            else
            {
                // On some platforms FullPath may be null; fall back to stream.
                var stream = await photo.OpenReadAsync();
                Preview.Source = ImageSource.FromStream(() => stream);
            }

            var info = new FileInfo(photo.FullPath ?? photo.FileName);
            FileInfoLabel.Text = $"{photo.FileName} ({(info.Exists ? info.Length / 1024 : 0)} KB)";
        }
        catch (Exception ex)
        {
            ErrorLabel.Text = $"Предпросмотр не доступен: {ex.Message}";
        }
    }

    private static async Task EnsurePermissions()
    {
        // Request camera and photo library permissions if needed.
        var cameraStatus = await Permissions.CheckStatusAsync<Permissions.Camera>();
        if (cameraStatus != PermissionStatus.Granted)
        {
            cameraStatus = await Permissions.RequestAsync<Permissions.Camera>();
        }

        var photosStatus = await Permissions.CheckStatusAsync<Permissions.Photos>();
        if (photosStatus != PermissionStatus.Granted)
        {
            photosStatus = await Permissions.RequestAsync<Permissions.Photos>();
        }
    }
}
