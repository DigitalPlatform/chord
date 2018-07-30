using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DigitalPlatform.SIP2
{
    public class VariableLengthField 
    {
        //public string Name { get; set; }
        public string ID { get; set; }
        public bool IsRequired { get; set; }
        public string Value { get; set; }

        public VariableLengthField (string id, bool required)
        {
            //this.Name = name;
            this.ID = id;
            this.IsRequired = required;
        }

        public VariableLengthField(string id, bool required,string value)
        {
            //this.Name = name;
            this.ID = id;
            this.IsRequired = required;
            this.Value = value;
        }

    }
}
