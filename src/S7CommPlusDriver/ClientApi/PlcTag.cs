using System;
using System.Text;

namespace S7CommPlusDriver.ClientApi
{
    /* Ideas for improvement:
     * - Optional initial value in constructor
     * - Uniform the different PLC datatypes on the biggest .Net type,
     *   to reduce the amount of types (e.g. on integer types only 64 bit).
     * Many datatypes differ only by the type in the protocol.
     * But there's some special handling needed for type like String, WString, date/time etc.
     */
    public abstract class PlcTag
    {
        public string Name;
        public ItemAddress Address;
        public short Quality;
        public uint Datatype;

        public ulong LastReadError;
        public ulong LastWriteError;

        public PlcTag(string name, ItemAddress address, uint softdatatype)
        {
            Name = name;
            Address = address;
            Datatype = softdatatype;
            Quality = PlcTagQC.TAG_QUALITY_WAITING_FOR_INITIAL_DATA;
        }

        public abstract void ProcessReadResult(object obj, UInt64 error);

        public virtual void ProcessWriteResult(UInt64 error)
        {
            LastWriteError = error;
        }

        public abstract PValue GetWriteValue();

        protected static int CheckErrorAndType(ulong error, object valueObj, Type checkType)
        {
            int res;
            if (error != 0)
            {
                Console.WriteLine("CheckErrorAndType(): error=" + error);
                res = -1;
            }
            else if (valueObj.GetType() != checkType)
            {
                Console.WriteLine("CheckErrorAndType(): Type of value is not as excpected. Expected: " + checkType + " Received: " + valueObj.GetType() + ".");
                res = -1;
            }
            else
            {
                res = 0;
            }
            return res;
        }

        protected static string ResultString(PlcTag tag, string value )
        {
            return String.Format("{0:X02}: {1}", tag.Quality, value);
        }

        protected static int BcdByteToInt(byte value)
        {
            return (10 * (value / 16)) + (value % 16);
        }

        protected static byte IntToBcdByte(int value)
        {
            return (byte)((value / 10 * 16) + (value % 10));
        }

        protected static ushort BcdUshortToUshort(ushort value)
        {
            return (ushort)((value & 0x000f) + ((value & 0x00f0) >> 4) * 10 + ((value & 0x0f00) >> 8) * 100 + ((value & 0xf000) >> 12) * 1000);
        }

        protected static ushort UshortToBcdUshort(ushort value)
        {
            var b = new byte[4];
            b[0] = (byte)(value % 10);
            value /= 10;
            b[1] = (byte)(value % 10);
            value /= 10;
            b[2] = (byte)(value % 10);
            value /= 10;
            b[3] = (byte)(value % 10);
            return (ushort)(b[0] + (b[1] << 4) + (b[2] << 8) + (b[3] << 12));
        }
    }

    public class PlcTagBool : PlcTag
    {
        private bool m_Value;

        public bool Value
        {
            get { return m_Value; }
            set { m_Value = value; }
        }

        public PlcTagBool(string name, ItemAddress address, uint softdatatype) : base(name, address, softdatatype) { }

        public override void ProcessReadResult(object valueObj, ulong error)
        {
            LastReadError = error;
            if (CheckErrorAndType(error, valueObj, typeof(ValueBool)) == 0)
            {
                Value = ((ValueBool)valueObj).GetValue();
                Quality = PlcTagQC.TAG_QUALITY_GOOD;
            }
            else
            {
                Quality = PlcTagQC.TAG_QUALITY_BAD;
            }
        }

        public override PValue GetWriteValue()
        {
            return new ValueBool(Value);
        }

        public override string ToString()
        {
            return ResultString(this, Value.ToString());
        }
    }

    public class PlcTagByte : PlcTag
    {
        private byte m_Value;

        public byte Value
        {
            get { return m_Value; }
            set { m_Value = value; }
        }

        public PlcTagByte(string name, ItemAddress address, uint softdatatype) : base(name, address, softdatatype) { }

        public override void ProcessReadResult(object valueObj, ulong error)
        {
            LastReadError = error;
            if (CheckErrorAndType(error, valueObj, typeof(ValueByte)) == 0)
            {
                Value = ((ValueByte)valueObj).GetValue();
                Quality = PlcTagQC.TAG_QUALITY_GOOD;
            }
            else
            {
                Quality = PlcTagQC.TAG_QUALITY_BAD;
            }
        }

        public override PValue GetWriteValue()
        {
            return new ValueByte(Value);
        }

        public override string ToString()
        {
            return ResultString(this, Value.ToString());
        }
    }

    public class PlcTagChar : PlcTag
    {
        private char m_Value;
        string m_Encoding = "ISO-8859-1";

        public char Value
        {
            get { return m_Value; }
            set { m_Value = value; } // TODO: check if fits in ASCII area, include the encoding?
        }
        
        public PlcTagChar(string name, ItemAddress address, uint softdatatype) : base(name, address, softdatatype) { }

        public override void ProcessReadResult(object valueObj, ulong error)
        {
            LastReadError = error;
            if (CheckErrorAndType(error, valueObj, typeof(ValueUSInt)) == 0)
            {
                var v = new byte[1];
                v[0] = ((ValueUSInt)valueObj).GetValue();
                Value = Encoding.GetEncoding(m_Encoding).GetString(v)[0];
                Quality = PlcTagQC.TAG_QUALITY_GOOD;
            }
            else
            {
                Quality = PlcTagQC.TAG_QUALITY_BAD;
            }
        }

