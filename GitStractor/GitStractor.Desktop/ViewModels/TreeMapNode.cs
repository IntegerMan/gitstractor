using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace GitStractor.Desktop.ViewModels;

public class TreeMapNode
{
    public string Label { get; set; }
    
    public double Value { get; set; }

    public ObservableCollection<TreeMapNode> Children { get; set; } = new();
}