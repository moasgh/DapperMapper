# Install the package
```csharp
  Install-Package DapperMapper

  https://www.nuget.org/packages/DapperMapper
```

# Tools
DapperMapper will ease your Data Management in 
<a href="https://docs.microsoft.com/en-us/sql/ssms/sql-server-management-studio-ssms?view=sql-server-2017&OCID=AID739534_SEM_9tLWfSfr&MarinID=s9tLWfSfr_340829462613_sql%20server%20management%20studio_e_c__68566393156_kwd-299629594689_&viewFallbackFrom=sql-server-2017%3F"> Microsoft SQL SERVER </a> and 
and <a href = "https://www.sqlite.org/index.html"> Sqlite</a> 
with the same interface. 
We provide Bulkinsert from JSON file in the package named "JSONAM" ease as one line of code to insert millions record to the supported database.

# JSONAM
1. Set your connection
```csharp
  // you can disregard App.Config and set the connection string on your program
  ConnectionManager.Set(ConnectionManager.ConnectionType.UserConfig, "#YOURCONNECTIONSTRING");
  // you can set the data base provider [Default set on MSSQLServer]
  ConnectionManager.Set(ConnectionManager.ConnectionType.UserConfig, "#YOURCONNECTIONSTRING",ConnectionManager.Provider.SqlClient);
  ConnectionManager.Set(ConnectionManager.ConnectionType.UserConfig, "#YOURCONNECTIONSTRING",ConnectionManager.Provider.Sqlite);
```
2. Start your Bulk insert
```csharp
	Jsonam j = new Jsonam();
	await j.InsertToSqlAsync(TableName: "Person", path: "People.json",DuplicateCheckingColumn:  "pid");
```

# DapperMapper

1. Create Connection String named "Repositoryconn":

```csharp
  <connectionStrings>
    <add name="Repositoryconn" connectionString="#YOUR CONNECTIONSTRING#"/>
  </connectionStrings>
```

```csharp
  // you can disregard App.Config and set the connection string on your program
  ConnectionManager.Set(ConnectionManager.ConnectionType.UserConfig, "#YOURCONNECTIONSTRING");
  // you can set the data base provider [Default set on MSSQLServer]
  ConnectionManager.Set(ConnectionManager.ConnectionType.UserConfig, "#YOURCONNECTIONSTRING",ConnectionManager.Provider.SqlClient);
  ConnectionManager.Set(ConnectionManager.ConnectionType.UserConfig, "#YOURCONNECTIONSTRING",ConnectionManager.Provider.Sqlite);
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

4. Data Management ability after mapping

```csharp

  User user = new User();
  usermap.Insert(user);
  Usermap.Update(user);
  Usermap.Delete(user);
  // Create the table
  // detect the type of sql based on SqlServer Data Type Mapping
  // https://docs.microsoft.com/en-us/dotnet/framework/data/adonet/sql-server-data-type-mappings
  Usermap.Create();
  
  Usermap.Count();
  Usermap.FindByID(1);
  IEnumerable<User> result = Usermap.SearchBy(s=>s.Name.Contains("John"));
  IEnumerable<User> result2 =Usermap.SearchBy(s=>s.Name.Contains("John") && s.Family == "Doe");
  IEnumerable<User> all = Usermap.GetAll();
  
```

5. Open Sql Query

```csharp
  var result = OpenSql<dynamic>.DynamicExecute("Select top(1) Name + ' ' + Family as FullName from User");
  Console.Write(result[0].FullName);
```

