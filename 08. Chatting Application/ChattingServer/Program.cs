namespace ChattingServer
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Enter \"START\" for running server");
            Console.WriteLine("Enter \"STOP\" for terminate server");
            Console.WriteLine("Enter \"CHECK\" for see how many people connected");
            Console.WriteLine("Enter \"QUIT\" for terminate program");

            string input;
            while(true)
            {
                input = Console.ReadLine() ?? "";
                if(input.ToLower().Equals("start"))
                {

                }
                else if(input.ToLower().Equals("stop"))
                {
                    Console.WriteLine("Are you sure to shutdown server?  ['p']");
                    input = Console.ReadLine() ?? "";
                    if (input.Equals("q"))
                    {
                        Console.WriteLine("OK, bye");
                        return;
                    }
                }
                else if(input.ToLower().Equals("check"))
                {

                }
                else if(input.ToLower().Equals("quit"))
                {
                    Console.WriteLine("Are you sure to quit?  ['q']");
                    input = Console.ReadLine() ?? "";
                    if (input.Equals("q"))
                    {
                        Console.WriteLine("OK, bye");
                        return;
                    }
                }
                else
                {
                    continue;
                }
            }
        }
    }
}
