using System.Windows;
using FireDrop.ViewModels;

namespace FireDrop
{
    /// <summary>
    ///     Interaction logic for ReceiveFileWindow.xaml
    /// </summary>
    public partial class ReceiveFileWindow : IView
    {
        public ReceiveFileWindow()
        {
            InitializeComponent();
            var vm = DataContext as ReceiveFileViewModel;
            vm.View = this;
        }

        public Window WindowHandle => this;
    }
}