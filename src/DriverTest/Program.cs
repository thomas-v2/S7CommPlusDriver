//#define _TEST_BASIC_VAR
#define _TEST_PLCTAG

using System;
using System.Collections.Generic;
using S7CommPlusDriver;

using S7CommPlusDriver.ClientApi;


namespace DriverTest
{
    class Program
    {
        static void Main(string[] args)
        {
            string HostIp = "192.168.1.30";
            string Password = "";
            string Username = "";
            int res;
            List<ItemAddress> readlist = new List<ItemAddress>();
            Console.WriteLine("Main - START");
            // Als Parameter lässt sich die IP-Adresse übergeben, sonst Default-Wert von oben
            if (args.Length >= 1)
            {
                HostIp = args[0];
            }
            // Als Parameter lässt sich das Passwort übergeben, sonst Default-Wert von oben (kein Passwort)
            if (args.Length >= 2)
            {
                Password = args[1];
            }
            // Als Parameter lässt sich der Username übergeben, sonst Default-Wert von oben (kein Username)
            if (args.Length >= 3)
            {
                Username = args[2];
            }
            Console.WriteLine("Main - Versuche Verbindungsaufbau zu: " + HostIp);

            S7CommPlusConnection conn = new S7CommPlusConnection();
            res = conn.Connect(HostIp, Password, Username);
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

#if _TEST_PLCTAG
                #region Werte aller Variablen einlesen
                Console.WriteLine("Main - Lese Werte aller Variablen aus");

                List<PlcTag> taglist = new List<PlcTag>();
                var tags = new List<PlcTag>();

                foreach (var v in vars)
                {
                    taglist.Add(PlcTags.TagFactory(v.Name, new ItemAddress(v.AccessSequence), v.Softdatatype));
                }
                foreach (var t in taglist)
                {
                    tags.Add(t);
                }
                res = conn.ReadTags(tags);
                if (res == 0)
                {
                    Console.WriteLine("====================== VARIABLENHAUSHALT ======================");

                    string formatstring = "{0,-80}{1,-30}{2,-20}{3,-20}";
                    Console.WriteLine(String.Format(formatstring, "SYMBOLIC-NAME", "ACCESS-SEQUENCE", "TYP", "QC: VALUE"));
                    for (int i = 0; i < vars.Count; i++)
                    {
                        string s;

                        s = String.Format(formatstring, taglist[i].Name, taglist[i].Address.GetAccessString(), Softdatatype.Types[taglist[i].Datatype], taglist[i].ToString());
                        Console.WriteLine(s);
                    }
                }
                #endregion
#endif

#if _TEST_BASIC_VAR
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
#endif

                /*
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

                /*
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
                */

                /*
                #region Test: SPS in Stopp setzen
                Console.WriteLine("Setze SPS in STOP...");
                conn.SetPlcOperatingState(1);
                Console.WriteLine("Taste drücken um wieder in RUN zu setzen...");
                Console.ReadKey();
                Console.WriteLine("Setze SPS in RUN...");
                conn.SetPlcOperatingState(3);
                #endregion
                */
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
