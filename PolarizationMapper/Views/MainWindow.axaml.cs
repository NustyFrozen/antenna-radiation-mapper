using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using PolarizationMapper.Models;
using ScottPlot;
using ScottPlot.Avalonia;
using ScottPlot.Plottables;
using Image = ScottPlot.Image;

namespace PolarizationMapper.Views;

public partial class MainWindow : Window
{
    private Image? _img;
    private CoordinateRect _imgPos = new();
    private EditMode _modeValue = EditMode.NONE;
    string _filePath = string.Empty;
    private Vector2 _scalePixels,_offset;
    private Tuple<double, double>? _scaleRange;
    private Vector2 _mousePosOnChart = new Vector2();
    private polar[] _points = new polar[360];
    private EditMode Mode
    {
        get
        {
            return _modeValue;
        }
        set
        {
            plotter.Focus();
            _modeValue = value;
        }
    }
    
    public MainWindow()
    {
        InitializeComponent();
        plotter.Plot.DataBackground.Color = Color.FromHex("#070707");
        
        plotter.Plot.FigureBackground.Color = Color.FromHex("#181818");
        plotter.Plot.DataBackground.Color = Color.FromHex("#1f1f1f");
        plotter.Plot.Axes.Color(Color.FromHex("#d7d7d7"));
        plotter.Plot.Grid.MajorLineColor = Color.FromHex("#404040");
        plotter.Plot.Legend.IsVisible = false;
        DropZoneText.Text = "Please Drop an image(format .jpg/.png/.bitmap)";
    }
    List<IPlottable> cachedPlots = new List<IPlottable>();
    private void clearCache()
    {
        cachedPlots.ForEach(x => plotter.Plot.Remove(x));
        cachedPlots.Clear();
    }
    private void AvaPlot1_MouseMoved(object? sender, PointerEventArgs e)
    {
        if (sender is not AvaPlot avaPlot)
            return;
        clearCache();
        var pt = e.GetPosition(avaPlot);  
        var mousePixel = new Pixel((float)pt.X, (float)pt.Y);
        Coordinates dataCoords = avaPlot.Plot.GetCoordinates(mousePixel);
        _mousePosOnChart = new Vector2((float) dataCoords.X, (float)dataCoords.Y);
        switch (Mode)
        {
            case EditMode.PICK_OFFSET:
                cachedPlots.Add(plotter.Plot.Add.Line(new CoordinateLine(plotter.Plot.Axes.Bottom.Min, _mousePosOnChart.Y,
                    plotter.Plot.Axes.Bottom.Max,
                    _mousePosOnChart.Y)));
                cachedPlots.Add(plotter.Plot.Add.Line(new CoordinateLine(_mousePosOnChart.X, plotter.Plot.Axes.Left.Min,
                    _mousePosOnChart.X, plotter.Plot.Axes.Left.Max)));
                break;
            case EditMode.PICK_SCALEX:
            {
                cachedPlots.Add(plotter.Plot.Add.Line(new CoordinateLine(0,
                    0,
                    _mousePosOnChart.X,0)));
                cachedPlots.Add(plotter.Plot.Add.Text($"Scale X: {Math.Abs(_mousePosOnChart.X)}",new Coordinates(_mousePosOnChart.X, 0)));
                break;
            }
            case EditMode.PICK_SCALEY:
            {
                cachedPlots.Add(plotter.Plot.Add.Line(new CoordinateLine(0,
                    0,
                    0, _mousePosOnChart.Y)));
                cachedPlots.Add(plotter.Plot.Add.Text($"Scale Y: {Math.Abs(_mousePosOnChart.Y)}",new Coordinates(0, _mousePosOnChart.Y)));
                break;
            }
            case EditMode.MAP:
            {
                RenderPolarMap();
                cachedPlots.Add(plotter.Plot.Add.Line(new CoordinateLine(_mousePosOnChart.X,
                    _mousePosOnChart.Y,
                    0, 0)));
                
                break;
            }
        }
        plotter.Refresh();
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
             _filePath = files.GetFiles()!.First().Path.AbsolutePath;
            bool isSupportedFileFormat =  _filePath.EndsWith(".jpeg") |  _filePath.EndsWith(".png") |  _filePath.EndsWith(".bmp");
            if (!isSupportedFileFormat)
            {
           
                _filePath = "File is not Supported, please try a different format";
                return;
            }
            _img = new Image(_filePath);
            DropZone.IsVisible = false;
            UpdateImage(new CoordinateRect(left: -_img.Width/2, right: _img.Width/2, bottom: -_img.Height/2, top: _img.Height/2));
        }
    }

    public void UpdateImage(CoordinateRect o)
    {
        _imgPos = o;
        plotter.Plot.Remove(typeof(IPlottable)); //remove all drawings
        plotter.Plot.Remove(typeof(ImageRect)); //remove image
        plotter.Plot.Add.ImageRect(_img, o);
        plotter.Plot.Add.Text("Center of the Image", new Coordinates());
        plotter.Plot.Add.Circle(new Coordinates(), 1);
    }

    private void SetCenter(object? sender, RoutedEventArgs e) => Mode = EditMode.PICK_OFFSET;

    private void SetScaleX(object? sender, RoutedEventArgs e) => Mode = EditMode.PICK_SCALEX;

    private void SetScaleY(object? sender, RoutedEventArgs e) => Mode = EditMode.PICK_SCALEY;

    private void BeginMapping(object? sender, RoutedEventArgs e)
    {
        if (Mode == EditMode.MAP)
        {
            Mode = EditMode.NONE;
            DropZone.IsVisible = true;
            DropZoneText.Text =
                $"File Has been Saved To\n {CoordinatesToCsv.CreateCsvfile(_filePath, _points.ToList(), _scaleRange, _scalePixels.X)}";
        }
        else
        {
            for (int i = 0; i < 360; i++)
            {
                _points[i] = new polar();
            }
            (sender as Button).Content = "Finisih Mapping";
            Mode = EditMode.MAP;
        }
        
    }

    private void RenderPolarMap()
    {
        
        for (int i = 1; i < 360; i++)
        {
            var point1 = _points[i].toCartisian();
            var point2 = _points[i - 1].toCartisian();
            cachedPlots.Add(plotter.Plot.Add.Line(new CoordinateLine(point1.X,point1.Y,point2.X,point2.Y)));
        }
    }
    private void Plotter_OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key != Key.LeftAlt)
         return;
        var newmode = EditMode.NONE;
        switch (Mode)
        {
            case EditMode.PICK_OFFSET:
            {
                var position = _imgPos;
                position.Left -= _mousePosOnChart.X;
                position.Right -= _mousePosOnChart.X;
                position.Top -=_mousePosOnChart.Y;
                position.Bottom -= _mousePosOnChart.Y;
                _offset = new Vector2(_mousePosOnChart.X,_mousePosOnChart.Y);
                UpdateImage(position);
                break;
            }
            case EditMode.PICK_SCALEX:
            {
                _scalePixels.X = Math.Abs(_mousePosOnChart.X);
                newmode = EditMode.PICK_SCALEY;
                break;
            }
            case EditMode.PICK_SCALEY:
            {
                _scalePixels.Y = Math.Abs(_mousePosOnChart.Y);
                plotter.Plot.Remove(typeof(Ellipse));
                var ratio = _scalePixels.X/_scalePixels.Y;
                _img = _img.Resized(_img.Width,(int)Math.Round(_img.Height * ratio));
                _imgPos = new CoordinateRect(left: -_img.Width / 2, right: _img.Width / 2, bottom: -_img.Height / 2,
                    top: _img.Height / 2);
                var position = _imgPos;
                position.Left -= _offset.X;
                position.Right -= _offset.X;
                position.Top -=_offset.Y;
                position.Bottom -= _offset.Y;
                UpdateImage(position);
                plotter.Plot.Add.Circle(new
                     Coordinates(),_scalePixels.X);
                break;
            }
            case EditMode.MAP:
            {
                polar coordinates = new polar(-_mousePosOnChart);
                var angle = (Convert.ToInt32(Math.Round(coordinates.angle)))%360;
                _points[angle % 360] = coordinates;
                newmode = EditMode.MAP;
                break;
            }
                default:
                newmode = Mode;
                    break;
        }
        Mode = newmode;
        plotter.Refresh();
    }

    private void ApplyScaleValue(object? sender, TextChangedEventArgs e)
    {
        if (double.TryParse(StartTextBox.Text, out var start) && double.TryParse(RangeTextBox.Text, out var end))
            _scaleRange = new Tuple<double,double>(start, end);
    }
}