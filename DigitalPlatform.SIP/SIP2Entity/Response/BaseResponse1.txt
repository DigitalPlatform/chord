using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DigitalPlatform.SIP2.SIP2Entity
{
    public abstract class BaseResponse1 : SIP2Message
    {
        protected void ResponseInit(string command)
        {
            if (!string.IsNullOrEmpty(command.Trim()))
            {
                string[] fields = command.Split(new string[]
                {
                    SIPConst.FIELD_TERMINATOR
                },
                StringSplitOptions.RemoveEmptyEntries);
                foreach (string field in fields)
                {
                    string strField = field.Trim();
                    if (string.IsNullOrEmpty(strField) || strField.Length < 2)
                        continue;

                    string strFieldID = strField.Substring(0, 2);
                    string strFieldData = strField.Substring(2);
                    if (string.IsNullOrEmpty(strFieldData))
                        continue;

                    switch (strFieldID)
                    {
                        case "BD":
                            this._homeAddress = ConcatBySeparator(this._homeAddress, "|", strFieldData);
                            break;
                        case "BE":
                            this._emailAddress = ConcatBySeparator(this._emailAddress, "|", strFieldData);
                            break;
                        case "BF":
                            this._homePhoneNumber = ConcatBySeparator(this._homePhoneNumber, "|", strFieldData);
                            break;
                        case "BG":
                            this._owner = ConcatBySeparator(this._owner, "|", strFieldData);
                            break;
                        case "AM":
                            this._libraryName = ConcatBySeparator(this._libraryName, "|", strFieldData);
                            break;
                        case "AL":
                            this._blockedCardMsg = ConcatBySeparator(this._blockedCardMsg, "|", strFieldData);
                            break;
                        case "AN":
                            this._terminalLocation = ConcatBySeparator(this._terminalLocation, "|", strFieldData);
                            break;
                        case "BM":
                            this._renewedItems = ConcatBySeparator(this._renewedItems, "|", strFieldData);
                            break;
                        case "BO":
                            this._feeAcknowledged = ParseSIPChar(strFieldData);
                            break;
                        case "AO":
                            this._institutionId = ConcatBySeparator(this._institutionId, "|", strFieldData);
                            break;
                        case "BL":
                            this._validPatron = ParseSIPChar(strFieldData);
                            break;
                        case "BN":
                            this._unrenewedItems = ConcatBySeparator(this._unrenewedItems, "|", strFieldData);
                            break;
                        case "AH":
                            this._dueDate = ConcatBySeparator(this._dueDate, "|", strFieldData);
                            break;
                        case "AJ":
                            this._titleIdentifier = ConcatBySeparator(this._titleIdentifier, "|", strFieldData);
                            break;
                        case "BI":
                            this._cancel = ParseSIPChar(strFieldData);
                            break;
                        case "BH":
                            this._currencyType = ConcatBySeparator(this._currencyType, "|", strFieldData);
                            break;
                        case "AD":
                            this._patronPassword = ConcatBySeparator(this._patronPassword, "|", strFieldData);
                            break;
                        case "BK":
                            this._transactionId = ConcatBySeparator(this._transactionId, "|", strFieldData);
                            break;
                        case "BU":
                            this._recallItems = ConcatBySeparator(this._recallItems, "|", strFieldData);
                            break;
                        case "AE":
                            this._personalName = ConcatBySeparator(this._personalName, "|", strFieldData);
                            break;
                        case "AG":
                            this._printLine = ConcatBySeparator(this._printLine, "|", strFieldData);
                            break;
                        case "BT":
                            this._feeType = ConcatBySeparator(this._feeType, "|", strFieldData);
                            break;
                        case "AF":
                            this._screenMessage = ConcatBySeparator(this._screenMessage, "|", strFieldData);
                            break;
                        case "BV":
                            this._feeAmount = ConcatBySeparator(this._feeAmount, "|", strFieldData);
                            break;
                        case "BW":
                            this._expirationDate = ConcatBySeparator(this._expirationDate, "|", strFieldData);
                            break;
                        case "AB":
                            this._itemIdentifier = ConcatBySeparator(this._itemIdentifier, "|", strFieldData);
                            break;
                        case "BQ":
                            this._endItem = ConcatBySeparator(this._endItem, "|", strFieldData);
                            break;
                        case "AA":
                            this._patronIdentifier = ConcatBySeparator(this._patronIdentifier, "|", strFieldData);
                            break;
                        case "AC":
                            this._terminalPassword = ConcatBySeparator(this._terminalPassword, "|", strFieldData);
                            break;
                        case "BP":
                            this._startItem = ConcatBySeparator(this._startItem, "|", strFieldData);
                            break;
                        case "BR":
                            this._queuePosition = ConcatBySeparator(this._queuePosition, "|", strFieldData);
                            break;
                        case "BS":
                            this._pickupLocation = ConcatBySeparator(this._pickupLocation, "|", strFieldData);
                            break;
                        case "BY":
                            this._holdType = ParseSIPChar(strFieldData);
                            break;
                        case "AY":
                            this._sequenceNumber = ParseSIPChar(strFieldData);
                            break;
                        case "BX":
                            this._supportedMessages = ConcatBySeparator(this._supportedMessages, "|", strFieldData);
                            break;
                        case "AZ":
                            this._checksum = strFieldData;
                            break;
                        case "BZ":
                            this._chargedItemsLimit = strFieldData;
                            break;
                        case "AT":
                            this._overdueItems = ConcatBySeparator(this._overdueItems, ",", strFieldData);
                            break;
                        case "AV":
                            this._fineItems = ConcatBySeparator(this._fineItems, ",", strFieldData);
                            break;
                        case "AU":
                            this._chargedItems = ConcatBySeparator(this._chargedItems, ",", strFieldData);
                            break;
                        case "AQ":
                            this._permanentLocation = ConcatBySeparator(this._permanentLocation, "|", strFieldData);
                            break;
                        case "AP":
                            this._currentLocation = ConcatBySeparator(this._currentLocation, "|", strFieldData);
                            break;
                        case "AS":
                            this._holdItems = ConcatBySeparator(this._holdItems, ",", strFieldData);
                            break;
                        case "CP":
                            this._locationCode = ConcatBySeparator(this._locationCode, "|", strFieldData);
                            break;
                        case "CQ":
                            this._validPatronPassword = ParseSIPChar(strFieldData);
                            break;
                        case "CC":
                            this._feeLimit = strFieldData;
                            break;
                        case "CB":
                            this._holdItemsLimit = strFieldData;
                            break;
                        case "CG":
                            this._feeIdentifier = ConcatBySeparator(this._feeIdentifier, "|", strFieldData);
                            break;
                        case "CF":
                            this._holdQueueLength = ConcatBySeparator(this._holdQueueLength, "|", strFieldData);
                            break;
                        case "CA":
                            this._overdueItemsLimit = strFieldData;
                            break;
                        case "CD":
                            this._unavailableHoldItems = ConcatBySeparator(this._unavailableHoldItems, "|", strFieldData);
                            break;
                        case "CH":
                            this._itemProperties = ConcatBySeparator(this._itemProperties, "|", strFieldData);
                            break;
                        case "CK":
                            this._mediaType = ConcatBySeparator(this._mediaType, "|", strFieldData);
                            break;
                        case "CJ":
                            this._recallDate = ConcatBySeparator(this._recallDate, "|", strFieldData);
                            break;
                        case "CN":
                            this._loginUserId = ConcatBySeparator(this._loginUserId, "|", strFieldData);
                            break;
                        case "CI":
                            this._securityInhibit = ParseSIPChar(strFieldData);
                            break;
                        case "CM":
                            this._holdPickupDate = ConcatBySeparator(this._holdPickupDate, "|", strFieldData);
                            break;
                        case "CL":
                            this._sortBin = ConcatBySeparator(this._sortBin, "|", strFieldData);
                            break;
                        case "CO":
                            this._loginPassword = ConcatBySeparator(this._loginPassword, "|", strFieldData);
                            break;
                    }
                }
            }
        }

        public static string ConcatBySeparator(
            string input,
            string separator,
            string value)
        {
            if (string.IsNullOrEmpty(input))
                return value;
            else
                return string.Concat(input, separator, value);
        }
    }
}
