using System.Windows;
using MahApps.Metro.Controls;

namespace FireDrop
{
    public interface IView
    {
        string Name { get; }
        Window WindowHandle { get; }
    }
}