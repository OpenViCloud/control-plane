using System.IO.Pipelines;
using System.Net.WebSockets;

namespace Titan.Tests.Infrastructure.Incus;

// Two connected WebSocket ends over in-memory pipes: the customer's end of a shell session without a host.
internal static class InMemoryWebSocket
{
    public static (WebSocket Server, WebSocket Client) Pair()
    {
        var toServer = new Pipe();
        var toClient = new Pipe();
        var serverStream = new DuplexStream(toServer.Reader.AsStream(), toClient.Writer.AsStream());
        var clientStream = new DuplexStream(toClient.Reader.AsStream(), toServer.Writer.AsStream());
        WebSocket server = WebSocket.CreateFromStream(serverStream, isServer: true, subProtocol: null, keepAliveInterval: Timeout.InfiniteTimeSpan);
        WebSocket client = WebSocket.CreateFromStream(clientStream, isServer: false, subProtocol: null, keepAliveInterval: Timeout.InfiniteTimeSpan);
        return (server, client);
    }

    private sealed class DuplexStream(Stream read, Stream write) : Stream
    {
        public override bool CanRead => true;
        public override bool CanWrite => true;
        public override bool CanSeek => false;
        public override long Length => throw new NotSupportedException();
        public override long Position { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }
        public override void Flush() => write.Flush();
        public override Task FlushAsync(CancellationToken ct) => write.FlushAsync(ct);
        public override int Read(byte[] buffer, int offset, int count) => read.Read(buffer, offset, count);
        public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken ct = default) => read.ReadAsync(buffer, ct);
        public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken ct) => read.ReadAsync(buffer, offset, count, ct);
        public override void Write(byte[] buffer, int offset, int count) => write.Write(buffer, offset, count);
        public override ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken ct = default) => write.WriteAsync(buffer, ct);
        public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken ct) => write.WriteAsync(buffer, offset, count, ct);
        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
        public override void SetLength(long value) => throw new NotSupportedException();

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                read.Dispose();
                write.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
