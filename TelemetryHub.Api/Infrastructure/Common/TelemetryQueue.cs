using System.Threading.Channels;
using TelemetryHub.Api.Domain.Telemetry.Entities;

namespace TelemetryHub.Api.Infrastructure.Common;

public class TelemetryQueue
{
    private readonly Channel<TelemetryEvent> _channel = Channel.CreateBounded<TelemetryEvent>(10000);
    public ChannelWriter<TelemetryEvent> Writer => _channel.Writer;
    public ChannelReader<TelemetryEvent> Reader => _channel.Reader;
}