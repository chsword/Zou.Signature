using System;
using System.Collections.Generic;
using System.Text;
using Org.Xml.Sax.Helpers;
using Org.Xml.Sax;

namespace Zou.Signature
{
    public class XmlParser : DefaultHandler
    {
       public  List<String> List { get; set; }
        StringBuilder _builder;
        public override void StartDocument()
        {
            List = new List<String>();
        }

        public override void StartElement(string uri, string localName, string qName, IAttributes attributes)
        {
            _builder = new StringBuilder();
            if (localName == "path")
            {
                List.Add(attributes.GetValue("d"));
            }
        }
        public override void EndElement(string uri, string localName, string qName)
        {
            if (localName=="path")
            {

            }
        }
        public override void Characters(char[] ch, int start, int length)
        {
            String tempString = new String(ch, start, length);
            _builder.Append(tempString);
        }
    }
}
