using System.Numerics;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;

namespace PolarizationMapper.ViewModels;

public partial class ChartDisplayViewModel : ViewModelBase
{
    [ObservableProperty]
    private Bitmap _image;
    
    private Vector2 center;
    private int scaleSize;
    private int start;
    private int scalePX;
}