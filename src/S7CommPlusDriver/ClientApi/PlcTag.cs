using System;
using System.Text;

namespace S7CommPlusDriver.ClientApi
{
    /* Ideen:
     * - Initialwert im Konstruktor (Optional)
     * - Die verschiedenen SPS-Variablentypen auf den jeweils größten
     *   .Net Typen vereinheitlichen und somit die Anzahl verringern.
     *   Beispielsweise bei den Ganzzahltypen nur noch 64Bit Signed/Unsigned
     *
     * Viele Datentypen unterscheiden sich nur in den übertragenen Typen.
     * Jedoch ganz spezielle Besonderheiten bei String, WString, Datum/Zeiten usw., ansonsten ließe sich das
     * generisch lösen anstatt jeden Typ explizit auszuprogrammieren.
     */
    public abstract class PlcTag
    {
        public string Name;
        public ItemAddress Address;
        public short Quality;
        public uint Datatype;

        public PlcTag(string name, ItemAddress address, uint softdatatype)
        {
            Name = name;
            Address = address;
            Datatype = softdatatype;
            Quality = PlcTagQC.TAG_QUALITY_WAITING_FOR_INITIAL_DATA;
        }
        // TODO: Dürfte nur für PlcTags sichtbar sein
        public abstract void ProcessRead(object obj, UInt64 error);

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

        protected static int BCD_Byte_to_int(byte value)
        {
            return (10 * (value / 16)) + (value % 16);
        }

        protected static ushort BCD_ushort_to_ushort(ushort value)
        {
            return (ushort)((value & 0x000f) + ((value & 0x00f0) >> 4) * 10 + ((value & 0x0f00) >> 8) * 100 + ((value & 0xf000) >> 12) * 1000);
        }
    }

    //------------------------------------------------------------------------------------------------
    public class PlcTagBool : PlcTag
    {
        bool Value;

        public PlcTagBool(string name, ItemAddress address, uint softdatatype) : base(name, address, softdatatype) { }

        public override void ProcessRead(object valueObj, ulong error)
        {
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

        public bool GetValue()
        {
            return Value;
        }

        public override string ToString()
        {
            return ResultString(this, Value.ToString());
        }
    }

    public class PlcTagByte : PlcTag
    {
        byte Value;

        public PlcTagByte(string name, ItemAddress address, uint softdatatype) : base(name, address, softdatatype) { }

        public override void ProcessRead(object valueObj, ulong error)
        {
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

        public byte GetValue()
        {
            return Value;
        }

        public override string ToString()
        {
            return ResultString(this, Value.ToString());
        }
    }

    public class PlcTagChar : PlcTag
    {
        char Value;
        string m_Encoding = "ISO-8859-1";
        public PlcTagChar(string name, ItemAddress address, uint softdatatype) : base(name, address, softdatatype) { }

