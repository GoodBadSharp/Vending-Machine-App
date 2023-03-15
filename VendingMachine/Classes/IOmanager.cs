using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Xml.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace VendingMachine
{
     class IOmanager<T> where T: class
    {
        public void SerializeData(T genericRepository)
        {
            string className = typeof(T).Name;
            using (var stream = new FileStream($"..//..//{className}Data.xml", FileMode.Create, FileAccess.Write))
            {
                XmlSerializer xml = new XmlSerializer(typeof(T));
                xml.Serialize(stream, genericRepository);
            }
        }

        public T DerserizlizeData()
        {
            string className = typeof(T).Name;
            using (var stream = new FileStream($"..//..//{className}Data.xml", FileMode.Open, FileAccess.Read))
            {
                XmlSerializer xml = new XmlSerializer(typeof(T));
                return (T)xml.Deserialize(stream);
            }
        }

        // Algorithms for creating initial XML data files from raw txt files

        //using (StreamReader stream = new StreamReader(@"../../IntialCashStock.txt"))
        //{
        //    string line;
        //    string[] buffer;
        //    while ((line = stream.ReadLine()) != null)
        //    {
        //        buffer = line.Split(' ');
        //        _repository.CashStock.Add(new Credit(int.Parse(buffer[0]), int.Parse(buffer[1])));
        //    }
        //}
        //_dataManager.SerializeData(_repository);

        //using (StreamReader stream = new StreamReader(@"../../InitialSandwichStock.txt"))
        //{
        //    string line;
        //    string[] buffer;
        //    while ((line = stream.ReadLine()) != null)
        //    {
        //        buffer = line.Split(' ');
        //        _repository.ProductStock.Add(new Sandwich($"{buffer[0]} {buffer[1]}", int.Parse(buffer[2]), int.Parse(buffer[3])));
        //    }
        //}
        //_dataManager.SerializeData(_repository);
    }
}
