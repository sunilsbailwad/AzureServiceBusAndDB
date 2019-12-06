using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;
using System;
using System.Data.SqlClient;

namespace CoreSenderApp
{
    class Program
    {
        const string ServiceBusConnectionString = "Endpoint=sb://tqlservicebus.servicebus.windows.net/;SharedAccessKeyName=tqlPolicy;SharedAccessKey=AvW4X71zcw3rcnOH+OHGUnIgihOer5Gho5MQ/26p958=;";
        const string QueueName = "tqlqueue";
        static IQueueClient queueClient;

        public static async Task Main(string[] args)
        {
            const int numberOfMessages = 10;
            queueClient = new QueueClient(ServiceBusConnectionString, QueueName);
            const string DbConnectionString = "server=(localdb)\\MSSQLLocalDB;Database=TQL_TestDB_SB;Integrated Security=True;";

            Console.WriteLine("======================================================");
            Console.WriteLine("Press ENTER key to exit after sending all the messages.");
            Console.WriteLine("======================================================");

            // Send messages.
            await SendMessagesAsync(numberOfMessages);

            //Insert Message in DB
            int test= await InsertMessagesAsync(numberOfMessages, DbConnectionString);

            Console.ReadKey();
            await queueClient.CloseAsync();

        }
      

        public static Task<int> InsertMessagesAsync(int numberOfMessagesToSend, string sConnectionString)
        {  
            string messageBody = $"Message {1}";           
            string msg = messageBody;

            return Task.Run(() =>
                {
                    using (var cnn = new SqlConnection(sConnectionString)) {                      
                        cnn.Open();
                        SqlCommand command = new SqlCommand();
                        command.CommandType = System.Data.CommandType.Text;
                        command.CommandText = "INSERT QuestMsg1 ( QueMessage) VALUES ('" + msg + "')";
                        command.Connection = cnn;
                        SqlDataAdapter adapter = new SqlDataAdapter();
                        return command.ExecuteNonQuery();
                    }
                });
           
            }
        static async Task SendMessagesAsync(int numberOfMessagesToSend)
        {
            try
            {
                for (var i = 0; i < numberOfMessagesToSend; i++)
                {
                    // Create a new message to send to the queue.
                    string messageBody = $"Message {i}";
                    var message = new Message(Encoding.UTF8.GetBytes(messageBody));

                    // Write the body of the message to the console.
                    Console.WriteLine($"Sending message: {messageBody}");

                    // Send the message to the queue.
                    await queueClient.SendAsync(message);
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine($"{DateTime.Now} :: Exception: {exception.Message}");
            }
        }
    }
}
