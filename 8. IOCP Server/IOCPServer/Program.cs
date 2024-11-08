using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Collections.Concurrent;

namespace IOCPServer
{
    internal class Program
    {
        // 클라이언트 소켓과 관련된 이벤트 인수를 저장하는 딕셔너리
        static ConcurrentDictionary<Socket, SocketAsyncEventArgs> clientSockets = new ConcurrentDictionary<Socket, SocketAsyncEventArgs>();

        // 모든 클라이언트에게 메시지를 브로드캐스트하는 함수
        static void BroadCast(string message, Socket excludeSocket)
        {
            byte[] messageBytes = Encoding.UTF8.GetBytes(message); // 메시지를 바이트 배열로 변환
            foreach (var client in clientSockets.Keys)
            {
                // 제외할 소켓을 제외하고 메시지 전송
                if (client != excludeSocket)
                {
                    try
                    {
                        client.SendAsync(new ArraySegment<byte>(messageBytes), SocketFlags.None); // 비동기로 메시지 전송
                    }
                    catch (SocketException)
                    {
                        Console.WriteLine("Failed to send message to a client."); // 소켓 전송 실패 시 메시지 출력
                    }
                }
            }
        }

        // 클라이언트로부터 받은 메시지를 처리하는 함수
        static void OnReceive(object? sender, SocketAsyncEventArgs e)
        {
            var handler = (Socket)sender!; // 메시지를 보낸 클라이언트 소켓
            if (e.BytesTransferred > 0 && e.SocketError == SocketError.Success) // 전송된 데이터가 있고 에러가 없는 경우
            {
                string receivedMessage = Encoding.UTF8.GetString(e.Buffer, 0, e.BytesTransferred); // 받은 데이터를 문자열로 변환
                Console.WriteLine($"Received message from {handler.RemoteEndPoint}: \"{receivedMessage}\"");

                // 모든 클라이언트에게 메시지 브로드캐스트
                BroadCast($"{handler.RemoteEndPoint}: {receivedMessage}", handler);

                // "exit" 메시지 처리
                if (receivedMessage.Trim().ToLower() == "exit")
                {
                    Console.WriteLine("Client has sent closing message.");
                    CloseClientSocket(handler); // 클라이언트 연결 종료
                    return;
                }

                // 다음 수신 대기
                if (!handler.ReceiveAsync(e))
                {
                    OnReceive(handler, e); // 비동기 작업이 즉시 완료되면 직접 호출
                }
            }
            else
            {
                CloseClientSocket(handler); // 데이터가 없거나 에러가 발생한 경우 소켓 닫기
            }
        }

        // 클라이언트 소켓 연결을 종료하는 함수
        static void CloseClientSocket(Socket clientSocket)
        {
            clientSockets.TryRemove(clientSocket, out _); // 딕셔너리에서 소켓 제거
            try
            {
                clientSocket.Shutdown(SocketShutdown.Both); // 소켓 종료
            }
            catch (SocketException) { }
            clientSocket.Close(); // 소켓 닫기
            Console.WriteLine("Client disconnected.");
        }

        // 클라이언트 연결을 처리하는 함수
        static void AcceptCallback(object? sender, SocketAsyncEventArgs e)
        {
            var listenSocket = (Socket)sender!; // 리스닝 소켓
            var handler = e.AcceptSocket; // 연결된 클라이언트 소켓

            if (handler != null && handler.Connected)
            {
                Console.WriteLine("Client connected: " + handler.RemoteEndPoint);

                // 클라이언트 소켓의 수신 이벤트 준비
                var receiveEventArgs = new SocketAsyncEventArgs();
                receiveEventArgs.Completed += OnReceive; // 수신 완료 이벤트에 OnReceive 연결
                receiveEventArgs.SetBuffer(new byte[1024], 0, 1024); // 버퍼 설정
                clientSockets[handler] = receiveEventArgs; // 클라이언트 소켓 저장

                // 클라이언트 소켓에 대해 비동기 수신 대기
                if (!handler.ReceiveAsync(receiveEventArgs))
                {
                    OnReceive(handler, receiveEventArgs); // 비동기 작업이 즉시 완료되면 직접 호출
                }
            }

            // 다음 클라이언트 연결 대기
            e.AcceptSocket = null; // AcceptSocket을 null로 설정하여 다음 연결 준비
            if (!listenSocket.AcceptAsync(e))
            {
                AcceptCallback(listenSocket, e); // 비동기 작업이 즉시 완료되면 직접 호출
            }
        }

        // 서버를 시작하고 클라이언트 연결을 대기하는 함수
        static void StartListening()
        {
            IPEndPoint ipEndPoint = new(IPAddress.Loopback, 25000); // 로컬호스트 IP와 포트를 설정
            Socket listenSocket = new Socket(ipEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp); // TCP 소켓 생성

            listenSocket.Bind(ipEndPoint); // 소켓 바인딩
            listenSocket.Listen(10); // 대기열 크기 설정
            Console.WriteLine("Listening socket is waiting for a connection...");

            // 클라이언트 연결 요청을 처리할 이벤트 인수 준비
            var acceptEventArgs = new SocketAsyncEventArgs();
            acceptEventArgs.Completed += AcceptCallback; // 연결 완료 이벤트에 AcceptCallback 연결
            if (!listenSocket.AcceptAsync(acceptEventArgs))
            {
                AcceptCallback(listenSocket, acceptEventArgs); // 비동기 작업이 즉시 완료되면 직접 호출
            }
        }

        static void Main(string[] args)
        {
            StartListening(); // 서버 시작 및 클라이언트 연결 대기
            Console.WriteLine("Press ENTER to stop the server.");
            Console.ReadLine(); // 서버 종료 대기
        }
    }
}
