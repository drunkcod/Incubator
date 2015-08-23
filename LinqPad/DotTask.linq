<Query Kind="Program">
  <Namespace>System.Threading.Tasks</Namespace>
  <Namespace>System.Drawing</Namespace>
</Query>

async Task Main() {
	var t = GraphvizDot(async x => {
		for(var i = 0; i != 1000; ++i)
			await Task.Yield();
		await x.WriteLineAsync(@"digraph {
			graph[rankdir=LR]
			node[shape=square; fontname=Consolas] 
			Hello -> World 
		}".Dump());
	});

	"Wait for it..".Dump();
	(await t).Dump();
}

async Task<Image> GraphvizDot(Func<StreamWriter,Task> input) {
	var p = Process.Start(new ProcessStartInfo {
		UseShellExecute = false,
		CreateNoWindow = true,
		RedirectStandardOutput = true,
		RedirectStandardInput = true,
		FileName = "dot",
		Arguments = "-Tpng",
	});
	var outputBytes = new MemoryStream();
	await Task.WhenAll(
		input(p.StandardInput).ContinueWith(x => p.StandardInput.Close(), TaskContinuationOptions.ExecuteSynchronously),
		p.StandardOutput.BaseStream.CopyToAsync(outputBytes)
	).ConfigureAwait(false);
	outputBytes.Position = 0;
	return Image.FromStream(outputBytes);
}