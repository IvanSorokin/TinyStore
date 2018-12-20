using System;
using System.Linq;
using TinyStore.Attributes;
using TinyStore.Core;

namespace Program
{
    [CollectionName("users")]
    class Employee
    {
        public string Name { get; set; }
    }

    class Program
    {
        static void Main(string[] args)
        {
            var store = new Store("/Users/ivansorokin/Desktop");
            var employee1 = new Employee() { Name = "Alexey" };
            var employee2 = new Employee() { Name = "Ivan" };

            store.Save(employee1.Name, employee1);
            store.Save(employee2.Name, employee2);

            Console.WriteLine(store.FindById<Employee>("Alexey")?.Name);
            Console.WriteLine(store.FindByQuery<Employee>(z => z.Name == "Ivan").Single().Name);

            store.DeleteById<Employee>("Ivan");
            store.DeleteByQuery<Employee>(z => z.Name == "Alexey");

            Console.WriteLine(store.FindById<Employee>("Alexey")?.Name ?? "Deleted");
            Console.WriteLine(store.FindByQuery<Employee>(z => z.Name == "Ivan").SingleOrDefault()?.Name ?? "Deleted");
        }
    }
}
