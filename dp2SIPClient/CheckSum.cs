/**************************************************************
 * 
 *  (c) 2017 Mark Lesniak - Nice and Nerdy LLC
 *  
 * 
 *  Implementation of the Standard Interchange Protocol version 
 *  2.0.  Used to standardize queries across multiple database
 *  architectures.  
 *  
 *  Permission is hereby granted, free of charge, to any person obtaining a copy
 *  of this software and associated documentation files (the "Software"), to deal
 *  in the Software without restriction, including without limitation the rights
 *  to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 *  copies of the Software, and to permit persons to whom the Software is
 *  furnished to do so, subject to the following conditions:
 *
 *  The above copyright notice and this permission notice shall be included in
 *  all copies or substantial portions of the Software.
 *
 *  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 *  IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 *  FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 *  AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 *  LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 *  OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
 *  THE SOFTWARE.
 *  
 * 
**************************************************************/


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SIP2
{
    //  Adapted from VB.NET from the Library Tech Guy blog
    //  http://librarytechguy.blogspot.com/2009/11/sip2-checksum_13.html

    public class CheckSum
    {
        public static string ApplyChecksum(string strMsg)
        {
            int intCtr;
            char[] chrArray;
            int intAscSum;
            bool blnCarryBit;
            string strBinVal = String.Empty;
            string strInvBinVal;
            string strNewBinVal = String.Empty;

            // Transfer SIP message to a a character array.Loop through each character of the array,
            // converting the character to an ASCII value and adding the value to a running total.

            intAscSum = 0;
            chrArray = strMsg.ToCharArray();

            for (intCtr = 0; intCtr <= chrArray.Length - 1; intCtr++)
            {
                intAscSum = intAscSum + (chrArray[intCtr]);
            }

            // Next, convert ASCII sum to a binary digit by: 
            // 1) taking the remainder of the ASCII sum divided by 2 
            // 2) Repeat until sum reaches 0 
            // 3) Pad to 16 digits with leading zeroes 

            do
            {
                strBinVal = (intAscSum % 2).ToString() + strBinVal;
                intAscSum = intAscSum / 2;
            } while (intAscSum > 0);

            strBinVal = strBinVal.PadLeft(16, '0');

            // Next, invert all bits in binary number. 
            chrArray = strBinVal.ToCharArray();
            strInvBinVal = "";

            for (intCtr = 0; intCtr <= chrArray.Length - 1; intCtr++)
            {
                if (chrArray[intCtr] == '0') { strInvBinVal = strInvBinVal + '1'; }
                else { strInvBinVal = strInvBinVal + '0'; }
            }


            // Next, add 1 to the inverted binary digit. Loop from least significant digit (rightmost) to most (leftmost); 
            // if digit is 1, flip to 0 and retain carry bit to next significant digit. 

            blnCarryBit = true;
            chrArray = strInvBinVal.ToCharArray();

            for (intCtr = chrArray.Length - 1; intCtr >= 0; intCtr--)
            {
                if (blnCarryBit == true)
                {
                    if (chrArray[intCtr] == '0')
                    {
                        chrArray[intCtr] = '1';
                        blnCarryBit = false;
                    }
                    else
                    {
                        chrArray[intCtr] = '0';
                        blnCarryBit = true;
                    }
                }
                strNewBinVal = chrArray[intCtr] + strNewBinVal;
            }

            // Finally, convert binary digit to hex value, append to original SIP message. 
            return  (Convert.ToInt16(strNewBinVal, 2)).ToString("X");
        }
    }


}
