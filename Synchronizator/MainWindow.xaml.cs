using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;   
using System.Windows.Input;
using WindowsInput;
using Gma.System.MouseKeyHook;
using System.IO;
using System.Windows.Forms;
using WindowsInput.Native;
using System.Windows.Media.Imaging;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using System.Windows.Controls;

namespace Synchronizator
{
    public partial class MainWindow : Window
    {
        private UdpClient udpListener;
        private UdpClient udpSender;
        private InputSimulator inputSimulator;
        private IKeyboardMouseEvents _globalHook;
        private IKeyboardMouseEvents _globalHookSettings;
        //private DateTime lastSendTime = DateTime.MinValue;
        Task UDPListenerTask = null;

        //private readonly object sendLock = new object();

        string selected_parameter = "";
        bool isMouseSettingsSelected = false;
        readonly string MYDIRECTORY = $"{Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)}/Synchronizator";
        readonly string CONFIG_PATH = $"{Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)}/Synchronizator/parameters.json";
        readonly string IPADDRESS_PATH = $"{Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)}/Synchronizator/ips.txt";
        bool connected = false;

        readonly Dictionary<string, Keys> winFormsKeycodes = new Dictionary<string, Keys>
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

        readonly Dictionary<string, VirtualKeyCode> virtualKeycodes = new Dictionary<string, VirtualKeyCode>
        {
            {"D0", VirtualKeyCode.VK_0},
            {"D1", VirtualKeyCode.VK_1},
            {"D2", VirtualKeyCode.VK_2},
            {"D3", VirtualKeyCode.VK_3},
            {"D4", VirtualKeyCode.VK_4},
            {"D5", VirtualKeyCode.VK_5},
            {"D6", VirtualKeyCode.VK_6},
            {"D7", VirtualKeyCode.VK_7},
            {"D8", VirtualKeyCode.VK_8},
            {"D9", VirtualKeyCode.VK_9},
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

        readonly Dictionary<string, MouseButtons> mouseKeycodes = new Dictionary<string, MouseButtons>
        {
            {"Left", MouseButtons.Left},
            {"Right", MouseButtons.Right},
            {"Middle", MouseButtons.Middle}
        };

        readonly Dictionary<string, int> parameters = new Dictionary<string, int>()
        {
            {"Прыжок", 1}, // Готово
            {"Приседание", 1},
            {"Медленная ходьба", 1}, // Готово
            {"Огонь", 1}, // Готово
            {"Альтернативный огонь", 1}, // Готово
            {"Переключение оружия", 1}, // Готово
            {"Выбросить оружие", 1 }, // Готово
            {"Взаимодействие", 1 }, // Готово
            {"Перезарядка", 1 }, // Готово
            {"Осмотр оружия", 1 }, // Готово
            {"Голосовой чат", 1 } // Готово
        };

        public MainWindow()
        {
            InitializeComponent();
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

            if (File.Exists(IPADDRESS_PATH))
            {
                var ipAddresses = File.ReadAllLines(IPADDRESS_PATH);
                viewModel.IPAdresses.Clear(); // Очищаем коллекцию перед загрузкой новых данных

                foreach (var ipAddress in ipAddresses)
                {
                    // Добавляем IP-адрес в коллекцию
                    viewModel.AddItem(ipAddress, new BitmapImage(new Uri("pack://application:,,,/Assets/Images/grayPin.png")));
                }
            }
            _globalHookSettings = Hook.GlobalEvents();
            _globalHookSettings.KeyDown += GlobalHook_KeyDownSettings;
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) { DragMove(); }

        private void ConnectLocal_Button(object sender, RoutedEventArgs e)
        {
            if (connected)
            {
                StopServer();
                return;
            }
            StartServer();
        }

        private void PinWindow_Button(object sender, RoutedEventArgs e)
        {
            if (this.Topmost)
            {
                this.Topmost = false;
                PinButton_Image.Source = new BitmapImage(new Uri("pack://application:,,,/Assets/Images/grayPin.png"));
                return;
            }
            this.Topmost = true;
            PinButton_Image.Source = new BitmapImage(new Uri("pack://application:,,,/Assets/Images/bluePin.png"));
        }

        private async void SettingsWindow_Click(object sender, RoutedEventArgs e)
        {
            var viewModel = (ViewModel)this.DataContext;
            parameter_name.Content = "Список IP-адресов";
            HideAllBodyWindows();
            configurationMenu_grid.Visibility = Visibility.Visible;
            settings_Grid.Visibility = Visibility.Visible;
            saveConfig_Button.Visibility = Visibility.Hidden;
            Grid.SetColumnSpan(closeConfig_Button, 2);
            await viewModel.CheckIPAddressesAsync();
        }

        private void CreateConfig()
        {
            if (!Directory.Exists(MYDIRECTORY)) { Directory.CreateDirectory(MYDIRECTORY); }
            const string CONFIG = "{\n" +
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
              "  \"Медленная ходьба\": {\n" +
              "    \"Enabled\": false,\n" +
              "    \"Keybinds\": {\n" +
              "      \"MainKey\": \"LeftShift\"\n" +
              "    }\n" +
              "  },\n" +
              "  \"Огонь\": {\n" +
              "    \"Enabled\": false,\n" +
              "    \"Keybinds\": {\n" +
              "      \"MainKey\": \"Left\"\n" +
              "    }\n" +
              "  },\n" +
              "  \"Альтернативный огонь\": {\n" +
              "    \"Enabled\": false,\n" +
              "    \"Keybinds\": {\n" +
              "      \"MainKey\": \"Right\"\n" +
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
              "  \"Взаимодействие\": {\n" +
              "    \"Enabled\": false,\n" +
              "    \"Keybinds\": {\n" +
              "      \"MainKey\": \"E\"\n" +
              "    }\n" +
              "  },\n" +
              "  \"Перезарядка\": {\n" +
              "    \"Enabled\": false,\n" +
              "    \"Keybinds\": {\n" +
              "      \"MainKey\": \"R\"\n" +
              "    }\n" +
              "  },\n" +
              "  \"Осмотр оружия\": {\n" +
              "    \"Enabled\": false,\n" +
              "    \"Keybinds\": {\n" +
              "      \"MainKey\": \"F\"\n" +
              "    }\n" +
              "  },\n" +
              "  \"Голосовой чат\": {\n" +
              "    \"Enabled\": false,\n" +
              "    \"Keybinds\": {\n" +
              "      \"MainKey\": \"C\"\n" +
              "    }\n" +
              "  }\n" +
            "}";
            File.WriteAllText(CONFIG_PATH, CONFIG);
        }

        private void MinimizeWindow_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void CloseWindow_Click(object sender, RoutedEventArgs e)       
        {
            if (_globalHook != null) StopServer();
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

            if (selected_parameter == "Огонь" || selected_parameter == "Альтернативный огонь") { isMouseSettingsSelected = true; } else { isMouseSettingsSelected = false; }

            if (selected_parameter == "Переключение оружия")
            {
                HideAllBodyWindows();
                weaponSwap_Config_Grid.Visibility = Visibility.Visible;

                mainWeapon_keybind.Text = loadedViewModel.Parameters.Where(x => x.Key == "Переключение оружия").SelectMany(j => j.Value.Keybinds).Where(k => k.Key == "MainWeapon").Select(k => k.Value).FirstOrDefault();
                secondaryWeapon_keybind.Text = loadedViewModel.Parameters.Where(x => x.Key == "Переключение оружия").SelectMany(j => j.Value.Keybinds).Where(k => k.Key == "SecondaryWeapon").Select(k => k.Value).FirstOrDefault();
                knifeWeapon_keybind.Text = loadedViewModel.Parameters.Where(x => x.Key == "Переключение оружия").SelectMany(j => j.Value.Keybinds).Where(k => k.Key == "Knife").Select(k => k.Value).FirstOrDefault();
                grenadesWeapon_keybind.Text = loadedViewModel.Parameters.Where(x => x.Key == "Переключение оружия").SelectMany(j => j.Value.Keybinds).Where(k => k.Key == "Grenades").Select(k => k.Value).FirstOrDefault();
                bombWeapon_keybind.Text = loadedViewModel.Parameters.Where(x => x.Key == "Переключение оружия").SelectMany(j => j.Value.Keybinds).Where(k => k.Key == "Bomb").Select(k => k.Value).FirstOrDefault();
            }
            else if (isMouseSettingsSelected)
            {
                mouse_input_keybind.Text = loadedViewModel.Parameters.Where(x => x.Key == selected_parameter).SelectMany(j => j.Value.Keybinds).Select(k => k.Value).FirstOrDefault();
                HideAllBodyWindows();
                mouseInput_Config_Grid.Visibility = Visibility.Visible;
            }
            else 
            {
                enter_keybind.Text = loadedViewModel.Parameters.Where(x => x.Key == selected_parameter).SelectMany(j => j.Value.Keybinds).Select(k => k.Value).FirstOrDefault();
                HideAllBodyWindows();
                mainConfig_Grid.Visibility = Visibility.Visible;
            }
        }

        private void KeyboardInput(object sender, System.Windows.Input.KeyEventArgs e)
        {
            System.Windows.Controls.TextBox textBox = sender as System.Windows.Controls.TextBox;
            string key = e.Key.ToString();
            if (key == "Back") { textBox.Text = ""; return; }
            textBox.Text = key;
            e.Handled = true;
        }

        private void MouseInput(object sender, MouseButtonEventArgs e)
        {
            System.Windows.Controls.TextBox textBox = sender as System.Windows.Controls.TextBox;
            string key = e.ChangedButton.ToString();
            textBox.Text = key;
            e.Handled = true;
        }
        
        private void MouseWheelInput(object sender, MouseWheelEventArgs e)
        {
            System.Windows.Controls.TextBox textBox = sender as System.Windows.Controls.TextBox;
            if (e.Delta > 0) { textBox.Text = "Up"; }
            else { textBox.Text = "Down"; }
        }

        private void closeConfigurationMenu_button(object sender, RoutedEventArgs e)
        {
            configurationMenu_grid.Visibility = Visibility.Hidden;
            HideAllBodyWindows();
            EraseInputs();
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
            else if (isMouseSettingsSelected)
            {
                keybinds.Add("MouseButton", mouse_input_keybind.Text);
            }
            else 
            {
                keybinds.Add("MainKey", enter_keybind.Text);
            }

            viewModelConfig.AddParameter(selected_parameter, true, keybinds);
            viewModelConfig.SaveToJson(CONFIG_PATH);
        }

        private async void AddIPAdress_Button(object sender, RoutedEventArgs e)
        {
            var viewModel = (ViewModel)this.DataContext;

            string IPAddress = IPAddress_TextBox.Text;
            if (IsValidIPAddress(IPAddress))
            {
                viewModel.AddItem(IPAddress, new BitmapImage(new Uri("pack://application:,,,/Assets/Images/grayPin.png")));
                viewModel.SaveIPAdresses(IPADDRESS_PATH);
                IPAddress_TextBox.Clear();
            }

            await viewModel.CheckIPAddressesAsync();
        }

        private async void RefreshConnectionStatus(object sender, RoutedEventArgs e)
        {
            var viewModel = (ViewModel)this.DataContext;
            await viewModel.CheckIPAddressesAsync();
        }

        private async void GlobalHook_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            var loadedViewModel = new ViewModelConfiguration();
            loadedViewModel.LoadFromJson(CONFIG_PATH);

            bool buttonIsPressed = false;

            if (e.Button == GetMouseButtonFromDictionary(loadedViewModel.Parameters.Where(x => x.Key == "Огонь").SelectMany(j => j.Value.Keybinds).Select(k => k.Value).FirstOrDefault()) && loadedViewModel.Parameters.Where(x => x.Key == "Огонь").Select(j => j.Value.Enabled).FirstOrDefault() && !buttonIsPressed)
            {
                buttonIsPressed = true;
                await SendMessageToMultipleComputers("FireD");
                return;
            }
            if (e.Button == GetMouseButtonFromDictionary(loadedViewModel.Parameters.Where(x => x.Key == "Альтернативный огонь").SelectMany(j => j.Value.Keybinds).Select(k => k.Value).FirstOrDefault()) && loadedViewModel.Parameters.Where(x => x.Key == "Альтернативный огонь").Select(j => j.Value.Enabled).FirstOrDefault() && !buttonIsPressed)
            {
                buttonIsPressed = true;
                await SendMessageToMultipleComputers("Secondary");
                return;
            }
        }