        public override PValue GetWriteValue()
        {
            var c = new char[1];
            c[0] = Value;
            byte[] b = Encoding.GetEncoding(m_Encoding).GetBytes(c);
            var pv = new ValueUSInt(b[0]);
            return pv;
        }

        public void SetCharEncoding(string encoding)
        {
            m_Encoding = encoding;
        }

        public override string ToString()
        {
            return ResultString(this, Value.ToString());
        }
    }

    public class PlcTagWord : PlcTag
    {
        private ushort m_Value;

        public ushort Value
        {
            get { return m_Value; }
            set { m_Value = value; }
        } 

        public PlcTagWord(string name, ItemAddress address, uint softdatatype) : base(name, address, softdatatype) { }

        public override void ProcessReadResult(object valueObj, ulong error)
        {
            LastReadError = error;
            if (CheckErrorAndType(error, valueObj, typeof(ValueWord)) == 0)
            {
                Value = ((ValueWord)valueObj).GetValue();
                Quality = PlcTagQC.TAG_QUALITY_GOOD;
            }
            else
            {
                Quality = PlcTagQC.TAG_QUALITY_BAD;
            }
        }

        public override PValue GetWriteValue()
        {
            return new ValueWord(Value);
        }

        public override string ToString()
        {
            return ResultString(this, Value.ToString());
        }
    }

    public class PlcTagInt : PlcTag
    {
        private short m_Value;

        public short Value
        {
            get { return m_Value; }
            set { m_Value = value; }
        } 

        public PlcTagInt(string name, ItemAddress address, uint softdatatype) : base(name, address, softdatatype) { }

        public override void ProcessReadResult(object valueObj, ulong error)
        {
            LastReadError = error;
            if (CheckErrorAndType(error, valueObj, typeof(ValueInt)) == 0)
            {
                Value = ((ValueInt)valueObj).GetValue();
                Quality = PlcTagQC.TAG_QUALITY_GOOD;
            }
            else
            {
                Quality = PlcTagQC.TAG_QUALITY_BAD;
            }
        }

        public override PValue GetWriteValue()
        {
            return new ValueInt(Value);
        }

        public override string ToString()
        {
            return ResultString(this, Value.ToString());
        }
    }

    public class PlcTagDWord : PlcTag
    {
        private uint m_Value;

        public uint Value
        {
            get { return m_Value; }
            set { m_Value = value; }
        } 

        public PlcTagDWord(string name, ItemAddress address, uint softdatatype) : base(name, address, softdatatype) { }

        public override void ProcessReadResult(object valueObj, ulong error)
        {
            LastReadError = error;
            if (CheckErrorAndType(error, valueObj, typeof(ValueDWord)) == 0)
            {
                Value = ((ValueDWord)valueObj).GetValue();
                Quality = PlcTagQC.TAG_QUALITY_GOOD;
            }
            else
            {
                Quality = PlcTagQC.TAG_QUALITY_BAD;
            }
        }

        public override PValue GetWriteValue()
        {
            return new ValueDWord(Value);
        }

        public override string ToString()
        {
            return ResultString(this, Value.ToString());
        }
    }

    public class PlcTagDInt : PlcTag
    {
        private int m_Value;

        public int Value
        {
            get { return m_Value; }
            set { m_Value = value; }
        }

        public PlcTagDInt(string name, ItemAddress address, uint softdatatype) : base(name, address, softdatatype) { }

        public override void ProcessReadResult(object valueObj, ulong error)
        {
            LastReadError = error;
            if (CheckErrorAndType(error, valueObj, typeof(ValueDInt)) == 0)
            {
                Value = ((ValueDInt)valueObj).GetValue();
                Quality = PlcTagQC.TAG_QUALITY_GOOD;
            }
            else
            {
                Quality = PlcTagQC.TAG_QUALITY_BAD;
            }
        }

        public override PValue GetWriteValue()
        {
            return new ValueDInt(Value);
        }

        public override string ToString()
        {
            return ResultString(this, Value.ToString());
        }
    }

    public class PlcTagReal : PlcTag
    {
        private float m_Value;

        public float Value
        {
            get { return m_Value; }
            set { m_Value = value; }
        }

        public PlcTagReal(string name, ItemAddress address, uint softdatatype) : base(name, address, softdatatype) { }

        public override void ProcessReadResult(object valueObj, ulong error)
        {
            LastReadError = error;
            if (CheckErrorAndType(error, valueObj, typeof(ValueReal)) == 0)
            {
                Value = ((ValueReal)valueObj).GetValue();
                Quality = PlcTagQC.TAG_QUALITY_GOOD;
            }
            else
            {
                Quality = PlcTagQC.TAG_QUALITY_BAD;
            }
        }

        public override PValue GetWriteValue()
        {
            return new ValueReal(Value);
        }

        public override string ToString()
        {
            return ResultString(this, Value.ToString());
        }
    }

    public class PlcTagDate : PlcTag
    {
        // Specifies the number of days from January 1, 1990.
        // .Net has no type with only date or only time
        // TODO: Switch to .Net 6 (for DateOnly) or stay just as UInt?
        private DateTime m_Value;

        public DateTime Value
        {
            get
            {
                return m_Value;
            }

            set
            {
                if (value >= new DateTime(1990, 1, 1) && value <= new DateTime(2169, 6, 6))
                {
                    m_Value = value;
                }
                else
                {
                    throw new ArgumentOutOfRangeException("Value", "Date must be >= 1990-01-01 and <= 2169-06-06");
                }
            }
        }

