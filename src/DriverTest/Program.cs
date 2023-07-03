using System;
using System.Collections.Generic;
using S7CommPlusDriver;

namespace DriverTest
{
    class Program
    {
        static void Main(string[] args)
        {
            string HostIp;
            int res;
            List<ItemAddress> readlist = new List<ItemAddress>();
            Console.WriteLine("Main - START");
            // Als Parameter lässt sich die IP-Adresse übergeben, sonst Default-Wert von unten
            if (args.Length == 1)
            {
                HostIp = args[0];
            }
            else
            {
                HostIp = "192.168.1.30";
            }
            Console.WriteLine("Main - Versuche Verbindungsaufbau zu: " + HostIp);

            S7CommPlusConnection conn = new S7CommPlusConnection();
            res = conn.Connect(HostIp);
            if (res == 0)
            {
                Console.WriteLine("Main - Connect fertig");

                #region Variablenhaushalt browsen
                Console.WriteLine("Main - Starte Browse...");
                // Variablenhaushalt auslesen
                List<VarInfo> vars = new List<VarInfo>();
                res = conn.Browse(out vars);
                Console.WriteLine("Main - Browse res=" + res);

                #endregion
                #region Werte aller Variablen einlesen
                Console.WriteLine("Main - Lese Werte aller Variablen aus");
                foreach (var v in vars)
                {
                    readlist.Add(new ItemAddress(v.AccessSequence));
                }
                List<object> values = new List<object>();
                List<UInt64> errors = new List<UInt64>();

                // Fehlerhafte Variable setzen
                //readlist[2].LID[0] = 123;
                res = conn.ReadValues(readlist, out values, out errors);
                #endregion


                #region Variablenhaushalt mit Werten ausgeben

                if (res == 0)
                {
                    Console.WriteLine("====================== VARIABLENHAUSHALT ======================");

                    // Liste ausgeben
                    string formatstring = "{0,-80}{1,-30}{2,-20}{3,-20}";
                    Console.WriteLine(String.Format(formatstring, "SYMBOLIC-NAME", "ACCESS-SEQUENCE", "TYP", "VALUE"));
                    for (int i = 0; i < vars.Count; i++)
                    { 
                        string s = String.Format(formatstring, vars[i].Name, vars[i].AccessSequence, Softdatatype.Types[vars[i].Softdatatype], values[i]);
                        Console.WriteLine(s);
                    }
                    Console.WriteLine("===============================================================");
                }
                #endregion
                /*
                #region Test: Dauerlauf
                for (int i = 1; i <= 10000; i++)
                {
                    res = conn.ReadValues(readlist, out values, out errors);
                    if (res != 0)
                    {
                        Console.WriteLine("Fehler bei Durchlauf " + i);
                        break;
                    }
                    if (i % 10 == 0)
                    {
                        Console.WriteLine("Durchlauf: " + i);
                    }
                }
                #endregion

                #region Test: Wert schreiben

                List<PValue> writevalues = new List<PValue>();
                PValue writeValue = new ValueInt(8888);
                writevalues.Add(writeValue);
                List<ItemAddress> writelist = new List<ItemAddress>();
                writelist.Add(new ItemAddress("8A0E0001.F"));
                errors.Clear();
                res = conn.WriteValues(writelist, writevalues, out errors);
                Console.WriteLine("res=" + res);

                #endregion
                */


                #region Test: Absolutadressen lesen
                // Daten aus nicht "optimierten" Datenbausteinen lesen
                readlist.Clear();
                ItemAddress absAdr = new ItemAddress();
                absAdr.SetAccessAreaToDatablock(100); // DB 100
                absAdr.SymbolCrc = 0;

                absAdr.AccessSubArea = Ids.DB_ValueActual;
                absAdr.LID.Add(3);  // LID_OMS_STB_ClassicBlob
                absAdr.LID.Add(0);  // Blob Start Offset, Anfangsadresse
                absAdr.LID.Add(20); // 20 Bytes

                readlist.Add(absAdr);

                values.Clear();
                errors.Clear();

                res = conn.ReadValues(readlist, out values, out errors);
                Console.WriteLine(values.ToString());

                #endregion


                #region Test: SPS in Stopp setzen
                Console.WriteLine("Setze SPS in STOP...");
                conn.SetPlcOperatingState(1);
                Console.WriteLine("Taste drücken um wieder in RUN zu setzen...");
                Console.ReadKey();
                Console.WriteLine("Setze SPS in RUN...");
                conn.SetPlcOperatingState(3);

                #endregion

                conn.Disconnect();
            }
            else
            {
                Console.WriteLine("Main - Connect fehlgeschlagen!");
            }
            Console.WriteLine("Main - ENDE. Bitte Taste drücken.");
            Console.ReadKey();
        }
    }
}
