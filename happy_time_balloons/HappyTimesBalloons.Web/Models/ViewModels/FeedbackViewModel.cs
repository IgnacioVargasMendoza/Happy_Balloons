namespace HappyTimesBalloons.Web.Models.ViewModels
{
    public class FeedbackViewModel
    {
        public string Titulo { get; set; }
        public string Descripcion { get; set; }
        public string Tipo { get; set; }              // "Bug" | "Idea" | "Mejora"
        public string ScreenshotBase64 { get; set; }  // puede ser null
    }
}
