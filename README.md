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
    }

    class Program
    {
        static void Main(string[] args)
        {
            var store = new Store("/Users/ivansorokin/Desktop");
            var employee = new Employee() { Name = "Alexey" };

            store.Save(employee.Name, employee);
            Console.WriteLine(store.FindById<Employee>("Alexey")?.Name);
            Console.WriteLine(store.FindByQuery<Employee>(z => z.Name == "Ivan").SingleOrDefault()?.Name);
        }
    }
}
```
