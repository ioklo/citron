using System;
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

using Gum.App.Compiler;

namespace Gum.App.Gummy
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : Window
    {
        public static RoutedCommand ToggleShowInputBox = new RoutedCommand();

        public bool inputBoxVisible;
        public bool InputBoxVisible
        {
            get
            {
                return inputBoxVisible;
            }

            set
            {
                inputBoxVisible = value;

                if (!inputBoxVisible)
                {
                    MainGrid.RowDefinitions[1].Height = new GridLength(0);
                    MainGrid.RowDefinitions[2].Height = new GridLength(0);
                }
                else
                {
                    MainGrid.RowDefinitions[1].Height = new GridLength(0, GridUnitType.Auto);
                    MainGrid.RowDefinitions[2].Height = new GridLength(100, GridUnitType.Pixel);
                }
            }
        }

        private REPL repl;

        public MainWindow()
        {
            InitializeComponent();
            InputBox.Focus();

            InputBoxVisible = true;

            repl = new REPL();
            repl.OnOutput += OnREPLOutput;
            repl.Init();
        }

        private void Write(string text)
        {
            Output.Inlines.Add(text);
            ConsoleBox.ScrollToEnd();
        }

        private void WriteLine(string text)
        {
            Output.Inlines.Add(text);
            Output.Inlines.Add(new LineBreak());
            ConsoleBox.ScrollToEnd();
        }

        private void OnREPLOutput(string text)
        {
            Write(text);
        }

        private async void InputBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyboardDevice.Modifiers == ModifierKeys.Control && e.Key == Key.Enter )
            {
                string code = InputBox.Text.Trim();
                WriteLine(code);

                await repl.Process(code);
                InputBox.Text = "";

                e.Handled = true;
                return;
            }
            
            if (e.Key == Key.Up && InputBox.Text.Length == 0)
            {
                e.Handled = true;
                return;
            }

            if( e.Key == Key.Tab )
            {
                InputBox.SelectedText = "    ";
                InputBox.SelectionStart = InputBox.SelectionStart + 4;
                InputBox.SelectionLength = 0;
                e.Handled = true;
            }
        }
        
        private void ToggleShowInputBox_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            InputBoxVisible = !InputBoxVisible;
            if (InputBoxVisible)
            {
                InputBox.Focus();
                ConsoleBox.ScrollToEnd();
            }
            else
                ConsoleBox.Focus();
        }        
    }
}
