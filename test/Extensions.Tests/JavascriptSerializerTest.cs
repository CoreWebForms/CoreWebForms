// MIT License.

using System.Collections;
using System.Text.Json;
using System.Web.Script.Serialization;
using System.Web.UI.WebControls;
using NUnit.Framework;

#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning disable CS8601 // Possible null reference assignment.
#pragma warning disable CS8602 // Dereference of a possibly null reference.
#pragma warning disable CS8603 // Possible null reference return.
#pragma warning disable CS8604 // Possible null reference argument.
#pragma warning disable CS8613 // Nullability of reference types in return type doesn't match implicitly implemented member.
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
#pragma warning disable CS8619 // Nullability of reference types in value doesn't match target type.
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
#pragma warning disable CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
#pragma warning disable CS8765 // Nullability of type of parameter doesn't match overridden member.
#pragma warning disable CS8767 // Nullability of type of parameter doesn't match implemented member.

namespace WebForms.Extensions.Tests;

[TestFixture]
public class JavascriptSerializerTest
{

    [Test]
    public void JavaScriptSerializerConverterTest()
    {
        var serializer = new JavaScriptSerializer();
        serializer.RegisterConverters([new ListItemCollectionConverter()]);
        var listItemCollection = GetListItemCollection();
        var result = serializer.Serialize(listItemCollection);
        Assert.That(result, Is.Not.Null);
        var recoveredList = serializer.Deserialize<ListItemCollection>(result);
        recoveredList ??= new ListItemCollection();

        Assert.That(recoveredList.Count, Is.EqualTo(listItemCollection.Count));
        CheckListItemCollection(recoveredList, listItemCollection);
    }

    [Test]
    public void JavaScriptSerializerConverterTestWithList()
    {
        var serializer = new JavaScriptSerializer();
        serializer.RegisterConverters([new ListItemCollectionConverter()]);
        var listItemCollection = GetListItemCollection();
        var list = new List<ListItemCollection>();
        list.Add(listItemCollection);
        list.Add(listItemCollection);
        var result = serializer.Serialize(list);
        Assert.That(result, Is.Not.Null);

        var recoveredList = serializer.Deserialize<List<ListItemCollection>>(result);
        recoveredList ??= new List<ListItemCollection>();

        Assert.That(recoveredList.Count, Is.EqualTo(list.Count));
        for (int i = 0; i < recoveredList.Count; i++)
        {
            Assert.IsTrue(recoveredList[i] is ListItemCollection);
        }

        var nextListCollection = GetListItemCollection();
        var nextList = new List<ListItemCollection>();
        nextList.Add(nextListCollection);
        nextList.Add(nextListCollection);
        var nextresult = serializer.Serialize(nextList);
        Assert.That(result, Is.Not.Null);
        Assert.That(nextresult, Is.EqualTo(result));
    }

    [Test]
    public void JavaScriptSerializerConverterWithCustomClass()
    {
        var serializer = new JavaScriptSerializer();
        serializer.RegisterConverters([new ListItemCollectionConverter()]);
        var listItemCollection = GetListItemCollection();
        var customer = new CustomObject
        {
            Name = "John",
            Items = listItemCollection
        };
        var result = serializer.Serialize(customer);
        Assert.That(result, Is.Not.Null);

        var recoveredList = serializer.Deserialize<CustomObject>(result);
        recoveredList ??= new CustomObject();

        Assert.That(recoveredList.Name, Is.EqualTo(customer.Name));
        CheckListItemCollection(customer.Items, recoveredList.Items);

    }

    [Test]
    public void JavaScriptSerializerNoConverter()
    {
        var serializer = new JavaScriptSerializer();
        var customer = new Customer
        {
            Name = "John",
            Numbers = new List<int> { 1, 2, 3 },
            Dictionary = new Dictionary<string, string> { { "1", "One" }, { "2", "Two" } }
        };
        var result = serializer.Serialize(customer);
        Assert.IsNotNull(result);

        var recoveredList = serializer.Deserialize<Customer>(result);
        recoveredList ??= new Customer();

        Assert.That(recoveredList.Name, Is.EqualTo(customer.Name));
        recoveredList.Numbers.ForEach(n => Assert.IsTrue(customer.Numbers.Contains(n)));
        recoveredList.Dictionary.ToList().ForEach(kvp => Assert.IsTrue(customer.Dictionary.ContainsKey(kvp.Key)));

    }

