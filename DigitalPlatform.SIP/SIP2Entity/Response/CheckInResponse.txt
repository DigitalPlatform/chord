using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DigitalPlatform.SIP2.SIP2Entity
{
    public class CheckInResponse : BaseResponse1
    {
        public char OK
        {
            set
            {
                this._ok = value;
            }
        }

        public char RenewalOk
        {
            set
            {
                this._renewalOk = value;
            }
        }

        public char MagneticMedia
        {
            set
            {
                this._magneticMedia = value;
            }
        }

        public char Desensitize
        {
            set
            {
                this._desensitize = value;
            }
        }

        public string TransactionDate
        {
            set
            {
                this._transactionDate = value;
            }
        }

        public string InstitutionId
        {
            set
            {
                this._itemIdentifier = value;
            }
        }

        public string PatronIdentifier
        {
            set
            {
                this._patronIdentifier = value;
            }
        }

        public string ItemIdentifier
        {
            set
            {
                this._itemIdentifier = value;
            }
        }

        public string TitleIdentifier
        {
            set
            {
                this._titleIdentifier = value;
            }
        }

        public string DueDate
        {
            set
            {
                this._dueDate = value;
            }
        }

        public string FeeType
        {
            set
            {
                this._feeType = value;
            }
        }

        public char SecurityInhibit
        {
            set
            {
                this._securityInhibit = value;
            }
        }

        public string CurrencyType
        {
            set
            {
                this._currencyType = value;
            }
        }

        public string FeeAmount
        {
            set
            {
                this._feeAmount = value;
            }
        }

        public string MediaType
        {
            set
            {
                this._mediaType = value;
            }
        }

        public string PermanentLocation
        {
            set
            {
                this._permanentLocation = value;
            }
        }

        public string ItemProperties
        {
            set
            {
                this._itemProperties = value;
            }
        }

        public string TransactionId
        {
            set
            {
                this._transactionId = value;
            }
        }

        public string ScreenMessage
        {
            get
            {
                return this._screenMessage;
            }

            set
            {
                if (!string.IsNullOrEmpty(this._screenMessage))
                    this._screenMessage += ",";
                this._screenMessage += value;
            }
        }

        public string PrintLine
        {
            get
            {
                return this._printLine;
            }

            set
            {
                this._printLine = value;
            }
        }


        public CheckInResponse()
        {
            this.MessageIdentifier = "10";
            Init();
        }


        public string GetMessage()
        {
            StringBuilder sb = new StringBuilder(1024);
            sb.Append(this.MessageIdentifier);
            sb.Append(this._ok.ToString()).Append(this._resensitize.ToString());
            sb.Append(this._magneticMedia.ToString()).Append(this._alert.ToString());
            sb.Append(this._transactionDate);
            sb.Append("AO").Append(this._institutionId).Append(SIPConst.FIELD_TERMINATOR);
            sb.Append("AA").Append(this._patronIdentifier).Append(SIPConst.FIELD_TERMINATOR);
            sb.Append("AB").Append(this._itemIdentifier).Append(SIPConst.FIELD_TERMINATOR);
            sb.Append("AJ").Append(this._titleIdentifier).Append(SIPConst.FIELD_TERMINATOR);
            sb.Append("AQ").Append(this._permanentLocation).Append(SIPConst.FIELD_TERMINATOR);

            /*
             * sort bin            CL    variable-length optional field
             * patron identifier   AA    variable-length optional field.  ID of the patron who had the item checked out.
             * media type          CK    3-char, fixed-length optional field
             * item properties     CH    variable-length optional field
             */

            sb.Append("AF").Append(this._screenMessage).Append(SIPConst.FIELD_TERMINATOR);
            sb.Append("AG").Append(this._printLine).Append(SIPConst.FIELD_TERMINATOR);
            return sb.ToString();
        }

        private void Init()
        {
            this._ok = '0';
            this._magneticMedia = 'N';
            this._resensitize = 'Y';
            this._alert = 'N';
            this._transactionDate = DateTime.Now.ToString("yyyyMMdd    HHmmss");
        }
    }
}
