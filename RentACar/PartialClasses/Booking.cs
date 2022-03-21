using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RentACar
{
    partial class Booking
    {
        //string override for popup confirmation
        public override string ToString()
        {
            return String.Format($"Car ID: {Car.Id}\n" +
                $"Make: {Car.Make}\n" +
                $"Model: {Car.Model}\n" +
                $"Rental Date: {StartDate.ToString("dd/MM/yyyy")}\n" +
                $"Return Date: {EndDate.ToString("dd/MM/yyyy")}");
        }
    }
}
