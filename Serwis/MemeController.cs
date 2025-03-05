using Confluent.Kafka;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Threading.Tasks;

[Route("api/[controller]")]
[ApiController]
public class MemeController : ControllerBase
{
    private readonly HttpClient _httpClient;
    private readonly string _kafkaTopic = "memes-topic";
    private readonly string _kafkaBootstrapServers = "localhost:9092"; // IPv4 zamiast localhost


    public MemeController(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    [HttpGet("random")]
    public async Task<IActionResult> GetRandomMeme()
    {
        // Pobierz losowy obrazek (tu można podpiąć API do memów)
        string imageUrl = "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcT_2RbhVjVx8jsxXvqrnBn4-4uEc0dF_Llw0A&s";

        // Wysyłamy obraz do Kafka
        await SendToKafka(imageUrl);
        return Ok(new { message = "Mem wysłany do Kafka", url = imageUrl });
    }

    private async Task SendToKafka(string imageUrl)
    {
        var config = new ProducerConfig { BootstrapServers = _kafkaBootstrapServers };

        try
        {
            using var producer = new ProducerBuilder<Null, string>(config).Build();
            await producer.ProduceAsync(_kafkaTopic, new Message<Null, string> { Value = imageUrl });
            Console.WriteLine($"✅ Wysłano obraz do Kafka: {imageUrl}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Błąd Kafka: {ex.Message}");
        }
    }
}