        public PlcTagDate(string name, ItemAddress address, uint softdatatype) : base(name, address, softdatatype)
        {
            m_Value = new DateTime(1990, 1, 1);
        }

        public override void ProcessReadResult(object valueObj, ulong error)
        {
            LastReadError = error;
            if (CheckErrorAndType(error, valueObj, typeof(ValueUInt)) == 0)
            {
                ushort v = ((ValueUInt)valueObj).GetValue();
                Value = new DateTime(1990, 1, 1).AddDays(v);

                Quality = PlcTagQC.TAG_QUALITY_GOOD;
            }
            else
            {
                Quality = PlcTagQC.TAG_QUALITY_BAD;
            }
        }

        public override PValue GetWriteValue()
        {
            var dtbase = new DateTime(1990, 1, 1);
            return new ValueUInt((ushort)(Value - dtbase).Days);
        }

        public override string ToString()
        {
            return ResultString(this, Value.ToShortDateString());
        }
    }

    public class PlcTagTimeOfDay : PlcTag
    {
        // TODO: .Net has no type with only date or only time
        // Specification: 01:02:03 = 3723000 number of milliseconds since 00:00:00
        private uint m_Value;

        /// <summary>
        /// Number of milliseconds since 00:00:00, must be below 86400000ms
        /// </summary>
        public uint Value
        {
            get
            {
                return m_Value;
            }

            set
            { 
                if (value < 86400000)
                {
                    m_Value = value;
                }
                else
                {
                    throw new ArgumentOutOfRangeException("Value", "Number if milliseconds must be < 86400000");
                }
            }
        }

        public PlcTagTimeOfDay(string name, ItemAddress address, uint softdatatype) : base(name, address, softdatatype) { }

        public override void ProcessReadResult(object valueObj, ulong error)
        {
            LastReadError = error;
            if (CheckErrorAndType(error, valueObj, typeof(ValueUDInt)) == 0)
            {
                Value = ((ValueUDInt)valueObj).GetValue();

                Quality = PlcTagQC.TAG_QUALITY_GOOD;
            }
            else
            {
                Quality = PlcTagQC.TAG_QUALITY_BAD;
            }
        }

        public override PValue GetWriteValue()
        {
            return new ValueUDInt(Value);
        }

        public override string ToString()
        {
            uint v, h, m, s, ms;
            string tod;
            v = Value;
            ms = v % 1000;
            v /= 1000;
            s = v % 60;
            v /= 60;
            m = v % 60;
            v /= 60;
            h = v;
            if (ms > 0)
            {
                tod = String.Format("{0:D02}:{1:D02}:{2:D02}.{3:D03}", h, m, s, ms);
            }
            else
            {
                tod = String.Format("{0:D02}:{1:D02}:{2:D02}", h, m, s);
            }
            return ResultString(this, tod);
        }
    }

    public class PlcTagTime: PlcTag
    {
        private int m_Value;

        /// <summary>
        /// In milliseconds, with sign
        /// </summary>
        public int Value
        {
            get { return m_Value; }
            set { m_Value = value; }
        }
        
        public PlcTagTime(string name, ItemAddress address, uint softdatatype) : base(name, address, softdatatype) { }

        public override void ProcessReadResult(object valueObj, ulong error)
        {
            LastReadError = error;
            if (CheckErrorAndType(error, valueObj, typeof(ValueDInt)) == 0)
            {
                Value = ((ValueDInt)valueObj).GetValue();
                Quality = PlcTagQC.TAG_QUALITY_GOOD;
            }
            else
            {
                Quality = PlcTagQC.TAG_QUALITY_BAD;
            }
        }

        public override PValue GetWriteValue()
        {
            return new ValueDInt(Value);
        }

        public override string ToString()
        {
            int[] divs = { 86400000, 3600000, 60000, 1000, 1 };
            string[] vfmt = { "{0}d", "{0:D02}h", "{0:D02}m", "{0:D02}s", "{0:D03}ms" };
            int vtime = Value;
            bool time_negative = false;
            int val;
            string ts = String.Empty;
            if (vtime == 0)
            {
                ts = "0ms";
            }
            else
            {
                if (vtime < 0)
                {
                    ts = "-";
                    time_negative = true;
                    for (int i = 0; i < 5; i++)
                    {
                        divs[i] = -divs[i];
                    }
                }

                for (int i = 0; i < 5; i++)
                {
                    val = vtime / divs[i];
                    vtime -= val * divs[i];
                    if (val > 0)
                    {
                        ts += String.Format(vfmt[i], val);
                        if ((!time_negative && vtime > 0) || (time_negative && vtime < 0))
                        {
                            ts += "_";
                        }
                    }
                }
            }
            return ResultString(this, ts);
        }
    }

    public class PlcTagS5Time : PlcTag
    {
        // Specification S5Time:
        // Bits 15, 14: not used
        // Bits 13, 12: time base in binary, 00=10ms, 01=100ms, 10=1s, 11=10s
        // Bits 11..0: time values BCD coded (0 to 999)
        // S5Time_9S_990MS = <Value type="Word">2457</Value>
        // S5Time_2H_46M_30S_0MS = <Value type="Word">14745</Value>

        private ushort m_TimeValue;
        private ushort m_TimeBase;

