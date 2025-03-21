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
using System.Windows.Forms;
using WindowsInput.Native;


namespace Synchronizator
{
    public partial class MainWindow : Window
    {
        private TcpListener listener;
        private InputSimulator inputSimulator;
        private IKeyboardMouseEvents _globalHook;

        string selected_parameter = "";
        const string CONFIG_PATH = "parameters.json";

        Dictionary<string, Keys> winFormsKeycodes = new Dictionary<string, Keys>
        {
            {"A", Keys.A},
            {"B", Keys.B},
            {"C", Keys.C},
            {"D", Keys.D},
            {"E", Keys.E},
            {"F", Keys.F},
            {"G", Keys.G},
            {"H", Keys.H},
            {"I", Keys.I},
            {"J", Keys.J},
            {"K", Keys.K},
            {"L", Keys.L},
            {"M", Keys.M},
            {"N", Keys.N},
            {"O", Keys.O},
            {"P", Keys.P},
            {"Q", Keys.Q},
            {"R", Keys.R},
            {"S", Keys.S},
            {"T", Keys.T},
            {"U", Keys.U},
            {"V", Keys.V},
            {"W", Keys.W},
            {"X", Keys.X},
            {"Y", Keys.Y},
            {"Z", Keys.Z},
            {"Space", Keys.Space},
            {"LeftCtrl", Keys.LControlKey},
            {"RightCtrl", Keys.RControlKey},
            {"LeftShift", Keys.LShiftKey},
            {"RightShift", Keys.RShiftKey},
            {"Enter", Keys.Enter},
            {"Escape", Keys.Escape},
            {"Backspace", Keys.Back},
            {"Tab", Keys.Tab},
            {"Left", Keys.Left},
            {"Right", Keys.Right},
            {"Up", Keys.Up},
            {"Down", Keys.Down},
            {"Home", Keys.Home},
            {"End", Keys.End},
            {"PageUp", Keys.PageUp},
            {"PageDown", Keys.PageDown},
            {"D1", Keys.D1},
            {"D2", Keys.D2},
            {"D3", Keys.D3},
            {"D4", Keys.D4},
            {"D5", Keys.D5},
            {"D6", Keys.D6},
            {"D7", Keys.D7},
            {"D8", Keys.D8},
            {"D9", Keys.D9},
            {"D0", Keys.D0}
        };

        Dictionary<string, VirtualKeyCode> virtualKeycodes = new Dictionary<string, VirtualKeyCode>
        {
            {"0", VirtualKeyCode.VK_0},
            {"1", VirtualKeyCode.VK_1},
            {"2", VirtualKeyCode.VK_2},
            {"3", VirtualKeyCode.VK_3},
            {"4", VirtualKeyCode.VK_4},
            {"5", VirtualKeyCode.VK_5},
            {"6", VirtualKeyCode.VK_6},
            {"7", VirtualKeyCode.VK_7},
            {"8", VirtualKeyCode.VK_8},
            {"9", VirtualKeyCode.VK_9},
            {"A", VirtualKeyCode.VK_A},
            {"B", VirtualKeyCode.VK_B},
            {"C", VirtualKeyCode.VK_C},
            {"D", VirtualKeyCode.VK_D},
            {"E", VirtualKeyCode.VK_E},
            {"F", VirtualKeyCode.VK_F},
            {"G", VirtualKeyCode.VK_G},
            {"H", VirtualKeyCode.VK_H},
            {"I", VirtualKeyCode.VK_I},
            {"J", VirtualKeyCode.VK_J},
            {"K", VirtualKeyCode.VK_K},
            {"L", VirtualKeyCode.VK_L},
            {"M", VirtualKeyCode.VK_M},
            {"N", VirtualKeyCode.VK_N},
            {"O", VirtualKeyCode.VK_O},
            {"P", VirtualKeyCode.VK_P},
            {"Q", VirtualKeyCode.VK_Q},
            {"R", VirtualKeyCode.VK_R},
            {"S", VirtualKeyCode.VK_S},
            {"T", VirtualKeyCode.VK_T},
            {"U", VirtualKeyCode.VK_U},
            {"V", VirtualKeyCode.VK_V},
            {"W", VirtualKeyCode.VK_W},
            {"X", VirtualKeyCode.VK_X},
            {"Y", VirtualKeyCode.VK_Y},
            {"Z", VirtualKeyCode.VK_Z},
            {"Insert", VirtualKeyCode.INSERT},
            {"Backspace", VirtualKeyCode.BACK},
            {"Tab", VirtualKeyCode.TAB},
            {"Enter", VirtualKeyCode.RETURN},
            {"Escape", VirtualKeyCode.ESCAPE},
            {"Space", VirtualKeyCode.SPACE},
            {"Left", VirtualKeyCode.LEFT},
            {"Right", VirtualKeyCode.RIGHT},
            {"Up", VirtualKeyCode.UP},
            {"Down", VirtualKeyCode.DOWN},
            {"Home", VirtualKeyCode.HOME},
            {"End", VirtualKeyCode.END},
            {"PageUp", VirtualKeyCode.PRIOR},
            {"PageDown", VirtualKeyCode.NEXT},
            {"LeftCtrl", VirtualKeyCode.LCONTROL},
            {"RightCtrl", VirtualKeyCode.RCONTROL},
            {"LeftShift", VirtualKeyCode.LSHIFT},
            {"RightShift", VirtualKeyCode.RSHIFT}
        };

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
            loadedViewModel.LoadFromJson(CONFIG_PATH);
            var viewModel = (ViewModel)this.DataContext;

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
            var button = sender as System.Windows.Controls.Button;
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

