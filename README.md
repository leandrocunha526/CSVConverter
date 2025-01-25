# Sobre

A API disponibilizada em <https://restful-api.dev/> lista arrays de dispositivos eletrônicos e seus atributos. As instruções de chamadas podem ser encontradas no mesmo endereço.

O objetivo deste teste é desenvolver uma rotina em c# que seja capaz de invocar o endpoint da referida API e filtrar os objetos, de forma a imprimir em um arquivo .csv todos os dispositivos da marca Apple, somente com os atributos nome e preço, cada um em uma coluna separada.

Não serão aceitos outputs com outras marcas na exibição.

## Observações

O System.Text.Json funciona também, e é uma alternativa moderna e eficiente ao Newtonsoft.Json para manipulação de JSON no .NET.

```C#
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace AppleDevicesCsvExporter
{
    public class DeviceData
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("data")]
        public Dictionary<string, object> Data { get; set; }
    }

    class Program
    {
        static async Task Main(string[] args)
        {
            var apiUrl = "https://restful-api.dev/api/objects"; // URL da API

            try
            {
                // Chamada à API
                var devices = await GetDevicesFromApi(apiUrl);

                // Filtrar dispositivos da Apple
                var appleDevices = FilterAppleDevices(devices);

                // Exportar para CSV
                ExportToCsv(appleDevices, "AppleDevices.csv");

                Console.WriteLine("Arquivo CSV gerado com sucesso: AppleDevices.csv");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao processar: {ex.Message}");
            }
        }

        // Método para obter dados da API
        static async Task<List<DeviceData>> GetDevicesFromApi(string apiUrl)
        {
            using var client = new HttpClient();
            var response = await client.GetStringAsync(apiUrl);

            // Desserializa o JSON em uma lista de dispositivos
            var devices = JsonSerializer.Deserialize<List<DeviceData>>(response, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return devices;
        }

        // Método para filtrar dispositivos da Apple
        static List<DeviceData> FilterAppleDevices(List<DeviceData> devices)
        {
            var appleDevices = new List<DeviceData>();

            foreach (var device in devices)
            {
                if (device.Name != null && device.Name.Contains("Apple", StringComparison.OrdinalIgnoreCase))
                {
                    appleDevices.Add(device);
                }
            }

            return appleDevices;
        }

        // Método para exportar dispositivos filtrados para CSV
        static void ExportToCsv(List<DeviceData> devices, string fileName)
        {
            using var writer = new StreamWriter(fileName);
            writer.WriteLine("Nome,Preço"); // Cabeçalho do CSV

            foreach (var device in devices)
            {
                var price = "N/A"; // Valor padrão se preço não estiver disponível

                if (device.Data != null && device.Data.TryGetValue("price", out var priceValue))
                {
                    price = priceValue.ToString();
                }

                writer.WriteLine($"{device.Name},{price}");
            }
        }
    }
}
```

Versão usada: .NET 9.0.102

### Demais testes realizados

```C#
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace AppleDeviceCsvExporter
{
    class Program
    {
        static async Task Main(string[] args)
        {
            string apiUrl = "https://api.restful-api.dev/objects"; // URL da API
            string outputFilePath = "AppleDevices.csv"; // Caminho do arquivo CSV de saída

            try
            {
                // Chama a API para obter a lista de dispositivos
                var devices = await FetchDevicesAsync(apiUrl);

                // Filtra apenas os dispositivos da Apple
                var appleDevices = devices
                    .Where(device => device.Name.Contains("Apple", StringComparison.OrdinalIgnoreCase))
                    .ToList();

                // Se encontrar dispositivos Apple, exibe no console e exporta para CSV
                if (appleDevices.Any())
                {
                    // Exporta para CSV
                    ExportToCsv(appleDevices, outputFilePath);
                    Console.WriteLine($"Arquivo CSV gerado com sucesso em: {Path.GetFullPath(outputFilePath)}");
                }
                else
                {
                    Console.WriteLine("Nenhum dispositivo da Apple foi encontrado.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro: {ex.Message}");
            }
        }

        static async Task<List<Device>> FetchDevicesAsync(string apiUrl)
        {
            using var httpClient = new HttpClient();

            // Faz a requisição HTTP para obter os dispositivos
            var response = await httpClient.GetAsync(apiUrl);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Erro ao acessar a API: StatusCode: {response.StatusCode}, Resposta: {errorContent}");
                throw new Exception($"Falha ao acessar a API. StatusCode: {response.StatusCode}, Resposta: {errorContent}");
            }

            // Lê o conteúdo da resposta
            var responseContent = await response.Content.ReadAsStringAsync();

            // Deserializa a resposta da API em uma lista de dispositivos
            try
            {
                var devices = JsonSerializer.Deserialize<List<Device>>(responseContent);
                return devices ?? new List<Device>();
            }
            catch (JsonException jsonEx)
            {
                Console.WriteLine($"Erro ao processar o JSON: {jsonEx.Message}");
                throw new Exception("Erro ao processar o JSON retornado pela API.", jsonEx);
            }
        }

        static void ExportToCsv(IEnumerable<Device> devices, string filePath)
        {
            using var writer = new StreamWriter(filePath);
            writer.WriteLine("Nome,Preço"); // Cabeçalho do CSV

            foreach (var device in devices)
            {
                var name = device.Name;

                // Verifica se o preço está presente no campo Data
                var price = (device.Data != null && device.Data.Price.HasValue) 
                    ? device.Data.Price.Value.ToString() 
                    : "N/A"; // Se o preço não estiver disponível, usa "N/A"

                // Escreve os dados do dispositivo no arquivo CSV
                writer.WriteLine($"{name},{price}");
            }
        }
    }

    // Classe representando os dispositivos
    public class Device
    {
        public string? Name { get; set; } // Nome do dispositivo
        public DeviceData? Data { get; set; } // Dados adicionais, incluindo preço
    }

    // Classe representando os dados do dispositivo (como preço e outros atributos)
    public class DeviceData
    {
        public decimal? Price { get; set; } // Preço do dispositivo (nullable)
    }
}
```
