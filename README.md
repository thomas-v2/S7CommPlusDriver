# S7commPlusDriver

Kommunikationstreiber für den Datenaustausch mit S7-1200/1500 Steuerungen.

## Entwicklungsstand

Dies ist aktuell ein Entwicklungsstand und nicht für den Produktiveinsatz vorgesehen.

Ziel ist es, einen Kommunikationstreiber zu entwickeln, welcher den Zugriff auf den Variablenhaushalt
von S7 1200/1500 Steuerungen über den symbolischen Zugriff auf sogenannte "optimierte" Bereiche erlaubt.

Diese Implementierung ist vollständig in C# verfasst. Für die TLS-Verschlüsselung wird die OpenSSL-Bibliothek verwendet.

## Systemvorraussetzungen

### CPU
Der Kommunikationstreiber unterstützt **ausschließlich** CPUs mit einer Firmware welche die sichere Kommunikation
über das TLS-Protokoll erlaubt. Das wären nach aktuellem Wissensstand
- S7 1200 mit einer Firmware >= V4.3 (TLS 1.3 ab V4.5)
- S7 1500 mit einer Firmware >= V2.9

Wichtig ist dabei, dass nicht nur nur eine CPU mit entsprechender Firmware vorhanden ist, sondern auch in der Entwicklungsumgebung
mit der ensprechenden Version projektiert wurde. Das ist nur mit einer TIA-Portal Version >= V17 möglich.

### OpenSSL
Für die TLS-Kommunikation wird OpenSSL verwendet. Ist OpenSSL in der entsprechenden Version installiert, dann sollte ein entsprechender
Systempfad zum Installationsverzeichnis eingetragen sein. Alternativ können die notwendigen Dateien (dlls) auch in das Verzeichnis mit
der ausführbaren Datei abgelegt werden. Das wären je nach verwendetem Betriebssystem:

Für 32 Bit:
- libcrypto-3.dll
- libssl-3.dll

Für 64 Bit:
- libcrypto-3-x64.dll
- libssl-3-x64.dll

## Getestete Kommunikation
Mit folgenden Geräten wurde bisher erfolgreich getestet:
- S7 1211 mit Firmware V4.5
- TIA Plcsim V17 (mit Nettoplcsim)
- TIA Plcsim V18 (mit Nettoplcsim)

## Analyse mit Wireshark
Aufgrund der Verschlüsselung können die übertragenen Daten ohne weitere Informationen mit Wireshark nicht mehr eingesehen werden.
Zur Treiberentwicklung ist im Projekt eine Funktion integriert, welche die ausgehandelten Secrets in eine Textdatei
(key_YYYYMMDD_hhmmss.log) ausgibt. Mit diesen Informationen ist es Wireshark möglich die Kommunikation zu entschlüsseln und darzustellen.

Um Wireshark diese Information verfügbar zu machen, existieren zwei Möglichkeiten:
1. Die Log-Datei in ein Verzeichnis abzulegen und Wireshark dieses bekannt zu machen. Dazu in Wireshark *Menü* → *Einstellungen* aufrufen.
Unter *Protocols* den Punkt *TLS* anwählen, und im Feld *(Pre)-Master-Secret log filename* die entsprechende Datei auswählen
2. Die Secrets direkt in die Wireshark-Aufzeichnung integrieren

Zur Weitergabe an andere Personen zur Analyse, ist Punkt 2 zu bevorzugen, da alles notwendige in einer Aufzeichnung vorhanden ist.
Die Integration geschieht über das Programm "editcap.exe" im Wireshark Installationsverzeichnis. Dazu muss eine Aufzeichnung in
Wireshark mit der Endung *.pcapng* gespeichert werden.

Über die Eingabeaufforderung werden mit folgender Anweisung in die Aufzeichnung "test-capture.pcapng" die Secrets aus "key.log"
integriert, und in der Datei "test-capture-with-keys.pcapng" gespeichert. Wird letztere Datei dann in Wireshark geöffnet, kann die
Kommunikation dort entschlüsselt, dekodiert und dem Protokoll entsprechend dargestellt werden.
Die key.log kann bei Bedarf anschließend gelöscht werden.
```
"C:\Program Files\Wireshark\editcap.exe" --inject-secrets tls,key.log test-capture.pcapng test-capture-with-keys.pcapng
```

Zur Vereinfachung habe ich ein kleines Hilfsprogramm mit einer grafischen Oberfläche geschrieben, auf das die Dateien per Drag&Drop
gezogen werden können, und das auf Tastendruck editcap aufruft. Das Programm ist hier verfügbar:

https://github.com/thomas-v2/PcapKeyInjector

