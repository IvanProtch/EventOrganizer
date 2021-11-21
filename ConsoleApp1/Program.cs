using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using EventOrganizerModel;

namespace ConsoleApp1
{
    public class Program
    {
        public static void Main(string[] args)
        {
            EventOrganizerModel.Organizer organizer = new Organizer(new User() {Login = "ipp" });
        }
    }
}