        /// <summary>
        /// TimeValue: between 0..999, factor is TimeBase
        /// </summary>
        public ushort TimeValue
        {
            get
            { 
                return m_TimeValue;
            }
            set
            {
                if (value >= 0 && value <= 999)
                {
                    m_TimeValue = value;
                }
                else
                {
                    throw new ArgumentOutOfRangeException("Value", "TimeValue must be >= 0 and <= 999");
                }
            }
        }
        /// <summary>
        /// TimeBase 0=10ms, 1=100ms, 2=1s, 3=10s
        /// </summary>
        public ushort TimeBase
        {
            get
            {
                return m_TimeBase;
            }
            set
            {
                if (value >= 0 && value <= 3)
                {
                    m_TimeBase = value;
                }
                else
                {
                    throw new ArgumentOutOfRangeException("Value", "TimeBase must be >= 0 and <= 3");
                }
            }
        }

        public PlcTagS5Time(string name, ItemAddress address, uint softdatatype) : base(name, address, softdatatype) { }

        public override void ProcessReadResult(object valueObj, ulong error)
        {
            LastReadError = error;
            if (CheckErrorAndType(error, valueObj, typeof(ValueWord)) == 0)
            {
                var v = ((ValueWord)valueObj).GetValue();
                TimeValue = BcdUshortToUshort((ushort)(v & (ushort)0x0FFF));
                TimeBase = (ushort)((v & (ushort)0x3000) >> 12);
                Quality = PlcTagQC.TAG_QUALITY_GOOD;
            }
            else
            {
                Quality = PlcTagQC.TAG_QUALITY_BAD;
            }
        }

        public override PValue GetWriteValue()
        {
            ushort v;
            v = UshortToBcdUshort(TimeValue);
            v |= (ushort)((TimeBase & 0x3) << 12);
            return new ValueWord(v);
        }

        public override string ToString()
        {
            string s = String.Empty;
            // Scale down to milliseconds
            switch(TimeBase)
            {
                case 0:
                    s = (TimeValue * 10).ToString() + "ms";
                    break;
                case 1:
                    s = (TimeValue * 100).ToString() + "ms";
                    break;
                case 2:
                    s = (TimeValue * 1000).ToString() + "ms";
                    break;
                case 3:
                    s = (TimeValue * 10000).ToString() + "ms";
                    break;
            }
            return ResultString(this, s);
        }
    }

    public class PlcTagDateAndTime : PlcTag
    {
        /* BCD coded:
         * YYMMDDhhmmssuuuQ
         * uuu = milliseconds
         * Q = Weekday 1=Su, 2=Mo, 3=Tu, 4=We, 5=Th, 6=Fr, 7=Sa
         */
        private DateTime m_Value;

        public DateTime Value
        {
            get
            {
                return m_Value;
            }

            set
            {
                if (value >= new DateTime(1990, 1, 1) && value < new DateTime(2090, 1, 1))
                {
                    m_Value = value;
                }
                else
                {
                    throw new ArgumentOutOfRangeException("Value", "DateTime must be >= 1990-01-01 and < 2090-01-01");
                }
            }
        }

        public PlcTagDateAndTime(string name, ItemAddress address, uint softdatatype) : base(name, address, softdatatype)
        {
            Value = new DateTime(1990, 1, 1);
        }

        public override void ProcessReadResult(object valueObj, ulong error)
        {
            LastReadError = error;
            if (CheckErrorAndType(error, valueObj, typeof(ValueUSIntArray)) == 0)
            {
                var v = ((ValueUSIntArray)valueObj).GetValue();
                int[] ts = new int[8];
                for (int i = 0; i < 7; i++)
                {
                    ts[i] = BcdByteToInt(v[i]);
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
                Value = new DateTime(year, ts[1], ts[2], ts[3], ts[4], ts[5]);
                Value = Value.AddMilliseconds(ts[6] * 10 + ts[7]);

                Quality = PlcTagQC.TAG_QUALITY_GOOD;
            }
            else
            {
                Quality = PlcTagQC.TAG_QUALITY_BAD;
            }
        }

        public override PValue GetWriteValue()
        {
            int[] ts = new int[8];
            byte[] b = new byte[8];
            if (Value.Year < 2000)
            {
                // 90-99 = 1990-1999
                ts[0] = Value.Year - 1900;
            }
            else
            {
                // 00-89 = 2000-2089
                ts[0] = Value.Year - 2000;
            }
            ts[1] = Value.Month;
            ts[2] = Value.Day;
            ts[3] = Value.Hour;
            ts[4] = Value.Minute;
            ts[5] = Value.Second;
            ts[6] = Value.Millisecond / 10;
            ts[7] = (Value.Millisecond % 10) << 4; // Don't set the weekday
            for (int i = 0; i < 7; i++)
            {
                b[i] = IntToBcdByte(ts[i]);
            }
            b[7] = (byte)ts[7];
            return new ValueUSIntArray(b);
        }

        public override string ToString()
        {
            string ts = Value.ToString();
            if (Value.Millisecond > 0)
            {
                ts += String.Format(".{0:D03}", Value.Millisecond);
            }
            return ResultString(this, ts);
        }
    }

    public class PlcTagString : PlcTag
    {
        private string m_Value;
        private byte m_MaxLength = 254;
        private string m_Encoding = "ISO-8859-1";

        public string Value
        {
            get
            {
                return m_Value;
            }

            set
            {
                if (value.Length <= m_MaxLength)
                {
                    m_Value = value;
                }
                else
                {
                    throw new ArgumentOutOfRangeException("Value", "String is longer than the allowed max. length of " + m_MaxLength);
                }
            }
        }

        public PlcTagString(string name, ItemAddress address, uint softdatatype, byte maxlength = 254) : base(name, address, softdatatype)
        {
            m_MaxLength = maxlength;
        }

