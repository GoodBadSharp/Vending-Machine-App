using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace VendingMachine
{
    public class ProductManager
    {

        IOmanager<ProductRepository> _dataManager = new IOmanager<ProductRepository>();
        ProductRepository _repository = new ProductRepository();
        public event Action<int> PassPricePurchasedHandler;
        bool _dataRetrieved = true;

        public ProductRepository Repository
        {
            get { return _repository; }
        }

        public bool DataRetieved
        {
            get { return _dataRetrieved; }
            set { _dataRetrieved = value; }
        }


        public ProductManager()
        {
            try
            {
                _repository = _dataManager.DerserizlizeData();
            }
            catch
            {
                System.Windows.MessageBox.Show("Error loading products data! No items available!", "Error", System.Windows.MessageBoxButton.OK);
                _dataRetrieved = false;
            }
            
        }

        public void PurchaseProduct(int productNumber)
        {
            Repository.ProductStock[productNumber-1].Quantity--;
            _dataManager.SerializeData(_repository);
            PassPricePurchasedHandler?.Invoke(_repository.ProductStock[productNumber-1].Price); // to _cashManager.DecreaseFundsOnPurchase
        }

        public int ReturnPriceByID(int passedProductID)
        {
            int possiblePrice = 0;
            foreach (var product in _repository.ProductStock)
            {
                if (product.ID == passedProductID)
                {
                    possiblePrice = product.Price;
                    break;
                }
            }
            return possiblePrice;
        }



    }
}
