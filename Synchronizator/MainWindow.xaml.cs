using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
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
        string selected_parameter = "";
        const string CONFIG_PATH = "C:/Users/gnomuk/Desktop/json/parameters.json";

        Dictionary<string, int> parameters = new Dictionary<string, int>()
        {
            {"Ходьба", 0},
            {"Прыжок", 1},
            {"Приседание", 1},
            {"ЛКМ", 0},
            {"ПКМ", 0},
            {"Переключение оружия", 1},
            {"Выбросить оружие", 1 }
        };

        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = new ViewModel();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var loadedViewModel = new ViewModelConfiguration();
            var viewModel = (ViewModel)this.DataContext;

            loadedViewModel.LoadFromJson(CONFIG_PATH);

            // Проверяем загруженные данные
            foreach (var parameter in loadedViewModel.Parameters)
            {
                var parameterName = parameter.Key;
                var enabled = parameter.Value.Enabled;
                var keybindsLoaded = parameter.Value.Keybinds;

                viewModel.AddItem(parameterName, Convert.ToBoolean(parameters[parameterName]), enabled);
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
            EraseInputs();

            var button = sender as Button;
            var item = button?.DataContext;
            selected_parameter = (item as LVItemSource_Class)?.Parameter;

            parameter_name.Content = selected_parameter;
            configurationMenu_grid.Visibility = Visibility.Visible;
            
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

        private void closeConfigurationMenu_button(object sender, RoutedEventArgs e)
        {
            configurationMenu_grid.Visibility = Visibility.Hidden;
            EraseInputs();
        }

        private void EraseInputs()
        {
            enter_keybind.Text = "";
        }

        private void ApplyChanges_button(object sender, RoutedEventArgs e)
        {
            var viewModelConfig = new ViewModelConfiguration();
            var loadedModelConfig = new ViewModelConfiguration();
            var viewModel = (ViewModel)this.DataContext;
            loadedModelConfig.LoadFromJson(CONFIG_PATH);

            foreach (var parameter in loadedModelConfig.Parameters)
            {
                var keybinds = new Dictionary<string, string>();

                foreach (var keybind in parameter.Value.Keybinds)
                {
                    keybinds.Add(keybind.Key, keybind.Value);
                }
                viewModelConfig.AddParameter(parameter.Key, viewModel.Items.Where(item => item.Parameter == parameter.Key).Select(item => item.IsChecked).ToList().FirstOrDefault(), keybinds);
            }
            viewModelConfig.SaveToJson(CONFIG_PATH);

            //foreach (var selectedItem in selectedItems)
            //{
            //    Debug.WriteLine($"Выбранный элемент: {selectedItem}");
            //}

            //foreach (var parameter in viewModelConfig.Parameters)
            //{
            //    var parameterName = parameter.Key;
            //    var checkboxState = parameter.Value.checkboxState;
            //    var keybinds1 = parameter.Value.keybinds;

            //    Console.WriteLine($"Parameter: {parameterName}, Checkbox State: {checkboxState}");

            //    foreach (var keybind in keybinds1)
            //    {
            //        Console.WriteLine($"  Keybind: {keybind.Key} => {keybind.Value}");
            //    }
            //}
        }

        private void SaveParameterConfiguration_Button(object sender, RoutedEventArgs e)
        {
            var viewModelConfig = new ViewModelConfiguration();
            viewModelConfig.LoadFromJson(CONFIG_PATH);
            var keybinds = new Dictionary<string, string>();

            keybinds.Add("BIND", enter_keybind.Text);

            viewModelConfig.AddParameter(selected_parameter, true, keybinds);
            viewModelConfig.SaveToJson(CONFIG_PATH);
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
