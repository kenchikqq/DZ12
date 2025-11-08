using System.Windows;
using ProcessThreadDemo.Services;

namespace ProcessThreadDemo
{
    public partial class MainWindow : Window
    {
        private readonly NotepadService _notepadService;
        private readonly CmdService _cmdService;

        public MainWindow()
        {
            InitializeComponent();

            _notepadService = new NotepadService(Dispatcher);
            _cmdService = new CmdService(Dispatcher);
        }

        private void StartNotepadButton_Click(object sender, RoutedEventArgs e)
        {
            startNotepadButton.IsEnabled = false;

            _notepadService.StatusUpdated += (status) =>
            {
                notepadStatusText.Text = status;
            };

            _notepadService.ResultUpdated += (result) =>
            {
                resultText.Text = result;
            };

            _notepadService.Completed += () =>
            {
                startNotepadButton.IsEnabled = true;
            };

            _notepadService.StartNotepadAndCloseAfter3Seconds();
        }

        private void RunCmdButton_Click(object sender, RoutedEventArgs e)
        {
            runCmdButton.IsEnabled = false;

            _cmdService.StatusUpdated += (status) =>
            {
                cmdStatusText.Text = status;
            };

            _cmdService.ResultUpdated += (result) =>
            {
                resultText.Text = result;
            };

            _cmdService.Completed += () =>
            {
                runCmdButton.IsEnabled = true;
            };

            _cmdService.ExecuteDirCommand();
        }
    }
}