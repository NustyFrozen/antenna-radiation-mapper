using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;

namespace PolarizationMapper.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    [ObservableProperty]
    string _filePath = "Please Drop an image(format .jpg/.png/.bitmap)";
    [ObservableProperty]
    private Bitmap? _image;
    public void SetBitmap(Bitmap img) => Image = img;
    public void HandleFileDrop(string file)
    {
        bool isSupportedFileFormat =  file.EndsWith(".jpeg") |  file.EndsWith(".png") |  file.EndsWith(".bmp");
        if (!isSupportedFileFormat)
        {
            _filePath = "File is not Supported, please try a different format";
            return;
        }
        
    }
}