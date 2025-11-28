using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace S7CommPlusDriver.ClientApi;

public class PlcTagBoolArray : PlcTag
{
    private bool[] m_Value;

    public bool[] Value
    {
        get { return m_Value; }
        set { m_Value = value; }
    }

    public PlcTagBoolArray(string name, ItemAddress address, uint softdatatype) : base(name, address, softdatatype)
    {
        m_Value = new bool[1];
    }

    public override void ProcessReadResult(object valueObj, ulong error)
    {
        LastReadError = error;
        if (CheckErrorAndType(error, valueObj, typeof(ValueBoolArray)) == 0)
        {
            Value = ((ValueBoolArray)valueObj).GetValue();

            Quality = PlcTagQC.TAG_QUALITY_GOOD;
        }
        else
        {
            Quality = PlcTagQC.TAG_QUALITY_BAD;
        }
    }

    public override PValue GetWriteValue()
    {
        return new ValueBoolArray(Value);
    }

    public override string ToString()
    {
        var val = new ValueBoolArray(Value);
        return ResultString(this, val.ToString());
    }
}

public class PlcTagByteArray : PlcTag
{
    private byte[] m_Value;

    public byte[] Value
    {
        get { return m_Value; }
        set { m_Value = value; }
    }

    public PlcTagByteArray(string name, ItemAddress address, uint softdatatype) : base(name, address, softdatatype)
    {
        m_Value = new byte[1];
    }

    public override void ProcessReadResult(object valueObj, ulong error)
    {
        LastReadError = error;
        if (CheckErrorAndType(error, valueObj, typeof(ValueByteArray)) == 0)
        {
            Value = ((ValueByteArray)valueObj).GetValue();

            Quality = PlcTagQC.TAG_QUALITY_GOOD;
        }
        else
        {
            Quality = PlcTagQC.TAG_QUALITY_BAD;
        }
    }

    public override PValue GetWriteValue()
    {
        return new ValueByteArray(Value);
    }

    public override string ToString()
    {
        var val = new ValueByteArray(Value);
        return ResultString(this, val.ToString());
    }
}

public class PlcTagWordArray : PlcTag
{
    private ushort[] m_Value;

    public ushort[] Value
    {
        get { return m_Value; }
        set { m_Value = value; }
    }

    public PlcTagWordArray(string name, ItemAddress address, uint softdatatype) : base(name, address, softdatatype)
    {
        m_Value = new ushort[1];
    }

    public override void ProcessReadResult(object valueObj, ulong error)
    {
        LastReadError = error;
        if (CheckErrorAndType(error, valueObj, typeof(ValueWordArray)) == 0)
        {
            Value = ((ValueWordArray)valueObj).GetValue();

            Quality = PlcTagQC.TAG_QUALITY_GOOD;
        }
        else
        {
            Quality = PlcTagQC.TAG_QUALITY_BAD;
        }
    }

    public override PValue GetWriteValue()
    {
        return new ValueWordArray(Value);
    }

    public override string ToString()
    {
        var val = new ValueWordArray(Value);
        return ResultString(this, val.ToString());
    }
}

public class PlcTagIntArray : PlcTag
{
    private short[] m_Value;

    public short[] Value
    {
        get { return m_Value; }
        set { m_Value = value; }
    }

    public PlcTagIntArray(string name, ItemAddress address, uint softdatatype) : base(name, address, softdatatype)
    {
        m_Value = new short[1];
    }

    public override void ProcessReadResult(object valueObj, ulong error)
    {
        LastReadError = error;
        if (CheckErrorAndType(error, valueObj, typeof(ValueIntArray)) == 0)
        {
            Value = ((ValueIntArray)valueObj).GetValue();

            Quality = PlcTagQC.TAG_QUALITY_GOOD;
        }
        else
        {
            Quality = PlcTagQC.TAG_QUALITY_BAD;
        }
    }

    public override PValue GetWriteValue()
    {
        return new ValueIntArray(Value);
    }

    public override string ToString()
    {
        var val = new ValueIntArray(Value);
        return ResultString(this, val.ToString());
    }
}

public class PlcTagDWordArray : PlcTag
{
    private uint[] m_Value;

    public uint[] Value
    {
        get { return m_Value; }
        set { m_Value = value; }
    }

    public PlcTagDWordArray(string name, ItemAddress address, uint softdatatype) : base(name, address, softdatatype)
    {
        m_Value = new uint[1];
    }

