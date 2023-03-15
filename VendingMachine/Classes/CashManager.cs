using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Xml.Serialization;

namespace VendingMachine
{
    public class CashManager
    {
        public Action<int> CashUpdateHandler;
        public Action<int> ChangeUpdateHandler;
        public event Func<int, int> GetProductPriceCallback;

        IOmanager<CashRepository> _dataManager = new IOmanager<CashRepository>();
        CashRepository _repository = new CashRepository();
        int[] _preparedChange = new int[4];
        
        public CashRepository Repository
        {
            get { return _repository; }
        }

        public int[] PreparedChange
        {
            get { return _preparedChange; }
            set { _preparedChange = value; }
        }

        public CashManager()
        {
            try
            {
                _repository = _dataManager.DerserizlizeData();
                _repository.CashStock.Sort((credit1, credit2) => credit1.Denomination.CompareTo(credit2.Denomination)); //sorting in ascending order
                _dataManager.SerializeData(_repository);
            }
            catch
            {
                System.Windows.MessageBox.Show("Error loading cash repository! Change may be unavailable!", "Error", System.Windows.MessageBoxButton.OK);
            }
        }

        public void UpdateAvailableFunds (int credit, bool isAdded)
        {
            if (isAdded)
            {
                _repository.AvailableFunds += credit;
                _repository.AddCredit(credit);
                _dataManager.SerializeData(_repository);
            }
            else
            {
                _repository.AvailableFunds -= credit;
                _dataManager.SerializeData(_repository);
            }
            CashUpdateHandler?.Invoke(_repository.AvailableFunds);
            _dataManager.SerializeData(_repository);
        }

        public void DecreaseFundsOnPurchase(int purchasedProductPrice)
        {
            _repository.AvailableFunds -= purchasedProductPrice;
            CashUpdateHandler?.Invoke(_repository.AvailableFunds); // to ShopWindow.UpdateFunds
            _dataManager.SerializeData(_repository);
            PrepareChange(_repository.AvailableFunds);
        }


        // sorting algorithm assumes that that there are 4 types of coins (denominations of 1, 2, 5, 10 rub) available,
        // given CashRepository was sorted in ascending order (see CashManager's constructor)
        // [0] - 1 rub, [1] - 2 rub, [2] - 5 rub, [3] - 10 rub
        private void PrepareChange(int neccesaryChange)
        {
            #region Calculations
            int possibleChange = 0;
            int[] requiredNumberOfCoins = new int[4] { 0, 0, 0, 0 };
            var r = Repository.CashStock.ConvertAll(credit => new Credit(credit.Denomination, credit.Quantity)); // creating deep copy for independent analysis

            if (neccesaryChange != 0)
            {
                int fiveUsed = 0;

                while ((r[3].Quantity != 0) && (possibleChange + 10 <= neccesaryChange))
                {
                    possibleChange += 10;
                    r[3].Quantity--;
                    requiredNumberOfCoins[3]++;
                }

                while ((r[2].Quantity != 0) && (possibleChange + 5 <= neccesaryChange))
                {
                    possibleChange += 5;
                    r[2].Quantity--;
                    requiredNumberOfCoins[2]++;
                    fiveUsed++;
                }

                while ((r[1].Quantity != 0) && (possibleChange + 2 <= neccesaryChange))
                {
                    possibleChange += 2;
                    r[1].Quantity--;
                    requiredNumberOfCoins[1]++;
                }

                while ((r[0].Quantity != 0) && (possibleChange + 1 <= neccesaryChange))
                {
                    possibleChange += 1;
                    r[0].Quantity--;
                    requiredNumberOfCoins[0]++;
                }

                // the case when when 5 rub coin was not used and 10 rub coins caused odd residue, which prevented the use of 2 rub coins
                if (possibleChange != neccesaryChange && fiveUsed == 0)
                {
                    possibleChange = 0;
                    requiredNumberOfCoins = new int[4] { 0, 0, 0, 0 };
                    r = Repository.CashStock.ConvertAll(credit => new Credit(credit.Denomination, credit.Quantity));

                    while ((r[3].Quantity != 0) && (possibleChange + 20 <= neccesaryChange))  // using 10 rub coin one time less
                    {
                        possibleChange += 10;
                        r[3].Quantity--;
                        requiredNumberOfCoins[3]++;
                    }

                    possibleChange += 5;    // using 5 rub coin once
                    r[2].Quantity--;
                    requiredNumberOfCoins[2]++;

                    while ((r[1].Quantity != 0) && (possibleChange + 2 <= neccesaryChange))
                    {
                        possibleChange += 2;
                        r[1].Quantity--;
                        requiredNumberOfCoins[1]++;
                    }

                    while ((r[0].Quantity != 0) && (possibleChange + 1 <= neccesaryChange))
                    {
                        possibleChange += 1;
                        r[0].Quantity--;
                        requiredNumberOfCoins[0]++;
                    }
                }

                // the case when odd denomination of 5 rub coin somehow prevented the use of 2 rub coins
                else
                {
                    possibleChange = 0;
                    requiredNumberOfCoins = new int[4] { 0, 0, 0, 0 };
                    r = Repository.CashStock.ConvertAll(credit => new Credit(credit.Denomination, credit.Quantity));

                    while ((r[3].Quantity != 0) && (possibleChange + 10 <= neccesaryChange))
                    {
                        possibleChange += 10;
                        r[3].Quantity--;
                        requiredNumberOfCoins[3]++;
                    }

                    while ((r[2].Quantity != 0) && (possibleChange + 10 <= neccesaryChange)) // using 5 rub coin one time less
                    {
                        possibleChange += 5;
                        r[2].Quantity--;
                        requiredNumberOfCoins[2]++;
                    }

                    while ((r[1].Quantity != 0) && (possibleChange + 2 <= neccesaryChange))
                    {
                        possibleChange += 2;
                        r[1].Quantity--;
                        requiredNumberOfCoins[1]++;
                    }

                    while ((r[0].Quantity != 0) && (possibleChange + 1 <= neccesaryChange))
                    {
                        possibleChange += 1;
                        r[0].Quantity--;
                        requiredNumberOfCoins[0]++;
                    }
                }
            }
            #endregion
            _preparedChange = requiredNumberOfCoins;
            ChangeUpdateHandler?.Invoke(neccesaryChange); // to ShopWindow.UpdateAvailableChange
        }

