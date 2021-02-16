using System;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;
using Grpc.Core.Interceptors;
using static MyService;

namespace duplex_streaming_call_hang
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var repro = new Repro();
            await repro.ReadWrite();
            Thread.Sleep(1000);
            Console.WriteLine("Done!");
        }
    }

    class StreamReader<TResponse> : IAsyncStreamReader<TResponse>
    {
        public TResponse Current => throw new NotImplementedException();

        public Task<bool> MoveNext(CancellationToken cancellationToken)
        {
            return Task.FromResult(true);
        }
    }


    class TestInterceptor: Interceptor{
        public override AsyncDuplexStreamingCall<TRequest, TResponse> AsyncDuplexStreamingCall<TRequest, TResponse>
        (ClientInterceptorContext<TRequest, TResponse> context, AsyncDuplexStreamingCallContinuation<TRequest, TResponse> continuation)
        {
            var response = continuation(context);
			return new AsyncDuplexStreamingCall<TRequest, TResponse>(
				response.RequestStream,
				new StreamReader<TResponse>(),
				response.ResponseHeadersAsync,
				response.GetStatus,
				response.GetTrailers,
				response.Dispose);
        }
    }
    class MyServiceServer : MyServiceBase
    {
        public override async Task ReadWrite(IAsyncStreamReader<Request> requestStream,IServerStreamWriter<Response> responseStream, ServerCallContext context){
            await responseStream.WriteAsync(new Response());
            Console.WriteLine("[server] Request sent!");
            var res = await requestStream.MoveNext();
            if(res)
                Console.WriteLine("[server] Got response!");
        }
    }
    

    class Repro {
        Channel _channel;
        MyServiceClient _client;
        Server _server;

        public Repro(){
            /*start gRPC server*/
            _server = new Server
            {
                Services = { MyService.BindService(new MyServiceServer()) },
                Ports = { new ServerPort("localhost", 1234, ServerCredentials.Insecure) }
            };
            _server.Start();

            /*start gRPC client*/
            _channel = new Channel("localhost:1234", ChannelCredentials.Insecure);
            var callInvoker = _channel.CreateCallInvoker()
            .Intercept(new TestInterceptor());
            _client = new MyServiceClient(callInvoker);
        }

        public async Task ReadWrite(){
            var call = _client.ReadWrite();
            var response = call.ResponseStream;
            var request = call.RequestStream;

            await request.WriteAsync(new Request());
            Console.WriteLine("[client] Request sent!");
            var res = await response.MoveNext();
            if(res)
                Console.WriteLine("[client] Got response!");
        }

        private Task<Metadata> GetMetadata()
        {
            return Task.FromResult(new Metadata());
        }

    }
}