        private async void GlobalHook_MouseUp(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            bool buttonIsPressed = false;

            var loadedViewModel = new ViewModelConfiguration();
            loadedViewModel.LoadFromJson(CONFIG_PATH);

            if (e.Button == GetMouseButtonFromDictionary(loadedViewModel.Parameters.Where(x => x.Key == "Огонь").SelectMany(j => j.Value.Keybinds).Select(k => k.Value).FirstOrDefault()) && loadedViewModel.Parameters.Where(x => x.Key == "Огонь").Select(j => j.Value.Enabled).FirstOrDefault() && !buttonIsPressed)
            {
                buttonIsPressed = true;
                await SendMessageToMultipleComputers("FireU");
                return;
            }
        }

        //private void GlobalHook_MouseWheel(object sender, System.Windows.Forms.MouseEventArgs e)
        //{
        //    if (e.Delta > 0)
        //    { 

        //    }
        //    else if (e.Delta < 0)
        //    {

        //    }
        //}

        private async void GlobalHook_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            bool buttonIsPressed = false;

            var loadedViewModel = new ViewModelConfiguration();
            loadedViewModel.LoadFromJson(CONFIG_PATH);

            //Медленная ходьба
            if (e.KeyCode == GetWinFormsKeyCodeFromDictionary(loadedViewModel.Parameters.Where(x => x.Key == "Медленная ходьба").SelectMany(j => j.Value.Keybinds).Select(k => k.Value).FirstOrDefault()) && loadedViewModel.Parameters.Where(x => x.Key == "Медленная ходьба").Select(j => j.Value.Enabled).FirstOrDefault() && !buttonIsPressed)
            {
                buttonIsPressed = true;
                await SendMessageToMultipleComputers("ShiftD");
                return;
            }
            //Приседание
            if (e.KeyCode == GetWinFormsKeyCodeFromDictionary(loadedViewModel.Parameters.Where(x => x.Key == "Приседание").SelectMany(j => j.Value.Keybinds).Select(k => k.Value).FirstOrDefault()) && loadedViewModel.Parameters.Where(x => x.Key == "Приседание").Select(j => j.Value.Enabled).FirstOrDefault() && !buttonIsPressed)
            {
                buttonIsPressed = true;
                await SendMessageToMultipleComputers("CtrlD");
                return;
            }
            //Голосовой чат
            if (e.KeyCode == GetWinFormsKeyCodeFromDictionary(loadedViewModel.Parameters.Where(x => x.Key == "Голосовой чат").SelectMany(j => j.Value.Keybinds).Select(k => k.Value).FirstOrDefault()) && loadedViewModel.Parameters.Where(x => x.Key == "Голосовой чат").Select(j => j.Value.Enabled).FirstOrDefault() && !buttonIsPressed)
            {
                buttonIsPressed = true;
                await SendMessageToMultipleComputers("VoiceD");
                return;
            }


