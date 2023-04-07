using MediaFoundation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Media;
using Telerik.Windows.Controls;
using Telerik.Windows.Controls.TreeMap;

namespace GitStractor.Desktop.ViewModels;

public class TreeMapNode
{
    public readonly static ValueGradientColorizer Colorizer;

    static TreeMapNode()
    {
        Colorizer = new();
        Colorizer.IsAbsolute = true;
        Colorizer.RangeMinimum = 0;
        Colorizer.RangeMaximum = 1;
        Colorizer.GradientStops.Add(new GradientStop(Color.FromRgb(240, 247, 33), 1));
        Colorizer.GradientStops.Add(new GradientStop(Color.FromRgb(203, 72, 120), 0.5));
        Colorizer.GradientStops.Add(new GradientStop(Color.FromRgb(14, 8, 136), 0));
    }

    public string Label { get; set; }
    
    public double Value { get; set; }
    public double ColorValue { get; set; }

    public string ToolTip { get; set; }

    public Brush BackgroundBrush
    {
        get
        {
            double value = Math.Max(0, Math.Min(1, ColorValue));

            return Colorizer.SelectBrush(value);
        }
    }

    public ObservableCollection<TreeMapNode> Children { get; set; } = new();
}