    public override void ProcessReadResult(object valueObj, ulong error)
    {
        LastReadError = error;
        if (CheckErrorAndType(error, valueObj, typeof(ValueDWordArray)) == 0)
        {
            Value = ((ValueDWordArray)valueObj).GetValue();

            Quality = PlcTagQC.TAG_QUALITY_GOOD;
        }
        else
        {
            Quality = PlcTagQC.TAG_QUALITY_BAD;
        }
    }

    public override PValue GetWriteValue()
    {
        return new ValueDWordArray(Value);
    }

    public override string ToString()
    {
        var val = new ValueDWordArray(Value);
        return ResultString(this, val.ToString());
    }
}

public class PlcTagDIntArray : PlcTag
{
    private int[] m_Value;

    public int[] Value
    {
        get { return m_Value; }
        set { m_Value = value; }
    }

    public PlcTagDIntArray(string name, ItemAddress address, uint softdatatype) : base(name, address, softdatatype)
    {
        m_Value = new int[1];
    }

    public override void ProcessReadResult(object valueObj, ulong error)
    {
        LastReadError = error;
        if (CheckErrorAndType(error, valueObj, typeof(ValueDIntArray)) == 0)
        {
            Value = ((ValueDIntArray)valueObj).GetValue();

            Quality = PlcTagQC.TAG_QUALITY_GOOD;
        }
        else
        {
            Quality = PlcTagQC.TAG_QUALITY_BAD;
        }
    }

    public override PValue GetWriteValue()
    {
        return new ValueDIntArray(Value);
    }

    public override string ToString()
    {
        var val = new ValueDIntArray(Value);
        return ResultString(this, val.ToString());
    }
}

public class PlcTagRealArray : PlcTag
{
    private float[] m_Value;

    public float[] Value
    {
        get { return m_Value; }
        set { m_Value = value; }
    }

    public PlcTagRealArray(string name, ItemAddress address, uint softdatatype) : base(name, address, softdatatype)
    {
        m_Value = new float[1];
    }

    public override void ProcessReadResult(object valueObj, ulong error)
    {
        LastReadError = error;
        if (CheckErrorAndType(error, valueObj, typeof(ValueRealArray)) == 0)
        {
            Value = ((ValueRealArray)valueObj).GetValue();

            Quality = PlcTagQC.TAG_QUALITY_GOOD;
        }
        else
        {
            Quality = PlcTagQC.TAG_QUALITY_BAD;
        }
    }

    public override PValue GetWriteValue()
    {
        return new ValueRealArray(Value);
    }

    public override string ToString()
    {
        var val = new ValueRealArray(Value);
        return ResultString(this, val.ToString());
    }
}

public class PlcTagUSIntArray : PlcTag
{
    private byte[] m_Value;

    public byte[] Value
    {
        get { return m_Value; }
        set { m_Value = value; }
    }

    public PlcTagUSIntArray(string name, ItemAddress address, uint softdatatype) : base(name, address, softdatatype)
    {
        m_Value = new byte[1];
    }

    public override void ProcessReadResult(object valueObj, ulong error)
    {
        LastReadError = error;
        if (CheckErrorAndType(error, valueObj, typeof(ValueUSIntArray)) == 0)
        {
            Value = ((ValueUSIntArray)valueObj).GetValue();

            Quality = PlcTagQC.TAG_QUALITY_GOOD;
        }
        else
        {
            Quality = PlcTagQC.TAG_QUALITY_BAD;
        }
    }

    public override PValue GetWriteValue()
    {
        return new ValueUSIntArray(Value);
    }

    public override string ToString()
    {
        var val = new ValueUSIntArray(Value);
        return ResultString(this, val.ToString());
    }
}

public class PlcTagUIntArray : PlcTag
{
    private ushort[] m_Value;

    public ushort[] Value
    {
        get { return m_Value; }
        set { m_Value = value; }
    }

    public PlcTagUIntArray(string name, ItemAddress address, uint softdatatype) : base(name, address, softdatatype)
    {
        m_Value = new ushort[1];
    }

    public override void ProcessReadResult(object valueObj, ulong error)
    {
        LastReadError = error;
        if (CheckErrorAndType(error, valueObj, typeof(ValueUIntArray)) == 0)
        {
            Value = ((ValueUIntArray)valueObj).GetValue();

            Quality = PlcTagQC.TAG_QUALITY_GOOD;
        }
        else
        {
            Quality = PlcTagQC.TAG_QUALITY_BAD;
        }
    }

    public override PValue GetWriteValue()
    {
        return new ValueUIntArray(Value);
    }

