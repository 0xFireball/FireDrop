using System.Windows;

namespace FireDrop
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : IView
    {
        public MainWindow()
        {
            InitializeComponent();
            var vm = DataContext as ApplicationViewModel;
            vm.View = this;
        }

        public Window WindowHandle => this;
    }
}