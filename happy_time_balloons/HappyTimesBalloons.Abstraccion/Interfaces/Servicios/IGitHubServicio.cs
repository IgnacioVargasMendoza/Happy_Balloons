using System.Threading.Tasks;

namespace HappyTimesBalloons.Abstraccion.Interfaces.Servicios
{
    public interface IGitHubServicio
    {
        Task<string> CrearIssueAsync(string titulo, string cuerpo, string[] labels);
    }
}
