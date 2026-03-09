using Microsoft.AspNetCore.Mvc;
using MvcCorePaginacionRegistros.Models;
using MvcCorePaginacionRegistros.Repositories;

namespace MvcCorePaginacionRegistros.Controllers
{
    public class DepartamentosController : Controller
    {
        private RepositoryHospital repo;
        public DepartamentosController(RepositoryHospital repo)
        {
            this.repo = repo;
        }
        public async Task<IActionResult> Details(int? posicion,int iddept)
        {
            if (posicion == null)
            {
                posicion = 1;
            }
            ViewData["DEPT"] = await this.repo.FindDepartamentoAsync(iddept);
            ModelEmpleadosOficio model = 
                await this.repo.GetGrupoEmpleadosDepartamentosOutAsync((int)posicion, iddept);

            int registros = await this.repo.GetEmpleadosDepartamentosCountAsync(iddept);
            ViewData["REGISTROS"] = registros;
            int anterior = (int)posicion - 1;
            if (anterior == 0)
            {
                anterior = 1;
            }
            ViewData["ANTERIOR"] = anterior;
            int siguiente = (int)posicion + 1;
            if(siguiente > registros) 
            {
                siguiente = registros;
            }
            ViewData["SIGUIENTE"] = siguiente;
            return View(model);
        }
        public async Task<IActionResult> DetailsEntityFramework(int? posicion,int iddept)
        {
            if (posicion == null)
            {
                posicion = 1;
            }
            ViewData["DEPT"] = await this.repo.FindDepartamentoAsync(iddept);
            ModelEmpleadoDepartamento model = 
                await this.repo.GetGrupoEmpleadosDepartamentosEFAsync((int)posicion, iddept);

            int registros = await this.repo.GetEmpleadosDepartamentosCountAsync(iddept);
            ViewData["REGISTROS"] = registros;
            int anterior = (int)posicion - 1;
            if (anterior == 0)
            {
                anterior = 1;
            }
            ViewData["ANTERIOR"] = anterior;
            int siguiente = (int)posicion + 1;
            if(siguiente > registros) 
            {
                siguiente = registros;
            }
            ViewData["SIGUIENTE"] = siguiente;
            return View(model);
        }
    }
}