        public override void ProcessReadResult(object valueObj, ulong error)
        {
            LastReadError = error;
            if (CheckErrorAndType(error, valueObj, typeof(ValueUSIntArray)) == 0)
            {
                var v = ((ValueUSIntArray)valueObj).GetValue();
                int max_len = v[0];
                int act_len = v[1];
                // IEC 61131-3 states ISO-646 IRV, with optional extensions like "Latin-1 Supplement".
                // Siemens TIA-Portal gives warnings using other than 7 Bit ASCII characters.
                // Let the user define his local encoding via SetStringEncoding().
                Value = Encoding.GetEncoding(m_Encoding).GetString(v, 2, act_len);
                Quality = PlcTagQC.TAG_QUALITY_GOOD;
            }
            else
            {
                Quality = PlcTagQC.TAG_QUALITY_BAD;
            }
        }

        public override PValue GetWriteValue()
        {
            // Must write the complete array of MaxLength of the string (plus two bytes header).
            byte[] sb = Encoding.GetEncoding(m_Encoding).GetBytes(Value);
            var b = new byte[m_MaxLength + 2];
            b[0] = m_MaxLength;
            b[1] = (byte)sb.Length;
            for (int i = 0; i < sb.Length; i++)
            {
                b[i + 2] = sb[i];
            }
            return new ValueUSIntArray(b);
        }

        public void SetStringEncoding(string encoding)
        {
            m_Encoding = encoding;
        }

        public override string ToString()
        {
            return ResultString(this, Value);
        }
    }

    public class PlcTagPointer : PlcTag
    {
        private byte[] m_Value;

        public byte[] Value
        {
            get { return m_Value; }
            set { m_Value = value; }
        }

        public PlcTagPointer(string name, ItemAddress address, uint softdatatype) : base(name, address, softdatatype)
        {
            m_Value = new byte[6];
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
            int dbnr = Value[0] * 256 + Value[1];
            int area = Value[2];
            int bitnr = Value[5] & 0x7;
            int bytenr = (int)(Value[5] >> 3) + ((int)Value[4]) * 32 + (int)(Value[3] & 0x7) * 8192;
            return ResultString(this, String.Format("DB={0} Area=0x{1:X02} Byte={2} Bit={3}", dbnr, area, bytenr, bitnr));
        }
    }

    public class PlcTagAny: PlcTag
    {
        private byte[] m_Value;

        public byte[] Value
        {
            get { return m_Value; }
            set { m_Value = value; }
        } 

        public PlcTagAny(string name, ItemAddress address, uint softdatatype) : base(name, address, softdatatype)
        {
            m_Value = new byte[10];
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
            int hdr = Value[0];
            int datatype = Value[1];
            int factor = Value[2] * 256 + Value[3];
            int dbnr = Value[4] * 256 + Value[5];
            int area = Value[6];
            int bitnr = Value[9] & 0x7;
            int bytenr = (int)(Value[9] >> 3) + ((int)Value[8]) * 32 + (int)(Value[7] & 0x7) * 8192;

            return ResultString(this, String.Format("HDR={0:X02} Type={1:X02} Factor={2} DB={3} Area=0x{4:X02} Byte={5} Bit={6}", hdr, datatype, factor, dbnr, area, bytenr, bitnr));
        }
    }

    public class PlcTagLReal : PlcTag
    {
        private double m_Value;

        public double Value
        {
            get { return m_Value; }
            set { m_Value = value; }
        }

        public PlcTagLReal(string name, ItemAddress address, uint softdatatype) : base(name, address, softdatatype) { }

        public override void ProcessReadResult(object valueObj, ulong error)
        {
            LastReadError = error;
            if (CheckErrorAndType(error, valueObj, typeof(ValueLReal)) == 0)
            {
                Value = ((ValueLReal)valueObj).GetValue();
                Quality = PlcTagQC.TAG_QUALITY_GOOD;
            }
            else
            {
                Quality = PlcTagQC.TAG_QUALITY_BAD;
            }
        }

        public override PValue GetWriteValue()
        {
            return new ValueLReal(Value);
        }

        public override string ToString()
        {
            return ResultString(this, Value.ToString());
        }
    }

    public class PlcTagULInt : PlcTag
    {
        private ulong m_Value;

        public ulong Value
        {
            get { return m_Value; }
            set { m_Value = value; }
        }

        public PlcTagULInt(string name, ItemAddress address, uint softdatatype) : base(name, address, softdatatype) { }

        public override void ProcessReadResult(object valueObj, ulong error)
        {
            LastReadError = error;
            if (CheckErrorAndType(error, valueObj, typeof(ValueULInt)) == 0)
            {
                Value = ((ValueULInt)valueObj).GetValue();
                Quality = PlcTagQC.TAG_QUALITY_GOOD;
            }
            else
            {
                Quality = PlcTagQC.TAG_QUALITY_BAD;
            }
        }

        public override PValue GetWriteValue()
        {
            return new ValueULInt(Value);
        }

        public override string ToString()
        {
            return ResultString(this, Value.ToString());
        }
    }

    public class PlcTagLInt : PlcTag
    {
        private long m_Value;

        public long Value
        {
            get { return m_Value; }
            set { m_Value = value; }
        }

        public PlcTagLInt(string name, ItemAddress address, uint softdatatype) : base(name, address, softdatatype) { }