Damit Wireshark das S7comm-Plus Protokoll dekodieren kann, ist die entsprechende dll in das Wireshark Installationsverzeichnis abzulegen.
Näheres dazu und Download der dll bei Sourceforge unter:

https://sourceforge.net/projects/s7commwireshark/

## PlcTag-Klasse: Umsetzung der SPS Datentypen in PlcTags

Bei einigen Datentypen ist es notwendig, zur Verarbeitung der Antwort der SPS den Typ vorab zu kennen, um ihn in ein
sinnvollen Datentyp in .Net zu konvertieren. Dazu wird die PlcTag Klasse bereitgestellt.

In der Tabelle sind alle in der SPS zur Zeit (TIA V18) möglichen Datentypen aufgeführt, mit dem Datentyp in dem sie
auf dem Netzwerk im S7comm-Plus-Protokoll übertragen werden, sowie welcher .Net Datentyp in den PlcTag Klassen daraus
resultiert.

<style scoped>
table {
  font-size: 11px;
}
</style>

| Supported | PLC Datentyp              | PLC Kategorie     | PLC Info          | Netzwerk Datentyp             | .Net Datentyp PlcTag          | Sonstiges                                         |
| :-------: | --------------------------| ----------------- | ----------------- | ----------------------------- | ----------------------------- | ------------------------------------------------- |
| &check;   | AOM_IDENT                 | Hardwaredatentypen|                   | ValueDWord                    | PlcTagDWord -> uint           |                                                   |
| &check;   | Any                       | Zeiger            | Parameter         | ValueUSIntArray[10]           | byte[10]                      |                                                   |
| &check;   | Array[n..m]               |                   |                   |                               |                               | Zugriff auf Einzelelemente direkt möglich         |
| &check;   | Block_FB                  | Parametertypen    | Parameter         | ValueUInt                     | PlcTagUInt -> ushort          |                                                   |
| &check;   | Block_FC                  | Parametertypen    | Parameter         | ValueUInt                     | PlcTagUInt -> ushort          |                                                   |
| &check;   | Bool                      | Binärzahlen       |                   | ValueBool                     | bool                          |                                                   |
| &check;   | Byte                      | Bitfolgen         |                   | ValueByte                     | byte                          |                                                   |
| &check;   | CONN_ANY                  | Hardwaredatentypen|                   | ValueWord                     | PlcTagWord -> ushort          |                                                   |
| &check;   | CONN_OUC                  | Hardwaredatentypen|                   | ValueWord                     | PlcTagWord -> ushort          |                                                   |
| &check;   | CONN_PRG                  | Hardwaredatentypen|                   | ValueWord                     | PlcTagWord -> ushort          |                                                   |
| &check;   | CONN_R_ID                 | Hardwaredatentypen|                   | ValueDWord                    | PlcTagDWord -> uint           |                                                   |
| &check;   | CREF                      | Systemdatentypen  |                   | ValueStruct / packed          |                               | Zugriff auf Einzelelemente direkt möglich         |
| &check;   | Char                      | Zeichenfolgen     |                   | ValueUSInt                    | char                          | Encoding Voreinstellung ISO-8859-1 für non-ASCII  |
| &check;   | Counter                   | Parametertypen    | Parameter         | ValueUInt                     | PlcTagUInt -> ushort          |                                                   |
| &check;   | Date                      | Datum und Uhrzeit |                   | ValueUInt                     | DateTime                      | TODO: Nur Datum gültig!                           |
| &check;   | Date_And_Time             | Datum und Uhrzeit |                   | ValueUSIntArray[8]            | DateTime                      |                                                   |
| &check;   | DB_ANY                    | Hardwaredatentypen|                   | ValueUInt                     | PlcTagUInt -> ushort          |                                                   |
| &check;   | DB_DYN                    | Hardwaredatentypen|                   | ValueUInt                     | PlcTagUInt -> ushort          |                                                   |
| &check;   | DB_WWW                    | Hardwaredatentypen|                   | ValueUInt                     | PlcTagUInt -> ushort          |                                                   |
| &check;   | DInt                      | Ganzzahlen        |                   | ValueDInt                     | int                           |                                                   |
| &check;   | DTL                       | Datum und Uhrzeit |                   | ValueStruct / packed          | byte[12]                      | 33554499, Zugriff auf Einzelelemente direkt möglich. TODO: Hier löschen, oder als DateTime? |
| &check;   | DWord                     | Bitfolgen         |                   | ValueDWord                    | uint                          |                                                   |
| &check;   | EVENT_ANY                 | Hardwaredatentypen|                   | ValueDWord                    | PlcTagDWord -> uint           |                                                   |
| &check;   | EVENT_ATT                 | Hardwaredatentypen|                   | ValueDWord                    | PlcTagDWord -> uint           |                                                   |
| &check;   | EVENT_HWINT               | Hardwaredatentypen|                   | ValueDWord                    | PlcTagDWord -> uint           |                                                   |
| &check;   | ErrorStruct               |                   |                   | ValueStruct / packed          |                               | Zugriff auf Einzelelemente direkt möglich         |
| &check;   | HW_ANY                    | Hardwaredatentypen|                   | ValueWord                     |                               |                                                   |
| &check;   | HW_DEVICE                 | Hardwaredatentypen|                   | ValueWord                     | PlcTagWord -> ushort          |                                                   |
| &check;   | HW_DPMASTER               | Hardwaredatentypen|                   | ValueWord                     | PlcTagWord -> ushort          |                                                   |
| &check;   | HW_DPSLAVE                | Hardwaredatentypen|                   | ValueWord                     | PlcTagWord -> ushort          |                                                   |
| &check;   | HW_HSC                    | Hardwaredatentypen|                   | ValueWord                     | PlcTagWord -> ushort          |                                                   |
| &check;   | HW_IEPORT                 | Hardwaredatentypen|                   | ValueWord                     | PlcTagWord -> ushort          |                                                   |
| &check;   | HW_INTERFACE              | Hardwaredatentypen|                   | ValueWord                     | PlcTagWord -> ushort          |                                                   |
| &check;   | HW_IO                     | Hardwaredatentypen|                   | ValueWord                     | PlcTagWord -> ushort          |                                                   |
| &check;   | HW_IOSYSTEM               | Hardwaredatentypen|                   | ValueWord                     | PlcTagWord -> ushort          |                                                   |
| &check;   | HW_MODULE                 | Hardwaredatentypen|                   | ValueWord                     | PlcTagWord -> ushort          |                                                   |
| &check;   | HW_PTO                    | Hardwaredatentypen|                   | ValueWord                     | PlcTagWord -> ushort          |                                                   |
| &check;   | HW_PWM                    | Hardwaredatentypen|                   | ValueWord                     | PlcTagWord -> ushort          |                                                   |
| &check;   | HW_SUBMODULE              | Hardwaredatentypen|                   | ValueWord                     | PlcTagWord -> ushort          |                                                   |
| &check;   | IEC_COUNTER               | Systemdatentypen  |                   | ValueStruct / packed          |                               | 33554462, Zugriff auf Einzelelemente direkt möglich |
| &check;   | IEC_DCOUNTER              | Systemdatentypen  |                   | ValueStruct / packed          |                               | Zugriff auf Einzelelemente direkt möglich         |
| &check;   | IEC_LCOUNTER              | Systemdatentypen  |                   | ValueStruct / packed          |                               | Zugriff auf Einzelelemente direkt möglich         |
| &check;   | IEC_LTIMER                | Systemdatentypen  |                   | ValueStruct / packed          |                               | Zugriff auf Einzelelemente direkt möglich         |
| &check;   | IEC_SCOUNTER              | Systemdatentypen  |                   | ValueStruct / packed          |                               | Zugriff auf Einzelelemente direkt möglich         |
| &check;   | IEC_TIMER                 | Systemdatentypen  |                   | ValueStruct / packed          |                               | 33554463, Zugriff auf Einzelelemente direkt möglich |
| &check;   | IEC_UCOUNTER              | Systemdatentypen  |                   | ValueStruct / packed          |                               | Zugriff auf Einzelelemente direkt möglich         |
| &check;   | IEC_UDCOUNTER             | Systemdatentypen  |                   | ValueStruct / packed          |                               | Zugriff auf Einzelelemente direkt möglich         |
| &check;   | IEC_ULCOUNTER             | Systemdatentypen  |                   | ValueStruct / packed          |                               | Zugriff auf Einzelelemente direkt möglich         |
| &check;   | IEC_USCOUNTER             | Systemdatentypen  |                   | ValueStruct / packed          |                               | Zugriff auf Einzelelemente direkt möglich         |
| &check;   | Int                       | Ganzzahlen        |                   | ValueInt                      | short                         |                                                   |
| &check;   | LDT                       | Datum und Uhrzeit |                   | ValueTimestamp                | ulong                         |                                                   |
| &check;   | LInt                      | Ganzzahlen        |                   | ValueLInt                     | long                          |                                                   |
| &check;   | LReal                     | Gleitpunktzahlen  |                   | ValueLReal                    | double                        |                                                   |
| &check;   | LTime                     | Zeiten            |                   | ValueTimespan                 | long                          | Anzahl ns                                         |
| &check;   | LTime_Of_Day (LTOD)       | Datum und Uhrzeit |                   | ValueULInt                    | ulong                         | Anzahl ns seit 00:00:00 Uhr                       |
| &check;   | LWord                     | Bitfolgen         |                   | ValueLWord                    | ulong                         |                                                   |
| &check;   | NREF                      | Systemdatentypen  |                   | ValueStruct / packed          |                               | Zugriff auf Einzelelemente direkt möglich         |
| &check;   | OB_ANY                    | Hardwaredatentypen|                   | ValueInt                      | PlcTagInt -> short            |                                                   |
| &check;   | OB_ATT                    | Hardwaredatentypen|                   | ValueInt                      | PlcTagInt -> short            |                                                   |
| &check;   | OB_CYCLIC                 | Hardwaredatentypen|                   | ValueInt                      | PlcTagInt -> short            |                                                   |
| &check;   | OB_DELAY                  | Hardwaredatentypen|                   | ValueInt                      | PlcTagInt -> short            |                                                   |
| &check;   | OB_DIAG                   | Hardwaredatentypen|                   | ValueInt                      | PlcTagInt -> short            |                                                   |
| &check;   | OB_HWINT                  | Hardwaredatentypen|                   | ValueInt                      | PlcTagInt -> short            |                                                   |
| &check;   | OB_PCYCLE                 | Hardwaredatentypen|                   | ValueInt                      | PlcTagInt -> short            |                                                   |
| &check;   | OB_STARTUP                | Hardwaredatentypen|                   | ValueInt                      | PlcTagInt -> short            |                                                   |
| &check;   | OB_TIMEERROR              | Hardwaredatentypen|                   | ValueInt                      | PlcTagInt -> short            |                                                   |
| &check;   | OB_TOD                    | Hardwaredatentypen|                   | ValueInt                      | PlcTagInt -> short            |                                                   |
| &check;   | PIP                       | Hardwaredatentypen|                   | ValueUInt                     | PlcTagUInt -> ushort          |                                                   |
| &check;   | Pointer                   | Zeiger            | Parameter         | ValueUSIntArray[6]            | byte[6]                       |                                                   |
| &check;   | PORT                      | Hardwaredatentypen|                   | ValueUInt                     | PlcTagUInt -> ushort          |                                                   |
| &check;   | RTM                       | Hardwaredatentypen|                   | ValueUInt                     | PlcTagUInt -> ushort          |                                                   |
| &check;   | Real                      | Gleitpunktzahlen  |                   | ValueReal                     | float                         |                                                   |
| &check;   | Remote                    | Zeiger            | Parameter         | ValueUSIntArray[10]           | PlcTagAny -> byte[10]         | Identisch zu Any-Pointer                          |
| &check;   | S5Time                    | Zeiten            |                   | ValueWord                     | ushort, ushort                | TODO: TimeBase, TimeValue. Vereinheitlichen?      |
| &check;   | SInt                      | Ganzzahlen        |                   | ValueSInt                     | sbyte                         |                                                   |
| &check;   | String                    | Zeichenfolgen     |                   | ValueUSIntArray[stringlen + 2]| string                        | Encoding Voreinstellung ISO-8859-1 für non-ASCII  |
| &check;   | Struct                    |                   |                   |                               |                               | Zugriff auf Einzelelemente direkt möglich         |
| &check;   | Time                      | Zeiten            |                   | ValueDInt                     | int                           | Anzahl ms mit Vorzeichen                          |
| &check;   | Time_Of_Day (TOD)         | Datum und Uhrzeit |                   | ValueUDInt                    | uint                          | Anzahl ms seit 00:00:00 Uhr                       |
| &check;   | Timer                     | Parametertypen    | Parameter         | ValueUInt                     | PlcTagUInt -> ushort          |                                                   |
| &check;   | UDInt                     | Ganzzahlen        |                   | ValueUDInt                    | uint                          |                                                   |
| &check;   | UInt                      | Ganzzahlen        |                   | ValueUInt                     | ushort                        |                                                   |
| &check;   | ULInt                     | Ganzzahlen        |                   | ValueULInt                    | ulong                         |                                                   |
| &check;   | USInt                     | Ganzzahlen        |                   | ValueUSInt                    | byte                          |                                                   |
| &cross;   | Variant                   | Zeiger            | Parameter         |                               |                               | Erhält keine Adresse                              |
| &check;   | WChar                     | Zeichenfolgen     |                   | ValueUInt                     | char                          |                                                   |
| &check;   | WString                   | Zeichenfolgen     |                   | ValueUIntArray[stringlen + 2] | string                        |                                                   |
| &check;   | Word                      | Bitfolgen         |                   | ValueWord                     | ushort                        |                                                   |

## Lizenz

Soweit nicht anders vermerkt, gilt für alle Quellcodes die GNU Lesser General Public License,
Version 3 oder später.

## Authors

* **Thomas Wiens** - *Initial work* - [thomas-v2](https://github.com/thomas-v2)
