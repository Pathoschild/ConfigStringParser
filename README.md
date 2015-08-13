**ConfigStringParser** is a tiny library which lets you read and write any configuration values into a [<s>connection</s> config string](https://en.wikipedia.org/wiki/Connection_string), and convert freely between string and object representations:
<table>
<tr>
<th>config string</th>
<th>↔</th>
<th>C# class</th>
</tr>
<tr>
<td>
<pre>
HostName=example.com; Port=587; EnableSsl=true
</pre>
</td>
<td>↔</td>
<td>
<pre>
public class EmailConfig
{
   public string HostName { get; set;}
   public int Port { get; set; }
   public bool EnableSSL { get; set;}
}
</pre>
</td>
</tr>
</table>

## Usage
### Creating a config string
You can create a new config string from scratch:

```c#
var parser = new ConfigStringParser();
parser.Add({ hostName = "example.com", port = 587, enableSSL = true });
```

...or start from one you have:

```c#
var parser = new ConfigStringParser("HostName=example.com; Port=587; EnableSsl=true");
```

You can freely add, remove, and overwrite values (keys aren't case sensitive):

```c#
parser.Add("UserName", "johnny");
parser["username"] = "billy";
parser.Remove("username");
```

### Mapping to an object
You can freely map the config string into an object:

  ```c#
  EmailConfig config = parser.MapTo<EmailConfig>();
  ```
  
  ...or fill an existing object:
  
  ```c#
  EmailConfig config = new EmailConfig();
  parser.MapTo(config);
  ```

You can optionally add data annotations to your class:
* [`[Required]`](https://msdn.microsoft.com/en-us/library/system.componentmodel.dataannotations.requiredattribute.aspx) means the value must be defined. If you try to map a config string without it, you'll get an informative `KeyNotFoundException`.
* [`[DisplayName("Other Name")]`](https://msdn.microsoft.com/en-us/library/system.componentmodel.displaynameattribute.aspx) provides an alternative name that can appear in the config string. For example, this lets you map a config string like `Host Name=example.com` to a property named `HostName`.

The parser will automatically map most primitive types (including `bool`, `string`, `Guid`, `double`, `int`, `long`, `short`, and enums), but doesn't handle dates.

### Comparing strings
You can check whether a string is equivalent to another one (regardless of order or formatting):
```c#
var parser = new ConfigStringParser("HostName=example.com; Port=587; EnableSsl=true");
bool isEqual = parser.IsEquivalentTo("EnableSSL=true; Port=587; HOSTNAME=example.com"); // true
```