            //Прыжок
            if (e.KeyCode == GetWinFormsKeyCodeFromDictionary(loadedViewModel.Parameters.Where(x => x.Key == "Прыжок").SelectMany(j => j.Value.Keybinds).Select(k => k.Value).FirstOrDefault()) && loadedViewModel.Parameters.Where(x => x.Key == "Прыжок").Select(j => j.Value.Enabled).FirstOrDefault() && !buttonIsPressed)
            {
                buttonIsPressed = true;
                await SendMessageToMultipleComputers("Jump");
                return;
            }

            //Выбросить оружие
            if (e.KeyCode == GetWinFormsKeyCodeFromDictionary(loadedViewModel.Parameters.Where(x => x.Key == "Выбросить оружие").SelectMany(j => j.Value.Keybinds).Select(k => k.Value).FirstOrDefault()) && loadedViewModel.Parameters.Where(x => x.Key == "Выбросить оружие").Select(j => j.Value.Enabled).FirstOrDefault() && !buttonIsPressed)
            {
                buttonIsPressed = true;
                await SendMessageToMultipleComputers("Drop");
                return;
            }

            //Взаимодействие
            if (e.KeyCode == GetWinFormsKeyCodeFromDictionary(loadedViewModel.Parameters.Where(x => x.Key == "Взаимодействие").SelectMany(j => j.Value.Keybinds).Select(k => k.Value).FirstOrDefault()) && loadedViewModel.Parameters.Where(x => x.Key == "Взаимодействие").Select(j => j.Value.Enabled).FirstOrDefault() && !buttonIsPressed)
            {
                buttonIsPressed = true;
                await SendMessageToMultipleComputers("Interact");
                return;
            }

