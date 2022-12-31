using Apollo.MIDI;

const string PATH = @"schu_143_1.mid";

var stringData = MidiManager.ReadFile(PATH);
MidiManager.WriteFile(stringData, "rewritten.mid"); 