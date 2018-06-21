using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Text;
using System.Diagnostics;

using System.Drawing;


namespace DigitalPlatform
{

	// byte[] �����ʵ�ú�����
	public class ByteArray
	{
        /*
        // ����һ��byte����
        public static byte[] Dup(byte [] source)
        {
            if (source == null)
                return null;

            byte [] result = null;
            result = EnsureSize(result, source.Length);

            Array.Copy(source, 0, result, 0, source.Length);

            return result;
        }*/

		// ��¡һ���ַ�����
		public static byte[] GetCopy(byte[] baContent)
		{
			if (baContent == null)
				return null;
			byte [] baResult = new byte[baContent.Length];
			Array.Copy(baContent, 0, baResult, 0, baContent.Length);
			return baResult;
		}

		// ��byte[]ת��Ϊ�ַ������Զ�̽����뷽ʽ
		public static string ToString(byte [] baContent)
		{
			ArrayList encodings = new ArrayList();

			encodings.Add(Encoding.UTF8);
			encodings.Add(Encoding.Unicode);

			for(int i=0;i<encodings.Count;i++)
			{
				Encoding encoding = (Encoding)encodings[i];

				byte [] Preamble = encoding.GetPreamble();

				if (baContent.Length < Preamble.Length)
					continue;

				if (ByteArray.Compare(baContent, Preamble, Preamble.Length) == 0)
					return encoding.GetString(baContent,
						Preamble.Length,
						baContent.Length - Preamble.Length);
			}

			// ȱʡ����UTF8
			return Encoding.UTF8.GetString(baContent);
		}

		// byte[] �� �ַ���
		public static string ToString(byte[] bytes,
			Encoding encoding)
		{
			int nIndex = 0;
			int nCount = bytes.Length;
			byte[] baPreamble = encoding.GetPreamble();
			if (baPreamble != null
				&& baPreamble.Length != 0
				&& bytes.Length >= baPreamble.Length)
			{
				byte[] temp = new byte[baPreamble.Length];
				Array.Copy(bytes,
					0,
					temp,
					0,
					temp.Length);

				bool bEqual = true;
				for(int i=0;i<temp.Length;i++)
				{
					if (temp[i] != baPreamble[i])
					{
						bEqual = false;
						break;
					}
				}

				if (bEqual == true)
				{
					nIndex = temp.Length;
					nCount = bytes.Length - temp.Length;
				}
			}

			return encoding.GetString(bytes,
				nIndex,
				nCount);
		}

		// �Ƚ�����byte[]�����Ƿ���ȡ�
		// parameter:
		//		timestamp1: ��һ��byte[]����
		//		timestamp2: �ڶ���byte[]����
		// return:
		//		0   ���
		//		���ڻ���С��0   ���ȡ��ȱȽϳ��ȡ�������ȣ�������ַ������
		public static int Compare(
			byte[] bytes1,
			byte[] bytes2)
		{
			if (bytes1 == null	&& bytes2 == null)
				return 0;
			if (bytes1 == null)
				return -1;
			if (bytes2 == null)
				return 1;

			int nDelta = bytes1.Length - bytes2.Length;
			if (nDelta != 0)
				return nDelta;

			for(int i=0;i<bytes1.Length;i++)
			{
				nDelta = bytes1[i] - bytes2[i];
				if (nDelta != 0)
					return nDelta;
			}

			return 0;
		}

		// �Ƚ�����byte����ľֲ�
		public static int Compare(
			byte[] bytes1,
			byte[] bytes2, 
			int nLength)
		{
			if (bytes1.Length < nLength || bytes2.Length < nLength)
				return Compare(bytes1, bytes2, Math.Min(bytes1.Length, bytes2.Length));

			for(int i=0;i<nLength;i++)
			{
				int nDelta = bytes1[i] - bytes2[i];
				if (nDelta != 0)
					return nDelta;
			}

			return 0;
		}

		public static int IndexOf(byte [] source,
			byte v,
			int nStartPos)
		{
			for(int i=nStartPos;i<source.Length;i++)
			{
				if (source[i] == v)
					return i;
			}
			return -1;
		}

		// ȷ������ߴ��㹻
		public static byte [] EnsureSize(byte [] source,
			int nSize)
		{
			if (source == null) 
			{
				return new byte[nSize];
			}

			if (source.Length < nSize) 
			{
				byte [] temp = new byte [nSize];
				Array.Copy(source, 
					0,
					temp,
					0,
					source.Length);
				return temp;	// �ߴ粻�����Ѿ����·��䣬���Ҽ̳���ԭ������
			}

			return source;	// �ߴ��㹻
		}


		// �ڻ�����β��׷��һ���ֽ�
		public static byte[] Add(byte[] source,
			byte v)
		{
			int nIndex = -1;
			if (source != null) 
			{
				nIndex = source.Length;
				source = EnsureSize(source, source.Length + 1);
			}
			else 
			{
				nIndex = 0;
				source = EnsureSize(source, 1);
			}

			source[nIndex] = v;

			return source;
		}

		// �ڻ�����β��׷�������ֽ�
		public static byte[] Add(byte[] source,
			byte[] v)
		{
			int nIndex = -1;
			if (source != null) 
			{
				nIndex = source.Length;
				source = EnsureSize(source, source.Length + v.Length);
			}
			else 
			{
                // 2011/1/22
                if (v == null)
                    return null;
				nIndex = 0;
				source = EnsureSize(source, v.Length);
			}

			Array.Copy(v,0,source, nIndex, v.Length);

			return source;
		}

        // 2011/9/12
        // �ڻ�����β��׷�������ֽ�
        public static byte[] Add(byte[] source,
            byte[] v,
            int nLength)
        {
            Debug.Assert(v.Length >= nLength, "");

            int nIndex = -1;
            if (source != null)
            {
                nIndex = source.Length;
                source = EnsureSize(source, source.Length + nLength);
            }
            else
            {
                if (v == null)
                    return null;
                nIndex = 0;
                source = EnsureSize(source, nLength);
            }

            Array.Copy(v, 0, source, nIndex, nLength);

            return source;
        }

        // �� source ͷ������һ�Ρ�source ��󱻸ı�
        public static byte [] Remove(ref byte [] source, int length)
        {
            if (length > source.Length)
                length = source.Length;

            byte [] result = new byte [length];
            Array.Copy(source, result, length);
            int rest = source.Length - length;
            byte[] temp = new byte[rest];
            if (rest > 0)
                Array.Copy(source, length, temp, 0, rest);

            source = temp;
            return result;
        }

		// �õ���16���Ʊ�ʾ��ʱ����ַ���
		public static string GetHexTimeStampString(byte [] baTimeStamp)
		{
			if (baTimeStamp == null)
				return "";
			string strText = "";
			for(int i=0;i<baTimeStamp.Length;i++) 
			{
				//string strHex = String.Format("{0,2:X}",baTimeStamp[i]);
				string strHex = Convert.ToString(baTimeStamp[i], 16);
				strText +=  strHex.PadLeft(2, '0');
			}

			return strText;
		}

		// �õ�byte[]���͵�ʱ���
		public static byte[] GetTimeStampByteArray(string strHexTimeStamp)
		{
			if (string.IsNullOrEmpty(strHexTimeStamp) == true)
				return null;

			byte [] result = new byte[strHexTimeStamp.Length / 2];

			for(int i=0;i<strHexTimeStamp.Length / 2;i++)
			{
				string strHex = strHexTimeStamp.Substring(i*2, 2);
				result[i] = Convert.ToByte(strHex, 16);

			}

			return result;
		}
	}


}



