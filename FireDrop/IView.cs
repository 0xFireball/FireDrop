using System.Windows;

namespace FireDrop
{
    public interface IView
    {
        string Name { get; }
        Window WindowHandle { get; }
    }
}