            //Перезарядка
            if (e.KeyCode == GetWinFormsKeyCodeFromDictionary(loadedViewModel.Parameters.Where(x => x.Key == "Перезарядка").SelectMany(j => j.Value.Keybinds).Select(k => k.Value).FirstOrDefault()) && loadedViewModel.Parameters.Where(x => x.Key == "Перезарядка").Select(j => j.Value.Enabled).FirstOrDefault() && !buttonIsPressed)
            {
                buttonIsPressed = true;
                await SendMessageToMultipleComputers("Reload");
                return;
            }

            //Осмотр оружия
            if (e.KeyCode == GetWinFormsKeyCodeFromDictionary(loadedViewModel.Parameters.Where(x => x.Key == "Осмотр оружия").SelectMany(j => j.Value.Keybinds).Select(k => k.Value).FirstOrDefault()) && loadedViewModel.Parameters.Where(x => x.Key == "Осмотр оружия").Select(j => j.Value.Enabled).FirstOrDefault() && !buttonIsPressed)
            {
                buttonIsPressed = true;
                await SendMessageToMultipleComputers("Inspect");
                return;
            }

            //Переключение оружия 
            if (loadedViewModel.Parameters.Where(x => x.Key == "Переключение оружия").Select(j => j.Value.Enabled).FirstOrDefault())
            {
                //Основное оружие
                if (e.KeyCode == GetWinFormsKeyCodeFromDictionary(loadedViewModel.Parameters.Where(x => x.Key == "Переключение оружия").SelectMany(j => j.Value.Keybinds).Where(k => k.Key == "MainWeapon").Select(k => k.Value).FirstOrDefault()) && !buttonIsPressed)
                {
                    buttonIsPressed = true;
                    await SendMessageToMultipleComputers("MainWeapon");
                    return;
                }
                //Пистолет
                if (e.KeyCode == GetWinFormsKeyCodeFromDictionary(loadedViewModel.Parameters.Where(x => x.Key == "Переключение оружия").SelectMany(j => j.Value.Keybinds).Where(k => k.Key == "SecondaryWeapon").Select(k => k.Value).FirstOrDefault()) && !buttonIsPressed)
                {
                    buttonIsPressed = true;
                    await SendMessageToMultipleComputers("SecondaryWeapon");
                    return;
                }
                // Нож
                if (e.KeyCode == GetWinFormsKeyCodeFromDictionary(loadedViewModel.Parameters.Where(x => x.Key == "Переключение оружия").SelectMany(j => j.Value.Keybinds).Where(k => k.Key == "Knife").Select(k => k.Value).FirstOrDefault()) && !buttonIsPressed)
                {
                    buttonIsPressed = true;
                    await SendMessageToMultipleComputers("Knife");
                    return;
                }
                // Гранаты
                if (e.KeyCode == GetWinFormsKeyCodeFromDictionary(loadedViewModel.Parameters.Where(x => x.Key == "Переключение оружия").SelectMany(j => j.Value.Keybinds).Where(k => k.Key == "Grenades").Select(k => k.Value).FirstOrDefault()) && !buttonIsPressed)
                {
                    buttonIsPressed = true;
                    await SendMessageToMultipleComputers("Grenades");
                    return;
                }
                //Бомба
                if (e.KeyCode == GetWinFormsKeyCodeFromDictionary(loadedViewModel.Parameters.Where(x => x.Key == "Переключение оружия").SelectMany(j => j.Value.Keybinds).Where(k => k.Key == "Bomb").Select(k => k.Value).FirstOrDefault()) && !buttonIsPressed)
                {
                    buttonIsPressed = true;
                    await SendMessageToMultipleComputers("Bomb");
                    return;
                }
            }
        }

