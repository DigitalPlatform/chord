using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DigitalPlatform.SIP2
{
    public class FixedLengthField
    {
        public string Name { get; set; }
        public int Length { get; set; }

        private string _value = "";
        public string Value {
            get
            {
                return _value;
            }
            set 
            {
                if (value == null)
                    throw new Exception("value值不能为null");
                if (value.Length != Length)
                    throw new Exception("value["+value+"]的长度与字段定义的长度["+this.Length+"] 不符");
                this._value = value;
            }
        }


        public FixedLengthField(string name, int length)
        {
            this.Name = name;
            this.Length = length;
        }

    }
}
