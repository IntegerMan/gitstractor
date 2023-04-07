using System.Windows.Media;

namespace GitStractor.Desktop.ViewModels
{
    public interface ITreeMapNode
    {
        Brush BackgroundBrush { get; }
        double ColorValue { get;  }
        string Label { get; }
        string ToolTip { get;  }
        double Value { get; }
    }
}