        public override void ProcessReadResult(object valueObj, ulong error)
        {
            LastReadError = error;
            if (CheckErrorAndType(error, valueObj, typeof(ValueLInt)) == 0)
            {
                Value = ((ValueLInt)valueObj).GetValue();
                Quality = PlcTagQC.TAG_QUALITY_GOOD;
            }
            else
            {
                Quality = PlcTagQC.TAG_QUALITY_BAD;
            }
        }

        public override PValue GetWriteValue()
        {
            return new ValueLInt(Value);
        }

        public override string ToString()
        {
            return ResultString(this, Value.ToString());
        }
    }

    public class PlcTagLWord : PlcTag
    {
        private ulong m_Value;

        public ulong Value
        {
            get { return m_Value; }
            set { m_Value = value; }
        }

        public PlcTagLWord(string name, ItemAddress address, uint softdatatype) : base(name, address, softdatatype) { }

        public override void ProcessReadResult(object valueObj, ulong error)
        {
            LastReadError = error;
            if (CheckErrorAndType(error, valueObj, typeof(ValueLWord)) == 0)
            {
                Value = ((ValueLWord)valueObj).GetValue();
                Quality = PlcTagQC.TAG_QUALITY_GOOD;
            }
            else
            {
                Quality = PlcTagQC.TAG_QUALITY_BAD;
            }
        }

        public override PValue GetWriteValue()
        {
            return new ValueLWord(Value);
        }

        public override string ToString()
        {
            return ResultString(this, Value.ToString());
        }
    }

    public class PlcTagUSInt : PlcTag
    {
        private byte m_Value;

        public byte Value
        {
            get { return m_Value; }
            set { m_Value = value; }
        }

        public PlcTagUSInt(string name, ItemAddress address, uint softdatatype) : base(name, address, softdatatype) { }

        public override void ProcessReadResult(object valueObj, ulong error)
        {
            LastReadError = error;
            if (CheckErrorAndType(error, valueObj, typeof(ValueUSInt)) == 0)
            {
                Value = ((ValueUSInt)valueObj).GetValue();
                Quality = PlcTagQC.TAG_QUALITY_GOOD;
            }
            else
            {
                Quality = PlcTagQC.TAG_QUALITY_BAD;
            }
        }

        public override PValue GetWriteValue()
        {
            return new ValueUSInt(Value);
        }

        public override string ToString()
        {
            return ResultString(this, Value.ToString());
        }
    }

    public class PlcTagUInt : PlcTag
    {
        private ushort m_Value;

        public ushort Value
        {
            get { return m_Value; }
            set { m_Value = value; }
        }

        public PlcTagUInt(string name, ItemAddress address, uint softdatatype) : base(name, address, softdatatype) { }

        public override void ProcessReadResult(object valueObj, ulong error)
        {
            LastReadError = error;
            if (CheckErrorAndType(error, valueObj, typeof(ValueUInt)) == 0)
            {
                Value = ((ValueUInt)valueObj).GetValue();
                Quality = PlcTagQC.TAG_QUALITY_GOOD;
            }
            else
            {
                Quality = PlcTagQC.TAG_QUALITY_BAD;
            }
        }

        public override PValue GetWriteValue()
        {
            return new ValueUInt(Value);
        }

        public override string ToString()
        {
            return ResultString(this, Value.ToString());
        }
    }

    public class PlcTagUDInt : PlcTag
    {
        private uint m_Value;

        public uint Value
        {
            get { return m_Value; }
            set { m_Value = value; }
        }

        public PlcTagUDInt(string name, ItemAddress address, uint softdatatype) : base(name, address, softdatatype) { }

        public override void ProcessReadResult(object valueObj, ulong error)
        {
            LastReadError = error;
            if (CheckErrorAndType(error, valueObj, typeof(ValueUDInt)) == 0)
            {
                Value = ((ValueUDInt)valueObj).GetValue();
                Quality = PlcTagQC.TAG_QUALITY_GOOD;
            }
            else
            {
                Quality = PlcTagQC.TAG_QUALITY_BAD;
            }
        }

        public override PValue GetWriteValue()
        {
            return new ValueUDInt(Value);
        }

        public override string ToString()
        {
            return ResultString(this, Value.ToString());
        }
    }

    public class PlcTagSInt : PlcTag
    {
        private sbyte m_Value;

        public sbyte Value
        {
            get { return m_Value; }
            set { m_Value = value; }
        }

        public PlcTagSInt(string name, ItemAddress address, uint softdatatype) : base(name, address, softdatatype) { }

        public override void ProcessReadResult(object valueObj, ulong error)
        {
            LastReadError = error;
            if (CheckErrorAndType(error, valueObj, typeof(ValueSInt)) == 0)
            {
                Value = ((ValueSInt)valueObj).GetValue();
                Quality = PlcTagQC.TAG_QUALITY_GOOD;
            }
            else
            {
                Quality = PlcTagQC.TAG_QUALITY_BAD;
            }
        }

        public override PValue GetWriteValue()
        {
            return new ValueSInt(Value);
        }

        public override string ToString()
        {
            return ResultString(this, Value.ToString());
        }
    }

    public class PlcTagWChar : PlcTag
    {
        private char m_Value;

        public char Value
        {
            get { return m_Value; }
            set { m_Value = value; }
        }

        public PlcTagWChar(string name, ItemAddress address, uint softdatatype) : base(name, address, softdatatype) { }

        public override void ProcessReadResult(object valueObj, ulong error)
        {
            LastReadError = error;
            if (CheckErrorAndType(error, valueObj, typeof(ValueUInt)) == 0)
            {
                Value = (char)((ValueUInt)valueObj).GetValue();
                Quality = PlcTagQC.TAG_QUALITY_GOOD;
            }
            else
            {
                Quality = PlcTagQC.TAG_QUALITY_BAD;
            }
        }

