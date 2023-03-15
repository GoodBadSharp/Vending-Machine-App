using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VendingMachine
{
    public class Credit
    {
        int _denomination;
        int _quantity;
        bool _isCoin;

        public int Denomination
        {
            get { return _denomination; }
            set { _denomination = value; }
        }

        public int Quantity
        {
            get { return _quantity; }
            set { _quantity = value; }
        }

        public bool IsCoin
        {
            get { return _isCoin; }
            set { _isCoin = value; }
        }

        public Credit()
        {
            //parameterless construstor for serialization
        }

        public Credit(int denomination, int quantity)
        {
            Denomination = denomination;
            Quantity = quantity;
            if (denomination > 10)
            { IsCoin = false; }
            else
            { IsCoin = true; }
        }
    }
}
