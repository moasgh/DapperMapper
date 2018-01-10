# DapperMapper
If you want to save your time in your development we recommend you to use DAPPERMAPPER we offer you simplest way to connect
your application to SQL server.
1. Create Connection String named "Repositoryconn":

```csharp
  <connectionStrings>
    <add name="Repositoryconn" connectionString="#YOUR CONNECTIONSTRING#"/>
  </connectionStrings>
```
2. Create Your Model

```csharp
  public class User
    {
        [Key(true)]
        public long ID { get; set; }
        public string UserName { get; set; }
        public string Name { get; set; }
        public string Family { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }
    }
```

3. Map your Model

```csharp
  DataMapper<User> usermap= new DataMapper<User>(nameof(User));
```

4. Enjoy abilities that we provide for you

```csharp

  User user = new User();
  usermap.Insert(user);
  Usermap.Update(user);
  Usermap.Delete(user);
  
  Usermap.Count();
  Usermap.FindByID(1);
  IEnumerable<User> result = Usermap.SearchBy(s=>s.Name.Contains("John"));
  IEnumerable<User> result2 =Usermap.SearchBy(s=>s.Name.Contains("John") && s.Family == "Doe");
  IEnumerable<User> all = Usermap.GetAll();
  
```
