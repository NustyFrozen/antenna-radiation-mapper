using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using PolarizationMapper.ViewModels;

namespace PolarizationMapper.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        
        
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
            var file = files.GetFiles().First().Path.AbsolutePath;
            (DataContext as MainWindowViewModel).HandleFileDrop(file);
            
        }
    }
}