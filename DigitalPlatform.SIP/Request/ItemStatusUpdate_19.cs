using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DigitalPlatform.SIP2.Request
{
    /*
     2.00 Item Status Update
     This message can be used to send item information to the ACS, without having to do a Checkout or Checkin operation.  The item properties could be stored on the ACS’s database.  The ACS should respond with an Item Status Update Response message.
     19<transaction date><institution id><item identifier><terminal password><item properties>
     19	18-char	AO	AB	AC	CH
     */
    public class ItemStatusUpdate_19 : BaseMessage
    {
        public ItemStatusUpdate_19()
        {
            this.CommandIdentifier = "19";

            //==前面的定长字段
            //<transaction date>
            //18-char	
            this.FixedLengthFields.Add(new FixedLengthField(SIPConst.F_TransactionDate, 18));

            //==后面变长字段
            //<institution id><item identifier><terminal password><item properties>
            //	AO	AB	AC	CH
            this.VariableLengthFields.Add(new VariableLengthField(SIPConst.F_AO_InstitutionId, true));
            this.VariableLengthFields.Add(new VariableLengthField(SIPConst.F_AB_ItemIdentifier, true));
            this.VariableLengthFields.Add(new VariableLengthField(SIPConst.F_AC_TerminalPassword, false ));
            this.VariableLengthFields.Add(new VariableLengthField(SIPConst.F_CH_ItemProperties, true));
        }

        
        // 18-char, fixed-length required field:  YYYYMMDDZZZZHHMMSS
        public string TransactionDate_18
        {
            get
            {
                return this.GetFixedFieldValue(SIPConst.F_TransactionDate);
            }
            set
            {
                if (value.Length != 18)
                    throw new Exception("transaction date参数长度须为18位。");

                this.SetFixedFieldValue(SIPConst.F_TransactionDate, value);
            }
        }

        // variable-length required field
        public string AO_InstitutionId_r
        {
            get
            {
                return this.GetVariableFieldValue(SIPConst.F_AO_InstitutionId);
            }
            set
            {
                this.SetVariableFieldValue(SIPConst.F_AO_InstitutionId, value);
            }
        }

        //  variable-length required field
        public string AB_ItemIdentifier_r
        {
            get
            {
                return this.GetVariableFieldValue(SIPConst.F_AB_ItemIdentifier);
            }
            set
            {
                this.SetVariableFieldValue(SIPConst.F_AB_ItemIdentifier, value);
            }
        }

        //  variable-length optional field
        public string AC_TerminalPassowrd_o
        {
            get
            {
                return this.GetVariableFieldValue(SIPConst.F_AC_TerminalPassword);
            }
            set
            {
                this.SetVariableFieldValue(SIPConst.F_AC_TerminalPassword, value);
            }
        }

        //  variable-length required field
        public string CH_ItemProperties_r
        {
            get
            {
                return this.GetVariableFieldValue(SIPConst.F_CH_ItemProperties);
            }
            set
            {
                this.SetVariableFieldValue(SIPConst.F_CH_ItemProperties, value);
            }
        }
        
    }
}