        private void KeyInput(object sender, System.Windows.Input.KeyEventArgs e)
        {
            System.Windows.Controls.TextBox textBox = sender as System.Windows.Controls.TextBox;
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
            bool gIsPressed = false;
            bool eIsPressed = false;

            var loadedViewModel = new ViewModelConfiguration();
            loadedViewModel.LoadFromJson(CONFIG_PATH);

            //Прыжок
            if (e.KeyCode == GetWinFormsKeyCodeFromDictionary(loadedViewModel.Parameters.Where(x => x.Key == "Прыжок").SelectMany(j => j.Value.Keybinds).Select(k => k.Value).FirstOrDefault()) && !spaceIsPressed)
            {
                spaceIsPressed = true;
                SendMessageToOtherComputer("Jump");
                return;
            }

            //Выбросить оружие
            if (e.KeyCode == GetWinFormsKeyCodeFromDictionary(loadedViewModel.Parameters.Where(x => x.Key == "Выбросить оружие").SelectMany(j => j.Value.Keybinds).Select(k => k.Value).FirstOrDefault()) && !gIsPressed)
            {
                gIsPressed = true;
                SendMessageToOtherComputer("Drop");
                return;
            }

            //Взаимодействие
            if (e.KeyCode == GetWinFormsKeyCodeFromDictionary(loadedViewModel.Parameters.Where(x => x.Key == "Выбросить оружие").SelectMany(j => j.Value.Keybinds).Select(k => k.Value).FirstOrDefault()) && !eIsPressed)
            {
                eIsPressed = true;
                SendMessageToOtherComputer("Interact");
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

                var loadedViewModel = new ViewModelConfiguration();
                loadedViewModel.LoadFromJson(CONFIG_PATH);

                switch (responseData)
                {
                    case "Jump": 
                        PressButton(GetVirtualKeyCodeFromDictionary(loadedViewModel.Parameters.Where(x => x.Key == "Прыжок").SelectMany(j => j.Value.Keybinds).Select(k => k.Value).FirstOrDefault()));
                        break;

                    case "Drop":
                        PressButton(GetVirtualKeyCodeFromDictionary(loadedViewModel.Parameters.Where(x => x.Key == "Выбросить оружие").SelectMany(j => j.Value.Keybinds).Select(k => k.Value).FirstOrDefault()));
                        break;

                    case "Interact":
                        PressButton(GetVirtualKeyCodeFromDictionary(loadedViewModel.Parameters.Where(x => x.Key == "Взаимодействие").SelectMany(j => j.Value.Keybinds).Select(k => k.Value).FirstOrDefault()));
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

        private Keys GetWinFormsKeyCodeFromDictionary(string key) { return winFormsKeycodes[key]; }

        private VirtualKeyCode GetVirtualKeyCodeFromDictionary(string key) { return virtualKeycodes[key]; }

        private void PressButton(VirtualKeyCode keyCode)
        {
            _globalHook.KeyDown -= GlobalHook_KeyDown;
            inputSimulator.Keyboard.KeyDown(keyCode);
            inputSimulator.Keyboard.KeyUp(keyCode);
            _globalHook.KeyDown += GlobalHook_KeyDown;
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
