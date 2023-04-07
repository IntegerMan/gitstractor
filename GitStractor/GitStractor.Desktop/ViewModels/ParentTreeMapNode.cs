using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Media;

namespace GitStractor.Desktop.ViewModels;

public class ParentTreeMapNode : ITreeMapNode
{
    public string Label { get; set; }

    public double Value => Children.Sum(c => c.Value);
    public double ColorValue => Children.Average(c => c.Value);

    public string ToolTip { get; set; }

    public Brush BackgroundBrush
    {
        get
        {
            return new SolidColorBrush(Color.FromRgb(84,84,84));
            /*
            double value = Math.Max(0, Math.Min(1, ColorValue));

            return GradientColorizer.Colorizer.SelectBrush(value);
            */
        }
    }

    public ObservableCollection<ITreeMapNode> Children { get; set; } = new();
    public string Key { get; set; }
}