        public override void ProcessRead(object valueObj, ulong error)
        {
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

        public char GetValue()
        {
            return Value;
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
        ushort Value;

        public PlcTagWord(string name, ItemAddress address, uint softdatatype) : base(name, address, softdatatype) { }

        public override void ProcessRead(object valueObj, ulong error)
        {
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

        public ushort GetValue()
        {
            return Value;
        }

        public override string ToString()
        {
            return ResultString(this, Value.ToString());
        }
    }

    public class PlcTagInt : PlcTag
    {
        short Value;

        public PlcTagInt(string name, ItemAddress address, uint softdatatype) : base(name, address, softdatatype) { }

        public override void ProcessRead(object valueObj, ulong error)
        {
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

        public short GetValue()
        {
            return Value;
        }

        public override string ToString()
        {
            return ResultString(this, Value.ToString());
        }
    }

    public class PlcTagDWord : PlcTag
    {
        uint Value;

        public PlcTagDWord(string name, ItemAddress address, uint softdatatype) : base(name, address, softdatatype) { }

        public override void ProcessRead(object valueObj, ulong error)
        {
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

        public uint GetValue()
        {
            return Value;
        }

        public override string ToString()
        {
            return ResultString(this, Value.ToString());
        }
    }

    public class PlcTagDInt : PlcTag
    {
        int Value;

        public PlcTagDInt(string name, ItemAddress address, uint softdatatype) : base(name, address, softdatatype) { }

        public override void ProcessRead(object valueObj, ulong error)
        {
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

        public int GetValue()
        {
            return Value;
        }

        public override string ToString()
        {
            return ResultString(this, Value.ToString());
        }
    }

    public class PlcTagReal : PlcTag
    {
        float Value;

        public PlcTagReal(string name, ItemAddress address, uint softdatatype) : base(name, address, softdatatype) { }

        public override void ProcessRead(object valueObj, ulong error)
        {
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

        public float GetValue()
        {
            return Value;
        }

        public override string ToString()
        {
            return ResultString(this, Value.ToString());
        }
    }

    public class PlcTagDate : PlcTag
    {
        // Gibt die Anzahl Tage vom 1.1.1990 an.
        // .Net besitzt in dieser Version keinen Datentyp der nur das Datum oder nur die Zeit beinhaltet.
        // TODO: Entweder einen eigenen Datentyp generieren, das als UInt-Wert belassen, oder so wie jetzt als Datetime?
        // Wobei das and DateTime eigentlich nicht richtig ist. In .Net 6 soll es ein DateOnly geben?

        DateTime Value = new DateTime(1990, 1, 1);

        public PlcTagDate(string name, ItemAddress address, uint softdatatype) : base(name, address, softdatatype) { }

        public override void ProcessRead(object valueObj, ulong error)
        {
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

        public DateTime GetValue()
        {
            return Value;
        }

        public override string ToString()
        {
            return ResultString(this, Value.ToShortDateString());
        }
    }

    public class PlcTagTimeOfDay : PlcTag
    {
        // TODO: Gleiches wie bei Date, gibt es in .Net keinen Typ der nur die Uhrzeit beinhaltet, bzw. erst bei .Net 6.
        // Format: 01:02:03 Uhr = 3723000 Anzahl Millisekunden seit 0 Uhr

        uint Value;

        public PlcTagTimeOfDay(string name, ItemAddress address, uint softdatatype) : base(name, address, softdatatype) { }

        public override void ProcessRead(object valueObj, ulong error)
        {
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

        public uint GetValue()
        {
            return Value;
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
        int Value; // Anzahl Millisekunden, mit Vorzeichen

        public PlcTagTime(string name, ItemAddress address, uint softdatatype) : base(name, address, softdatatype) { }

        public override void ProcessRead(object valueObj, ulong error)
        {
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

        public int GetValue()
        {
            return Value;
        }

        public override string ToString()
        {
            int[] divs = { 86400000, 3600000, 60000, 1000, 1 };
            string[] vfmt = { "{0}D", "{0:D02}H", "{0:D02}M", "{0:D02}S", "{0:D03}MS" };
            int vtime = Value;
            bool time_negative = false;
            int val;
            string ts = String.Empty;
            if (vtime == 0)
            {
                ts = "0MS";
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
        // Aufbau S5Time:
        // Bits 15, 14: irrelevant
        // Bits 13, 12: Zeitbasis binär, 00=10ms, 01=100ms, 10=1s, 11=10s
        // Bits 11..0: Zeitwert im BCD-Format (0 bis 999)
        // S5Time_9S_990MS         <Value type="Word">2457</Value>
        // S5Time_2H_46M_30S_0MS    <Value type="Word">14745</Value>
        ushort TimeValue;
        ushort TimeBase;

        public PlcTagS5Time(string name, ItemAddress address, uint softdatatype) : base(name, address, softdatatype) { }

        public override void ProcessRead(object valueObj, ulong error)
        {
            if (CheckErrorAndType(error, valueObj, typeof(ValueWord)) == 0)
            {
                var v = ((ValueWord)valueObj).GetValue();
                TimeValue = BCD_ushort_to_ushort((ushort)(v & (ushort)0x0FFF));
                TimeBase = (ushort)((v & (ushort)0x3000) >> 12);
                Quality = PlcTagQC.TAG_QUALITY_GOOD;
            }
            else
            {
                Quality = PlcTagQC.TAG_QUALITY_BAD;
            }
        }

        public ushort GetTimeValue()
        {
            return TimeValue;
        }

        public ushort GetTimeBase()
        {
            return TimeValue;
        }

        public override string ToString()
        {
            string s = String.Empty;
            // Vorerst alles auf Millisekunden normieren
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
         * uuu = Millisekunden
         * Q = Weekday 1=Su, 2=Mo, 3=Tu, 4=We, 5=Th, 6=Fr, 7=Sa
         */
        DateTime Value = new DateTime(1990, 1, 1);

        public PlcTagDateAndTime(string name, ItemAddress address, uint softdatatype) : base(name, address, softdatatype) { }

        public override void ProcessRead(object valueObj, ulong error)
        {
            if (CheckErrorAndType(error, valueObj, typeof(ValueUSIntArray)) == 0)
            {
                var v = ((ValueUSIntArray)valueObj).GetValue();
                int[] ts = new int[8];
                for (int i = 0; i < 7; i++)
                {
                    ts[i] = BCD_Byte_to_int(v[i]);
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

        public DateTime GetValue()
        {
            return Value;
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
        string Value;
        string m_Encoding = "ISO-8859-1";
        public PlcTagString(string name, ItemAddress address, uint softdatatype) : base(name, address, softdatatype) { }

        public override void ProcessRead(object valueObj, ulong error)
        {
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

        public void SetStringEncoding(string encoding)
        {
            m_Encoding = encoding;
        }

        public string GetValue()
        {
            return Value;
        }

        public override string ToString()
        {
            return ResultString(this, Value);
        }
    }

    public class PlcTagPointer : PlcTag
    {
        byte[] Value = new byte[6];

        public PlcTagPointer(string name, ItemAddress address, uint softdatatype) : base(name, address, softdatatype) { }

        public override void ProcessRead(object valueObj, ulong error)
        {
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

        public byte[] GetValue()
        {
            return Value;
        }

        public override string ToString()
        {
            int dbnr = Value[0] * 256 + Value[1];
            int area = Value[2];
            int bitnr = Value[5] & 0x7;
            int bytenr = Value[5] >> 3 + Value[4] * 32 + (Value[3] & 0x7) * 8192;

            return ResultString(this, String.Format("DB={0} Area=0x{1:X02} Byte={2} Bit={3}", dbnr, area, bytenr, bitnr));
        }
    }

    public class PlcTagAny: PlcTag
    {
        byte[] Value = new byte[10];

        public PlcTagAny(string name, ItemAddress address, uint softdatatype) : base(name, address, softdatatype) { }

        public override void ProcessRead(object valueObj, ulong error)
        {
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

        public byte[] GetValue()
        {
            return Value;
        }

        public override string ToString()
        {
            int hdr = Value[0];
            int datatype = Value[1];
            int factor = Value[2] * 256 + Value[3];
            int dbnr = Value[4] * 256 + Value[5];
            int area = Value[6];
            int bitnr = Value[9] & 0x7;
            int bytenr = Value[9] >> 3 + Value[8] * 32 + (Value[7] & 0x7) * 8192;

            return ResultString(this, String.Format("HDR={0:X02} Type={1:X02} Factor={2} DB={3} Area=0x{4:X02} Byte={5} Bit={6}", hdr, datatype, factor, dbnr, area, bytenr, bitnr));
        }
    }

    public class PlcTagLReal : PlcTag
    {
        double Value;
        public PlcTagLReal(string name, ItemAddress address, uint softdatatype) : base(name, address, softdatatype) { }

        public override void ProcessRead(object valueObj, ulong error)
        {
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

        public double GetValue()
        {
            return Value;
        }

        public override string ToString()
        {
            return ResultString(this, Value.ToString());
        }
    }

    public class PlcTagULInt : PlcTag
    {
        ulong Value;

        public PlcTagULInt(string name, ItemAddress address, uint softdatatype) : base(name, address, softdatatype) { }

        public override void ProcessRead(object valueObj, ulong error)
        {
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

        public ulong GetValue()
        {
            return Value;
        }

        public override string ToString()
        {
            return ResultString(this, Value.ToString());
        }
    }

    public class PlcTagLInt : PlcTag
    {
        long Value;

        public PlcTagLInt(string name, ItemAddress address, uint softdatatype) : base(name, address, softdatatype) { }

        public override void ProcessRead(object valueObj, ulong error)
        {
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

        public long GetValue()
        {
            return Value;
        }

        public override string ToString()
        {
            return ResultString(this, Value.ToString());
        }
    }

    public class PlcTagLWord : PlcTag
    {
        ulong Value;

        public PlcTagLWord(string name, ItemAddress address, uint softdatatype) : base(name, address, softdatatype) { }

        public override void ProcessRead(object valueObj, ulong error)
        {
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

        public ulong GetValue()
        {
            return Value;
        }

        public override string ToString()
        {
            return ResultString(this, Value.ToString());
        }
    }

    public class PlcTagUSInt : PlcTag
    {
        byte Value;

        public PlcTagUSInt(string name, ItemAddress address, uint softdatatype) : base(name, address, softdatatype) { }

        public override void ProcessRead(object valueObj, ulong error)
        {
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

        public byte GetValue()
        {
            return Value;
        }

        public override string ToString()
        {
            return ResultString(this, Value.ToString());
        }
    }

    public class PlcTagUInt : PlcTag
    {
        ushort Value;

        public PlcTagUInt(string name, ItemAddress address, uint softdatatype) : base(name, address, softdatatype) { }

        public override void ProcessRead(object valueObj, ulong error)
        {
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

        public ushort GetValue()
        {
            return Value;
        }

        public override string ToString()
        {
            return ResultString(this, Value.ToString());
        }
    }

    public class PlcTagUDInt : PlcTag
    {
        uint Value;

        public PlcTagUDInt(string name, ItemAddress address, uint softdatatype) : base(name, address, softdatatype) { }

        public override void ProcessRead(object valueObj, ulong error)
        {
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

        public uint GetValue()
        {
            return Value;
        }

        public override string ToString()
        {
            return ResultString(this, Value.ToString());
        }
    }

    public class PlcTagSInt : PlcTag
    {
        sbyte Value;

        public PlcTagSInt(string name, ItemAddress address, uint softdatatype) : base(name, address, softdatatype) { }

        public override void ProcessRead(object valueObj, ulong error)
        {
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

        public sbyte GetValue()
        {
            return Value;
        }

        public override string ToString()
        {
            return ResultString(this, Value.ToString());
        }
    }

    public class PlcTagWChar : PlcTag
    {
        char Value;

        public PlcTagWChar(string name, ItemAddress address, uint softdatatype) : base(name, address, softdatatype) { }

        public override void ProcessRead(object valueObj, ulong error)
        {
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

        public char GetValue()
        {
            return Value;
        }

        public override string ToString()
        {
            return ResultString(this, Value.ToString());
        }
    }
  
    public class PlcTagWString : PlcTag
    {
        string Value;
        public PlcTagWString(string name, ItemAddress address, uint softdatatype) : base(name, address, softdatatype) { }

        public override void ProcessRead(object valueObj, ulong error)
        {
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

        public string GetValue()
        {
            return Value;
        }

        public override string ToString()
        {
            return ResultString(this, Value);
        }
    }

    public class PlcTagLTime : PlcTag
    {
        long Value;

        public PlcTagLTime(string name, ItemAddress address, uint softdatatype) : base(name, address, softdatatype) { }

        public override void ProcessRead(object valueObj, ulong error)
        {
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

        private string ValueAsString()
        {
            // TODO: Duplikat von ToString() von ValueTimespan.
            // Dort gibt es darum herum aber ein XML Element.
            string str;
            long[] divs = { 86400000000000, 3600000000000, 60000000000, 1000000000, 1000000, 1000, 1 };
            string[] vfmt = { "{0}d", "{0:00}h", "{0:00}m", "{0:00}s", "{0:000}ms", "{0:000}us", "{0:000}ns" };
            long val;
            long timespan = Value;
            bool time_negative = false;
            if (timespan == 0)
            {
                str = "000ns";
            }
            else
            {
                if (timespan < 0)
                {
                    str = "-";
                    time_negative = true;
                    for (int i = 0; i < 7; i++)
                    {
                        divs[i] = -divs[i];
                    }
                }
                else
                {
                    str = "";
                }

                for (int i = 0; i < 7; i++)
                {
                    val = timespan / divs[i];
                    timespan -= val * divs[i];
                    if (val > 0)
                    {
                        str += String.Format(vfmt[i], (Int32)val);
                        if ((!time_negative && timespan > 0) || (time_negative && timespan < 0))
                        {
                            str += "_";
                        }
                    }

                }
            }
            return str;
        }

        public long GetValue()
        {
            return Value;
        }

        public override string ToString()
        {
            return ResultString(this, ValueAsString());
        }
    }

    public class PlcTagLTOD : PlcTag
    {
        // TODO: Wie bei den 32 Bit Typen Date/TOD, gibt es in .Net keinen Typ der nur die Uhrzeit beinhaltet, bzw. erst bei .Net 6.
        // Anzahl Nanosekunden seit 0 Uhr
        ulong Value;

        public PlcTagLTOD(string name, ItemAddress address, uint softdatatype) : base(name, address, softdatatype) { }

        public override void ProcessRead(object valueObj, ulong error)
        {
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

        public ulong GetValue()
        {
            return Value;
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
        ulong Value;

        public PlcTagLDT(string name, ItemAddress address, uint softdatatype) : base(name, address, softdatatype) { }

        public override void ProcessRead(object valueObj, ulong error)
        {
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

        private string ValueAsString()
        {
            // TODO: Duplikat von ToString() von ValueTimestamp.
            // Dort gibt es darum herum aber ein XML Element.
            DateTime dt = new DateTime(1970, 1, 1);
            ulong v, ns;
            string fmt;
            v = Value;
            ns = v % 1000000000;
            v /= 1000000000;

            dt = dt.AddSeconds(v);

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
                return dt.ToString();
            }
            return String.Format(fmt, dt.ToString(), ns);
        }

        public ulong GetValue()
        {
            return Value;
        }

        public override string ToString()
        {
            return ResultString(this, ValueAsString());
        }
    }

    public class PlcTagDTL : PlcTag
    {
        // TODO: Einheitliche Typen über alle Zeitformate einsetzen.
        // Leider existiert unter .Net kein Typ mit Nanosekunden-Auflösung.
        byte[] Value = new byte[12];

        public PlcTagDTL(string name, ItemAddress address, uint softdatatype) : base(name, address, softdatatype) { }

        public override void ProcessRead(object valueObj, ulong error)
        {
            if (CheckErrorAndType(error, valueObj, typeof(ValueStruct)) == 0)
            {
                var struct_val = (ValueStruct)valueObj;
                // Der Wert ist die ID mit der Typbeschreibung
                // Für DTL 33554499 = TI_LIB.SimpleType.67
                // Dann folgt eine PackedStruct, mit Interface timestamp, transportflags, und einem ByteArray mit 12 bytes.
                // Aus den Arraywerten wieder die Einzelwertezusammensetzen:
                // 0, 1: YEAR, UInt
                // 2: MONTH, USInt
                // 3: DAY, USInt
                // 4: WEEKDAY, USInt
                // 5: HOUR, USInt
                // 6: MINUTE, USInt
                // 7: SECOND, USInt
                // 8, 9, 10, 11: NANOSECOND, UDInt
                if (struct_val.GetValue() == 33554499)
                {
                    var elem = struct_val.GetStructElement(33554499);
                    if (elem.GetType() == typeof(ValueByteArray))
                    {
                        Value = ((ValueByteArray)elem).GetValue();
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

        public byte[] GetValue()
        {
            return Value;
        }

        public override string ToString()
        {
            string fmt;
            int year = (int)Value[0] * 256 + (int)Value[1];
            uint ns = (uint)Value[8] * 16777216 + (uint)Value[9] * 65536 + (uint)Value[10] * 256 + (uint)Value[11];

            DateTime dt = new DateTime(year, Value[2], Value[3], Value[5], Value[6], Value[7]);

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
                return dt.ToString();
            }
            return String.Format(fmt, dt.ToString(), ns);
        }
    }
}