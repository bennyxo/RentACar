using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

namespace RentACar
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //create database connection
        RentACarEntities db = new RentACarEntities();

        ObservableCollection<Car> result = new ObservableCollection<Car>();

        public MainWindow()
        {
            InitializeComponent();
        }

        #region Window Load
        private void Window_Loaded(object sender, RoutedEventArgs e)
        { 
            //populate Car Type combobox
            cbxCarType.ItemsSource = Enum.GetNames(typeof(CarSizes));
        }
        #endregion

        #region Search Button Click
        private void btnSeach_Click(object sender, RoutedEventArgs e)
        {
            string size, errorMessage = "";
            DateTime startDate, endDate;

            //clear observable collection
            result.Clear();

            //error checking

            //check if Car Type has been selected
            if (cbxCarType.SelectedIndex < 0)
            {
                errorMessage = "Please select a car type";
            }

            //check start date is selected
            if (dpStartDate.SelectedDate == null)
            {
                if (String.IsNullOrEmpty(errorMessage))
                {
                    errorMessage = "Please select a start date";
                }

                else
                {
                    errorMessage += "\nPlease select a start date";
                }
            }

            //check start date is at least today
            if ((dpStartDate.SelectedDate != null) && (dpStartDate.SelectedDate.Value <= DateTime.Today))
            {
                if (String.IsNullOrEmpty(errorMessage))
                {
                    errorMessage = "Please select a start date after today";
                }

                else
                {
                    errorMessage += "\nPlease select a start date after today";
                }
            }

            //check end date enterred
            if (dpEndDate.SelectedDate == null)
            {
                if (String.IsNullOrEmpty(errorMessage))
                {
                    errorMessage = "Please select an end date";
                }

                else
                {
                    errorMessage += "\nPlease select an end date";
                }
            }

            //check end date is greater or equal to start date
            if ((dpEndDate.SelectedDate != null) && (dpEndDate.SelectedDate.Value <= dpStartDate.SelectedDate.Value))
            {
                if (String.IsNullOrEmpty(errorMessage))
                {
                    errorMessage = "Please select an end date greater than start date";
                }

                else
                {
                    errorMessage += "\nPlease select an end date greater than start date";
                }
            }

            //check if error and display if so
            if (!String.IsNullOrEmpty(errorMessage))
            {
                MessageBox.Show(errorMessage, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            //check availability if no errors
            else
            {

                size = cbxCarType.SelectedItem.ToString();
                startDate = dpStartDate.SelectedDate.Value;
                endDate = dpEndDate.SelectedDate.Value;

                CheckAvailability(size, startDate, endDate);
            }

        }
        #endregion

        #region Available Cars Selection Changed
        private void lbxAvailableCars_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string display = "";
            Car selectedCar = lbxAvailableCars.SelectedItem as Car;
            DateTime startDate = dpStartDate.SelectedDate.Value;
            DateTime endDate = dpStartDate.SelectedDate.Value;

            //check if a car is selected
            if (lbxAvailableCars.SelectedItem != null)
            {
                //update Selected Car message
                display = string.Format(selectedCar.GetCarDetails() + "\n" +
                    $"Rental Date: {startDate.ToString("dd/MM/yyyy")}\n" +
                    $"Return Date: {endDate.ToString("dd/MM/yyyy")}\n");

                //unhide 'Selected Car' textblock & 
                tblkSelectedCar.Visibility = Visibility.Visible;
                tblkBookingInfo.Visibility = Visibility.Visible;
                tblkBookingInfo.Text = display;

                //display image of selected car
                DisplayCarImage(selectedCar.Id);
            }

            else
            {
                //hide textblocks & image if no car seelcted
                imgCar.Visibility = Visibility.Hidden;
                tblkBookingInfo.Visibility = Visibility.Hidden;
                tblkSelectedCar.Visibility = Visibility.Hidden;
            }
        }

        #endregion

        #region Book Button Click
        private void btnBook_Click(object sender, RoutedEventArgs e)
        {
            //declare variables
            DateTime startDate, endDate;
            Car selectedCar;
            int carId;
            string errorMessage = "";

            //error checking

            //check if start date entered
            if (dpStartDate.SelectedDate == null)
            {
                errorMessage = "Please select a start date";
            }
            //check end date enterred
            if (dpEndDate.SelectedDate == null)
            {
                if (String.IsNullOrEmpty(errorMessage))
                {
                    errorMessage = "Please select an end date";
                }

                else
                {
                    errorMessage += "\nPlease select an end date";
                }
            }
            //check car has been selected
            if (lbxAvailableCars.SelectedItem == null)
            {
                if (String.IsNullOrEmpty(errorMessage))
                {
                    errorMessage = "Please select a car to book";
                }
                else
                {
                    errorMessage += "\nPlease select a car to book";
                }
            }

            //check if there is an error message, and display if so
            if (!String.IsNullOrEmpty(errorMessage))
            {
                MessageBox.Show(errorMessage, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else
            {
                selectedCar = lbxAvailableCars.SelectedItem as Car;
                startDate = dpStartDate.SelectedDate.Value;
                endDate = dpEndDate.SelectedDate.Value;
                carId = selectedCar.Id;

                Booking newBooking = new Booking
                {
                    Car = selectedCar,
                    StartDate = startDate,
                    EndDate = endDate,
                    CarId = carId
                };

                //add booking to DB
                db.Bookings.Add(newBooking);
                //save DB changes
                db.SaveChanges();

                //reset fields
                ResetFields();

                //display confirmation message
                MessageBox.Show(newBooking.ToString(), "Booking Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        #endregion

        //methods

        #region CheckAvailability Method
        private void CheckAvailability(string size, DateTime startDate, DateTime endDate)
        {
            var carsBooked = from b in db.Bookings
                             where
                               ((startDate >= b.StartDate) && (startDate <= b.EndDate)) ||
                               ((endDate >= b.StartDate) && (endDate <= b.EndDate)) ||
                               ((startDate <= b.StartDate) && (endDate >= b.StartDate) && (endDate <= b.EndDate)) ||
                               ((startDate >= b.StartDate) && (startDate <= b.EndDate) && (endDate >= b.EndDate)) ||
                               ((startDate <= b.StartDate) && (endDate >= b.EndDate))
                             select b;

            var availableCars = db.Cars.Where(c => !carsBooked.Any(b => b.CarId == c.Id));
            foreach (var c in availableCars)
            {
                
                if (c.Size == size)
                {
                    result.Add(c);
                }

            }
            lbxAvailableCars.ItemsSource = result;
        }
        #endregion

        #region ResetFields Method

        private void ResetFields()
        {
            result.Clear();
            cbxCarType.SelectedIndex = -1;
            dpStartDate.SelectedDate = null;
            dpEndDate.SelectedDate = null;
            tblkBookingInfo.Visibility = Visibility.Hidden;
            tblkSelectedCar.Visibility = Visibility.Hidden;


        }
        #endregion

        #region DisplayCarImage Method

        public void DisplayCarImage(int id)
        {
            switch (id)
            {
                case 1:
                    {
                        imgCar.Source = new BitmapImage(new Uri("pack://application:,,,/images/1.png"));
                        imgCar.Visibility = Visibility.Visible;
                        break;
                    }
                case 2:
                    {
                        imgCar.Source = new BitmapImage(new Uri("pack://application:,,,/images/2.png"));
                        imgCar.Visibility = Visibility.Visible;
                        break;
                    }
                case 3:
                    {
                        imgCar.Source = new BitmapImage(new Uri("pack://application:,,,/images/3.png"));
                        imgCar.Visibility = Visibility.Visible;
                        break;
                    }
                case 4:
                    {
                        imgCar.Source = new BitmapImage(new Uri("pack://application:,,,/images/4.png"));
                        imgCar.Visibility = Visibility.Visible;
                        break;
                    }
                case 5:
                    {
                        imgCar.Source = new BitmapImage(new Uri("pack://application:,,,/images/5.png"));
                        imgCar.Visibility = Visibility.Visible;
                        break;
                    }
                case 6:
                    {
                        imgCar.Source = new BitmapImage(new Uri("pack://application:,,,/images/6.png"));
                        imgCar.Visibility = Visibility.Visible;
                        break;
                    }
                case 7:
                    {
                        imgCar.Source = new BitmapImage(new Uri("pack://application:,,,/images/7.png"));
                        imgCar.Visibility = Visibility.Visible;
                        break;
                    }
                case 8:
                    {
                        imgCar.Source = new BitmapImage(new Uri("pack://application:,,,/images/8.png"));
                        imgCar.Visibility = Visibility.Visible;
                        break;
                    }
                case 9:
                    {
                        imgCar.Source = new BitmapImage(new Uri("pack://application:,,,/images/9.png"));
                        imgCar.Visibility = Visibility.Visible;
                        break;
                    }
                case 10:
                    {
                        imgCar.Source = new BitmapImage(new Uri("pack://application:,,,/images/10.png"));
                        imgCar.Visibility = Visibility.Visible;
                        break;
                    }

                default:
                    break;
            }
        }

        #endregion


    }//end class

    //enum for car size
    public enum CarSizes { Small, Medium, Large, SUV };
}