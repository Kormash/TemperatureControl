﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Temperature_Control.Models;

namespace Temperature_Control
{
    class Program
    {
        static void Main(string[] args)
        {
            //DateTime date = new DateTime(2016,12,11);
            TopChanceMold();
        }

        void CreateDatabase()
        {
            using (var reader = new StreamReader(@"C:\Users\Erik\Desktop\DB Inlämmning\TemperaturData.csv"))
            {
                List<TempData> insideList = new List<TempData>();
                List<TempData> outsideList = new List<TempData>();


                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    var values = line.Split(',');

                    if (values[1].Equals("Inne"))
                    {
                        string temp = values[2].Replace('.', ',');

                        TempData tempData = new TempData(DateTime.Parse(values[0]), float.Parse(temp), int.Parse(values[3]));

                        insideList.Add(tempData);
                    }
                    else if (values[1].Equals("Ute"))
                    {
                        string temp = values[2].Replace('.', ',');

                        TempData tempData = new TempData(DateTime.Parse(values[0]), float.Parse(temp), int.Parse(values[3]));

                        outsideList.Add(tempData);
                    }

                }

                using (var context = new EFContext())
                {
                    foreach (TempData tempData in outsideList)
                    {
                        Outside toAdd = new Outside();
                        toAdd.Date = tempData.date;
                        toAdd.Moisture = tempData.moisture;
                        toAdd.Temperature = tempData.temperature;

                        context.Add(toAdd);
                    }

                    foreach (TempData tempData in insideList)
                    {
                        Inside toAdd = new Inside();
                        toAdd.Date = tempData.date;
                        toAdd.Moisture = tempData.moisture;
                        toAdd.Temperature = tempData.temperature;

                        context.Add(toAdd);
                    }

                    context.SaveChanges();
                }
            }
        }

        static void AverageTempOutside(DateTime date)
        {
            using (var context = new EFContext())
            {
                var query = context.Outside
                    .Where(x => x.Date.Date == date.Date)
                    .Average(x => x.Temperature);


                Console.WriteLine("Average Temperature Outside for " + date.Date + " is " + query);


                //Possible Bias where Many Measurements in one time of day, and few in another time.
            }
        }

        static void AverageTempInside(DateTime date)
        {
            using (var context = new EFContext())
            {
                var query = context.Inside
                    .Where(x => x.Date.Date == date.Date)
                    .Average(x => x.Temperature);


                Console.WriteLine("Average Temperature Inside for " + date.Date + " is " + query);


                //Possible Bias where Many Measurements in one time of day, and few in another time.
            }
        }

        static void TopTempOutside()
        {
            using (var context = new EFContext())
            {
                var query = context.Outside
                    .GroupBy(l => l.Date.Date)

                    .Select(cl => new 
                    {
                        Date = cl.Key,
                        AverageTemp = cl.Average(c => c.Temperature)
                    }).ToList().OrderByDescending(x => x.AverageTemp);

                foreach (var day in query)
                {
                    Console.WriteLine(day);
                }


            }
        }

        static void TopTempInside()
        {
            using (var context = new EFContext())
            {
                var query = context.Inside
                    .GroupBy(l => l.Date.Date)

                    .Select(cl => new
                    {
                        Date = cl.Key,
                        AverageTemp = cl.Average(c => c.Temperature)
                    }).ToList().OrderByDescending(x => x.AverageTemp);

                foreach (var day in query)
                {
                    Console.WriteLine(day);
                }


            }
        }

        static void TopChanceMold()
        {

            List<MouldData> MouldRiskList = new List<MouldData>();

            using (var context = new EFContext())
            {
                var query = context.Outside
                    .GroupBy(l => l.Date.Date)

                    .Select(cl => new
                    {
                        Date = cl.Key,
                        AverageTemp = cl.Average(c => c.Temperature),
                        AverageMold = cl.Average(c => c.Moisture),
                    }).ToList();


                foreach (var day in query)
                {
                    double RHCrit = 80;
                    double MouldIndex;
                    double Temperature = day.AverageTemp;
                    double RH = day.AverageMold;

                    if (Temperature <= 20)
                    {
                        RHCrit = -0.00267 * Math.Pow(Temperature, 3) + 0.160 * Math.Pow(Temperature, 2) - 3.13 * Temperature + 100;
                    }

                    if (Temperature <= 0 || Temperature >= 50)
                    {
                        MouldIndex = 0;
                    }
                    else
                    {
                        MouldIndex = 1 + 7 * ((RHCrit - RH) / (RHCrit - 100)) - 2 * Math.Pow(((RHCrit - RH) / (RHCrit - 100)), 2);
                        if(MouldIndex < 0)
                        {
                            MouldIndex = 0;
                        }
                    }

                    MouldData mouldData = new MouldData(day.Date, day.AverageTemp, day.AverageMold, MouldIndex);
                    MouldRiskList.Add(mouldData);
                }

                var q2 = MouldRiskList.OrderByDescending(m => m.MouldIndex).Take(10);

                foreach(var day in q2)
                {
                    Console.WriteLine(day.Date + " " + day.MouldIndex + " " + day.AverageTemperature + " " + day.AverageMoisture);
                }
            }

            



        }
    }

    public class TempData
    {
        public DateTime date;
        public float temperature;
        public int moisture;
        public TempData(DateTime dateTime, float temp, int moist)
        {
            date = dateTime;
            temperature = temp;
            moisture = moist;
        }
    }

    public class MouldData
    {
        public DateTime Date;
        public double AverageTemperature;
        public double AverageMoisture;
        public double MouldIndex;

        public MouldData(DateTime date, double avgTemp, double avgMoist, double mould)
        {
            Date = date;
            AverageTemperature = avgTemp;
            AverageMoisture = avgMoist;
            MouldIndex = mould;
        }
    }
}