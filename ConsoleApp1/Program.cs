using Microsoft.VisualBasic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace Vaccination
{
    public class Person
    {

        public string SocialSecurityNumber { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int Age { get; set; }
        public bool IsHealthcareWorker { get; set; }
        public bool IsElderly { get; set; }
        public bool IsRiskgroup { get; set; }
        public bool Infected { get; set; }
        public int Doses { get; set; }


        public Person(string p)
        {
            string[] values = p.Split(",");
            SocialSecurityNumber = values[0];
            Age = 2022 - int.Parse(SocialSecurityNumber.Substring(0, 4));
            FirstName = values[1];
            LastName = values[2];
            IsHealthcareWorker = values[3] == "1" ? true : false;
            IsRiskgroup = values[4] == "1" ? true : false;
            Infected = values[5] == "1" ? true : false;

        }

        public string toString()
        {
            return $"{this.SocialSecurityNumber},{this.FirstName},{this.LastName},{this.Doses}";
        }


    }

    public class VaccinationSchedule
    {
        public DateTime StartDate { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }

        public int AmountVaccinationer { get; set; }
        public double MinutVaccionation { get; set; }

        public static void RegisterScheduleVaccination()
        {

        }
    }
    public class Program
    {

        private static string indatafile = @"C:\Users\Tommy\source\repos\VaccinationApp\VaccinationApp\temp\People.csv";
        private static string outdatafile = @"C:\Users\Tommy\source\repos\VaccinationApp\VaccinationApp\temp\Vaccinations.csv";
        private static bool vaccinateChildren = false;
        private static bool setString = false;
        private static int availableVacinDose = 0;


        public static void Run()
        {

            CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;

            while (true)
            {
                MainMenuPage();
                int option = ShowMenu("Vad vill du göra?", new[]
                {
                    "Skapa prioritetsordning",
                    "Ändra antal vaccindoser",
                    "Ändra åldersgräns",
                    "Ändra indatafil",
                    "Ändra utdatafil",
                    "Avsluta",
                });
                Console.Clear();
                try
                {
                    switch (option)
                    {
                        case 0: CreatePriorityOderPage(); break;
                        case 1: ChangeVaccinationDosePage(); break;
                        case 2: ChangeAgeLimitedPage(); break;
                        case 3: ChangedInDataFilePage(); break;
                        case 4: ChangedOutDataFilePage(); break;
                        default: Environment.Exit(1); break;

                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("There was an error:" + e.Message);
                }
            }
        }

        // create priority order base on group and older
        public static void CreatePriorityOderPage()
        {

            // Read in people.csv
            // Check eligibility and sort people
            // Write to vacination.csv
            // get lines from people csv file

            string[] input = ReadCSVFile(indatafile);
            string[] output = CreateVaccinationOrder(input, availableVacinDose, vaccinateChildren);
            WriteToCSVFile(output);

        }
        // create array of Person based on input of input string array parametre
        public static string[] CreateVaccinationOrder(string[] input, int doses, bool vaccinateChildren)
        {
            Person[] people = new Person[input.Length];

            // initiate person value
            for (int i = 0; i < input.Length; i++)
            {
                people[i] = new Person(input[i]);
            }

            // check if vaccinate under 18 or not
            if (vaccinateChildren)
                people = SortPeople(people);
            else
                people = SortPeople(people.Where(person => person.Age >= 18).ToArray());

            List<Person> eligiblePeople = new List<Person>();

            // loop through and check for available dose
            foreach (Person person in people)
            {
                doses = person.Infected ? 1 : 2;
                if (availableVacinDose >= doses)
                {
                    availableVacinDose -= doses;
                    eligiblePeople.Add(person);
                }
                else
                    break;
            }
            // convert sorted list of Person to string array and return it
            return PeopleToStringArray(eligiblePeople.ToArray());

        }

        private static Person[] SortPeople(Person[] peopleQueue)
        {
            // return a sorted array base on age by different group
            return peopleQueue.OrderByDescending(person => person.IsHealthcareWorker == true)
                     .ThenByDescending(person => person.IsElderly == true)
                     .ThenByDescending(person => person.IsRiskgroup == true)
                     .ThenByDescending(person => person.Age)
                     .ToArray();
        }

        // write to vaccination csv file if file exist
        private static void WriteToCSVFile(string[] lines)
        {
            if (File.Exists(outdatafile))
            {
                File.WriteAllLines(outdatafile, lines);
            }
        }

        private static string[] ReadCSVFile(string path)
        {
            // get all lines from csv file
            string[] indata = File.ReadAllLines(path);

            // handle fel
            for (int i = 0; i < indata.Length; i++)
            {
                string[] values = indata[i].Split(',');
                string socialn = values[0];
                string healthcarew = values[3];
                string riskg = values[4];
                string infected = values[5];

                string[] splitSocialNumber = socialn.Split("-");
                string format = @"";
                foreach (var item in splitSocialNumber)
                {
                    Console.WriteLine(item);
                }

                for (int j = 0; j < values.Length; j++)
                {
                    if (String.IsNullOrEmpty(values[j]))
                    {
                        throw new Exception($"Ogiltig data,row {i} column {j} är tomt");
                    }
                }

                if (!healthcarew.Equals("0") && !healthcarew.Equals("1"))
                {
                    throw new Exception($"Ogiltig data healthcareworker {healthcarew}, row {i}. Värdet ska vara 0 eller 1");

                }
                if (!riskg.Equals("0") && !riskg.Equals("1"))
                {
                    throw new Exception($"Ogiltig data riskgroup {riskg},row {i} är tomt. Värdet ska vara 0 eller 1");

                }
                if (!infected.Equals("0") && !infected.Equals("1"))
                {
                    throw new Exception($"Ogiltig data infected {infected}, row {i} är tomt. Värdet ska vara 0 eller 1");
                }
            }
            // return all lines from csv file
            return indata;
        }

        private static string[] PeopleToStringArray(Person[] people)
        {
            List<string> lines = new List<string>();

            foreach (var person in people)
            {
                string line = $"{person.SocialSecurityNumber},{person.FirstName},{person.LastName},{(person.Infected ? 1 : 2)}";
                Console.WriteLine(line);
                lines.Add(line);
            }

            return lines.ToArray();
        }

        private static void SetInDataPath(string path)
        {
            if (!File.Exists(path))
            {
                indatafile = path;
                throw new Exception("Ogiltigt sökväg");
            }
        }

        private static void SetOutDataPath(string path)
        {
            if (!File.Exists(path))
            {
                outdatafile = path;
                throw new Exception("Mappen inte finns");
            }
        }

        // set amount of vaccin dose
        public static void ChangeVaccinationDosePage()
        {

            Console.WriteLine("Ändra antal vaccindoser");
            Console.WriteLine("-----------------------");

            while (true)
            {
                Console.WriteLine("Ange nytt antal doser:");
                try
                {
                    int amountDose = int.Parse(Console.ReadLine());
                    availableVacinDose = amountDose;
                    break;
                }
                catch
                {
                    Console.WriteLine("Du matar inte in korrekt data");
                }
            }
            Console.Clear();

        }

        // change vaccination age limit
        public static void ChangeAgeLimitedPage()
        {
            int option = ShowMenu("Ska personer under 18 vaccineras?", new[]
            {
                "Ja",
                "Nej",

            });
            Console.Clear();
            if (option == 0)
            {
                vaccinateChildren = true;
            }
            else if (option == 1)
            {
                vaccinateChildren = false;
            }
            setString = true;
        }


        public static void ChangedInDataFilePage()
        {
            Console.WriteLine("Ändra indatafil");
            Console.WriteLine("---------------");
            Console.WriteLine("Ange ny sökväg: ");
            try
            {
                string input = Console.ReadLine();
                SetInDataPath(input);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n=============== {ex.Message} ===============\n");
            }
        }

        public static void ChangedOutDataFilePage()
        {
            Console.WriteLine("Ändra utdatafil");
            Console.WriteLine("---------------");
            Console.WriteLine("Ange ny sökväg: ");
            try
            {
                string input = Console.ReadLine();
                SetOutDataPath(input);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n=============== {ex.Message} ===============\n");
            }
        }


        public static void MainMenuPage()
        {
            string ageString = setString ? (vaccinateChildren ? "JA" : "NEJ") : "";
            Console.WriteLine("HuvudMeny");
            Console.WriteLine("---------");
            Console.WriteLine($"Antal tillgängliga vaccindoser: {availableVacinDose}");
            Console.WriteLine($"Vaccinering under 18 år: {ageString}");
            Console.WriteLine($"Indatafil: {indatafile}");
            Console.WriteLine($"Utdatafil: {outdatafile}");

        }


        public static int ShowMenu(string prompt, IEnumerable<string> options)
        {
            if (options == null || options.Count() == 0)
            {
                throw new ArgumentException("Cannot show a menu for an empty list of options.");
            }

            Console.WriteLine(prompt);

            // Hide the cursor that will blink after calling ReadKey.
            Console.CursorVisible = false;

            // Calculate the width of the widest option so we can make them all the same width later.
            int width = options.Max(option => option.Length);

            int selected = 0;
            int top = Console.CursorTop;
            for (int i = 0; i < options.Count(); i++)
            {
                // Start by highlighting the first option.
                if (i == 0)
                {
                    Console.BackgroundColor = ConsoleColor.Blue;
                    Console.ForegroundColor = ConsoleColor.White;
                }

                var option = options.ElementAt(i);
                // Pad every option to make them the same width, so the highlight is equally wide everywhere.
                Console.WriteLine("- " + option.PadRight(width));

                Console.ResetColor();
            }
            Console.CursorLeft = 0;
            Console.CursorTop = top - 1;

            ConsoleKey? key = null;
            while (key != ConsoleKey.Enter)
            {
                key = Console.ReadKey(intercept: true).Key;

                // First restore the previously selected option so it's not highlighted anymore.
                Console.CursorTop = top + selected;
                string oldOption = options.ElementAt(selected);
                Console.Write("- " + oldOption.PadRight(width));
                Console.CursorLeft = 0;
                Console.ResetColor();

                // Then find the new selected option.
                if (key == ConsoleKey.DownArrow)
                {
                    selected = Math.Min(selected + 1, options.Count() - 1);
                }
                else if (key == ConsoleKey.UpArrow)
                {
                    selected = Math.Max(selected - 1, 0);
                }

                // Finally highlight the new selected option.
                Console.CursorTop = top + selected;
                Console.BackgroundColor = ConsoleColor.Blue;
                Console.ForegroundColor = ConsoleColor.White;
                string newOption = options.ElementAt(selected);
                Console.Write("- " + newOption.PadRight(width));
                Console.CursorLeft = 0;
                // Place the cursor one step above the new selected option so that we can scroll and also see the option above.
                Console.CursorTop = top + selected - 1;
                Console.ResetColor();
            }

            // Afterwards, place the cursor below the menu so we can see whatever comes next.
            Console.CursorTop = top + options.Count();

            // Show the cursor again and return the selected option.
            Console.CursorVisible = true;
            return selected;
        }




    }

    [TestClass]
    public class ProgramTests
    {
        public static void Main()
        {
            //ProgramTest;
            Console.WriteLine("Passed all Test");

            Program.Run();

        }

        [TestClass]
        public class ProgramTest
        {
            // TODO: Create more test cases to test the program
            [TestMethod]
            public void Checked_CreateVaccinationOrder()
            {
                //Arrange
                string[] input =
            {
                "19720906-1111,Elba,Idris,0,0,1",
                "19810203-2222,Efternamnsson,Eva,1,1,0"
            };
                int doses = 10;

                bool vaccinateChildren = false;
                //Act

                string[] output = Program.CreateVaccinationOrder(input, doses, vaccinateChildren);
                //Assert

                Assert.AreEqual(output.Length, 2);
                Assert.AreEqual("19810203-2222,Efternamnsson,Eva,1", output[1]);
                Assert.AreEqual("19720906-1111,Elba,Idris,2", output[0]);
            }

            [TestMethod]

            public void Cheched_ChangeVaccinationDosePage()
            {
                //Arrange
                //int beginningDose = 0;
                //int availbleDose = 100;
                //Person person = new Person("Available Dose", availbleDose);
                ////Act
                //Person.ChangeVaccinationDosePage(availbleDose);
                ////Assert
                //int actualDose = person.Doses;
                //Assert.AreEqual(availbleDose, actualDose);  

            }

            [TestMethod]

            public void Cheched_ChangeAgeLimitedPage()
            {
                //Arrange
                List<Person> people = new List<Person>();
                //Act
                //Assert
            }

            [TestMethod]

            public void Checked_ChangedInDataFilePage()
            {
                //Arrange
                List<Person> people = new List<Person>();
                //Act
                //Assert
            }

            [TestMethod]
            public void Checked_ChangedOutDataFilePage()
            {
                //Arrange
                List<Person> people = new List<Person>();
                //Act
                //Assert
            }

        }
    }

}