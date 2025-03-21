using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using WindowsInput;
using Gma.System.MouseKeyHook;
using System.IO;


namespace Synchronizator
{
    public partial class MainWindow : Window
    {
        private TcpListener listener;
        private InputSimulator inputSimulator;
        private IKeyboardMouseEvents _globalHook;

        string selected_parameter = "";
        const string CONFIG_PATH = "parameters.json";

        Dictionary<string, int> parameters = new Dictionary<string, int>()
        {
            {"Ходьба", 0},
            {"Прыжок", 1},
            {"Приседание", 1},
            {"Шифт", 0},
            {"ЛКМ", 0},
            {"ПКМ", 0},
            {"Переключение оружия", 1},
            {"Выбросить оружие", 1 },
            {"Кнопка действия", 1 }
        };

        public MainWindow()
        {
            InitializeComponent();
            StartServer();
            SubscribeToGlobalHook();
            inputSimulator = new InputSimulator();
            this.DataContext = new ViewModel();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (!File.Exists(CONFIG_PATH)) { CreateConfig(); }

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

            var loadedViewModel = new ViewModelConfiguration();
            loadedViewModel.LoadFromJson(CONFIG_PATH);
            var button = sender as Button;
            var item = button?.DataContext;
            selected_parameter = (item as LVItemSource_Class)?.Parameter;
            parameter_name.Content = selected_parameter;
            configurationMenu_grid.Visibility = Visibility.Visible;

            if (selected_parameter == "Переключение оружия")
            {
                mainConfig_Grid.Visibility = Visibility.Hidden;
                weaponSwap_Config_Grid.Visibility = Visibility.Visible;

                mainWeapon_keybind.Text = loadedViewModel.Parameters.Where(x => x.Key == "Переключение оружия").SelectMany(j => j.Value.Keybinds).Where(k => k.Key == "MainWeapon").Select(k => k.Value).FirstOrDefault();
                secondaryWeapon_keybind.Text = loadedViewModel.Parameters.Where(x => x.Key == "Переключение оружия").SelectMany(j => j.Value.Keybinds).Where(k => k.Key == "SecondaryWeapon").Select(k => k.Value).FirstOrDefault();
                knifeWeapon_keybind.Text = loadedViewModel.Parameters.Where(x => x.Key == "Переключение оружия").SelectMany(j => j.Value.Keybinds).Where(k => k.Key == "Knife").Select(k => k.Value).FirstOrDefault();
                grenadesWeapon_keybind.Text = loadedViewModel.Parameters.Where(x => x.Key == "Переключение оружия").SelectMany(j => j.Value.Keybinds).Where(k => k.Key == "Grenades").Select(k => k.Value).FirstOrDefault();
                bombWeapon_keybind.Text = loadedViewModel.Parameters.Where(x => x.Key == "Переключение оружия").SelectMany(j => j.Value.Keybinds).Where(k => k.Key == "Bomb").Select(k => k.Value).FirstOrDefault();
            }
            else
            {
                enter_keybind.Text = loadedViewModel.Parameters.Where(x => x.Key == selected_parameter).SelectMany(j => j.Value.Keybinds).Select(k => k.Value).FirstOrDefault();
                weaponSwap_Config_Grid.Visibility = Visibility.Hidden;
                mainConfig_Grid.Visibility = Visibility.Visible;
            }




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

        private void KeyInput(object sender, KeyEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            string key = e.Key.ToString();
            if (key == "Back") { textBox.Text = ""; return; }
            textBox.Text = e.Key.ToString();
            e.Handled = true;
        }

        private void closeConfigurationMenu_button(object sender, RoutedEventArgs e)
        {
            configurationMenu_grid.Visibility = Visibility.Hidden;
            EraseInputs();
        }

        private void EraseInputs()
        {
            enter_keybind.Clear();
            mainWeapon_keybind.Clear();
            secondaryWeapon_keybind.Clear();
            knifeWeapon_keybind.Clear();
            grenadesWeapon_keybind.Clear();
            bombWeapon_keybind.Clear();
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
        }

        private void SaveParameterConfiguration_Button(object sender, RoutedEventArgs e)
        {
            var viewModelConfig = new ViewModelConfiguration();
            viewModelConfig.LoadFromJson(CONFIG_PATH);
            var keybinds = new Dictionary<string, string>();

            if (selected_parameter == "Переключение оружия")
            {
                keybinds.Add("MainWeapon", mainWeapon_keybind.Text);
                keybinds.Add("SecondaryWeapon", secondaryWeapon_keybind.Text);
                keybinds.Add("Knife", knifeWeapon_keybind.Text);
                keybinds.Add("Grenades", grenadesWeapon_keybind.Text);
                keybinds.Add("Bomb", bombWeapon_keybind.Text);
            }
            else
            {
                keybinds.Add("MainKey", enter_keybind.Text);
            }

            viewModelConfig.AddParameter(selected_parameter, true, keybinds);
            viewModelConfig.SaveToJson(CONFIG_PATH);
        }

        private void CreateConfig()
        {

            const string CONFIG = "{\n" +
              "  \"Ходьба\": {\n" +
              "    \"Enabled\": false,\n" +
              "    \"Keybinds\": {\n" +
              "      \"BIND\": \"\"\n" +
              "    }\n" +
              "  },\n" +
              "  \"Прыжок\": {\n" +
              "    \"Enabled\": false,\n" +
              "    \"Keybinds\": {\n" +
              "      \"MainKey\": \"Space\"\n" +
              "    }\n" +
              "  },\n" +
              "  \"Приседание\": {\n" +
              "    \"Enabled\": false,\n" +
              "    \"Keybinds\": {\n" +
              "      \"MainKey\": \"LeftCtrl\"\n" +
              "    }\n" +
              "  },\n" +
              "  \"Шифт\": {\n" +
              "    \"Enabled\": false,\n" +
              "    \"Keybinds\": {\n" +
              "      \"BIND\": \"\"\n" +
              "    }\n" +
              "  },\n" +
              "  \"ЛКМ\": {\n" +
              "    \"Enabled\": false,\n" +
              "    \"Keybinds\": {\n" +
              "      \"BIND\": \"\"\n" +
              "    }\n" +
              "  },\n" +
              "  \"ПКМ\": {\n" +
              "    \"Enabled\": false,\n" +
              "    \"Keybinds\": {\n" +
              "      \"BIND\": \"\"\n" +
              "    }\n" +
              "  },\n" +
              "  \"Переключение оружия\": {\n" +
              "    \"Enabled\": false,\n" +
              "    \"Keybinds\": {\n" +
              "      \"MainWeapon\": \"D1\",\n" +
              "      \"SecondaryWeapon\": \"D2\",\n" +
              "      \"Knife\": \"D3\",\n" +
              "      \"Grenades\": \"D4\",\n" +
              "      \"Bomb\": \"D5\"\n" +
              "    }\n" +
              "  },\n" +
              "  \"Выбросить оружие\": {\n" +
              "    \"Enabled\": false,\n" +
              "    \"Keybinds\": {\n" +
              "      \"MainKey\": \"G\"\n" +
              "    }\n" +
              "  },\n" +
              "  \"Кнопка действия\": {\n" +
              "    \"Enabled\": false,\n" +
              "    \"Keybinds\": {\n" +
              "      \"MainKey\": \"E\"\n" +
              "    }\n" +
              "  }\n" +
            "}";
            File.WriteAllText(CONFIG_PATH, CONFIG);
        }

        private void SubscribeToGlobalHook()
        {
            _globalHook = Hook.GlobalEvents();
            _globalHook.KeyDown += GlobalHook_KeyDown;
        }

        private void UnsubscribeToGlobalHook()
        {
            _globalHook.KeyDown -= GlobalHook_KeyDown;
            _globalHook.Dispose();
        }

        private void GlobalHook_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            bool spaceIsPressed = false;
            bool ctrlIsPressed = false;
            bool shiftIsPressed = false;
            bool gIsPressed = false;
            bool eIsPressed = false;

            //bool wIsPressed = false;
            //bool sIsPressed = false;
            //bool aIsPressed = false;
            //bool dIsPressed = false;

            ////WASD Global Hook
            //if (e.KeyCode == System.Windows.Forms.Keys.W && !wIsPressed)
            //{
            //    wIsPressed = true;
            //    SendMessageToOtherComputer("WPressed");
            //    return;
            //}
            //if (e.KeyCode == System.Windows.Forms.Keys.S && !sIsPressed)
            //{
            //    sIsPressed = true;
            //    SendMessageToOtherComputer("SPressed");
            //    return;
            //}
            //if (e.KeyCode == System.Windows.Forms.Keys.A && !aIsPressed)
            //{
            //    aIsPressed = true;
            //    SendMessageToOtherComputer("APressed");
            //    return;
            //}
            //if (e.KeyCode == System.Windows.Forms.Keys.D && !dIsPressed)
            //{
            //    dIsPressed = true;
            //    SendMessageToOtherComputer("DPressed");
            //    return;
            //}

            //Прыжок
            if (e.KeyCode == System.Windows.Forms.Keys.Space && !spaceIsPressed)
            {
                spaceIsPressed = true;
                SendMessageToOtherComputer("SpacePressed");
                return;
            }

            // Приседание
            if (e.KeyCode == System.Windows.Forms.Keys.LControlKey && !ctrlIsPressed)
            {
                ctrlIsPressed = true;
                SendMessageToOtherComputer("CtrlPressed");
                return;
            }

            //Шифт
            if (e.KeyCode == System.Windows.Forms.Keys.LShiftKey && !shiftIsPressed)
            {
                shiftIsPressed = true;
                SendMessageToOtherComputer("ShiftPressed");
                return;
            }

            //Выбросить оружие
            if (e.KeyCode == System.Windows.Forms.Keys.G && !gIsPressed)
            {
                gIsPressed = true;
                SendMessageToOtherComputer("gPressed");
                return;
            }

            //Взаимодействие
            if (e.KeyCode == System.Windows.Forms.Keys.E && !eIsPressed)
            {
                eIsPressed = true;
                SendMessageToOtherComputer("ePressed");
                return;
            }
        }

