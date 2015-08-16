<Query Kind="Program">
  <Namespace>System.Threading.Tasks</Namespace>
  <Namespace>System.Drawing</Namespace>
  <Namespace>System.Collections.Concurrent</Namespace>
</Query>

void Main() {
	var times = Measure(250, () => {
		var timer = Stopwatch.StartNew();
		Task.WaitAll(
			GitLog(@"D:\Work\panelapi", new[]{ "an", "B" }, x => { 
				//new { Author = x[0].AsString(), Message = x[1].AsString() }.Dump();
				//if(x.Message.Split('\n').Length > 1) x.Dump();
				//c.Add(x);
			}));
	}).ToList();
	new {
		Min = times.Min(),
		Avg = TimeSpan.FromTicks(times.Sum(x => x.Ticks) / times.Count),
	}.Dump();
}

static class ArraySegmentExtensions
{
	public static string AsString(this ArraySegment<char> self) {
		return new string(self.Array, self.Offset, self.Count);
	}
}

IEnumerable<TimeSpan> Measure(int iters, Action action) {
	var timer = new Stopwatch();
	GC.Collect();
	var gen = new[]{ 0, 1, 2 };
	var gcStats = Array.ConvertAll(gen, GC.CollectionCount);
	
	for(var i = 0; i != iters; ++i) {
		timer.Restart();
		action();
		yield return timer.Elapsed;
	}
	
	Array.ConvertAll(gen, x => GC.CollectionCount(x) - gcStats[x]).Dump();
}

const char RecordSeparator = '\x1e';
const char UnitSeparator = '\x1f';

Task GitLog(string path, string[] fields, Action<ArraySegment<char>[]> push) {
	var format = new StringBuilder("log --format=");
	for(var i = 0; i != fields.Length; ++i)
		format.Append('%').Append(fields[i]).Append(UnitSeparator);
	format.Append(RecordSeparator);	
	var p = new Process {
		StartInfo = new ProcessStartInfo {
			UseShellExecute = false,
			CreateNoWindow = true,
			RedirectStandardOutput = true,
			FileName = "git",
			WorkingDirectory = path,
			Arguments = format.ToString(),
			StandardOutputEncoding = Encoding.UTF8,
		},
	};
	p.Start();
	p.PriorityBoostEnabled = true;

	var context = new GitLogContext(p.StandardOutput, fields, push);
	context.BeginRead();
	return context.Task;
}

class GitLogContext
{
	const int MaxRecordSize = 4096;
	static AsyncCallback Callback = x => ((GitLogContext)x.AsyncState).HandleBlock(x);

	readonly Decoder decoder;
	readonly Stream output;

	readonly Action<ArraySegment<char>[]> push;
	readonly TaskCompletionSource<object> tcs;
	readonly byte[] buffer;
	readonly char[] partBuffer;
	readonly ArraySegment<char>[] parts;
	int partStart, partEnd;
	int currentPart;
	
	public GitLogContext(StreamReader source, string[] fields, Action<ArraySegment<char>[]> push) {
		this.decoder = source.CurrentEncoding.GetDecoder();
		this.output = source.BaseStream;
		this.push = push;
		this.buffer = new byte[512];
		this.partBuffer = new char[MaxRecordSize];
		this.parts = new ArraySegment<char>[fields.Length];
		this.tcs = new TaskCompletionSource<object>();
	}
	
	public Task Task { get { return tcs.Task; } }
	
	void HandleBlock(IAsyncResult x) {
		var bytesRead = output.EndRead(x);
		if(bytesRead == 0) {
			tcs.SetResult(null);
			return;
		}
			
		var charsRead = decoder.GetChars(buffer, 0, bytesRead, partBuffer, partEnd);
		for(;charsRead != 0; --charsRead) {
			var c = partBuffer[partEnd++];
			switch(c) {
				case UnitSeparator:
					FinalizePart();
					break;
					
				case RecordSeparator:
					push(parts);
					//realign the starts
					Array.Copy(partBuffer, partEnd, partBuffer, 0, charsRead);
					currentPart = 0;
					partStart = partEnd = 0;
					break;
			}
		}
		BeginRead();
	}
	
	public void BeginRead() {
		output.BeginRead(buffer, 0, buffer.Length, Callback, this);
	}
		
	void FinalizePart() {
		while(partStart != partEnd && IsCarrigeReturnOrLineFeed(partBuffer[partStart]))
			++partStart;
			
		var trimEnd = partEnd - 1;
		while(trimEnd > partStart && IsCarrigeReturnOrLineFeed(partBuffer[trimEnd - 1]))
			--trimEnd;
		
		parts[currentPart++] = new ArraySegment<char>(partBuffer, partStart, trimEnd - partStart);
		partStart = partEnd;
	}
	
	static bool IsCarrigeReturnOrLineFeed(char c) {
		return c == '\n' || c == '\r';
	}
}