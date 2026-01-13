
using Newtonsoft.Json;
using Slippi.NET;
using Slippi.NET.Slp;
using Slippi.NET.Slp.EventStream;
using Slippi.NET.Slp.Reader.File;
using System.Buffers;

namespace SlippiTV.Server.Streams;

public class HostedStreams
{
    private readonly IServiceProvider _serviceProvider;
    private readonly StreamManager _streamManager;
    private readonly IWebHostEnvironment _hostEnvironment;

    public HostedStreams(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _streamManager = _serviceProvider.GetRequiredService<StreamManager>();
        _hostEnvironment = _serviceProvider.GetRequiredService<IWebHostEnvironment>();
    }

    public async Task BeginHostingAsync()
    {
        const string user = "SLIPPITV-0";

        while (true)
        {
            try
            {
                ActiveStream stream = _streamManager.CreateOrUpdateStream(user);
                const int chunkSize = 1024;
                byte[] buffer = ArrayPool<byte>.Shared.Rent(chunkSize);

                foreach (string slpFilePath in Directory.EnumerateFiles(
                    Path.Join(_hostEnvironment.ContentRootPath, "/hostedFiles"), "*.slp",
                    new EnumerationOptions() { RecurseSubdirectories = true }))
                {
                    try
                    {
                        var contextFilePath = Path.Join(Path.GetDirectoryName(slpFilePath), "context.json");
                        if (File.Exists(contextFilePath))
                        {
                            var contextShape = new
                            {
                                scores = new[]
                                {
                                    new
                                    {
                                        slots = new[]
                                        {
                                            new
                                            {
                                                displayNames = new string[0],
                                                ports = new int[0],
                                            }
                                        }
                                    }
                                }
                            };

                            var context = JsonConvert.DeserializeAnonymousType(File.ReadAllText(contextFilePath), contextShape);
                            if (context is not null)
                            {
                                stream.NameOverrides = [];
                                foreach (var slot in context.scores[0].slots)
                                {
                                    stream.NameOverrides[slot.ports[0]] = slot.displayNames[0];
                                }
                            }
                            else
                            {
                                stream.NameOverrides = null;
                            }
                        }
                        else
                        {
                            stream.NameOverrides = null;
                        }

                        using var readStream = new FileStream(slpFilePath, FileMode.Open, FileAccess.Read);

                        int frameStarts = 0;
                        List<byte[]> frameBuffer = new List<byte[]>();
                        SlpEventStream eventStream = new SlpEventStream(null);
                        eventStream.OnRaw += (sender, payload) =>
                        {
                            if (payload.Command == Slippi.NET.Types.Command.FRAME_START)
                            {
                                frameStarts++;
                                if (frameStarts == 2)
                                {
                                    Thread.Sleep(TimeSpan.FromMilliseconds(1000.0 / 60.0));
                                    frameStarts = 0;
                                }
                            }

                            if (payload.Command == Slippi.NET.Types.Command.GAME_END)
                            {
                                Thread.Sleep(TimeSpan.FromSeconds(15));
                            }

                            stream.WriteData(payload.Payload);
                        };

                        while (readStream.Position < readStream.Length)
                        {
                            if (readStream.Length - readStream.Position > chunkSize)
                            {
                                readStream.ReadExactly(buffer);
                                eventStream.Write(buffer);
                            }
                            else
                            {
                                byte[] remainder = new byte[(int)(readStream.Length - readStream.Position)];
                                readStream.ReadExactly(remainder);
                                eventStream.Write(remainder);

                                break;
                            }
                        }
                    }
                    catch { }
                }
            }
            catch { }
            finally
            {
                _streamManager.EndStream(user);
            }
        }
    }

    private void EventStream_OnCommand(object? sender, Slippi.NET.Slp.EventStream.Types.SlpStreamCommandEventArgs e)
    {
        throw new NotImplementedException();
    }
}