using System.ComponentModel.Design;
using System.Linq.Expressions;
using NAudio.Midi;

const string PATH = @"bach_846.mid";

static List<string> ReadFile()
{
    var file = new MidiFile(PATH, false);
    var data = new List<string>();

    data.Add(file.DeltaTicksPerQuarterNote.ToString());

    for (var trackNum = 0; trackNum < file.Tracks; trackNum++)
    {
        foreach (var e in file.Events[trackNum])
        {
            if (e.GetType() == typeof(NoteOnEvent))
            {
                try
                {
                    var onEvent = (NoteOnEvent)e;
                    var offEvent = onEvent.OffEvent;

                    var noteNumber = onEvent.NoteNumber;
                    var onTime = onEvent.AbsoluteTime;
                    var offTime = offEvent.AbsoluteTime;
                    var noteLength = onEvent.NoteLength;
                    var velocity = onEvent.Velocity;

                    data.Add($"P{noteNumber}");
                    data.Add(onTime.ToString());
                    data.Add(offTime.ToString());
                    data.Add(noteLength.ToString());
                    data.Add(velocity.ToString());
                }
                catch
                {
                    continue;
                }
            }
            else if (e.GetType() == typeof(TempoEvent))
            {
                var tempoEvent = (TempoEvent)e;
                data.Add($"T{tempoEvent.MicrosecondsPerQuarterNote}");
                data.Add(tempoEvent.AbsoluteTime.ToString());
            }
        }
    }

    return data;
}

static void WriteFile(List<string> data)
{
    var dTicks = int.Parse(data[0]);
    var collection = new MidiEventCollection(0, dTicks);

    var eventStart = 1;
    for (var i = 1; i < data.Count; i++)
    {
        var line = data[i];
        if (char.IsLetter(line[0]) && eventStart != i) // Everything before i is apart of a singular event
        {
            // Tempo event uses 2 lines, note event uses 4
            if (data[eventStart][0] == 'T')
            {
                var microsecondsPerQuarterNote = int.Parse(data[eventStart].Substring(1));
                var absoluteTime = long.Parse(data[eventStart + 1]);
                collection.AddEvent(new TempoEvent(microsecondsPerQuarterNote, absoluteTime), 1);
            }
            else if (data[eventStart][0] == 'P')
            {
                var noteNumber = int.Parse(data[eventStart].Substring(1));
                var onEventTime = long.Parse(data[eventStart + 1]);
                var offEventTime = long.Parse(data[eventStart + 2]);
                var duration = int.Parse(data[eventStart + 3]);
                var velocity = int.Parse(data[eventStart + 4]);
                
                collection.AddEvent(new NoteOnEvent(onEventTime, 1, noteNumber, velocity, duration), 1);
                collection.AddEvent(new NoteEvent(offEventTime, 1, MidiCommandCode.NoteOff, noteNumber, velocity), 1);
            }

            eventStart = i; 
        }
        
    }
    
    MidiFile.Export("rewritten.mid", collection);
}

var data = ReadFile();
WriteFile(data);

var completedString = string.Join('\n', data);
Console.WriteLine(completedString);