        private async void GlobalHook_KeyUp(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            var loadedViewModel = new ViewModelConfiguration();
            loadedViewModel.LoadFromJson(CONFIG_PATH);

            bool buttonIsPressed = false;
            //Медленная ходьба
            if (e.KeyCode == GetWinFormsKeyCodeFromDictionary(loadedViewModel.Parameters.Where(x => x.Key == "Медленная ходьба").SelectMany(j => j.Value.Keybinds).Select(k => k.Value).FirstOrDefault()) && loadedViewModel.Parameters.Where(x => x.Key == "Медленная ходьба").Select(j => j.Value.Enabled).FirstOrDefault() && !buttonIsPressed)
            {
                buttonIsPressed = true;
                await SendMessageToMultipleComputers("ShiftU");
                return;
            }
            //Приседание
            if (e.KeyCode == GetWinFormsKeyCodeFromDictionary(loadedViewModel.Parameters.Where(x => x.Key == "Приседание").SelectMany(j => j.Value.Keybinds).Select(k => k.Value).FirstOrDefault()) && loadedViewModel.Parameters.Where(x => x.Key == "Приседание").Select(j => j.Value.Enabled).FirstOrDefault() && !buttonIsPressed)
            {
                buttonIsPressed = true;
                await SendMessageToMultipleComputers("CtrlU");
                return;
            }
            //Голосовой чат
            if (e.KeyCode == GetWinFormsKeyCodeFromDictionary(loadedViewModel.Parameters.Where(x => x.Key == "Голосовой чат").SelectMany(j => j.Value.Keybinds).Select(k => k.Value).FirstOrDefault()) && loadedViewModel.Parameters.Where(x => x.Key == "Голосовой чат").Select(j => j.Value.Enabled).FirstOrDefault() && !buttonIsPressed)
            {
                buttonIsPressed = true;
                await SendMessageToMultipleComputers("VoiceU");
                return;
            }
        }

        private void GlobalHook_KeyDownSettings(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            var waveOut = new WaveOutEvent();
            var connectSignal = new SignalGenerator()
            {
                Gain = 0.2, // Громкость
                Frequency = 440, // Частота (в Гц)
                Type = SignalGeneratorType.Sin // Тип сигнала (синусоидальный)
            }.Take(TimeSpan.FromSeconds(0.3));

            var disconnectSignal = new SignalGenerator()
            {
                Gain = 0.2, // Громкость
                Frequency = 840, // Частота (в Гц)
                Type = SignalGeneratorType.Sin // Тип сигнала (синусоидальный)
            }.Take(TimeSpan.FromSeconds(0.3));


            if (e.KeyCode == Keys.F6 && connected)
            {
                waveOut.Init(connectSignal);
                waveOut.Play();
                StopServer();
                
            }
            else if (e.KeyCode == Keys.F6 && !connected)
            {
                waveOut.Init(disconnectSignal);
                waveOut.Play();
                StartServer();
            }
        }

