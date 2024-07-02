using System;
using System.Collections.Generic;

namespace Imsi
{
    internal class Program
    {
        static void Main(string[] args)
        {

            string mySqlProps = "Server=localhost;Uid=root;Pwd=;Database=smshub";
            string redisConnectionString = "localhost:6379";

            Console.WriteLine("Hello world");
            ImsiResolver imsiResolver = new ImsiResolver(mySqlProps, redisConnectionString);
            // imsiResolver.Init();

            //Console.WriteLine(imsiResolver.GetData("123123"));

            while (true)
            {
                Console.WriteLine("Enter an Option:");
                Console.WriteLine("1 to Add");
                Console.WriteLine("2 to Delete");
                Console.WriteLine("3 to Get");
                Console.WriteLine("4 to Exit");
                Console.WriteLine("5 to Clear Console");

                string choice = Console.ReadLine();

                switch (choice)
                {
                    case "1":
                        Console.WriteLine("Enter IMSI:");
                        string addImsi = Console.ReadLine();

                        Console.WriteLine("Enter Phone Number:");
                        string phoneNumber = Console.ReadLine();

                        imsiResolver.AddData(new List<KeyValuePair<string, string>> {
                            new KeyValuePair<string,string>(addImsi, phoneNumber)
                        });
                        Console.WriteLine($"Added IMSI: {addImsi}, Phone Number: {phoneNumber}.");
                        break;

                    case "2":
                        Console.WriteLine("Enter IMSI to delete:");
                        string deleteImsi = Console.ReadLine();

                        imsiResolver.DeleteData(deleteImsi);
                        Console.WriteLine($"Deleted IMSI: {deleteImsi}.");
                        break;

                    case "3":
                        Console.WriteLine("Enter IMSI to get data:");
                        string getImsi = Console.ReadLine();

                        string retrievedPhoneNumber = imsiResolver.GetData(getImsi);
                        if (retrievedPhoneNumber != null)
                        {
                            Console.WriteLine($"Retrieved Phone Number for IMSI {getImsi}: {retrievedPhoneNumber}");
                        }
                        else
                        {
                            Console.WriteLine($"IMSI {getImsi} not found.");
                        }
                        break;

                    case "4":
                        Console.WriteLine("Exiting...");
                        return;
                    case "5":
                        Console.Clear();
                        break;

                    default:
                        Console.WriteLine("Invalid choice. Please try again.");
                        break;
                }
                Console.WriteLine("---------------------------------------------------");
                Console.WriteLine();
            }


        }
    }
}