        private void StartServer()
        {
            listener = new TcpListener(IPAddress.Any, 5000);
            listener.Start();
            Task.Run(() => ListenForMessages());
        }

        private async void ListenForMessages()
        {
            while (true)
            {
                TcpClient client = await listener.AcceptTcpClientAsync();
                _ = Task.Run(() => HandleClient(client));
            }
        }

        private async Task HandleClient(TcpClient client)                                
        {
            using (client)    
            {
                NetworkStream stream = client.GetStream();
                byte[] data = new byte[256];
                int bytes = await stream.ReadAsync(data, 0, data.Length);
                string responseData = Encoding.UTF8.GetString(data, 0, bytes);

                switch (responseData)
                {
                    case "SpacePressed":
                        _globalHook.KeyDown -= GlobalHook_KeyDown;
                        inputSimulator.Keyboard.KeyDown(WindowsInput.Native.VirtualKeyCode.SPACE);
                        inputSimulator.Keyboard.KeyUp(WindowsInput.Native.VirtualKeyCode.SPACE);
                        _globalHook.KeyDown += GlobalHook_KeyDown;
                        break;
                    //case "WPressed":
                    //    _globalHook.KeyDown -= GlobalHook_KeyDown;
                    //    inputSimulator.Keyboard.KeyDown(WindowsInput.Native.VirtualKeyCode.VK_W);
                    //    _globalHook.KeyDown += GlobalHook_KeyDown;
                    //    break;
                    //case "SPressed":
                    //    _globalHook.KeyDown -= GlobalHook_KeyDown;
                    //    inputSimulator.Keyboard.KeyDown(WindowsInput.Native.VirtualKeyCode.VK_S);
                    //    _globalHook.KeyDown += GlobalHook_KeyDown;
                    //    break;
                    //case "APressed":
                    //    _globalHook.KeyDown -= GlobalHook_KeyDown;
                    //    inputSimulator.Keyboard.KeyDown(WindowsInput.Native.VirtualKeyCode.VK_A);
                    //    _globalHook.KeyDown += GlobalHook_KeyDown;
                    //    break;
                    //case "DPressed":
                    //    _globalHook.KeyDown -= GlobalHook_KeyDown;
                    //    inputSimulator.Keyboard.KeyDown(WindowsInput.Native.VirtualKeyCode.VK_D);
                    //    inputSimulator.Keyboard.KeyUp(WindowsInput.Native.VirtualKeyCode.VK_D);
                    //    _globalHook.KeyDown += GlobalHook_KeyDown;
                    //    break;

                    case "CtrlPressed":
                        _globalHook.KeyDown -= GlobalHook_KeyDown;
                        inputSimulator.Keyboard.KeyDown(WindowsInput.Native.VirtualKeyCode.CONTROL);
                        inputSimulator.Keyboard.KeyUp(WindowsInput.Native.VirtualKeyCode.CONTROL);
                        _globalHook.KeyDown += GlobalHook_KeyDown;
                        break;

                    case "ShiftPressed":
                        _globalHook.KeyDown -= GlobalHook_KeyDown;
                        inputSimulator.Keyboard.KeyDown(WindowsInput.Native.VirtualKeyCode.SHIFT);
                        inputSimulator.Keyboard.KeyUp(WindowsInput.Native.VirtualKeyCode.SHIFT);
                        _globalHook.KeyDown += GlobalHook_KeyDown;
                        break;

                    case "GPressed":
                        _globalHook.KeyDown -= GlobalHook_KeyDown;
                        inputSimulator.Keyboard.KeyDown(WindowsInput.Native.VirtualKeyCode.VK_G);
                        inputSimulator.Keyboard.KeyUp(WindowsInput.Native.VirtualKeyCode.VK_G);
                        _globalHook.KeyDown += GlobalHook_KeyDown;
                        break;

                    case "EPressed":
                        _globalHook.KeyDown -= GlobalHook_KeyDown;
                        inputSimulator.Keyboard.KeyDown(WindowsInput.Native.VirtualKeyCode.VK_E);
                        inputSimulator.Keyboard.KeyUp(WindowsInput.Native.VirtualKeyCode.VK_E);
                        _globalHook.KeyDown += GlobalHook_KeyDown;
                        break;
                }
            }
        }

        private async void SendMessageToOtherComputer(string message)
        {
            using (TcpClient client = new TcpClient("26.245.20.241", 5000))
            {
                NetworkStream stream = client.GetStream();
                byte[] data = Encoding.UTF8.GetBytes(message);
                await stream.WriteAsync(data, 0, data.Length);
            }
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