        private void StartServer()
        {
            SubscribeToGlobalHook();
            inputSimulator = new InputSimulator();
            udpListener = new UdpClient(58330);
            udpSender = new UdpClient(58331);
            UDPListenerTask = Task.Run(() => ListenForMessages());
            connected = true;
            ConnectButton_Image.Source = new BitmapImage(new Uri("pack://application:,,,/Assets/Images/greenPin.png"));
        }

        private void StopServer()
        {
            UnsubscribeFromGlobalHook();
            UDPListenerTask.Dispose();
            udpListener.Close();
            udpSender.Close();
            connected = false;
            ConnectButton_Image.Source = new BitmapImage(new Uri("pack://application:,,,/Assets/Images/redPin.png"));
        }

        private async void ListenForMessages()
        {
            while (true)
            {
                try
                {
                    var result = await udpListener.ReceiveAsync();
                    string message = Encoding.UTF8.GetString(result.Buffer);
                    HandleMessage(message);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error receiving message: {ex.Message}");
                    break;
                }
            }
        }

        private void HandleMessage(string message)
        {
            var loadedViewModel = new ViewModelConfiguration();
            loadedViewModel.LoadFromJson(CONFIG_PATH);

            switch (message)
            {
                    //Медленная ходьба
                case "ShiftD":
                    KeyboardButtonDown(GetVirtualKeyCodeFromDictionary(loadedViewModel.Parameters.Where(x => x.Key == "Медленная ходьба").SelectMany(j => j.Value.Keybinds).Select(k => k.Value).FirstOrDefault()));
                    break;
                case "ShiftU":
                    KeyboardButtonUp(GetVirtualKeyCodeFromDictionary(loadedViewModel.Parameters.Where(x => x.Key == "Медленная ходьба").SelectMany(j => j.Value.Keybinds).Select(k => k.Value).FirstOrDefault()));
                    break;

                    //Приседание
                case "CtrlD":
                    KeyboardButtonDown(GetVirtualKeyCodeFromDictionary(loadedViewModel.Parameters.Where(x => x.Key == "Приседание").SelectMany(j => j.Value.Keybinds).Select(k => k.Value).FirstOrDefault()));
                    break;
                case "CtrlU":
                    KeyboardButtonUp(GetVirtualKeyCodeFromDictionary(loadedViewModel.Parameters.Where(x => x.Key == "Приседание").SelectMany(j => j.Value.Keybinds).Select(k => k.Value).FirstOrDefault()));
                    break;

                //Голосовой чат
                case "VoiceD":
                    KeyboardButtonDown(GetVirtualKeyCodeFromDictionary(loadedViewModel.Parameters.Where(x => x.Key == "Голосовой чат").SelectMany(j => j.Value.Keybinds).Select(k => k.Value).FirstOrDefault()));
                    break;
                //Голосовой чат
                case "VoiceU":
                    KeyboardButtonUp(GetVirtualKeyCodeFromDictionary(loadedViewModel.Parameters.Where(x => x.Key == "Голосовой чат").SelectMany(j => j.Value.Keybinds).Select(k => k.Value).FirstOrDefault()));
                    break;


                //Огонь
                case "FireD":
                    MouseButtonDown();
                    break;
                case "FireU":
                    MouseButtonUp();
                    break;

                    //Прыжок
                case "Jump":
                    string key = loadedViewModel.Parameters.Where(x => x.Key == "Прыжок").SelectMany(j => j.Value.Keybinds).Select(k => k.Value).FirstOrDefault();
                    if (key == "Down" || key == "Up" || key == "Left" || key == "Right") { PressMouseButton(key); break; }
                    PressKeyboardButton(GetVirtualKeyCodeFromDictionary(key));
                    break;

                    //Выбросить оружие
                case "Drop":
                    PressKeyboardButton(GetVirtualKeyCodeFromDictionary(loadedViewModel.Parameters.Where(x => x.Key == "Выбросить оружие").SelectMany(j => j.Value.Keybinds).Select(k => k.Value).FirstOrDefault()));
                    break;
                        
                    //Осмотр оружия
                case "Interact":
                    PressKeyboardButton(GetVirtualKeyCodeFromDictionary(loadedViewModel.Parameters.Where(x => x.Key == "Взаимодействие").SelectMany(j => j.Value.Keybinds).Select(k => k.Value).FirstOrDefault()));
                    break;

                    //Альтернативный огонь
                case "Secondary":
                    PressMouseButton(loadedViewModel.Parameters.Where(x => x.Key == "Альтернативный огонь").SelectMany(j => j.Value.Keybinds).Select(k => k.Value).FirstOrDefault());
                    break;
                    
                    //Перезарядка
                case "Reload":
                    PressKeyboardButton(GetVirtualKeyCodeFromDictionary(loadedViewModel.Parameters.Where(x => x.Key == "Перезарядка").SelectMany(j => j.Value.Keybinds).Select(k => k.Value).FirstOrDefault()));
                    break;

                    //Осмотр оружия
                case "Inspect":
                    PressKeyboardButton(GetVirtualKeyCodeFromDictionary(loadedViewModel.Parameters.Where(x => x.Key == "Осмотр оружия").SelectMany(j => j.Value.Keybinds).Select(k => k.Value).FirstOrDefault()));
                    break;
                    
                    //Переключение оружия
                case "MainWeapon":
                    PressKeyboardButton(GetVirtualKeyCodeFromDictionary(loadedViewModel.Parameters.Where(x => x.Key == "Переключение оружия").SelectMany(j => j.Value.Keybinds).Where(k => k.Key == "MainWeapon").Select(k => k.Value).FirstOrDefault()));
                    break;
                case "SecondaryWeapon":
                    PressKeyboardButton(GetVirtualKeyCodeFromDictionary(loadedViewModel.Parameters.Where(x => x.Key == "Переключение оружия").SelectMany(j => j.Value.Keybinds).Where(k => k.Key == "SecondaryWeapon").Select(k => k.Value).FirstOrDefault()));
                    break;
                case "Knife":
                    PressKeyboardButton(GetVirtualKeyCodeFromDictionary(loadedViewModel.Parameters.Where(x => x.Key == "Переключение оружия").SelectMany(j => j.Value.Keybinds).Where(k => k.Key == "Knife").Select(k => k.Value).FirstOrDefault())); 
                    break;
                case "Grenades":
                    PressKeyboardButton(GetVirtualKeyCodeFromDictionary(loadedViewModel.Parameters.Where(x => x.Key == "Переключение оружия").SelectMany(j => j.Value.Keybinds).Where(k => k.Key == "Grenades").Select(k => k.Value).FirstOrDefault())); 
                    break;
                case "Bomb":
                    PressKeyboardButton(GetVirtualKeyCodeFromDictionary(loadedViewModel.Parameters.Where(x => x.Key == "Переключение оружия").SelectMany(j => j.Value.Keybinds).Where(k => k.Key == "Bomb").Select(k => k.Value).FirstOrDefault()));
                    break;
            }
        }

