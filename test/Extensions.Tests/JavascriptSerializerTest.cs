// MIT License.

using System.Collections;
using System.Collections.ObjectModel;
using System.Text.Json;
using System.Web.Script.Serialization;
using System.Web.UI.WebControls;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WebForms.Extensions.Tests;

[TestClass]
public class JavascriptSerializerTest
{

    [TestMethod]
    public void JavaScriptSerializerConverterTest()
    {
        var serializer = new JavaScriptSerializer();
        serializer.RegisterConverters([new ListItemCollectionConverter()]);
        var listItemCollection = GetListItemCollection();
        var result = serializer.Serialize(listItemCollection);
        Assert.IsNotNull(result);
        var recoveredList = serializer.Deserialize<ListItemCollection>(result);

        Assert.AreEqual(listItemCollection.Count, recoveredList.Count);
        CheckListItemCollection(recoveredList, listItemCollection);
    }

    [TestMethod]
    public void JavaScriptSerializerConverterTestWithList()
    {
        var serializer = new JavaScriptSerializer();
        serializer.RegisterConverters([new ListItemCollectionConverter()]);
        var listItemCollection = GetListItemCollection();
        var list = new List<ListItemCollection>();
        list.Add(listItemCollection);
        list.Add(listItemCollection);
        var result = serializer.Serialize(list);
        Assert.IsNotNull(result);

        var recoveredList = serializer.Deserialize<List<ListItemCollection>>(result);
        Assert.AreEqual(list.Count, recoveredList.Count);
        for (int i = 0; i < recoveredList.Count; i++)
        {
            Assert.IsTrue(recoveredList[i] is ListItemCollection);
        }

        var nextListCollection = GetListItemCollection();
        var nextList = new List<ListItemCollection>();
        nextList.Add(nextListCollection);
        nextList.Add(nextListCollection);
        var nextresult = serializer.Serialize(nextList);
        Assert.IsNotNull(result);
        Assert.AreEqual(result, nextresult);
    }

    [TestMethod]
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
        Assert.IsNotNull(result);

        var recoveredList = serializer.Deserialize<CustomObject>(result);
        Assert.AreEqual(customer.Name, recoveredList.Name);
        CheckListItemCollection(customer.Items, recoveredList.Items);

    }

    [TestMethod]
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
        Assert.AreEqual(customer.Name, recoveredList.Name);
        recoveredList.Numbers.ForEach(n => Assert.IsTrue(customer.Numbers.Contains(n)));
        recoveredList.Dictionary.ToList().ForEach(kvp => Assert.IsTrue(customer.Dictionary.ContainsKey(kvp.Key)));

    }

    [TestMethod]
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

        Assert.IsNotNull(weatherForecast);
        Assert.IsNotNull(weatherForecast2);
        Assert.AreEqual(weatherForecast.Date, weatherForecast2.Date);
        Assert.AreEqual(weatherForecast.TemperatureCelsius, weatherForecast2.TemperatureCelsius);
        Assert.AreEqual(weatherForecast.Summary, weatherForecast2.Summary);
        Assert.AreEqual(weatherForecast.SummaryField, weatherForecast2.SummaryField);
        Assert.IsNotNull(weatherForecast.TemperatureRanges);
        Assert.IsNotNull(weatherForecast2.TemperatureRanges);
        Assert.AreEqual(weatherForecast.TemperatureRanges.Count, weatherForecast2.TemperatureRanges.Count); 
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

    private void CheckListItemCollection(ListItemCollection listItemCollection, ListItemCollection recoveredList)
    {
        Assert.AreEqual(listItemCollection.Count, recoveredList.Count);
        for (int i = 0; i < listItemCollection.Count; i++)
        {
            Assert.IsNotNull(recoveredList.FindByValue(listItemCollection[i].Value));
        }
    }

    private ListItemCollection GetListItemCollection()
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
                throw new ArgumentNullException("dictionary");

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