    public override string ToString()
    {
        var val = new ValueUIntArray(Value);
        return ResultString(this, val.ToString());
    }
}

public class PlcTagUDIntArray : PlcTag
{
    private uint[] m_Value;

    public uint[] Value
    {
        get { return m_Value; }
        set { m_Value = value; }
    }

    public PlcTagUDIntArray(string name, ItemAddress address, uint softdatatype) : base(name, address, softdatatype)
    {
        m_Value = new uint[1];
    }

    public override void ProcessReadResult(object valueObj, ulong error)
    {
        LastReadError = error;
        if (CheckErrorAndType(error, valueObj, typeof(ValueUDIntArray)) == 0)
        {
            Value = ((ValueUDIntArray)valueObj).GetValue();

            Quality = PlcTagQC.TAG_QUALITY_GOOD;
        }
        else
        {
            Quality = PlcTagQC.TAG_QUALITY_BAD;
        }
    }

    public override PValue GetWriteValue()
    {
        return new ValueUDIntArray(Value);
    }

    public override string ToString()
    {
        var val = new ValueUDIntArray(Value);
        return ResultString(this, val.ToString());
    }
}

public class PlcTagSIntArray : PlcTag
{
    private sbyte[] m_Value;

    public sbyte[] Value
    {
        get { return m_Value; }
        set { m_Value = value; }
    }

    public PlcTagSIntArray(string name, ItemAddress address, uint softdatatype) : base(name, address, softdatatype)
    {
        m_Value = new sbyte[1];
    }

    public override void ProcessReadResult(object valueObj, ulong error)
    {
        LastReadError = error;
        if (CheckErrorAndType(error, valueObj, typeof(ValueSIntArray)) == 0)
        {
            Value = ((ValueSIntArray)valueObj).GetValue();

            Quality = PlcTagQC.TAG_QUALITY_GOOD;
        }
        else
        {
            Quality = PlcTagQC.TAG_QUALITY_BAD;
        }
    }

    public override PValue GetWriteValue()
    {
        return new ValueSIntArray(Value);
    }

    public override string ToString()
    {
        var val = new ValueSIntArray(Value);
        return ResultString(this, val.ToString());
    }
}

public class PlcTagDateAndTimeArray : PlcTag
{
    /* BCD coded:
        * YYMMDDhhmmssuuuQ
        * uuu = milliseconds
        * Q = Weekday 1=Su, 2=Mo, 3=Tu, 4=We, 5=Th, 6=Fr, 7=Sa
        */
    private DateTime[] m_Value;

    public DateTime[] Value
    {
        get
        {
            return m_Value;
        }

        set
        {
            bool dataOk = true;
            foreach (var item in value)
            {
                if (item < new DateTime(1990, 1, 1) && item >= new DateTime(2090, 1, 1))
                {
                    dataOk = false;
                    break;
                }
            }
            if (dataOk)
            {
                m_Value = value;
            }
            else
            {
                throw new ArgumentOutOfRangeException("Value", "DateTime must be >= 1990-01-01 and < 2090-01-01");
            }
        }
    }

    public PlcTagDateAndTimeArray(string name, ItemAddress address, uint softdatatype) : base(name, address, softdatatype)
    {
        Value = new DateTime[0];
    }

    public override void ProcessReadResult(object valueObj, ulong error)
    {
        LastReadError = error;
        if (CheckErrorAndType(error, valueObj, typeof(ValueUSIntArray)) == 0)
        {
            List<DateTime> dateTimes = new List<DateTime>();
            var v = ((ValueUSIntArray)valueObj).GetValue();
            int pos = 0;
            do
            {
                int[] ts = new int[8];
                for (int i = 0; i < 7; i++)
                {
                    ts[i] = BcdByteToInt(v[pos + i]);
                }
                // The left nibble of the last byte contains the LSD of milliseconds,
                // the right nibble the weekday (which we don't process here).
                ts[7] = v[7] >> 4;

                int year;
                if (ts[0] >= 90)
                {
                    year = 1900 + ts[0];
                }
                else
                {
                    year = 2000 + ts[0];
                }
                var value = new DateTime(year, ts[1], ts[2], ts[3], ts[4], ts[5]);
                value = value.AddMilliseconds(ts[6] * 10 + ts[7]);
                dateTimes.Add(value);
                pos += 8;
            } while (pos < v.Length);
            Value = dateTimes.ToArray();
            Quality = PlcTagQC.TAG_QUALITY_GOOD;
        }
        else
        {
            Quality = PlcTagQC.TAG_QUALITY_BAD;
        }
    }