        private async Task SendMessageToMultipleComputers(string message)
        {
            var viewModel = (ViewModel)this.DataContext;

            List<Task> tasks = new List<Task>();
            foreach (var ip in viewModel.IPAdresses.Select(item => item.IPAddress).ToList())
            {
                tasks.Add(SendMessageToOtherComputer(ip, message));
            }

            await Task.WhenAll(tasks);
        }

        private async Task SendMessageToOtherComputer(string ipAddress, string message)
        {
            //lock (sendLock)
            //{
            //    //Проверяем, прошло ли 100 миллисекунд с последней отправки
            //    if ((message != "FireU" && message != "ShiftU" && message != "CtrlU"))
            //    {
            //        if ((DateTime.Now - lastSendTime).TotalMilliseconds < 100) { return; }
            //        lastSendTime = DateTime.Now;
            //    }
            //}

            try
            {
                var endpoint = new IPEndPoint(IPAddress.Parse(ipAddress), 58330);
                byte[] data = Encoding.UTF8.GetBytes(message);
                await udpSender.SendAsync(data, data.Length, endpoint);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending message to {ipAddress}: {ex.Message}");
            }
        }

        private void SubscribeToGlobalHook()
        {
            _globalHook = Hook.GlobalEvents();
            _globalHook.KeyDown += GlobalHook_KeyDown;
            _globalHook.MouseDown += GlobalHook_MouseDown;
            _globalHook.MouseUp += GlobalHook_MouseUp;
            _globalHook.KeyUp += GlobalHook_KeyUp;
            //_globalHook.MouseWheel += GlobalHook_MouseWheel;
        }

        private void UnsubscribeFromGlobalHook()
        {
            _globalHook.KeyDown -= GlobalHook_KeyDown;
            _globalHook.MouseDown -= GlobalHook_MouseDown;
            _globalHook.MouseUp -= GlobalHook_MouseUp;
            _globalHook.KeyUp -= GlobalHook_KeyUp;
            _globalHook.Dispose();
        }
        
        private Keys GetWinFormsKeyCodeFromDictionary(string key) { return winFormsKeycodes[key]; }

