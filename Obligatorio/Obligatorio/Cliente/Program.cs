using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using ProtocolLibrary;

namespace Client
{
    class Program
    {
        static void Main(string[] args)
        {
            var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socket.Bind(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 0));
            socket.Connect("127.0.0.1", 20000);
            var connected = true;
            Console.WriteLine("Bienvenido al Sistema Client");
            Console.WriteLine("Opciones validas: ");
            Console.WriteLine("message  <mensaje> -> envia <mensaje> al server");
            Console.WriteLine("exit -> abandonar el programa");
            Console.WriteLine("Ingrese su opcion: ");
            while (connected)
            {
                var opcion = Console.ReadLine();
                switch (opcion)
                {
                    case "exit":
                        socket.Shutdown(SocketShutdown.Both);
                        socket.Close();
                        connected = false;
                        break;
                    case "message":
                        Console.WriteLine("Ingrese el mensaje a enviar:");
                        var mensaje = Console.ReadLine();
                        var header = new Header(HeaderConstants.Request, CommandConstants.Message, mensaje.Length);
                        var data = header.GetRequest();
                        var sentBytes = 0;
                        while (sentBytes < data.Length)
                        {
                            sentBytes += socket.Send(data, sentBytes, data.Length - sentBytes, SocketFlags.None);
                        }

                        sentBytes = 0;
                        var bytesMessage = Encoding.UTF8.GetBytes(mensaje);
                        while (sentBytes < bytesMessage.Length)
                        {
                            sentBytes += socket.Send(bytesMessage, sentBytes, bytesMessage.Length - sentBytes,
                                SocketFlags.None);
                        }

                        break;
                    default:
                        Console.WriteLine("Opcion invalida");
                        break;
                }
            }

            Console.WriteLine("Exiting Application");
        }
    }
}
