using Microsoft.EntityFrameworkCore;
using MvcCorePaginacionRegistros.Data;
using MvcCorePaginacionRegistros.Models;

namespace MvcCorePaginacionRegistros.Repositories
{
    #region VISTAS Y PROCEDIMIENTOS ALMACENADOS
    /*
     --ROW_NUMER() OVER (ORDER BY COLUMNA)
--SELECT TO SELECT
SELECT * FROM
(SELECT ROW_NUMBER() OVER (ORDER BY DEPT_NO) AS POSICION,
DEPT_NO, DNOMBRE, LOC FROM DEPT) QUERY
WHERE QUERY.POSICION = 2

create VIEW V_DEPARTAMENTO_INDIVIDUAL
AS
	SELECT CAST(ROW_NUMBER() OVER (ORDER BY DEPT_NO) as int) AS POSICION,
	DEPT_NO, DNOMBRE, LOC FROM DEPT
GO

SELECT * FROM V_DEPARTAMENTO_INDIVIDUAL
WHERE POSICION = 1
    */
    #endregion
    public class RepositoryHospital
    {
        private HospitalContext context;
        public RepositoryHospital(HospitalContext context)
        {
            this.context = context;
        }
        public async Task<int> GetNumeroRegistrosVistaDepartamentosAsync()
        {
            return await context.VistaDepartamentos.CountAsync();
        }
        public async Task<VistaDepartamento> GetVistaDepartamentoAsync(int posicion)
        {
            VistaDepartamento departamento =
                 await this.context.VistaDepartamentos.Where(d => d.Posicion == posicion).FirstOrDefaultAsync();
            return departamento;
        }
        public async Task<List<VistaDepartamento>> GetGrupoVistaDepartamentosAsync(int posicion)
        {
            var consulta = from datos in this.context.VistaDepartamentos
                           where datos.Posicion >= posicion && datos.Posicion < posicion + 2
                           select datos;
            return await consulta.ToListAsync();
        }
    }
}
