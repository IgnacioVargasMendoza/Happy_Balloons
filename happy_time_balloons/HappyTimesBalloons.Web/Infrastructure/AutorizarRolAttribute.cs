using System.Web.Mvc;

namespace HappyTimesBalloons.Web.Infrastructure
{
    // Extiende [Authorize] para distinguir "no autenticado" (→ login) de
    // "autenticado sin el rol requerido" (→ 403 Prohibido), evitando el
    // comportamiento confuso de redirigir al login cuando el usuario ya inició sesión.
    public class AutorizarRolAttribute : AuthorizeAttribute
    {
        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            if (filterContext.HttpContext.Request.IsAuthenticated)
            {
                filterContext.Result = new RedirectResult("~/Error/Prohibido");
            }
            else
            {
                base.HandleUnauthorizedRequest(filterContext);
            }
        }
    }
}