        private VirtualKeyCode GetVirtualKeyCodeFromDictionary(string key) { return virtualKeycodes[key]; }
        
        private MouseButtons GetMouseButtonFromDictionary(string key) { return mouseKeycodes[key]; }

        private void PressKeyboardButton(VirtualKeyCode keyCode)
        {
            _globalHook.KeyDown -= GlobalHook_KeyDown;
            inputSimulator.Keyboard.KeyDown(keyCode);
            inputSimulator.Keyboard.KeyUp(keyCode);
            _globalHook.KeyDown += GlobalHook_KeyDown;
        }

        private void KeyboardButtonDown(VirtualKeyCode keyCode)
        {
            _globalHook.KeyUp -= GlobalHook_KeyUp;
            _globalHook.KeyDown -= GlobalHook_KeyDown;
            inputSimulator.Keyboard.KeyDown(keyCode);
            _globalHook.KeyDown += GlobalHook_KeyDown;
            _globalHook.KeyUp += GlobalHook_KeyUp;
        }
        
        private void KeyboardButtonUp(VirtualKeyCode keyCode)
        {
            _globalHook.KeyUp -= GlobalHook_KeyUp;
            _globalHook.KeyDown -= GlobalHook_KeyDown;
            inputSimulator.Keyboard.KeyUp(keyCode);
            _globalHook.KeyDown += GlobalHook_KeyDown;
            _globalHook.KeyUp += GlobalHook_KeyUp;
        }

        private void MouseButtonDown()
        {
            _globalHook.MouseDown -= GlobalHook_MouseDown;
            _globalHook.MouseUp -= GlobalHook_MouseUp;
            inputSimulator.Mouse.LeftButtonDown();
            _globalHook.MouseDown += GlobalHook_MouseDown;
            _globalHook.MouseUp += GlobalHook_MouseUp;
        }

        private void MouseButtonUp()
        {
            _globalHook.MouseDown -= GlobalHook_MouseDown;
            _globalHook.MouseUp -= GlobalHook_MouseUp;
            inputSimulator.Mouse.LeftButtonUp();
            _globalHook.MouseDown += GlobalHook_MouseDown;
            _globalHook.MouseUp += GlobalHook_MouseUp;
        }

        private void PressMouseButton(string button)
        {
            switch (button)
            {
                case "Left":
                    _globalHook.MouseDown -= GlobalHook_MouseDown;
                    inputSimulator.Mouse.LeftButtonDown();
                    inputSimulator.Mouse.LeftButtonUp();
                    _globalHook.MouseDown += GlobalHook_MouseDown;
                    break;
                case "Right":
                    _globalHook.MouseDown -= GlobalHook_MouseDown;
                    inputSimulator.Mouse.RightButtonDown();
                    inputSimulator.Mouse.RightButtonUp();
                    _globalHook.MouseDown += GlobalHook_MouseDown;
                    break;
                case "Up":
                    _globalHook.MouseDown -= GlobalHook_MouseDown;
                    inputSimulator.Mouse.VerticalScroll(1);
                    _globalHook.MouseDown += GlobalHook_MouseDown;
                    break;
                case "Down":
                    _globalHook.MouseDown -= GlobalHook_MouseDown;
                    inputSimulator.Mouse.VerticalScroll(-1);
                    _globalHook.MouseDown += GlobalHook_MouseDown;
                    break;
            }
        }

        private void EraseInputs()
        {
            enter_keybind.Clear();
            mainWeapon_keybind.Clear();
            secondaryWeapon_keybind.Clear();
            knifeWeapon_keybind.Clear();
            grenadesWeapon_keybind.Clear();
            bombWeapon_keybind.Clear();
            mouse_input_keybind.Clear();
        }

        private void HideAllBodyWindows()
        {
            Grid.SetColumnSpan(closeConfig_Button, 1);
            var viewModel = (ViewModel)this.DataContext;
            settings_Grid.Visibility = Visibility.Hidden;
            mainConfig_Grid.Visibility= Visibility.Hidden;
            mouseInput_Config_Grid.Visibility = Visibility.Hidden;
            weaponSwap_Config_Grid.Visibility = Visibility.Hidden;
            saveConfig_Button.Visibility = Visibility.Visible;
        }

        static bool IsValidIPAddress(string ip)
        {
            return IPAddress.TryParse(ip, out _);
        }

        private void DeleteIPAddress_Click(object sender, RoutedEventArgs e)
        {
            var viewModel = this.DataContext as ViewModel;
            var button = sender as System.Windows.Controls.Button;
            var item = button.DataContext as Settings_ItemSource_Class;
            viewModel.DeleteIPAddress(item);

            viewModel.SaveIPAdresses(IPADDRESS_PATH);
        }
    }
}