    [Test]
    public void JavaScriptDeserialization()
    {
        string jsonString =
               """
                {
                  "Date": "2019-08-01T00:00:00-07:00",
                  "TemperatureCelsius": 25,
                  "Summary": "Hot",
                  "DatesAvailable": [
                    "2019-08-01T00:00:00-07:00",
                    "2019-08-02T00:00:00-07:00"
                  ],
                  "TemperatureRanges": {
                                "Cold": {
                                    "High": 20,
                      "Low": -10
                                },
                    "Hot": {
                                    "High": 60,
                      "Low": 20
                    }
                            },
                  "SummaryWords": [
                    "Cool",
                    "Windy",
                    "Humid"
                  ]
                }
                """;

        WeatherForecast? weatherForecast =
            JsonSerializer.Deserialize<WeatherForecast>(jsonString);
        JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
        WeatherForecast? weatherForecast2 =
            javaScriptSerializer.Deserialize<WeatherForecast>(jsonString);

        Assert.That(weatherForecast, Is.Not.Null);
        Assert.That(weatherForecast2, Is.Not.Null);
        Assert.That(weatherForecast2.Date, Is.EqualTo(weatherForecast.Date));
        Assert.That(weatherForecast2.TemperatureCelsius, Is.EqualTo(weatherForecast.TemperatureCelsius));
        Assert.That(weatherForecast2.Summary, Is.EqualTo(weatherForecast.Summary));
        Assert.That(weatherForecast2.SummaryField, Is.EqualTo(weatherForecast.SummaryField));
        Assert.That(weatherForecast.TemperatureRanges, Is.Not.Null);
        Assert.That(weatherForecast2.TemperatureRanges, Is.Not.Null);
        Assert.That(weatherForecast2.TemperatureRanges.Count, Is.EqualTo(weatherForecast.TemperatureRanges.Count)); 
    }

    public class WeatherForecast
    {
        public DateTimeOffset Date { get; set; }
        public int TemperatureCelsius { get; set; }
        public string? Summary { get; set; }
        public string? SummaryField;
        public IList<DateTimeOffset>? DatesAvailable { get; set; }
        public Dictionary<string, HighLowTemps>? TemperatureRanges { get; set; }
        public string[]? SummaryWords { get; set; }
    }

    public class HighLowTemps
    {
        public int High { get; set; }
        public int Low { get; set; }
    }

public class Customer
    {
        public string Name { get; set; }
        public List<Int32> Numbers { get; set; }
        public Dictionary<string, string> Dictionary { get; set; }
    }

    public class CustomObject
    {
        public string Name { get; set; }
        public ListItemCollection Items { get; set; }
    }

    private static void CheckListItemCollection(ListItemCollection listItemCollection, ListItemCollection recoveredList)
    {
        Assert.That(recoveredList.Count, Is.EqualTo(listItemCollection.Count));
        for (var i = 0; i < listItemCollection.Count; i++)
        {
            Assert.That(recoveredList.FindByValue(listItemCollection[i].Value), Is.Not.Null);
        }
    }

    private static ListItemCollection GetListItemCollection()
    {
        ListItemCollection list = new ListItemCollection();
        list.Add(new ListItem("1", "First Item"));
        list.Add(new ListItem("2", "Second Item"));
        list.Add(new ListItem("3", "Third Item"));
        return list;
    }

    internal class ListItemCollectionConverter : JavaScriptConverter
    {

        public override IEnumerable<Type> SupportedTypes
            //Define the ListItemCollection as a supported type.
            => [typeof(ListItemCollection)];

        public override IDictionary<string, object> Serialize(object obj, JavaScriptSerializer serializer)
        {
            var listType = obj as ListItemCollection;

            if (listType != null)
            {
                // Create the representation.
                Dictionary<string, object> result = new Dictionary<string, object>();
                ArrayList itemsList = new ArrayList();
                foreach (ListItem item in listType)
                {
                    //Add each entry to the dictionary.
                    Dictionary<string, object> listDict = new Dictionary<string, object>();
                    listDict.Add("Value", item.Value);
                    listDict.Add("Text", item.Text);
                    itemsList.Add(listDict);
                }
                result["List"] = itemsList;

                return result;
            }
            return new Dictionary<string, object>();
        }

        public override object Deserialize(IDictionary<string, object> dictionary, Type type, JavaScriptSerializer serializer)
        {
            if (dictionary == null)
                throw new ArgumentNullException(nameof(dictionary));

            if (type == typeof(ListItemCollection))
            {
                // Create the instance to deserialize into.
                ListItemCollection list = new ListItemCollection();

                // Deserialize the ListItemCollection's items.
                ArrayList itemsList = (ArrayList)dictionary["List"];
                for (int i = 0; i < itemsList.Count; i++)
                    list.Add(serializer.ConvertToType<ListItem>(itemsList[i]));

                return list;
            }
            return null;
        }
    }
}