        public override PValue GetWriteValue()
        {
            return new ValueUInt(Convert.ToUInt16(Value));
        }

        public override string ToString()
        {
            return ResultString(this, Value.ToString());
        }
    }
  
    public class PlcTagWString : PlcTag
    {
        private string m_Value;
        ushort m_MaxLength = 254;

        public string Value
        {
            get
            {
                return m_Value;
            }

            set
            {
                if (value.Length <= m_MaxLength)
                {
                    m_Value = value;
                }
                else
                {
                    throw new ArgumentOutOfRangeException("Value", "String is longer than the allowed max. length of " + m_MaxLength);
                }
            }
        }

        public PlcTagWString(string name, ItemAddress address, uint softdatatype, ushort maxlength = 254) : base(name, address, softdatatype)
        {
            m_MaxLength = maxlength;
        }

        public override void ProcessReadResult(object valueObj, ulong error)
        {
            LastReadError = error;
            if (CheckErrorAndType(error, valueObj, typeof(ValueUIntArray)) == 0)
            {
                var v = ((ValueUIntArray)valueObj).GetValue();
                ushort max_len = v[0];
                ushort act_len = v[1];

                byte[] b = new byte[act_len * 2];
                Buffer.BlockCopy(v, 4, b, 0, act_len * 2);
                Value = Encoding.Unicode.GetString(b, 0, act_len * 2);
                Quality = PlcTagQC.TAG_QUALITY_GOOD;
            }
            else
            {
                Quality = PlcTagQC.TAG_QUALITY_BAD;
            }
        }

        public override PValue GetWriteValue()
        {
            // Must write the complete array of MaxLength of the string (plus two ushort for the header).
            var b = new ushort[Value.Length + 2];
            b[0] = m_MaxLength;
            b[1] = (ushort)Value.Length;
            for (int i = 0; i < Value.Length; i++)
            {
                b[i + 2] = Convert.ToUInt16(Value[i]);
            }
            return new ValueUIntArray(b);
        }

        public override string ToString()
        {
            return ResultString(this, Value);
        }
    }

    public class PlcTagLTime : PlcTag
    {
        private long m_Value;

        public long Value
        {
            get { return m_Value; }
            set { m_Value = value; }
        }

        public PlcTagLTime(string name, ItemAddress address, uint softdatatype) : base(name, address, softdatatype) { }

        public override void ProcessReadResult(object valueObj, ulong error)
        {
            LastReadError = error;
            if (CheckErrorAndType(error, valueObj, typeof(ValueTimespan)) == 0)
            {
                Value = ((ValueTimespan)valueObj).GetValue();
                Quality = PlcTagQC.TAG_QUALITY_GOOD;
            }
            else
            {
                Quality = PlcTagQC.TAG_QUALITY_BAD;
            }
        }

        public override PValue GetWriteValue()
        {
            return new ValueTimespan(Value);
        }

        private string ValueAsString()
        {
            return ValueTimespan.ToString(Value);
        }

        public override string ToString()
        {
            return ResultString(this, ValueAsString());
        }
    }

    public class PlcTagLTOD : PlcTag
    {
        // TODO: Like the 32 Bit Types Date/TOD, in .Net there's no type for date / time only. Only in .Net 6.
        // Specification: Number of nanoseconds since 00:00:00.
        private ulong m_Value;

        /// <summary>
        /// Number of nanoseconds since 00:00:00, must be below 86400000000000ns
        /// </summary>
        public ulong Value
        {
            get
            {
                return m_Value;
            }

            set
            {
                if (value < 86400000000000)
                {
                    m_Value = value;
                }
                else
                {
                    throw new ArgumentOutOfRangeException("Value", "Number if nanoseconds must be < 86400000000000");
                }
                m_Value = value;
            }
        }

        public PlcTagLTOD(string name, ItemAddress address, uint softdatatype) : base(name, address, softdatatype) { }

        public override void ProcessReadResult(object valueObj, ulong error)
        {
            LastReadError = error;
            if (CheckErrorAndType(error, valueObj, typeof(ValueULInt)) == 0)
            {
                Value = ((ValueULInt)valueObj).GetValue();
                Quality = PlcTagQC.TAG_QUALITY_GOOD;
            }
            else
            {
                Quality = PlcTagQC.TAG_QUALITY_BAD;
            }
        }

        public override PValue GetWriteValue()
        {
            return new ValueULInt(Value);
        }

        public override string ToString()
        {
            ulong v, h, m, s, ns;
            string tod;
            v = Value;
            ns = v % 1000000000;
            v /= 1000000000;
            s = v % 60;
            v /= 60;
            m = v % 60;
            v /= 60;
            h = v;
            if (ns > 0)
            {
                tod = String.Format("{0:D02}:{1:D02}:{2:D02}.{3:D09}", h, m, s, ns);
            }
            else
            {
                tod = String.Format("{0:D02}:{1:D02}:{2:D02}", h, m, s);
            }
            return ResultString(this, tod);
        }
    }

    public class PlcTagLDT: PlcTag
    {
        private ulong m_Value;

        public ulong Value
        {
            get { return m_Value; }
            set { m_Value = value; }
        }

        public PlcTagLDT(string name, ItemAddress address, uint softdatatype) : base(name, address, softdatatype) { }

