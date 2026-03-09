using Microsoft.AspNetCore.Mvc;
using MvcCorePaginacionRegistros.Models;
using MvcCorePaginacionRegistros.Repositories;

namespace MvcCorePaginacionRegistros.Controllers
{
    public class PaginacionController : Controller
    {
        private RepositoryHospital repo;
        public PaginacionController(RepositoryHospital repo)
        {
            this.repo = repo;
        }
        public async Task<IActionResult> RegistroVistaDepartamento(int? posicion)
        {
            if (posicion == null)
            {
                posicion = 1;
            }
            int numeroRegistros = await this.repo.GetNumeroRegistrosVistaDepartamentosAsync();
            //primero 1
            //ultimo 4
            //anterior posicion - 1
            //siguiente posicion + 1
            int siguiente = posicion.Value + 1;
            if (siguiente > numeroRegistros)
            {
                siguiente = numeroRegistros;
            }
            int anterior = posicion.Value - 1;
            if (anterior < 1)
            {
                anterior = 1;
            }
            ViewData["ULTIMO"] = numeroRegistros;
            ViewData["ANTERIOR"] = anterior;
            ViewData["SIGUIENTE"] = siguiente;
            VistaDepartamento departamento = await this.repo.GetVistaDepartamentoAsync(posicion.Value);
            return View(departamento);
        }
        public async Task<IActionResult> GrupoVistaDepartamentos(int? posicion)
        {
            if (posicion == null)
            {
                posicion = 1;
            }
            //LO SIGUIENTE SERA QUE DEBEMOS DIBUJAR LOS NUMEROS DE PAGINA EN LOS LINKS
            //VOY A REALIZAR EL DIBUJO DESDE AQUI
            int numRegistros = await this.repo.GetNumeroRegistrosVistaDepartamentosAsync();
            ViewData["REGISTROS"] = numRegistros;
            
            List<VistaDepartamento> departamentos = await this.repo.GetGrupoVistaDepartamentosAsync(posicion.Value);
            return View(departamentos);
        }
        public async Task<IActionResult> GrupoDepartamentos(int? posicion)
        {
            if (posicion == null)
            {
                posicion = 1;
            }
            //LO SIGUIENTE SERA QUE DEBEMOS DIBUJAR LOS NUMEROS DE PAGINA EN LOS LINKS
            //VOY A REALIZAR EL DIBUJO DESDE AQUI
            int numRegistros = await this.repo.GetNumeroRegistrosVistaDepartamentosAsync();
            ViewData["REGISTROS"] = numRegistros;

            List<Departamento> departamentos = await this.repo.GetGrupoDepartamentosAsync(posicion.Value);
            return View(departamentos);
        }
        public IActionResult Index()
        {
            return View();
        }
    }
}
