using System;
using Npgsql;

class Program
{
    static void Main()
    {
        string connStr = "Host=localhost;Port=5432;Database=DestekCRMServisBlazorDb;Username=postgres;Password=Fast123;";
        using (var conn = new NpgsqlConnection(connStr))
        {
            conn.Open();
            foreach (var table in new[] { "BankaKasaHareketleri", "ServisCalismalari", "Faturalar" })
            {
                using (var cmd = new NpgsqlCommand($"SELECT count(*) FROM \"{table}\"", conn))
                {
                    try { Console.WriteLine($"{table}: {cmd.ExecuteScalar()}"); }
                    catch { Console.WriteLine($"{table}: Tablo yok veya hata"); }
                }
            }
        }
    }
}