        public override void ProcessReadResult(object valueObj, ulong error)
        {
            LastReadError = error;
            if (CheckErrorAndType(error, valueObj, typeof(ValueTimestamp)) == 0)
            {
                Value = ((ValueTimestamp)valueObj).GetValue();
                Quality = PlcTagQC.TAG_QUALITY_GOOD;
            }
            else
            {
                Quality = PlcTagQC.TAG_QUALITY_BAD;
            }
        }

        public override PValue GetWriteValue()
        {
            return new ValueTimestamp(Value);
        }

        private string ValueAsString()
        {
            return ValueTimestamp.ToString(Value);
        }

        public override string ToString()
        {
            return ResultString(this, ValueAsString());
        }
    }

    public class PlcTagDTL : PlcTag
    {
        private DateTime m_Value;
        private uint m_ValueNanosecond;

        public DateTime Value
        {
            get
            {
                return m_Value;
            }

            set
            {
                if (value >= new DateTime(1970, 1, 1) && value <= new DateTime(2262, 4, 11, 23, 47, 16))
                {
                    m_Value = value;
                }
                else
                {
                    throw new ArgumentOutOfRangeException("Value", "DateTime must be >= 1970-01-01 and <= 2262-04-11 23:47:16");
                }
            }
        }

        public uint ValueNanosecond
        {
            get
            {
                return m_ValueNanosecond;
            }

            set
            {
                if (value <= 999999999)
                {
                    m_ValueNanosecond = value;
                }
                else
                {
                    throw new ArgumentOutOfRangeException("Value", "Nanosecond must be <= 999999999");
                }
            }
        }

        public UInt64 DTLInterfaceTimestamp = 0x10ff4ad6dfd5774c; // Oct 23, 2008 16:38:30.406829900 UTC. Should be used from first browse method (or read) and set correctly

        public PlcTagDTL(string name, ItemAddress address, uint softdatatype) : base(name, address, softdatatype)
        {
            Value = new DateTime(1970, 1, 1);
        }

        public override void ProcessReadResult(object valueObj, ulong error)
        {
            LastReadError = error;
            if (CheckErrorAndType(error, valueObj, typeof(ValueStruct)) == 0)
            {
                var struct_val = (ValueStruct)valueObj;
                // That value is the ID which has the type description
                // E.g. DTL 33554499 = TI_LIB.SimpleType.67
                // Then comes a PackedStruct, with Interface timestamp, transportflags, and a ByteArray with 12 bytes.
                // Generate the separate values back from the array:
                // 0, 1: YEAR, UInt
                // 2: MONTH, USInt
                // 3: DAY, USInt
                // 4: WEEKDAY, USInt
                // 5: HOUR, USInt
                // 6: MINUTE, USInt
                // 7: SECOND, USInt
                // 8, 9, 10, 11: NANOSECOND, UDInt
                
                // Use the default timestamp, or refresh it from browsing the plc, or from reading dtl first
                DTLInterfaceTimestamp = struct_val.PackedStructInterfaceTimestamp;

                if (struct_val.GetValue() == 0x02000043)
                {
                    var elem = struct_val.GetStructElement(0x02000043);
                    if (elem.GetType() == typeof(ValueByteArray))
                    {
                        var barr = ((ValueByteArray)elem).GetValue();
                        int year = (int)barr[0] * 256 + (int)barr[1];
                        ValueNanosecond = (uint)barr[8] * 16777216 + (uint)barr[9] * 65536 + (uint)barr[10] * 256 + (uint)barr[11];
                        Value = new DateTime(year, barr[2], barr[3], barr[5], barr[6], barr[7]);
                        Quality = PlcTagQC.TAG_QUALITY_GOOD;
                    }
                    else
                    { 
                        Quality = PlcTagQC.TAG_QUALITY_BAD; 
                    }
                }
                else
                {
                    Quality = PlcTagQC.TAG_QUALITY_BAD;
                }
            }
            else
            {
                Quality = PlcTagQC.TAG_QUALITY_BAD;
            }
        }

        public override PValue GetWriteValue()
        {
            var struct_val = new ValueStruct(0x02000043); // 0x02000043 = TI_LIB.SimpleType.67 -> DTL Systemdatatype
            struct_val.PackedStructInterfaceTimestamp = DTLInterfaceTimestamp;
            var barr = new byte[12];
            barr[0] = (byte)(Value.Year >> 8);
            barr[1] = (byte)Value.Year;
            barr[2] = (byte)Value.Month;
            barr[3] = (byte)Value.Day;
            barr[4] = 0; // Weekday, don't set
            barr[5] = (byte)Value.Hour;
            barr[6] = (byte)Value.Minute;
            barr[7] = (byte)Value.Second;
            barr[8] = (byte)(ValueNanosecond >> 24);
            barr[9] = (byte)(ValueNanosecond >> 16);
            barr[10] = (byte)(ValueNanosecond >> 8);
            barr[11] = (byte)(ValueNanosecond);
            struct_val.AddStructElement(0x02000043, new ValueByteArray(barr));
            return struct_val;
        }

        public override string ToString()
        {
            string fmt;
            uint ns = ValueNanosecond;
            if ((ns % 1000) > 0)
            {
                fmt = "{0}.{1:D09}";
            }
            else if ((ns % 1000000) > 0)
            {
                fmt = "{0}.{1:D06}";
                ns /= 1000;
            }
            else if ((ns % 1000000000) > 0)
            {
                fmt = "{0}.{1:D03}";
                ns /= 1000000;
            }
            else
            {
                return Value.ToString();
            }
            return String.Format(fmt, Value.ToString(), ns);
        }
    }
}