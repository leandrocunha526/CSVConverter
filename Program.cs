using Newtonsoft.Json;

namespace AppleDevicesCsvExporter
{
    public class DeviceData
    {
        public string? Name { get; set; }
        public Dictionary<string, object> Data { get; set; } = new Dictionary<string, object>();
    }

    class Program
    {
        static async Task Main(string[] args)
        {
            var apiUrl = "https://api.restful-api.dev/objects";  // URL da API

            // Chama a API para obter os dados
            var devices = await GetDevicesFromApi(apiUrl);

            // Filtra os dispositivos da Apple
            var appleDevices = FilterAppleDevices(devices);

            // Exporta os dispositivos para um arquivo CSV
            ExportToCsv(appleDevices, "AppleDevices.csv");
        }

        // Método para fazer a requisição HTTP para a API
        static async Task<List<DeviceData>> GetDevicesFromApi(string apiUrl)
        {
            using var client = new HttpClient();
            var response = await client.GetStringAsync(apiUrl);
            var devices = JsonConvert.DeserializeObject<List<DeviceData>>(response);
            return devices!;
        }

        // Método para filtrar dispositivos da Apple
        static List<DeviceData> FilterAppleDevices(List<DeviceData> devices)
        {
            var appleDevices = new List<DeviceData>();

            foreach (var device in devices)
            {
                if (device.Name!.Contains("Apple"))
                {
                    appleDevices.Add(device);
                }
            }

            return appleDevices;
        }

        // Método para exportar os dispositivos filtrados para um arquivo CSV
        static void ExportToCsv(List<DeviceData> devices, string fileName)
        {
            using var writer = new StreamWriter(fileName);
            writer.WriteLine("Nome,Preço"); // Cabeçalho do CSV

            foreach (var device in devices)
            {
                var price = "N/A"; // Valor padrão se preço não estiver disponível

                if (device.Data != null && device.Data.ContainsKey("price"))
                {
                    price = device.Data["price"].ToString();
                }

                writer.WriteLine($"{device.Name},{price}");
            }
        }
    }
}
