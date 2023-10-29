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

## Lizenz

Soweit nicht anders vermerkt, gilt für alle Quellcodes die GNU Lesser General Public License,
Version 3 oder später.

## Authors

* **Thomas Wiens** - *Initial work* - [thomas-v2](https://github.com/thomas-v2)
