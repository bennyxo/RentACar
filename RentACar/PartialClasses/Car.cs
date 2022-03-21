using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RentACar
{
    partial class Car
    {

        public string GetCarDetails()
        {
            return string.Format($"Car ID: {Id}\n" +
                    $"Make: {Make}\n" +
                    $"Model: {Model}");
        }

        public override string ToString()
        {
            return String.Format("{0} - {1}", Make, Model);
        }
    }
}
