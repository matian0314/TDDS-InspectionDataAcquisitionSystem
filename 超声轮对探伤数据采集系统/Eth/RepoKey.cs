using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eth
{
    #region 参考链接
    /*
     *控制Json字符串的序列化和反序列化 
     *参考链接: https://www.it1352.com/379346.html
     *I have an C# MVC application which stores data as JSON strings in an XML document and also in MySQL DB Tables.

Recently I have received the requirement to store JSON strings in MySQL Database fields, to be converted into C# objects via Newtonsoft.Json, so I decided to implement a TypeConverter to convert JSON strings into custom C# Models.

Unfortunately I cannot use the following command anywhere in my solution to deserialize my JSON strings when the TypeConverter attribute is added to my C# Model:

JsonConvert.DeserializeObject<Foo>(json);
Removing the attribute resolve the issue however this prevents me from converting MySQL DB fields into custom C# objects.

Here is my C# Model with the TypeConverter Attribute added:

using System.ComponentModel;

[TypeConverter(typeof(FooConverter))]
public class Foo
{
    public bool a { get; set; }
    public bool b { get; set; }
    public bool c { get; set; }
    public Foo(){}
}
Here is my TypeConverter Class:

using Newtonsoft.Json;
using System;
using System.ComponentModel;

    public class FooConverter : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext context, System.Type sourceType)
        {
            if (sourceType == typeof(string))
            {
                return true;
            }
            return base.CanConvertFrom(context, sourceType);
        }
        public override object ConvertFrom(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
        {
            if (value is string)
            {
                string s = value.ToString().Replace("\\","");
                Foo f = JsonConvert.DeserializeObject<Foo>(s);
                return f;
            }
            return base.ConvertFrom(context, culture, value);
        }
    }
}
As soon as I add the attribute to the Foo Class I receive the following error:

Cannot deserialize the current JSON object (e.g. {"name":"value"}) into type 'Models.Foo' because the type requires a JSON string value to deserialize correctly.

To fix this error either change the JSON to a JSON string value or change the deserialized type so that it is a normal .NET type (e.g. not a primitive type like integer, not a collection type like an array or List) that can be deserialized from a JSON object. JsonObjectAttribute can also be added to the type to force it to deserialize from a JSON object.

I am using the following string (which works perfectly without adding the TypeConverter Attribute):

"{\"Foo\":{\"a\":true,\"b\":false,\"c\":false}}"
Not sure what's going on here, any ideas?

Many Thanks!!!

UPDATE
I have have discovered that I also have issues with actions on MVC API Controllers that accept the Test Class with Foo as a property or on controllers that accept Foo as an object when the TypeConverter attribute is added to the Foo Class.

Here is an example of a test controller which has issues:

public class TestController : ApiController
{
    [AcceptVerbs("POST", "GET")]
    public void PostTestClass(TestClass t)
    {
        // Returns null when TypeConverter attribute is added to the Foo Class
        return t.Foo; 
    }
    AcceptVerbs("POST", "GET")]
    public void PostFooObj(Foo f)
    {
        // Returns null when TypeConverter attribute is added to the Foo Class
        return f;
    }
}
The TypeConverter may be causing issues overriding the WebAPI model binding and returns null when either action above receives JSON via AJAX with the following structure:

// eg. PostTestClass(TestClass T)
{'Foo': {'a': false,'b': true,'c': false}};

// eg. PostFooObj(Foo f)
{'a': false,'b': true,'c': false}
When the TypeConverter Attribute is added to the Foo Class, the following method on the FooConverter TypeConverter class is called as soon the route is found:

    public override bool CanConvertFrom(ITypeDescriptorContext context, System.Type sourceType)
    {
        if (sourceType == typeof(string))
        {
            return true;
        }
        return base.CanConvertFrom(context, sourceType);
    }
The ConvertFrom method on the FooConverter TypeController is NOT called by the ApiController's action, which may be the cause of the issue.

Again it's a similar situation, where the controllers actions will work fine without the TypeConverter Attribute.

Any further help greatly appreciated!!

Many thanks.

解决方案
There are a few things going on here. First, a preliminary issue: even with no TypeConverter applied, your JSON does not correspond to your class Foo, it corresponds to some container class that contains a Foo property, for instance:

public class TestClass
{
    public Foo Foo { get; set; }
}
I.e. given your JSON string, the following will not work:

var json = "{\"Foo\":{\"a\":true,\"b\":false,\"c\":false}}";
var foo = JsonConvert.DeserializeObject<Foo>(json);
But the following will:

var test = JsonConvert.DeserializeObject<TestClass>(json);
I suspect this is simply a mistake in the question, so I'll assume you are looking to deserialize a class contain a property Foo.

The main problem you are seeing is that Json.NET will try to use a TypeConverter if one is present to convert a class to be serialized to a JSON string. From the docs:

Primitive Types

.Net: TypeConverter (convertible to String)
JSON: String

But in your JSON, Foo is not a JSON string, it is a JSON object, thus deserialization fails once the type converter is applied. An embedded string would look like this:

{"Foo":"{\"a\":true,\"b\":false,\"c\":false}"}
Notice how all the quotes have been escaped. And even if you changed your JSON format for Foo objects to match this, your deserialization would still fail as the TypeConverter and Json.NET try to call each other recursively.

Thus what you need to do is to globally disable use of the TypeConverter by Json.NET and fall back to default serialization while retaining use of the TypeConverter in all other situations. This is a bit tricky since there is no Json.NET attribute you can apply to disable use of type converters, instead you need a special contract resolver plus a special JsonConverter to make use of it:

public class NoTypeConverterJsonConverter<T> : JsonConverter
{
    static readonly IContractResolver resolver = new NoTypeConverterContractResolver();

    class NoTypeConverterContractResolver : DefaultContractResolver
    {
        protected override JsonContract CreateContract(Type objectType)
        {
            if (typeof(T).IsAssignableFrom(objectType))
            {
                var contract = this.CreateObjectContract(objectType);
                contract.Converter = null; // Also null out the converter to prevent infinite recursion.
                return contract;
            }
            return base.CreateContract(objectType);
        }
    }

    public override bool CanConvert(Type objectType)
    {
        return typeof(T).IsAssignableFrom(objectType);
    }

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        return JsonSerializer.CreateDefault(new JsonSerializerSettings { ContractResolver = resolver }).Deserialize(reader, objectType);
    }

    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        JsonSerializer.CreateDefault(new JsonSerializerSettings { ContractResolver = resolver }).Serialize(writer, value);
    }
}
And use it like:

[TypeConverter(typeof(FooConverter))]
[JsonConverter(typeof(NoTypeConverterJsonConverter<Foo>))]
public class Foo
{
    public bool a { get; set; }
    public bool b { get; set; }
    public bool c { get; set; }
    public Foo() { }
}

public class FooConverter : TypeConverter
{
    public override bool CanConvertFrom(ITypeDescriptorContext context, System.Type sourceType)
    {
        if (sourceType == typeof(string))
        {
            return true;
        }
        return base.CanConvertFrom(context, sourceType);
    }
    public override object ConvertFrom(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
    {
        if (value is string)
        {
            string s = value.ToString();
            //s = s.Replace("\\", "");
            Foo f = JsonConvert.DeserializeObject<Foo>(s);
            return f;
        }
        return base.ConvertFrom(context, culture, value);
    }
}
Example fiddle.

Finally, you should probably also implement the ConvertTo method in your type converter, see How to: Implement a Type Converter.
     */
    #endregion
    [TypeConverter(typeof(RepoKeyConvertor))]
    [Serializable]
    public class RepoKey
    {
        public string Ip { get; set; }
        /// <summary>
        /// 第几个通道，从0开始
        /// </summary>
        public int Channel { get; set; }
        /// <summary>
        /// 第几根车轴，从1开始
        /// </summary>
        public int Axle { get; set; }
        public RepoKey() { }
        public RepoKey(string ip, int channel, int axle)
        {
            Ip = ip;
            Channel = channel;
            Axle = axle;
        }
        public override string ToString()
        {
            return $"{Ip}-{Channel}-{Axle}";
        }
        public string ToFileName()
        {
            return $"{Ip}-{Axle}.dat";
        }
        public override bool Equals(object obj)
        {
            if (!(obj is RepoKey)) return false;
            var key = obj as RepoKey;
            if (this.Ip == key.Ip && this.Axle == key.Axle && this.Channel == key.Channel) return true;
            return false;
        }
        public override int GetHashCode()
        {
            return (this.ToString()).GetHashCode();
        }

    }
    public class RepoKeyConvertor : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext context, System.Type sourceType)
        {
            if (sourceType == typeof(string))
            {
                return true;
            }
            return base.CanConvertFrom(context, sourceType);
        }
        public override object ConvertFrom(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
        {
            if (value is string)
            {
                string s = value.ToString();
                s = s.Replace("\\", "");
                var pars = s.Split('-');
                RepoKey k = new RepoKey(pars[0], int.Parse(pars[1]), int.Parse(pars[2]));
                return k;
            }
            return base.ConvertFrom(context, culture, value);
        }
        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            return base.CanConvertTo(context, destinationType);
        }
        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            return value.ToString();
            //return base.ConvertTo(context, culture, value, destinationType);
        }
    }
}
