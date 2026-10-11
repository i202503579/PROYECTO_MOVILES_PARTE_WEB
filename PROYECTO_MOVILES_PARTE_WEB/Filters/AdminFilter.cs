using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace PROYECTO_MOVILES_PARTE_WEB.Filters
{
    public class AdminFilter : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var usuario = context.HttpContext.Session.GetString("UsuarioLogueado");

            if (string.IsNullOrEmpty(usuario))
            {
                context.Result = new RedirectToActionResult("Index", "Login", null);
            }
        }
    }
}
