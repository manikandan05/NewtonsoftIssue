using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using WebApplication3.Models;

namespace WebApplication3.Controllers
{

    [DataContract]
    public enum EditingType
    {
        [Obsolete("String property has been deprecated. Use StringEdit property instead")]
        [EnumMember(Value = "stringedit")]
        String,
        [EnumMember(Value = "stringedit")]
        StringEdit
    }

    public class JsonPropertyAttribute : Attribute
    {
        public JsonPropertyAttribute(string argName)
        {
            PropertyName = argName;
        }

        public string PropertyName { get; set; }
    }

    public class JsonConverterAttribute : Attribute
    {
        public Type ConverterType
        {
            get; set;
        }
        public string ValueType
        {
            get; set;
        }
        public JsonConverterAttribute(Type type, string valueType)
        {
            this.ConverterType = type;
            this.ValueType = valueType;
        }
        public JsonConverterAttribute(Type type)
        {
            this.ConverterType = type;

        }

    }
    public abstract class Converter
    {
        protected internal abstract IDictionary<string, object> BuildJsonDictionary(object value);

        public abstract string SerializeToJson(object inputObject);
    }
    public class StringEnumConverter : Converter
    {
        protected internal override IDictionary<string, object> BuildJsonDictionary(object value)
        {

            IDictionary<string, object> jsonDictionary = new Dictionary<string, object>();
            Type objectType = value.GetType();

            FieldInfo member = objectType.GetField(value.ToString(), BindingFlags.Static | BindingFlags.Public);

            var attrList = member.GetCustomAttributes(typeof(EnumMemberAttribute), true);
            var enumMember = attrList.OfType<EnumMemberAttribute>().FirstOrDefault();

            string val = enumMember != null ? enumMember.Value : value.ToString();

            jsonDictionary.Add(value.GetType().Name, val);

            return jsonDictionary;
        }


        public override string SerializeToJson(object inputObject)
        {
            var attrList = inputObject.GetType().GetCustomAttributes(false);
            var listAttr = attrList.ToList();
            FlagsAttribute flagAttr = attrList.Count() != 0 ? (FlagsAttribute)listAttr.Find(item => item.GetType() == typeof(FlagsAttribute)) : null;
            bool flag = (flagAttr != null) ? true : false;
            if (flag)
            {
                int value = (int)inputObject;
                return value.ToString();
            }
            else
            {
                IDictionary<string, object> enumDictionary = BuildJsonDictionary(inputObject);
                object enumValue = enumDictionary.First().Value;
                string enumstring = "\"" + enumValue.ToString() + "\"";
                return enumstring;
            }
        }
    }

    [Serializable]
    public class TestClass
    {
        private EditingType _editingType = EditingType.StringEdit;

        [JsonProperty("editType")]
        [DefaultValue(EditingType.StringEdit)]
        [JsonConverter(typeof(StringEnumConverter))]
        public EditingType EditType
        {
            get { return _editingType; }
            set { _editingType = value; }
        }
    }
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            JsonConvert.DeserializeObject("{\"editType\":\"stringedit\"}", typeof(TestClass));
            return View();
        }

        public IActionResult About()
        {
            ViewData["Message"] = "Your application description page.";

            return View();
        }

        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
        }

        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
