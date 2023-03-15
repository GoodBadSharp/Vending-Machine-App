using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VendingMachine
{
    public class ProductRepository
    {
        List<Sandwich> _productStock = new List<Sandwich>();

        public List<Sandwich> ProductStock
        {
            get { return _productStock; }
            set { _productStock = value; }
        }
    }
}
