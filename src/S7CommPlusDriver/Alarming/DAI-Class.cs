
// On event new alarm coming or going
public class Dai
{
    string ObjectVariableTypeName;
    
    ulong CpuAlarmId;
    byte AllStatesInfo;
    ushort AlarmDomain;
    int MessageType;
    
    uint SequenceCounter;
    
    // Optional lassen sich wohl die Texte mit anfordern:
    // Prüfen in Datei:
    // TIA Logfiles\TIA-V16\ProgramAlarm\v16-1511-programalarm-begleitwerte-fehler-MitAckVonWinCCProf.pcapng
    // In BrowseAlarms.cs ist die Klasse mit den Texten auch schon vorhanden.
    // In dem Blob-Array gibt es dazu vermutlich ein Sparsearray Key, welches die einzelnen Texte sind:
    // 0xa09c8001 = Infotext
    // 0xa09c8002 = Meldetext
    // 0xa09c8003 = Zusatztext 1
    // 0xa09c8004 ...
    // 0xa09c800b = Zusatztext 9
    struct HmiInfo
    {
        ushort SyntaxId;
        ushort Version;
        uint ClientAlarmId;
        byte Priority;
        byte Reserved1;
        byte Reserved2;
        byte Reserved3;
        ushort AlarmClass;
        byte Producer;
        byte GroupId;
        byte Flags;
    }
        
        
    public class AsCgs
    {
        uint Id; // DAI.Going, DAI_Coming???
        
        byte AllStatesInfo;
        DateAndTime Timestamp;
        //struct TI_LIB.SimpleType.275 AssociatedValues;
        DateAndTime AckTimestamp;
    }
}


// After acknowledge (AckJob oder AlarmingJob)
public class AlarmingJob
{
    string ObjectVariableTypeName;
    
    int AlarmJobState;
    DateAndTime JobTimestamp;
    string Application; // nicht immer da, bzw. neu hinzugekommen
    string Host; // nicht immer da, bzw. neu hinzugekommen
    string User; // nicht immer da, bzw. neu hinzugekommen
    List<AlarmAcknfyElem> AcknowledgementList;
    public class AlarmAcknfyElem
    {
        ulong CpuAlarmId;
        byte AllStatesInfo;
        short AckResult;
    }