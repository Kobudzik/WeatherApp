﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
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
using System.Xml.Linq;

namespace WeatherApp.Model
{
    public class WeatherGetter : INotifyPropertyChanged
    ///<remarks>
    //////API facade- this class translates user input to API request and also convert recived data into XML and then LIST
    ///</remarks>

    {
        /// <summary>
        /// holds url that will be generated by CreateHTTPRequest method
        /// </summary>
        string URL;

        /// <summary>
        /// holds XML data got from API request
        /// </summary>
        XDocument doc;

        /// <summary>
        /// holds data converted from XML (doc file) into a Llist form
        /// </summary>
        public List<DayWeather> daysList = new List<DayWeather>();
      
       
        
        /// <summary>
        /// used to stop working if something wrong happened, also used for displaing returned country and location from API
        /// </summary>
        bool _errorOccured = true;
        public bool ErrorOccured
        {
            get { return _errorOccured; }
            set
            {
                _errorOccured = value;
                OnPropertyChanged(nameof(ErrorOccured));
            }
        }

        string _returnedCountry;

        public string ReturnedCity
        {
            get { return _returnedCity; }
            set
            {
                _returnedCity = value;
                OnPropertyChanged(nameof(ReturnedCity));
            }
        }

        string _returnedCity;

        public string ReturnedCountry
        {
            get { return _returnedCountry; }
            set
            {
                _returnedCountry = value;
                OnPropertyChanged(nameof(ReturnedCountry));
            }
        }


        string _city;
        /// <summary>
        /// City entered by user
        /// </summary>
        public string City
        {
            get { return _city; }
            set
            {
                _city = value;
            }
        }

       
        string _days;
        /// <summary>
        /// Days entered by user
        /// </summary>
        public string Days
        {
            get { return _days; }
            set
            {
                _days = value;
            }
        }



        /// <summary>
        /// restore default app state
        /// </summary>
        public void Reset()
        {
            ErrorOccured = true;
            daysList.Clear();

        }

        /// <summary>
        /// generates API request using user given parameters
        /// </summary>
        public void CreateHTTPRequestURL()
        {
           
            if(City.Equals("") || Days.Equals(""))
            {
                MessageBox.Show("You must provide both values!", "ERROR!");
                ErrorOccured = true;
            }
            else if (!Regex.IsMatch(City, @"^[a-zA-Z]+$"))
            {
                MessageBox.Show("Use only latin letters in City field!", "ERROR!");
                ErrorOccured = true;
            }
            else
            { 
                URL = string.Format(@"http://api.openweathermap.org/data/2.5/forecast?q="+ City + "&mode=xml&appid=2517431d46cd54e4f965409583890e1c&cnt=" + Days + "&units=metric");
                ErrorOccured = false;
            }
        }

        /// <summary>
        /// creates XML data (from html file) recived from API
        /// </summary>
        public void GetXMLData()
        {
            if(URL==null && ErrorOccured == false)
            {
               MessageBox.Show("Empty URL passed!");
               ErrorOccured = true;

            }
            else if (ErrorOccured == false)
            { 
                string html = string.Empty;
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(URL);

                try
                {
                    using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                    using (Stream stream = response.GetResponseStream())

                    using (StreamReader reader = new StreamReader(stream))
                    {
                        html = reader.ReadToEnd();
                    }
                }
                catch(System.Net.WebException e)
                {
                    MessageBox.Show("MODEL- WEB: "+ e.Message,"ERROR!");
                    ErrorOccured = true;
                }

                     
                try
                {
                    doc= XDocument.Parse(html);
                }

                catch ( System.Xml.XmlException e)
                {
                    if(!ErrorOccured)
                        MessageBox.Show("MODEL- XML: "+ e.Message, "ERROR!");
                }
              
            }            
        }

        /// <summary>
        /// converts XML data into anonymous class- holds TemperatureValue, Clouds, Humidity
        /// then creates dayweather objects from anonymous class
        /// </summary>
        /// <returns></returns>
        public List<DayWeather> PopulateDayWeatherList()
        {
            if (ErrorOccured == false)
            {
                try
                {
                    //XML data to linq querry anonymous class
                    var days =
                        from day
                        in doc.Descendants("time")
                        select new
                        {
                            TemperatureValue = day.Element("temperature").Attribute("value").Value,
                            Clouds = day.Element("clouds").Attribute("value").Value,
                            Humidity = day.Element("humidity").Attribute("value").Value
                        };

                    int i = 0;
                    DateTime dateNow = DateTime.Now;
                    
                    //linq to list of DayWeather objects            
                    foreach (var day in days)
                    {
                        daysList.Add(new DayWeather
                            (
                            Convert.ToDouble(day.TemperatureValue, System.Globalization.CultureInfo.InvariantCulture),
                            day.Clouds,
                            Convert.ToInt32(day.Humidity, System.Globalization.CultureInfo.InvariantCulture),
                            dateNow.AddDays(i).Date));
                        i++;
                            
                    }
                    return daysList;
                }
                catch (Exception e)
                {
                    MessageBox.Show("List populating error: " + e.Message, "ERROR!");
                }
            }      
            return null;          
        }

        /// <summary>
        /// Writes ReturnedCity and ReturnedCountry- to show user which location is returned
        /// </summary>
        public void GetReturnedLocation()
        {
            if (ErrorOccured == false)
            {
                try
                {
                    //XML data to linq querry anonymous class
                    var loc =
                        from location
                        in doc.Descendants("location")
                        select new
                        {
                            City = location.Element("name").Value,
                            Country = location.Element("country").Value
                        };

                    ReturnedCity=  loc.First().City;
                    ReturnedCountry= loc.First().Country;
                }
                catch (Exception e)
                {
                    MessageBox.Show("Getting returned Location error" + e.Message, "ERROR!");
                }

            }
        }

        /// <summary>
        /// Used for data refreshing
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
