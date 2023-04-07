using System;
using System.Windows.Media;
using Telerik.Windows.Controls.TreeMap;

namespace GitStractor.Desktop.ViewModels;

public static class GradientColorizer
{
    private static readonly Lazy<ValueGradientColorizer> lazy = new(() => new ValueGradientColorizer
    {
        IsAbsolute = true,
        RangeMinimum = 0,
        RangeMaximum = 1,
        GradientStops = new GradientStopCollection
        {
            //new GradientStop(Color.FromRgb(240, 247, 33), 1),
            new GradientStop(Color.FromRgb(252, 189, 44), 1),
            new GradientStop(Color.FromRgb(203, 72, 120), 0.5),
            new GradientStop(Color.FromRgb(14, 8, 136), 0)
        }
    });
    public static ValueGradientColorizer Colorizer => lazy.Value;
}
