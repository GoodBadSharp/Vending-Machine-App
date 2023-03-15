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

namespace VendingMachine
{
    public delegate bool ProductAvailabilityCallback(int productID);
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        CashManager _cashManager = new CashManager();
        ProductManager _productManager = new ProductManager();
        List<Button> _leBoutons = new List<Button>();
        public ProductAvailabilityCallback ProductAvailability;
        event Action<string> PassNotification;

        int _chosenProductID;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void ShopWindow_Loaded(object sender, RoutedEventArgs e)
        {
            #region On Load Actions
            var walletWindow = new WalletWindow(_cashManager, _productManager.DataRetieved);

            // assigning handlers 
            walletWindow.Closed += WalletWindowClosedNotification;
            walletWindow.UpdateChange += UpdateAvailableChange;
            ProductAvailability += _cashManager.CheckProductAvailability;  // (3) Invoking handler fucntion in cashManager to check if sufficient funds/enough change for certain product
            _cashManager.CashUpdateHandler += UpdateFunds;
            _cashManager.ChangeUpdateHandler += UpdateAvailableChange;
            _productManager.PassPricePurchasedHandler += _cashManager.DecreaseFundsOnPurchase;
            _cashManager.GetProductPriceCallback += _productManager.ReturnPriceByID;
            PassNotification += walletWindow.DispalyNotification;

            CreateProductGrid();
            walletWindow.Show();
            PurchaseButton.IsEnabled = false;
            _cashManager.UpdateAvailableFunds(0,true); //update FundsBox for a case of antecedent machine shutdown with money inserted
            #endregion
        }

        public void UpdateFunds(int cash)
        {
            FundsBox.Text = $"{cash} rub";
            UpdateProductAvailability(); // (1) calling method to check if each product is available after inserting cash
        }

        public void UpdateAvailableChange(int possibleChange)
        {
            ChangeBox.Text = $"Change: {possibleChange} rub";
            if (possibleChange == 0) { ChangeButton.IsEnabled = false; }
        }

        public void CreateProductGrid()
        {
            #region UIModification
            var repoList = _productManager.Repository.ProductStock;
            DynamicGrid.ColumnDefinitions.Add(new ColumnDefinition());
            DynamicGrid.ColumnDefinitions.Add(new ColumnDefinition());
            DynamicGrid.ColumnDefinitions.Add(new ColumnDefinition());
            DynamicGrid.ColumnDefinitions.Add(new ColumnDefinition()); // Number of columns is static and equals 4

            for (int rowCount = 1; 4 * rowCount <= repoList.Count + 3; rowCount++)
            {
                if (rowCount > 1) { ResizeWindow(DynamicGrid); }
                DynamicGrid.RowDefinitions.Add(new RowDefinition()); // Adding sufficient number of rows
            }
            
            var imageSource = new BitmapImage(new Uri(@"pack://application:,,,/ProductPic.png"));

            for (int i = 0; i < repoList.Count; i++)
            {
                string noSpaceName = new string(repoList[i].Name.ToCharArray().Where(c => !Char.IsWhiteSpace(c)).ToArray());

                TextBlock captionBlock = new TextBlock { Name = $"{noSpaceName}CaptionBlock", Text = repoList[i].Name, VerticalAlignment = VerticalAlignment.Top, Margin = new Thickness(0,8,0,0), HorizontalAlignment = HorizontalAlignment.Center };
                Grid.SetRow(captionBlock, i / 4);
                Grid.SetColumn(captionBlock, i % 4);
                DynamicGrid.Children.Add(captionBlock);

                TextBlock priceBlock = new TextBlock { Name = $"{noSpaceName}PriceBlock", Text = $"Price: {repoList[i].Price}", VerticalAlignment = VerticalAlignment.Top, Margin = new Thickness(0, 25, 0, 0), HorizontalAlignment = HorizontalAlignment.Center };
                Grid.SetRow(priceBlock, i / 4);
                Grid.SetColumn(priceBlock, i % 4);
                DynamicGrid.Children.Add(priceBlock);

                Button productButton = new Button { Name = $"{noSpaceName}Button", Content = "choose", Height = 22, Width = 75, VerticalAlignment = VerticalAlignment.Bottom, Margin = new Thickness(0, 0, 0, 10), HorizontalAlignment = HorizontalAlignment.Center };
                if (repoList[i].Quantity == 0) { productButton.Content = "Out of Stock"; }
                productButton.Click += ChooseButton_Click;
                productButton.Tag = repoList[i].ID; // button number is uniquely denfined by product ID in repository upon UI implementation
                Grid.SetRow(productButton, i / 4);
                Grid.SetColumn(productButton, i % 4);
                DynamicGrid.Children.Add(productButton);

                Image sandwichImage = new Image { Source = imageSource, Height = 60, Width = 60, VerticalAlignment = VerticalAlignment.Top, Margin = new Thickness(0, 45, 0, 0), HorizontalAlignment = HorizontalAlignment.Center };
                Grid.SetRow(sandwichImage, i / 4);
                Grid.SetColumn(sandwichImage, i % 4);
                DynamicGrid.Children.Add(sandwichImage);
                
                _leBoutons.Add(productButton);
            }
            #endregion
        }

        private void ChooseButton_Click(object sender, RoutedEventArgs e)
        {
            #region Actions
            Button button = sender as Button;
            _chosenProductID = (int)button.Tag;
            foreach(Button b in _leBoutons)
            {
                b.Content = "choose";
            }
            button.Content = "YES!";
            PurchaseButton.IsEnabled = true;
            #endregion
        }

        private void PurchaseButton_Click(object sender, RoutedEventArgs e)
        {
            #region Actions
            _productManager.PurchaseProduct(_chosenProductID);
            foreach (Button b in _leBoutons)
            {
                if ((int)b.Tag == _chosenProductID)
                {
                    b.Content = "choose";
                    if (_productManager.Repository.ProductStock[_chosenProductID-1].Quantity == 0)
                    {
                        b.Content = "Out of stock";
                    }
                }

            }
            PurchaseButton.IsEnabled = false;
            PassNotification?.Invoke($"You purchased {_productManager.Repository.ProductStock[_chosenProductID-1].Name}");
            ChangeButton.IsEnabled = true;
            #endregion
        }

        private void ChangeButton_Click(object sender, RoutedEventArgs e)
        {
            _cashManager.GiveChange();
            ChangeButton.IsEnabled = false;
        }

        private void UpdateProductAvailability()
        {
            #region Loop with Check
            foreach(Button b in _leBoutons)
            {
                if(ProductAvailability.Invoke((int)b.Tag) && (_productManager.Repository.ProductStock[(int)b.Tag-1].Quantity != 0)) // (2) passing ID of respective product for each button
                {
                    b.IsEnabled = true;
                }
                else
                {
                    b.IsEnabled = false;
                }
            }
            #endregion
        }

        private void ResizeWindow(UIElement dimensionSource)
        {
            ShopWindow.Height += dimensionSource.RenderSize.Height;
            MainGrid.Height += dimensionSource.RenderSize.Height;
            DynamicGrid.Height += dimensionSource.RenderSize.Height;
        }

        private void ShopWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Application.Current.Shutdown(); //Application shutdown on MainWindow closure
        }

        private void WalletWindowClosedNotification(object sender, EventArgs e)
        {
            (sender as Window).Closed -= WalletWindowClosedNotification;
            Notification.Visibility = Visibility.Visible;
        }

    }
}
