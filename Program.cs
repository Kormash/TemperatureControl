using System;
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
            StartConsole();
        }

        static void StartConsole()
        {
            Console.WriteLine("Welcome to Weather Control. What would you like to do?");
            Console.WriteLine();
            Console.WriteLine("1: Average Temperature");
            Console.WriteLine("2: Warmest/Coldest Day");
            Console.WriteLine("3: Least/Most Moist Day");
            Console.WriteLine("4: Day with Lowest/Highest Mold Risk");
            Console.WriteLine("5: Date of Autumn");
            Console.WriteLine("6: Date of Winter");
            Console.WriteLine("7: Time of Door Open");
            Console.WriteLine("8: Biggest/Smallest Difference in Temperature");

            bool loop = true;
            while (loop)
            {
                var input = Console.ReadKey();
                int select;
                try
                {
                    select = int.Parse(input.KeyChar.ToString());
                }
                catch(Exception e)
                {
                    select = 0;
                }
                loop = false;
                switch (select)
                {
                    case 1:
                        //1: Average Temperature
                        AverageTemp();
                        break;
                    case 2:
                        //2: Warmest/Coldest Day
                        TopTemp();
                        break;
                    case 3:
                        //3: Least/Most Moist Day
                        TopMoist();
                        break;
                    case 4:
                        //4: Day with Lowest/Highest Mold Risk
                        TopChanceMold();
                        break;
                    case 5:
                        //5: Date of Autumn
                        AutumnDate();
                        break;
                    case 6:
                        //6: Date of Winter
                        WinterDate();
                        break;
                    case 7:
                        //7: Time of Door Open
                        DoorOpen();
                        break;
                    case 8:
                        //8: Biggest/Smallest Difference in Temperature
                        TopDiff();
                        break;
                    default:
                        loop = true;
                        break;  
                }
            }
        }

        static void LoopConsole()
        {
            Console.WriteLine();
            Console.WriteLine("Press any Key to return to main menu.");
            Console.ReadKey();
            Console.Clear();
            StartConsole();
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

        //1: Average Temperature
        static void AverageTemp()
        {
            Console.Clear();
            DateTime date = new DateTime();

            bool loop = true;
            while (loop)
            {
                Console.WriteLine("What day?: (YYYY-MM-DD)");
                DateTime userDateTime;
                if (DateTime.TryParse(Console.ReadLine(), out userDateTime))
                {
                    loop = false;
                    date = userDateTime;
                }
                else
                {
                    Console.Clear();
                    Console.WriteLine("Not a Valid Date.");
                }
            }

            loop = true;
            while (loop)
            {
                Console.Clear();
                Console.WriteLine("Inside(1) or Outside(2)?");
                var input = Console.ReadKey();
                int select;
                try
                {
                    select = int.Parse(input.KeyChar.ToString());
                }
                catch (Exception e)
                {
                    select = 0;
                }
                loop = false;
                switch (select)
                {
                    case 1:
                        //1: Inside
                        AverageTempInside(date);
                        break;
                    case 2:
                        //2: Outside
                        AverageTempOutside(date);
                        break;
                    default:
                        loop = true;
                        break;

                }
            }
            
        }

        static void AverageTempOutside(DateTime date)
        {
            using (var context = new EFContext())
            {
                try
                {
                    var query = context.Outside
                                        .Where(x => x.Date.Date == date.Date)
                                        .Average(x => x.Temperature);

                    Console.Clear();
                    Console.WriteLine("Average Temperature Inside for " + date.Date.ToString("dd/MM/yyyy") + " is " + Math.Round(query, 1) + " degrees");


                    //Possible Bias where Many Measurements in one time of day, and few in another time.
                }
                catch (Exception e)
                {
                    Console.Clear();
                    Console.WriteLine("No data exsists for this date, try another.");
                }
                LoopConsole();
            }
        }

        static void AverageTempInside(DateTime date)
        {
            using (var context = new EFContext())
            {
                try
                {
                    var query = context.Inside
                                        .Where(x => x.Date.Date == date.Date)
                                        .Average(x => x.Temperature);

                    Console.Clear();
                    Console.WriteLine("Average Temperature Inside for " + date.Date.ToString("dd/MM/yyyy") + " is " + Math.Round(query,1) + " degrees");


                    //Possible Bias where Many Measurements in one time of day, and few in another time.
                }
                catch(Exception e)
                {
                    Console.Clear();
                    Console.WriteLine("No data exsists for this date, try another.");
                }
                LoopConsole();
            }
        }

        //2: Warmest/Coldest Day
        static void TopTemp()
        {
            bool loop = true;
            bool highest = true;
            while (loop)
            {
                Console.Clear();
                Console.WriteLine("Highest(1) or Lowest(2) Temperature?");
                var input = Console.ReadKey();
                int select;
                try
                {
                    select = int.Parse(input.KeyChar.ToString());
                }
                catch (Exception e)
                {
                    select = 0;
                }
                loop = false;
                
                switch (select)
                {
                    case 1:
                        //1: Highest
                        highest = true;
                        break;
                    case 2:
                        //2: Lowest
                        highest = false;
                        break;
                    default:
                        loop = true;
                        break;

                }
            }


            loop = true;
            while (loop)
            {
                Console.Clear();
                Console.WriteLine("Inside(1) or Outside(2)?");
                var input = Console.ReadKey();
                int select;
                try
                {
                    select = int.Parse(input.KeyChar.ToString());
                }
                catch(Exception e)
                {
                    select = 0;
                }
                
                loop = false;
                switch (select)
                {
                    case 1:
                        //1: Inside
                        TopTempInside(highest);
                        break;
                    case 2:
                        //2: Outside
                        TopTempOutside(highest);
                        break;
                    default:
                        loop = true;
                        break;

                }
            }
        }

        static void TopTempOutside(bool highest)
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

                var list = query.Take(10);
                if (!highest)
                {
                    list = query.OrderBy(x => x.AverageTemp).Take(10);
                }
                Console.Clear();
                foreach (var day in list)
                {
                    Console.WriteLine(day.Date.ToString("dd/MM/yyyy") + ": " + Math.Round(day.AverageTemp,1) + " Degrees");
                }
                LoopConsole();
            }
        }

        static void TopTempInside(bool highest)
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

                var list = query.Take(10);
                if (!highest)
                {
                    list = query.OrderBy(x => x.AverageTemp).Take(10);
                }
                Console.Clear();
                foreach (var day in list)
                {
                    Console.WriteLine(day.Date.ToString("dd/MM/yyyy") + ": " + Math.Round(day.AverageTemp, 1) + " Degrees");
                }
                LoopConsole();
            }
        }

        //3: Least/Most Moist Day
        static void TopMoist()
        {
            bool loop = true;
            bool highest = true;
            while (loop)
            {
                Console.Clear();
                Console.WriteLine("Highest(1) or Lowest(2) Moisture?");
                var input = Console.ReadKey();
                int select;
                try
                {
                    select = int.Parse(input.KeyChar.ToString());
                }
                catch (Exception e)
                {
                    select = 0;
                }
                loop = false;

                switch (select)
                {
                    case 1:
                        //1: Highest
                        highest = true;
                        break;
                    case 2:
                        //2: Lowest
                        highest = false;
                        break;
                    default:
                        loop = true;
                        break;

                }
            }


            loop = true;
            while (loop)
            {
                Console.Clear();
                Console.WriteLine("Inside(1) or Outside(2)?");
                var input = Console.ReadKey();
                int select;
                try
                {
                    select = int.Parse(input.KeyChar.ToString());
                }
                catch (Exception e)
                {
                    select = 0;
                }
                loop = false;
                switch (select)
                {
                    case 1:
                        //1: Inside
                        TopMoistInside(highest);
                        break;
                    case 2:
                        //2: Outside
                        TopMoistOutside(highest);
                        break;
                    default:
                        loop = true;
                        break;

                }
            }
        }

        static void TopMoistInside(bool highest)
        {
            using (var context = new EFContext())
            {
                var query = context.Inside
                    .GroupBy(l => l.Date.Date)

                    .Select(cl => new
                    {
                        Date = cl.Key,
                        AverageMoist = cl.Average(c => c.Moisture)
                    }).ToList().OrderByDescending(x => x.AverageMoist);

                var list = query.Take(10);
                if (!highest)
                {
                    list = query.OrderBy(x => x.AverageMoist).Take(10);
                }
                Console.Clear();
                foreach (var day in list)
                {
                    Console.WriteLine(day.Date.ToString("dd/MM/yyyy") + ": " + Math.Round(day.AverageMoist, 1) + "%");
                }
                LoopConsole();
            }
        }

        static void TopMoistOutside(bool highest)
        {
            using (var context = new EFContext())
            {
                var query = context.Outside
                    .GroupBy(l => l.Date.Date)

                    .Select(cl => new
                    {
                        Date = cl.Key,
                        AverageMoist = cl.Average(c => c.Moisture)
                    }).ToList().OrderByDescending(x => x.AverageMoist);

                var list = query.Take(10);
                if (!highest)
                {
                    list = query.OrderBy(x => x.AverageMoist).Take(10);
                }
                Console.Clear();
                foreach (var day in list)
                {
                    Console.WriteLine(day.Date.ToString("dd/MM/yyyy") + ": " + Math.Round(day.AverageMoist, 1) + "%");
                }
                LoopConsole();
            }
        }

        //4: Day with Lowest/Highest Mold Risk
        static void TopChanceMold()
        {
            bool loop = true;
            bool highest = true;
            while (loop)
            {
                Console.Clear();
                Console.WriteLine("Highest(1) or Lowest(2) Mold Chance?");
                var input = Console.ReadKey();
                int select;
                try
                {
                    select = int.Parse(input.KeyChar.ToString());
                }
                catch (Exception e)
                {
                    select = 0;
                }
                loop = false;

                switch (select)
                {
                    case 1:
                        //1: Highest
                        highest = true;
                        break;
                    case 2:
                        //2: Lowest
                        highest = false;
                        break;
                    default:
                        loop = true;
                        break;

                }
            }


            loop = true;
            while (loop)
            {
                Console.Clear();
                Console.WriteLine("Inside(1) or Outside(2)?");
                var input = Console.ReadKey();
                int select;
                try
                {
                    select = int.Parse(input.KeyChar.ToString());
                }
                catch (Exception e)
                {
                    select = 0;
                }
                loop = false;
                switch (select)
                {
                    case 1:
                        //1: Inside
                        TopChanceMoldInside(highest);
                        break;
                    case 2:
                        //2: Outside
                        TopChanceMoldOutside(highest);
                        break;
                    default:
                        loop = true;
                        break;

                }
            }
        }

        static void TopChanceMoldInside(bool highest)
        {
            Console.Clear();
            List<MouldData> MouldRiskList = new List<MouldData>();

            using (var context = new EFContext())
            {
                var query = context.Inside
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
                        if (MouldIndex < 0)
                        {
                            MouldIndex = 0;
                        }
                    }

                    MouldData mouldData = new MouldData(day.Date, day.AverageTemp, day.AverageMold, MouldIndex);
                    MouldRiskList.Add(mouldData);
                }

                var list = MouldRiskList.OrderByDescending(m => m.MouldIndex).Take(10);
                if (!highest)
                {
                    list = MouldRiskList.OrderBy(m => m.MouldIndex).Take(10);
                }

                foreach (var day in list)
                {
                    Console.WriteLine(day.Date.ToString("dd/MM/yyyy") + ": Mold risk: " + Math.Round(day.MouldIndex, 2) + " Temperature: " + Math.Round(day.AverageTemperature,1) + " degrees. Moisture: " + Math.Round(day.AverageMoisture,1) + "%");
                }
            }
            LoopConsole();
        }

        static void TopChanceMoldOutside(bool highest)
        {
            Console.Clear();
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
                        if (MouldIndex < 0)
                        {
                            MouldIndex = 0;
                        }
                    }

                    MouldData mouldData = new MouldData(day.Date, day.AverageTemp, day.AverageMold, MouldIndex);
                    MouldRiskList.Add(mouldData);
                }

                var list = MouldRiskList.OrderByDescending(m => m.MouldIndex).Take(10);
                if (!highest)
                {
                    list = MouldRiskList.OrderBy(m => m.MouldIndex).Take(10);
                }

                foreach (var day in list)
                {
                    Console.WriteLine(day.Date.ToString("dd/MM/yyyy") + ": Mold risk: " + Math.Round(day.MouldIndex,2) + " Temperature: " + Math.Round(day.AverageTemperature, 1) + " degrees. Moisture: " + Math.Round(day.AverageMoisture, 1) + "%");
                }
            }
            LoopConsole();
        }

        //5: Date of Autumn
        static void AutumnDate()
        {
            // Meteorological Autumn occurs when the average temperature of a given day is 10C or lower.
            Console.Clear();

            using (var context = new EFContext())
            {
                var query = context.Outside
                    .GroupBy(l => l.Date.Date)
                    .Select(cl => new { Date = cl.Key, AverageTemp = cl.Average(c => c.Temperature) })
                    .ToList()
                    .OrderBy(x => x.Date);


                int counter = 0;
                DateTime date = DateTime.Now;
                foreach (var day in query)
                {
                    if (day.AverageTemp <= 10)
                    {
                        if (counter == 0)
                        {
                            date = day.Date;
                        }
                        counter++;
                    }
                    else
                    {
                        counter = 0;
                    }

                    if (counter == 5)
                    {
                        Console.WriteLine($"Meteorological Autumn start: {date.ToString("dd/MM/yyy")}");
                        break;
                    }

                }
                
            }
            LoopConsole();
        }

        //6: Date of Winter
        static void WinterDate()
        {
            // Meteorological Autumn occurs when the average temperature of a given day is 0C or lower.
            Console.Clear();

            using (var context = new EFContext())
            {
                var query = context.Outside
                    .GroupBy(l => l.Date.Date)
                    .Select(cl => new { Date = cl.Key, AverageTemp = cl.Average(c => c.Temperature) })
                    .ToList()
                    .OrderBy(x => x.Date);


                int counter = 0;
                DateTime date = DateTime.Now;
                foreach (var day in query)
                {
                    var AverageTemp = (int)day.AverageTemp; //Cast to int to get a result, otherwise 2016 never below averagetemp 0 five days in a row. == No Meteorological Winter.
                    if (AverageTemp <= 0)
                    {
                        if (counter == 0)
                        {
                            date = day.Date;
                        }
                        counter++;
                    }
                    else
                    {
                        counter = 0;
                    }

                    if (counter == 5)
                    {
                        Console.WriteLine($"Meteorological winter start: {date.ToString("dd/MM/yyy")}");
                        break;
                    }

                }
            }
            LoopConsole();
        }

        //7: Time of Door Open
        static void DoorOpen()
        {
            bool loop = true;
            bool highest = true;
            while (loop)
            {
                Console.Clear();
                Console.WriteLine("Most(1) or Least(2) Time Open?");
                var input = Console.ReadKey();
                int select;
                try
                {
                    select = int.Parse(input.KeyChar.ToString());
                }
                catch (Exception e)
                {
                    select = 0;
                }
                loop = false;

                switch (select)
                {
                    case 1:
                        //1: Most
                        highest = true;
                        break;
                    case 2:
                        //2: Least
                        highest = false;
                        break;
                    default:
                        loop = true;
                        break;

                }
            }
            Console.Clear();
            TopDoorOpen(highest);
        }

        static void TopDoorOpen(bool highest)
        {
            using (var context = new EFContext())
            {

                //Create InsideList
                var insideList = context.Inside
                .AsEnumerable().GroupBy(x =>
                {
                    var stamp = x.Date;
                    stamp = stamp.AddMinutes(-(stamp.Minute % 5));
                    stamp = stamp.AddMilliseconds(-stamp.Millisecond - 1000 * stamp.Second);
                    return stamp;
                })
                .Select(g => new { Date = g.Key, AvergeTemperature = g.Average(s => s.Temperature) })
                .OrderBy(x => x.Date)
                .ToList();

                //Create OutsideList
                var outsideList = context.Outside
                .AsEnumerable().GroupBy(x =>
                {
                    var stamp = x.Date;
                    stamp = stamp.AddMinutes(-(stamp.Minute % 5));
                    stamp = stamp.AddMilliseconds(-stamp.Millisecond - 1000 * stamp.Second);
                    return stamp;
                })
                .Select(g => new { Date = g.Key, AvergeTemperature = g.Average(s => s.Temperature) })
                .OrderBy(x => x.Date)
                .ToList();


                List<(DateTime, int)> doorOpenTime = new List<(DateTime, int)>();

                (DateTime, double) lastInsideData = (DateTime.Now, 0);
                (DateTime, double) lastOutsideData = (DateTime.Now, 0);

                foreach (var insideData in insideList)
                {
                    var outsideData = outsideList.Find(x => x.Date == insideData.Date);
                    if(outsideData != null)
                    {
                        if (insideData.Date == lastInsideData.Item1.AddMinutes(5))
                        {
                            if (insideData.AvergeTemperature < lastInsideData.Item2 && outsideData.AvergeTemperature > lastOutsideData.Item2)
                            {
                                (DateTime, int) toAdd = (insideData.Date, 5);
                                doorOpenTime.Add(toAdd);
                            }
                        }
                        (DateTime, double) toAddInside = (insideData.Date, insideData.AvergeTemperature);
                        (DateTime, double) toAddOutside = (outsideData.Date, outsideData.AvergeTemperature);
                        lastInsideData = toAddInside;
                        lastOutsideData = toAddOutside;
                    }
                }


                var query = doorOpenTime
                    .GroupBy(l => l.Item1.Date)

                    .Select(cl => new
                    {
                        Date = cl.Key,
                        TimeOpen = cl.Sum(c => c.Item2)
                    }).ToList().OrderByDescending(x => x.TimeOpen);

                var list = query.Take(10);
                if (!highest)
                {
                    list = query.OrderBy(x => x.TimeOpen).Take(10);
                }

                foreach (var day in list)
                {
                    Console.WriteLine(day.Date.ToString("dd/MM/yyyy") + ": " + day.TimeOpen + "Minutes");
                }
            }
            LoopConsole();

        }

        //8: Biggest/Smallest Difference in Temperature
        static void TopDiff()
        {
            bool loop = true;
            bool highest = true;
            while (loop)
            {
                Console.Clear();
                Console.WriteLine("Most(1) or Least(2) Temperature Differance?");
                var input = Console.ReadKey();
                int select;
                try
                {
                    select = int.Parse(input.KeyChar.ToString());
                }
                catch (Exception e)
                {
                    select = 0;
                }
                loop = false;

                switch (select)
                {
                    case 1:
                        //1: Most
                        highest = true;
                        break;
                    case 2:
                        //2: Least
                        highest = false;
                        break;
                    default:
                        loop = true;
                        break;

                }
            }
            Console.Clear();
            TopDifference(highest);
        }

        static void TopDifference(bool highest)
        {
            using (var context = new EFContext())
            {
                //Create InsideList
                var insideList = context.Inside
                    .AsEnumerable().GroupBy(x =>
                    {
                        var stamp = x.Date;
                        stamp = stamp.AddMinutes(-(stamp.Minute % 5));
                        stamp = stamp.AddMilliseconds(-stamp.Millisecond - 1000 * stamp.Second);
                        return stamp;
                    })
                    .Select(g => new { Date = g.Key, AvergeTemperature = g.Average(s => s.Temperature) })
                    .OrderBy(x => x.Date)
                    .ToList();

                //Create OutsideList
                var outsideList = context.Outside
                    .AsEnumerable().GroupBy(x =>
                    {
                        var stamp = x.Date;
                        stamp = stamp.AddMinutes(-(stamp.Minute % 5));
                        stamp = stamp.AddMilliseconds(-stamp.Millisecond - 1000 * stamp.Second);
                        return stamp;
                    })
                    .Select(g => new { Date = g.Key, AvergeTemperature = g.Average(s => s.Temperature) })
                    .OrderBy(x => x.Date)
                    .ToList();

                List<DoorData> differnceList = new List<DoorData>();

                foreach (var insideData in insideList)
                {
                    var outsideData = outsideList.Find(x => x.Date == insideData.Date);
                    if (outsideData != null)
                    {
                        double difference = Math.Abs(insideData.AvergeTemperature - outsideData.AvergeTemperature);
                        DoorData toAdd = new DoorData(insideData.Date, difference);
                        differnceList.Add(toAdd);
                        
                    }
                }

                var list = differnceList.OrderBy(x => x.Temperature).Take(10).ToList();
                if (highest)
                {
                    list = differnceList.OrderByDescending(x => x.Temperature).Take(10).ToList();
                }

                foreach(var data in list)
                {
                    Console.WriteLine(data.Date + " " +Math.Round(data.Temperature,1));
                }
            }
            LoopConsole();
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

    public class DoorData
    {
        public DateTime Date;
        public double Temperature;

        public DoorData(DateTime date, double temp)
        {
            Date = date;
            Temperature = temp;
        }
    }



}
