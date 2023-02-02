using System.Collections.Concurrent;
using System.Diagnostics;
using Apollo.MatrixMaths;
using Apollo.IO; 
using Apollo.NeuralNet;

var reconstruct = "Hello World".ToCharArray();
var s = ""; 

var stopwatch = new Stopwatch();
stopwatch.Start();
foreach (var c in reconstruct)
{
    s += c;
}

stopwatch.Stop();

Console.WriteLine($"{s} {stopwatch.Elapsed}");

s = "";

var bag = new ConcurrentBag<char>();
Parallel.ForEach(reconstruct, c =>
{
    bag.Add(c);
});
s = string.Join("", bag);

stopwatch.Stop();

Console.WriteLine($"{s} {stopwatch.Elapsed}");