    public override PValue GetWriteValue()
    {
        var byteStrings = new List<byte>();
        foreach (var item in Value)
        {
            int[] ts = new int[8];
            byte[] b = new byte[8];
            if (item.Year < 2000)
            {
                // 90-99 = 1990-1999
                ts[0] = item.Year - 1900;
            }
            else
            {
                // 00-89 = 2000-2089
                ts[0] = item.Year - 2000;
            }
            ts[1] = item.Month;
            ts[2] = item.Day;
            ts[3] = item.Hour;
            ts[4] = item.Minute;
            ts[5] = item.Second;
            ts[6] = item.Millisecond / 10;
            ts[7] = (item.Millisecond % 10) << 4; // Don't set the weekday
            for (int i = 0; i < 7; i++)
            {
                b[i] = IntToBcdByte(ts[i]);
            }
            b[7] = (byte)ts[7];
            byteStrings.AddRange(b);
        }
        return new ValueUSIntArray(byteStrings.ToArray());
    }

    public override string ToString()
    {
        string s = "<Value type =\"DateAndTimeArray\" size=\"" + Value.Length.ToString() + "\">";
        for (int i = 0; i < Value.Length; i++)
        {
            string ts = Value[i].ToString();
            if (Value[i].Millisecond > 0)
            {
                ts += String.Format(".{0:D03}", Value[i].Millisecond);
            }
            s += String.Format("<Value>{0}</Value>", ts);
        }
        s += "</Value>";
        return ResultString(this, s);
    }
}


public class PlcTagStringArray : PlcTag
{
    private string[] m_Value;
    private byte m_MaxLength = 254;
    private string m_Encoding = "ISO-8859-1";

    public string[] Value
    {
        get
        {
            return m_Value;
        }

        set
        {
            bool lengthOk = true;
            foreach (var item in value)
            {
                if (item.Length > m_MaxLength)
                {
                    lengthOk = false;
                    break;
                }
            }
            if (lengthOk)
            {
                m_Value = value;
            }
            else
            {
                throw new ArgumentOutOfRangeException("Value", "String is longer than the allowed max. length of " + m_MaxLength);
            }
        }
    }

    public PlcTagStringArray(string name, ItemAddress address, uint softdatatype, byte maxlength = 254) : base(name, address, softdatatype)
    {
        m_MaxLength = maxlength;
    }

    public override void ProcessReadResult(object valueObj, ulong error)
    {
        LastReadError = error;
        if (CheckErrorAndType(error, valueObj, typeof(ValueUSIntArray)) == 0)
        {
            List<string> strings = new List<string>();
            var v = ((ValueUSIntArray)valueObj).GetValue();
            int pos = 0;
            do
            {
                int max_len = v[pos];
                int act_len = v[pos + 1];
                // IEC 61131-3 states ISO-646 IRV, with optional extensions like "Latin-1 Supplement".
                // Siemens TIA-Portal gives warnings using other than 7 Bit ASCII characters.
                // Let the user define his local encoding via SetStringEncoding().
                var str = Encoding.GetEncoding(m_Encoding).GetString(v, pos + 2, act_len);
                strings.Add(str);
                pos += max_len + 2;

            } while (pos < v.Length);
            Value = strings.ToArray();
            Quality = PlcTagQC.TAG_QUALITY_GOOD;
        }
        else
        {
            Quality = PlcTagQC.TAG_QUALITY_BAD;
        }
    }

    public override PValue GetWriteValue()
    {
        var byteStrings = new List<byte>();
        foreach (var item in Value)
        {
            // Must write the complete array of MaxLength of the string (plus two bytes header).
            byte[] sb = Encoding.GetEncoding(m_Encoding).GetBytes(item);
            var b = new byte[m_MaxLength + 2];
            b[0] = m_MaxLength;
            b[1] = (byte)sb.Length;
            for (int i = 0; i < sb.Length; i++)
            {
                b[i + 2] = sb[i];
            }
            byteStrings.AddRange(b);
        }
        return new ValueUSIntArray(byteStrings.ToArray());
    }

    public void SetStringEncoding(string encoding)
    {
        m_Encoding = encoding;
    }

    public override string ToString()
    {
        string s = "<Value type =\"StringArray\" size=\"" + Value.Length.ToString() + "\">";
        for (int i = 0; i < Value.Length; i++)
        {
            s += String.Format("<Value>{0}</Value>", Value[i]);
        }
        s += "</Value>";
        return ResultString(this, s);
    }
}
