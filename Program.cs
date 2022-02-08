using Newtonsoft.Json;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace CodeTest
{
    class Program
    {
        static void Main(string[] args)
        {

            //call API endpoind
            //aggregate the data 
            //pass the data to the rabbitMQ 
            //another application to consume that post response 
            CallWebAPIAsync().Wait();
            AggregateData();
            
            //call the api every five minute, may be make the thread wait 




        }
        

        private static void AggregateData()
        {
            //loop through the response body and add the data 
            
        }

        static async Task<string> CallWebAPIAsync()
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("http://renewables-codechallenge.azurewebsites.net/");
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                string customerJsonString;
                HttpResponseMessage response = await client.GetAsync("api/Site/1");

                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine("Request Message Information:- \n\n" + response.RequestMessage + "\n");
                    Console.WriteLine("Response Message Header \n\n" + response.Content.Headers + "\n");
                    // Get the response
                    customerJsonString = await response.Content.ReadAsStringAsync();
                    Console.WriteLine("Your response data is: " + customerJsonString);
                   
                    // Deserialise the data
                    
                    var deserializedData = JsonConvert.DeserializeObject(customerJsonString);
                    //to-do , pass the data to AggregateData and store the result to be passed onto RabitMQ 
                    ConnectRabitMQ(customerJsonString);   // To do- dont call it from here, move it to a different class and to the computation thhere
                    
                    return customerJsonString;

                }
               
                else
                {
                    //throw an exception 
                }
            } return "Error"; // change the return statement 
           
        }

        private static void ConnectRabitMQ(string customerJsonString)
        {
            var factory = new ConnectionFactory() { HostName = "localhost" };
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.QueueDeclare(queue: "hello",
                                     durable: false,
                                     exclusive: false,
                                     autoDelete: false,
                                     arguments: null);

                // string message = "Hello World!";
                string message = (customerJsonString);;
                var body = Encoding.UTF8.GetBytes((string)customerJsonString);

                channel.BasicPublish(exchange: "",
                                     routingKey: "hello",
                                     basicProperties: null,
                                     body: body);
                Console.WriteLine(" [x] Sent {0}", message);
            }

            Console.WriteLine(" Press [enter] to exit.");
            Console.ReadLine();

        }

    }
}
