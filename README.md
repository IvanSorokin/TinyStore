This is a simple wrapper for the filesystem structure to use it like a database for small projects.
Usage:

```
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
        public int Age { get; set; }
    }

    class Program
    {
        static void Main(string[] args)
        {
            var store = new Store("/Users/ivansorokin/Desktop", useTypeNameForCollection : true, keepDbInMemory: true);
            var employee1 = new Employee() { Name = "Alexey" };
            var employee2 = new Employee() { Name = "Ivan", Age = 30};

            store.Save(employee1.Name, employee1);
            store.Save(employee2.Name, employee2);

            Console.WriteLine(store.FindById<Employee>("Alexey")?.Name);
            Console.WriteLine(store.FindByQuery<Employee>(z => z.Name == "Ivan").Single().Name);

            store.Modify<Employee>("Alexey", x => x.Age = 55);
            store.ModifyByQuery<Employee>(q => q.Age == 30, x => x.Age = 24);

            Console.WriteLine(store.FindById<Employee>("Alexey")?.Age);
            Console.WriteLine(store.FindByQuery<Employee>(z => z.Name == "Ivan").Single().Age);

            store.DeleteById<Employee>("Ivan");
            store.DeleteByQuery<Employee>(z => z.Name == "Alexey");

            Console.WriteLine(store.FindById<Employee>("Alexey")?.Name ?? "Deleted");
            Console.WriteLine(store.FindByQuery<Employee>(z => z.Name == "Ivan").SingleOrDefault()?.Name ?? "Deleted");
        }
    }
}
```
