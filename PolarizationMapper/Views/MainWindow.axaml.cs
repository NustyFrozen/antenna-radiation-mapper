using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using PolarizationMapper.Models;
using PolarizationMapper.ViewModels;
using ScottPlot;
using ScottPlot.Avalonia;
using Image = ScottPlot.Image;

namespace PolarizationMapper.Views;

public partial class MainWindow : Window
{
    private ScottPlot.Image img;
    private EditMode mode;
    string _filePath = string.Empty;
    private Vector2 center,scalePixels;
    private float start,distance;
    public MainWindow()
    {
        InitializeComponent();
        plotter.Plot.DataBackground.Color = Color.FromHex("#070707");
        
        plotter.Plot.FigureBackground.Color = Color.FromHex("#181818");
        plotter.Plot.DataBackground.Color = Color.FromHex("#1f1f1f");
        plotter.Plot.Axes.Color(Color.FromHex("#d7d7d7"));
        plotter.Plot.Grid.MajorLineColor = Color.FromHex("#404040");
        plotter.Plot.Legend.IsVisible = false;
    }
    List<>
    private void AvaPlot1_Tapped(object? sender, PointerEventArgs e)
    {
        if (sender is not AvaPlot avaPlot)
            return;

        // 1) get pixel position of the tap
        var pt = e.GetPosition(avaPlot);  

        // 2) wrap in a ScottPlot.Pixel
        var mousePixel = new Pixel((float)pt.X, (float)pt.Y);
       
        // 3) convert to data coordinates
        Coordinates dataCoords = avaPlot.Plot.GetCoordinates(mousePixel);

        double x = dataCoords.X;
        double y = dataCoords.Y;
        if (e.KeyModifiers != KeyModifiers.Alt)
        return;
        switch (expression)
        {
            
        }
        switch (mode)
        {
            case EditMode.PICK_OFFSET:
            {
                break;
            }
        }
    }
    public void DragOverHandler(object? sender, DragEventArgs e)
    {
        
        e.DragEffects = DragDropEffects.Copy;
        e.Handled = true;
    }

    public  void DropHandler(object? sender, DragEventArgs e)
    {
        
        if (e.Data is {} files)
        {
             _filePath = files.GetFiles().First().Path.AbsolutePath;
            bool isSupportedFileFormat =  _filePath.EndsWith(".jpeg") |  _filePath.EndsWith(".png") |  _filePath.EndsWith(".bmp");
            if (!isSupportedFileFormat)
            {
           
                _filePath = "File is not Supported, please try a different format";
                return;
            }
            img = new Image(_filePath);
            (DataContext as MainWindowViewModel).HandleFileDrop(_filePath);
            updateImage();
        }
    }

    public void updateImage()
    {
        plotter.Plot.Add.ImageRect(img, new CoordinateRect(left: -img.Width/2, right: img.Width/2, bottom: -img.Height/2, top: img.Height/2));
    }

    private void SetCenter(object? sender, RoutedEventArgs e) => mode = EditMode.PICK_OFFSET;

    private void SetScaleX(object? sender, RoutedEventArgs e) => mode = EditMode.PICK_SCALEX;

    private void SetScaleY(object? sender, RoutedEventArgs e) => mode = EditMode.PICK_SCALEY;

    private void BeginMapping(object? sender, RoutedEventArgs e) => mode = EditMode.MAP;
}