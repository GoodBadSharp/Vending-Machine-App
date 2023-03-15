using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VendingMachine
{
    public class CashRepository
    {
        int _availableFunds;
        List<Credit> _cashStock = new List<Credit>();
        //List<int> _possibleChanges = new List<int>();
        
        public int AvailableFunds
        {
            get { return _availableFunds; }
            set { _availableFunds = value; }
        }

        public List<Credit> CashStock
        {
            get { return _cashStock; }
            set { _cashStock = value; }
        }

        //public List<int> PossibleChanges
        //{
        //    get { return _possibleChanges; }
        //    set { _possibleChanges = value; }
        //}

        public void AddCredit(int credit)
        {
            foreach (var cred in CashStock)
            {
                if (credit == cred.Denomination)
                {
                    cred.Quantity++;
                    break;
                }
            }
        }

    }
}
