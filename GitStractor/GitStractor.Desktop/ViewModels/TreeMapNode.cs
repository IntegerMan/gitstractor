using MediaFoundation;
using System;
using System.Collections.Generic;
using System.Windows.Media;
using Telerik.Windows.Controls;

namespace GitStractor.Desktop.ViewModels;


public class TreeMapNode : ITreeMapNode
{
    public string Label { get; set; }

    public double Value { get; set; }
    public double ColorValue { get; set; }

    public string ToolTip { get; set; }

    public Brush BackgroundBrush
    {
        get
        {
            double value = Math.Max(0, Math.Min(1, ColorValue));

            return GradientColorizer.Colorizer.SelectBrush(value);
        }
    }
}
