# S7commPlusDriver

Kommunikationstreiber für den Datenaustausch mit S7-1200/1500 Steuerungen.

## Testprogramme mit allen Datentypen

Im Unterordner s7-1500 liegen Quelldateien für das TIA-Portal, aus denen sich ein Testprogramm
generieren lässt in dem alle möglichen Datentypen vorhanden sind.

- .scl = SCL-Quelldateien (Text)
- .db = Datenbaustein Quelldateien (Text)

Die einfachen Datentypen sind in einzelnen Datenbausteinen gemäß ihrer Kategorie zusammengefasst.
Bei Zahlenwerten ist dabei immer eine Variable mit dem kleinst- und größtmöglichen Wert (Min/Max) vorhanden.
Zudem je nach Bedarf einige Testwerte.

Um die Parametertypen und Zeiger zu generieren, ist für jeden dieser Typen ein FB mit zugehörigem Instanz-DB vorhanden.

Zu beachten ist, dass die S7-1200 nicht alle in der S7-1500 möglichen Datentypen unterstützt.

## Lizenz

Soweit nicht anders vermerkt, gilt für alle Quellcodes die GNU Lesser General Public License,
Version 3 oder später.

## Authors

* **Thomas Wiens** - *Initial work* - [thomas-v2](https://github.com/thomas-v2)
