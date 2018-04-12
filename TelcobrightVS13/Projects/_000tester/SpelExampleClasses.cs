using System;
using System.Collections;
using System.Collections.Generic;

namespace Utils
{
    public class Inventor
    {
        public string Name;
        public string Nationality;
        public string[] Inventions;
        private DateTime dob;
        private Place pob;
        public readonly List<int> Numbers = new List<int>() {1, 2, 3, 4};
        public Inventor() : this(null, DateTime.MinValue, null)
        { }

        public Inventor(string name, DateTime dateOfBirth, string nationality)
        {
            this.Name = name;
            this.dob = dateOfBirth;
            this.Nationality = nationality;
            this.pob = new Place();
        }

        public DateTime DOB
        {
            get { return dob; }
            set { dob = value; }
        }

        public Place PlaceOfBirth
        {
            get { return pob; }
        }

        public int GetAge(DateTime on)
        {
            // not very accurate, but it will do the job ;-)
            return on.Year - dob.Year;
        }
    }

    public class Place
    {
        public string City;
        public string Country;
    }

    public class Society
    {
        public string Name;
        public static string Advisors = "advisors";
        public static string President = "president";

        private IList members = new ArrayList();
        private IDictionary officers = new Hashtable();

        public IList Members
        {
            get { return members; }
        }

        public IDictionary Officers
        {
            get { return officers; }
        }

        public bool IsMember(string name)
        {
            bool found = false;
            foreach (Inventor inventor in members)
            {
                if (inventor.Name == name)
                {
                    found = true;
                    break;
                }
            }
            return found;
        }
    }
    //The code listings in this chapter use instances of the data populated with the following information.
    
}
