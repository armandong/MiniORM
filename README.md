# MiniORM
A lightweight ORM (object-relational mapper) for c#.net

### Basic Usage

```c#
public class Student
{
    public int StudentId { get; set; }
    public string Name { get; set; }
    public int Age { get; set; }
    public string Course { get; set; }
}
```

```c#
MySqlConnection connection = new MySqlConnection("server=host_name;database=db_name;uid=root;password=");

IMiniORM db = connection.MiniORM();

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
  
  // You can also use IEnumerable<T> as parameter
  db.Insert<Student>(new List<Student>
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