        public void GiveChange()
        {
            for (int i=0; i<4; i++)
            {
                _repository.CashStock[i].Quantity -= _preparedChange[i];
                _preparedChange[i] = 0;
            }
            _repository.AvailableFunds = 0;
            CashUpdateHandler?.Invoke(0); // to ShopWindow.UpdateFunds
            ChangeUpdateHandler?.Invoke(0); // to ShopWindow.UpdateAvailableChange
            _dataManager.SerializeData(_repository);
        }

        public bool CheckProductAvailability(int productID)
        {
            int price = GetProductPriceCallback.Invoke(productID); // to _productManager.ReturnPriceByID

            if ((price <= Repository.AvailableFunds) && (ChangeCalculationAlgorithm(Repository.AvailableFunds - price) >= 0))
            {
                return true;
            }
            else
            {
                return false;
            }

        }

        // sorting algorithm assumes that that there are 4 types of coins (denominations of 1, 2, 5, 10 rub) available,
        // given CashRepository was sorted in ascending order (see CashManager's constructor)
        // [0] - 1 rub, [1] - 2 rub, [2] - 5 rub, [3] - 10 rub
        public int ChangeCalculationAlgorithm(int neccesaryChange) 
        {
            #region Calculations
            int possibleChange = 0;
            var r = Repository.CashStock.ConvertAll(credit => new Credit(credit.Denomination, credit.Quantity)); // creating deep copy for independent analysis

            if (neccesaryChange != 0)
            {
                int fiveUsed = 0;
                while ((r[3].Quantity != 0) && (possibleChange + 10 <= neccesaryChange))
                {
                    possibleChange += 10;
                    r[3].Quantity--;
                }

                while ((r[2].Quantity != 0) && (possibleChange + 5 <= neccesaryChange))
                {
                    possibleChange += 5;
                    r[2].Quantity--;
                    fiveUsed++;
                }

                while ((r[1].Quantity != 0) && (possibleChange + 2 <= neccesaryChange))
                {
                    possibleChange += 2;
                    r[1].Quantity--;
                }

                while ((r[0].Quantity != 0) && (possibleChange + 1 <= neccesaryChange))
                {
                    possibleChange += 1;
                    r[0].Quantity--;
                }
                r = Repository.CashStock.ConvertAll(credit => new Credit(credit.Denomination, credit.Quantity));

                if (possibleChange == neccesaryChange)
                {
                    return possibleChange;
                }
                // in case odd denomination of 5 rub coin somehow prevented the use of 2 rub coins
                else if (r[1].Quantity != 0 && fiveUsed == 0 && r[2].Quantity != 0)
                {
                    possibleChange = 0;

                    while ((r[3].Quantity != 0) && (possibleChange + 20 <= neccesaryChange)) // using 10 rub coin one time less
                    {
                        possibleChange += 10;
                        r[3].Quantity--;
                    }

                    possibleChange += 5; // using 5 rub coin once
                    r[2].Quantity--;

                    while ((r[1].Quantity != 0) && (possibleChange + 2 <= neccesaryChange))
                    {
                        possibleChange += 2;
                        r[1].Quantity--;
                    }

                    while ((r[0].Quantity != 0) && (possibleChange + 1 <= neccesaryChange))
                    {
                        possibleChange += 1;
                        r[0].Quantity--;
                    }
                    r = Repository.CashStock.ConvertAll(credit => new Credit(credit.Denomination, credit.Quantity));

                    if (possibleChange == neccesaryChange)
                    {
                        return possibleChange;
                    }
                    else { return -1; }
                }
                // in case odd denomination of 5 rub coin somehow prevented the use of 2 rub coins (another case)
                else if (r[1].Quantity != 0 && fiveUsed > 0)
                {
                    possibleChange = 0;

                    while ((r[3].Quantity != 0) && (possibleChange + 10 <= neccesaryChange))
                    {
                        possibleChange += 10;
                        r[3].Quantity--;
                    }

                    while ((r[2].Quantity != 0) && (possibleChange + 10 <= neccesaryChange)) // using 5 rub coin one time less
                    {
                        possibleChange += 5;
                        r[2].Quantity--;
                    }

                    while ((r[1].Quantity != 0) && (possibleChange + 2 <= neccesaryChange))
                    {
                        possibleChange += 2;
                        r[1].Quantity--;
                    }

                    while ((r[0].Quantity != 0) && (possibleChange + 1 <= neccesaryChange))
                    {
                        possibleChange += 1;
                        r[0].Quantity--;
                    }

                    if (possibleChange == neccesaryChange)
                    {
                        return possibleChange;
                    }
                    else { return -1; }
                }
                else { return -1; }
            }
            { return 0; }
            #endregion
        }


    }
}
