using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Synchronizator
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //string[] parameters = new[] { "Ходьба", "Прыжок", "Приседание", "ЛКМ", "ПКМ", "Переключение оружия"};

        Dictionary<string, int> parameters = new Dictionary<string, int>()
        {
            {"Ходьба", 0},
            {"Прыжок", 1},
            {"Приседание", 1},
            {"ЛКМ", 0},
            {"ПКМ", 0},
            {"Переключение оружия", 1}
        };

        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = new ViewModel();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var viewModel = (ViewModel)this.DataContext;
            foreach (var parameter in parameters)
            {
                viewModel.AddItem(parameter.Key, Convert.ToBoolean(parameter.Value));
            }
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void MinimizeWindow_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void CloseWindow_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
        
        private void GearButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var item = button?.DataContext;

            MessageBox.Show((item as LVItemSource_Class)?.Parameter);

            //if (item != null)
            //{
            //    // Получаем ItemsSource из ListView
            //    var listView = FindParent<ListView>(button);
            //    var itemsSource = listView.ItemsSource as IList;

            //    // Находим индекс элемента
            //    int index = itemsSource.IndexOf(item);

            //    // Здесь вы можете выполнять нужные действия с индексом
            //    MessageBox.Show($"Кнопка нажата для элемента с индексом: {index}");
            //}
        }

        private void TextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            enter_keybind.Text = "";
            enter_keybind.Text += e.Key.ToString();
            e.Handled = true;
        }

        //private T FindParent<T>(DependencyObject child) where T : DependencyObject
        //{
        //    DependencyObject parentObject = VisualTreeHelper.GetParent(child);

        //    if (parentObject == null) return null;

        //    T parent = parentObject as T;
        //    return parent ?? FindParent<T>(parentObject);
        //}
    }
}
