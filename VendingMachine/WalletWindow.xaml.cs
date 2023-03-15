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
using System.Windows.Shapes;

namespace VendingMachine
{
    /// <summary>
    /// Логика взаимодействия для WalletWindow.xaml
    /// </summary>
    public partial class WalletWindow : Window
    {
        CashManager _cashManager;
        public event Action<int> UpdateChange;

        public WalletWindow(CashManager cashManager, bool IsEnabled)
        {
            InitializeComponent();
            _cashManager = cashManager;
            if (!IsEnabled)
            {
                OnFailureUIDisable();
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            int denomination;
            if (int.TryParse((String)button.Tag, out denomination))
            _cashManager.UpdateAvailableFunds(denomination, true);

            // Change is calculated only for cases when asked for immediately after purchase
            // Therefore other actions reset precalculated change because it may be incorrect in new conditions
            for (int i=0; i < _cashManager.PreparedChange.Length; i++)
            {
                _cashManager.PreparedChange[i] = 0;
            }
            UpdateChange?.Invoke(0);
        }

        public void DispalyNotification(string displayedText)
        {
            NotificationBlock.Text = displayedText;
        }

        public void OnFailureUIDisable()
        {
            HeaderBlock.Text = "The vending machine is out of order";
            Button1.IsEnabled = false;
            Button2.IsEnabled = false;
            Button5.IsEnabled = false;
            Button10.IsEnabled = false;
            Button50.IsEnabled = false;
            Button100.IsEnabled = false;
            Button500.IsEnabled = false;
            Button1000.IsEnabled = false;
        }
    }
}
