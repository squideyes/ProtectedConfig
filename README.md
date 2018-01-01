The ProtectedConfig NuGet makes it super easy to work with DPAPI-protected data using a fluent interface.  It only takes a few lines of code to get started:
```csharp
new ConfigManager()
    .Set("EMAIL", "somedude@someco.com")
    .Set("ID", 12345)
	.Set("ISACTIVE", true)
    .Set("INFO", new MyInfo())
    .Save(@"C:\MyApp\Settings\Config.json);
```
```csharp
var cm = new ConfigManager()
    .Load(@"C:\MyApp\Settings\Config.json");

var email = cm.Get<string>("EMAIL");
var id = cm.Get<int>("ID");
var isActive = cm.Get<bool>("ISACTIVE");
var info  = cm.Get<MyClass>("INFO");
```
| Value | Notes |
| ----- | ----- |
| WithCurrentUserScope() | Associates the protected config data with the current user. Only threads running under the current user can protect or unprotect the data.  NOTE: if ommitted the scope will default to "LocalMachine" allowing *any* process running on the computer to protect or unprotect the data. |
| Set&lt;T&gt;(string key, T value) | Associates a value with a given key.  Any value may be supplied as long as it can be serialized using Newtonsoft's Json.NET, including null.  Keys are case-insensitive. |
| Get&lt;T&gt;(string key) | Retrieves a previously-saved value associated with a given key then deserializes the value into a generic type T.  Keys are case-insensitive. |
| Load(Stream stream) | Loads a previously saved  DPAPI-encrypted config data stream using the current  scope and reading from the current stream position all the way  to the end.  NOTE: if a stream cannot be loaded then  subsequent Get&lt;T&gt;(string key) operations will return default values. |
| Load(string fileName) | Loads a previously saved  DPAPI-encrypted config data file  using the current scope.  NOTE: if a stream cannot be loaded then  subsequent Get&lt;T&gt;(string key) operations will return default values.  |
| Save(Stream stream) | Encrypts and saves config data to the supplied stream using DPAPI within the current scope. |
| Save(string fileName) | Encrypts and saves config data to the specified file using DPAPI and  the current scope.  If the file exists it will be overwritten. |
| WithEntropy(byte[] value) | Supplies additional entropy data to further secure the config data.  NOTE: if you call one of the Save() methods with a given set of entropy data you must call one of the Load() methods with the same entropy data. |
