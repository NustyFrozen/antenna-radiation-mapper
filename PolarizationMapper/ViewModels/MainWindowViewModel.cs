using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using PolarizationMapper.Models;
using ScottPlot;

namespace PolarizationMapper.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    [ObservableProperty]
    string _filePath = "Please Drop an image(format .jpg/.png/.bitmap)";
    
    [ObservableProperty]
    EditMode _currentMode = EditMode.NONE;

    [ObservableProperty] 
    bool _displayFileDrop = true;
    public MainWindowViewModel()
    {
        
    }
    public void HandleFileDrop(string file)
    {
        CurrentMode = EditMode.PICK_OFFSET;
        DisplayFileDrop = false;
    }
}