# MiniORM
A lightweight ORM (object-relational mapper) for c#.net

Basic Usage

```c#
public class Student
{
    public int StudentId { get; set; }
    public string Name { get; set; }
    public int Age { get; set; }
    public string Course { get; set; }
}
```

Implementation:
```c#
ORM orm = new ORM();

IDatabaseFacade db = orm.CreateConnection(new Connection
{
    Host = "localhost",
    User = "root",
    Password = "",
    Database = "db_name"
});

// It automatically begin and commit/rollback the transaction
await db.TransactionAsync(() =>
{
  IEnumerable<Student> studentList = db.LoadData<Student>("SELECT * FROM Students");
  
  db.Insert(new Student
  {
      StudentId = 1513594220,
      Name = "Bob",
      Age = "32",
      Course = "Computer Science"
  });
  
  // Or you can use IEnumerable<T> as parameter
  db.Insert<Employee>(new List<Employee>
  {
      new Student
      {
          StudentId = 1513594221,
          Name = "John",
          Age = "22",
          Course = "IT"
      },
      new Student
      {
          StudentId = 1513594222,
          Name = "Steve",
          Age = "36",
          Course = "Engineering"
      },
  });
 }
```
